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
using AudioApi.EventArgs.Voice;
using AudioApi.Interfaces;
using VoiceChat.Codec.Enums;
using AudioApi.Extensions;

namespace AudioApi
{
#pragma warning disable CS8618
    public class VoicePlayerBase : MonoBehaviour, IPlayerBase,IUnityMethod
    {
        /// <summary>
        /// AudioPlayers列表
        /// </summary>
        public static Dictionary<ReferenceHub, VoicePlayerBase> AudioPlayers { get; set; } = [];
        public OpusEncoder Encoder { get; } = new(OpusApplicationType.Voip);
        public PlaybackBuffer PlaybackBuffer { get; } = new PlaybackBuffer();
        public int HeadSamples { get; set; } = 1920;
        public byte[] EncodedBuffer { get; } = new byte[512];

        /// <summary>
        /// 玩家Hub
        /// </summary>
        public ReferenceHub Owner { get; set; }

        public bool stopTrack = false;
        public bool ready = false;
        public CoroutineHandle PlaybackCoroutine;
        public float allowedSamples;
        public int samplesPerSecond;
        public Queue<float> StreamBuffer { get; } = [];
        public VorbisReader VorbisReader { get; set; }
        public float[] SendBuffer { get; set; }
        public float[] ReadBuffer { get; set; }
        public float Volume { get; set; } = 100f;
        public List<string> AudioToPlay { get; set; } = [];
        public string CurrentPlay { get; set; }
        public MemoryStream CurrentPlayStream { get; set; }
        public bool Loop { get; set; } = false;
        public bool Shuffle { get; set; } = false;
        public bool Continue { get; set; } = true;
        public bool ShouldPlay { get; set; } = true;
        public bool LogDebug { get; set; } = false;
        public bool LogInfo { get; set; } = false;
        public bool IsFinished { get; set; } = false;
        public bool ClearOnFinish { get; set; } = false;
        public List<ReferenceHub> BroadcastTo { get; set; } = [];
        public Func<ReferenceHub,bool> BroadcastFunc { get; set; }
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
        /// 当生成触发
        /// </summary>
        public static event Action<PlayerBaseSpawnedEventArgs> OnSpawned;

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
        public virtual void Play(int queuePos)
        {
            if (PlaybackCoroutine.IsRunning)
                Timing.KillCoroutines(PlaybackCoroutine);
            PlaybackCoroutine = Timing.RunCoroutine(Playback(queuePos), Segment.FixedUpdate);
        }
        public virtual void Stoptrack(bool clear)
        {
            if (clear)
                AudioToPlay.Clear();
            stopTrack = true;
        }
        public virtual void Enqueue(string audio, int pos)
        {
            if (pos == -1)
                AudioToPlay.Add(audio);
            else
                AudioToPlay.Insert(pos, audio);
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
                    AudioToPlay = [.. AudioToPlay.OrderBy(i => Random.value)];
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
        public virtual void OnDestroy()
        {
            if (PlaybackCoroutine.IsRunning)
                Timing.KillCoroutines(PlaybackCoroutine);

            AudioPlayers.Remove(Owner);

            if (ClearOnFinish)
                NetworkServer.RemovePlayerForConnection(Owner.connectionToClient, true);
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

                foreach (var ply in ReferenceHub.AllHubs)
                {
                    var conn = ply.connectionToClient;
                    if (conn == null || !ply.IsConnected() || BroadcastTo.Count >= 1 && !BroadcastTo.Contains(ply)) continue;
                    if(BroadcastFunc != null)
                    {
                        if(BroadcastFunc.Invoke(ply))
                            conn.Send(new VoiceMessage(Owner, BroadcastChannel, EncodedBuffer, dataLen, false));
                    }
                    else
                        conn.Send(new VoiceMessage(Owner, BroadcastChannel, EncodedBuffer, dataLen, false));
                }
            }
        }
        public virtual void Start()
        {
            OnSpawned?.Invoke(new(this));
        }
        public virtual void Awake()
        {
        }
    }
}
