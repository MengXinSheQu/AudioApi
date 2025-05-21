using CentralAuth;
using MEC;
using Mirror;
using NVorbis;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using VoiceChat;
using VoiceChat.Codec;
using VoiceChat.Networking;
using Random = UnityEngine.Random;
using Log = LabApi.Features.Console.Logger;
using System;
using AudioApi.AudioCore.EventArgs.Voice;

namespace AudioApi.AudioCore
{
    public class VoicePlayerBase : MonoBehaviour
    {
        public static Dictionary<ReferenceHub, VoicePlayerBase> AudioPlayers = new Dictionary<ReferenceHub, VoicePlayerBase>();
        public const int HeadSamples = 1920;
        public OpusEncoder Encoder { get; } = new OpusEncoder(VoiceChat.Codec.Enums.OpusApplicationType.Voip);
        public PlaybackBuffer PlaybackBuffer { get; } = new PlaybackBuffer();
        public byte[] EncodedBuffer { get; } = new byte[512];
        public bool stopTrack = false;
        public bool ready = false;
        public CoroutineHandle PlaybackCoroutine;
        public float allowedSamples;
        public int samplesPerSecond;
        public Queue<float> StreamBuffer { get; } = new Queue<float>();
        public VorbisReader VorbisReader { get; set; }
        public float[] SendBuffer { get; set; }
        public float[] ReadBuffer { get; set; }
        /// <summary>
        /// 玩家Hub
        /// </summary>
        public ReferenceHub Owner { get; set; }
        /// <summary>
        /// 音频大小 百分比
        /// </summary>
        public float Volume { get; set; } = 100f;
        /// <summary>
        /// 所有播放的音乐
        /// </summary>
        public List<string> AudioToPlay = new List<string>();
        /// <summary>
        /// 正在播放的音乐路径
        /// </summary>
        public string CurrentPlay;
        /// <summary>
        /// 包含音频的数据流
        /// </summary>
        public MemoryStream CurrentPlayStream;
        /// <summary>
        /// 是否循环
        /// </summary>
        public bool Loop = false;
        /// <summary>
        /// 是否随机播放
        /// </summary>
        public bool Shuffle = false;
        /// <summary>
        /// 是否应在当前音频结束后继续切换播放
        /// </summary>
        public bool Continue = true;
        /// <summary>
        /// 是否播放音频
        /// </summary>
        public bool ShouldPlay = true;
        /// <summary>
        /// Debug日志
        /// </summary>
        public bool LogDebug = false;
        /// <summary>
        /// 消息日志
        /// </summary>
        public bool LogInfo = false;
        /// <summary>
        /// 音频是否播放完成
        /// </summary>
        public bool IsFinished = false;

        /// <summary>
        /// 播放后清理音频
        /// </summary>
        public bool ClearOnFinish = false;

        /// <summary>
        /// 所有接受播放的玩家Id
        /// </summary>
        public List<int> BroadcastTo = new List<int>();
        /// <summary>
        /// 音频播放类型
        /// </summary>
        public VoiceChatChannel BroadcastChannel { get; set; } = VoiceChatChannel.Proximity;
        /// <summary>
        /// 当音频选择中触发
        /// </summary>
        public static event Action<TrackSelectingEventArgs> OnTrackSelecting;
        /// <summary>
        /// 当音频选择完成触发
        /// </summary>
        public static event Action<TrackSelectedEventArgs> OnTrackSelected;
        /// <summary>
        /// 当音频加载完成触发
        /// </summary>
        public static event Action<TrackLoadedEventArgs> OnTrackLoaded;
        /// <summary>
        /// 当音频播放完成触发
        /// </summary>
        public static event Action<TrackFinishedEventArgs> OnFinishedTrack;
        /// <summary>
        /// 创建VoicePlayerBase
        /// </summary>
        /// <param name="hub">游戏对象</param>
        /// <returns><see cref="VoicePlayerBase"/></returns>
        public static VoicePlayerBase Create()
        {
            var player = new GameObject().AddComponent<VoicePlayerBase>();
            AudioPlayers.Add(player.Owner, player);
            return player;
        }
        /// <summary>
        /// 通过Hub获取VoicePlayerBase
        /// </summary>
        /// <param name="hub">玩家Hub</param>
        /// <returns><see cref="VoicePlayerBase"/></returns>
        public static VoicePlayerBase Get(ReferenceHub hub)
        {
            if (AudioPlayers.TryGetValue(hub, out VoicePlayerBase player))
            {
                return player;
            }

            player = hub.gameObject.AddComponent<VoicePlayerBase>();
            player.Owner = hub;

            AudioPlayers.Add(hub, player);
            return player;
        }
        /// <summary>
        /// 播放音频
        /// </summary>
        /// <param name="queuePos">音频列表位置</param>
        public virtual void Play(int queuePos)
        {
            if (PlaybackCoroutine.IsRunning)
                Timing.KillCoroutines(PlaybackCoroutine);
            PlaybackCoroutine = Timing.RunCoroutine(Playback(queuePos), Segment.FixedUpdate);
        }
        /// <summary>
        /// 停止播放音频
        /// </summary>
        /// <param name="Clear">如果为true 下次播放从头播放</param>
        public virtual void Stoptrack(bool Clear)
        {
            if (Clear)
                AudioToPlay.Clear();
            stopTrack = true;
        }
        /// <summary>
        /// 向列表加入音频
        /// </summary>
        /// <param name="audio">路径</param>
        /// <param name="pos">音频播放位置，使用-1转到队列末尾</param>
        public virtual void Enqueue(string audio, int pos)
        {
            if (pos == -1)
                AudioToPlay.Add(audio);
            else
                AudioToPlay.Insert(pos, audio);
        }
        public virtual void OnDestroy()
        {
            if (PlaybackCoroutine.IsRunning)
                Timing.KillCoroutines(PlaybackCoroutine);

            AudioPlayers.Remove(Owner);

            if (ClearOnFinish)
                NetworkServer.RemovePlayerForConnection(Owner.connectionToClient, true);
        }
        public virtual IEnumerator<float> Playback(int position)
        {
            stopTrack = false;
            IsFinished = false;
            int index = position;
            var trackselecting = new TrackSelectingEventArgs(this, index == -1, true);
            OnTrackSelecting?.Invoke(trackselecting);
            if(!trackselecting.IsAllowed)
            {
                if (AudioToPlay.Count >= 1)
                    Timing.RunCoroutine(Playback(0));
                yield break;
            }
            if (index != -1)
            {
                if (Shuffle)
                    AudioToPlay = AudioToPlay.OrderBy(i => Random.value).ToList();
                CurrentPlay = AudioToPlay[index];
                AudioToPlay.RemoveAt(index);
                if (Loop)
                {
                    AudioToPlay.Add(CurrentPlay);
                }
            }
            OnTrackSelected?.Invoke(new TrackSelectedEventArgs(this, index == -1, index));
            if (LogInfo)
                Log.Info($"加载音频中...");
            if (File.Exists(CurrentPlay))
            {
                if (!CurrentPlay.EndsWith(".ogg"))
                {
                    Log.Error($"音频 {CurrentPlay} 必须为.ogg格式");
                    yield return Timing.WaitForSeconds(1);
                    if (AudioToPlay.Count >= 1)
                        Timing.RunCoroutine(Playback(0));
                    yield break;
                }
                CurrentPlayStream = new MemoryStream(File.ReadAllBytes(CurrentPlay));
            }
            else
            {
                Log.Error($"音频 {CurrentPlay} 不存在，已经跳过");
                yield return Timing.WaitForSeconds(1);
                if (AudioToPlay.Count >= 1)
                    Timing.RunCoroutine(Playback(0));
                yield break;
            }
            CurrentPlayStream.Seek(0, SeekOrigin.Begin);
            VorbisReader = new VorbisReader(CurrentPlayStream);

            if (VorbisReader.Channels >= 2)
            {
                Log.Error($"音频 {CurrentPlay} 必须为单轨道");
                yield return Timing.WaitForSeconds(1);
                if (AudioToPlay.Count >= 1)
                    Timing.RunCoroutine(Playback(0));
                VorbisReader.Dispose();
                CurrentPlayStream.Dispose();
                yield break;
            }

            if (VorbisReader.SampleRate != 48000)
            {
                Log.Error($"音频 {CurrentPlay} 采样率必须为48000");
                yield return Timing.WaitForSeconds(1);
                if (AudioToPlay.Count >= 1)
                    Timing.RunCoroutine(Playback(0));
                VorbisReader.Dispose();
                CurrentPlayStream.Dispose();
                yield break;
            }
            OnTrackLoaded?.Invoke(new TrackLoadedEventArgs(this, CurrentPlay, index ,index == -1));

            if (LogInfo)
                Log.Info($"播放 {CurrentPlay}");

            samplesPerSecond = VoiceChatSettings.SampleRate * VoiceChatSettings.Channels;
            SendBuffer = new float[samplesPerSecond / 5 + HeadSamples];
            ReadBuffer = new float[samplesPerSecond / 5 + HeadSamples];
            int cnt;
            while ((cnt = VorbisReader.ReadSamples(ReadBuffer, 0, ReadBuffer.Length)) > 0)
            {
                if (stopTrack)
                {
                    VorbisReader.SeekTo(VorbisReader.TotalSamples - 1);
                    stopTrack = false;
                }
                while (!ShouldPlay)
                {
                    yield return Timing.WaitForOneFrame;
                }
                while (StreamBuffer.Count >= ReadBuffer.Length)
                {
                    ready = true;
                    yield return Timing.WaitForOneFrame;
                }
                for (int i = 0; i < ReadBuffer.Length; i++)
                {
                    StreamBuffer.Enqueue(ReadBuffer[i]);
                }
            }

            if (LogInfo)
                Log.Info($"播放完成");

            int nextQueuepos = 0;
            if (Continue && Loop && index == -1)
            {
                nextQueuepos = -1;
                Timing.RunCoroutine(Playback(nextQueuepos));
                OnFinishedTrack?.Invoke(new TrackFinishedEventArgs(this, CurrentPlay, index == -1));
                yield break;
            }

            if (Continue && AudioToPlay.Count >= 1)
            {
                IsFinished = true;
                Timing.RunCoroutine(Playback(nextQueuepos));
                OnFinishedTrack?.Invoke(new TrackFinishedEventArgs(this, CurrentPlay, index == -1));
                yield break;
            }

            IsFinished = true;
            OnFinishedTrack?.Invoke(new TrackFinishedEventArgs(this, CurrentPlay, index == -1));

            if (ClearOnFinish)
                Destroy(this);
        }
        public virtual void Update()
        {
            if (Owner == null || !ready || StreamBuffer.Count == 0 || !ShouldPlay) return;

            allowedSamples += Time.deltaTime * samplesPerSecond;
            int toCopy = Mathf.Min(Mathf.FloorToInt(allowedSamples), StreamBuffer.Count);
            if (LogDebug)
                Log.Debug($"1 {toCopy} {allowedSamples} {samplesPerSecond} {StreamBuffer.Count} {PlaybackBuffer.Length} {PlaybackBuffer.WriteHead}");
            if (toCopy > 0)
            {
                for (int i = 0; i < toCopy; i++)
                {
                    PlaybackBuffer.Write(StreamBuffer.Dequeue() * (Volume / 100f));
                }
            }

            if (LogDebug)
                Log.Debug($"2 {toCopy} {allowedSamples} {samplesPerSecond} {StreamBuffer.Count} {PlaybackBuffer.Length} {PlaybackBuffer.WriteHead}");

            allowedSamples -= toCopy;

            while (PlaybackBuffer.Length >= 480)
            {
                PlaybackBuffer.ReadTo(SendBuffer, 480, 0L);
                int dataLen = Encoder.Encode(SendBuffer, EncodedBuffer, 480);

                foreach (var plr in ReferenceHub.AllHubs)
                {
                    if (plr.connectionToClient == null || !PlayerIsConnected(plr) || (BroadcastTo.Count >= 1 && !BroadcastTo.Contains(plr.PlayerId))) continue;

                    plr.connectionToClient.Send(new VoiceMessage(Owner, BroadcastChannel, EncodedBuffer, dataLen, false));
                }
            }
        }

        /// <summary>
        /// 检测玩家是否连接服务器 且玩家为真玩家
        /// </summary>
        /// <param name="hub">玩家Hub</param>
        private bool PlayerIsConnected(ReferenceHub hub)
        {
            return hub.authManager.InstanceMode == ClientInstanceMode.ReadyClient &&
                   hub.nicknameSync.NickSet &&
                   !hub.isLocalPlayer &&
                   !string.IsNullOrEmpty(hub.authManager.UserId) &&
                   !hub.authManager.UserId.Contains("Dummy");
        }
    }
}
