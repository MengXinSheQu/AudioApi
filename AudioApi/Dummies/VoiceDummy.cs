using AudioApi.Compents;
using Mirror;
using System.Collections.Generic;
using UnityEngine;
using VoiceChat;

namespace AudioApi.Dummies
{ 
    /// <summary>
    /// 一个音乐假人生成器
    /// </summary>
    public static class VoiceDummy
    {
        /// <summary>
        /// 假人列表
        /// </summary>
        public static Dictionary<int, ReferenceHub> List { get; } = [];
        /// <summary>
        /// 清理全部假人
        /// </summary>
        /// <returns>若返回true 则成功清理所有假人</returns>
        public static bool Clear()
        {
            try
            {
                foreach (var player in List.Values)
                {
                    NetworkServer.RemovePlayerForConnection(player.connectionToClient,true);
                }
                List.Clear();
                return true;
            }
            catch { return false; }
        }
        /// <summary>
        /// 对单一玩家播放音乐
        /// <para><paramref name="player"/> 玩家的Hub</para>
        /// <para><paramref name="Id"/> 假人Id 若没有会创建他</para>
        /// <para><paramref name="file"/> 文件路径</para>
        /// <para><paramref name="Volume"/>音频大小 100为100% 默认为50f</para>
        /// <para><paramref name="Loop"/>是否循环 默认为false</para>
        /// </summary>
        /// <para><paramref name="player"/> 玩家的Hub</para>
        /// <param name="Id">假人Id 若没有会创建他</param>
        /// <param name ="file">文件路径</param>
        /// <param name="Volume">音频大小 100为100%</param>
        /// <param name="Loop">是否循环</param>
        public static void PlayToPlayer(this ReferenceHub player, int Id, string file, float Volume = 50f, bool Loop = false)
        {
            if (!List.ContainsKey(Id))
                Add(Id, "Bot");
            ReferenceHub component = List[Id];
            VoicePlayerBase VoicePlayerBase = VoicePlayerBase.Get(component);
            VoicePlayerBase.Enqueue(file, -1);
            VoicePlayerBase.LogDebug = false;
            VoicePlayerBase.BroadcastTo.Add(player);
            VoicePlayerBase.Volume = Volume;
            VoicePlayerBase.Loop = Loop;
            VoicePlayerBase.Play(0);
        }
        /// <summary>
        /// 向全体玩家播放音乐
        /// <para><paramref name="Id"/> 假人Id 若没有会创建他</para>
        /// <para><paramref name="file"/> 文件</para>
        /// <para><paramref name="Volume"/>音频大小 100为100% 默认为50f</para>
        /// <para><paramref name="Loop"/>是否循环 默认为false</para>
        /// </summary>
        /// <param name="Id">假人Id 若没有会创建他</param>
        /// <param name = "file">文件路径</param>
        /// <param name="Volume">音频大小 100为100%</param>
        /// <param name="Loop">是否循环</param>
        public static void Play(int Id, string file, float Volume = 50f, bool Loop = false)
        {
            if (!List.ContainsKey(Id))
                Add(Id, "Bot");
            ReferenceHub component = List[Id];
            VoicePlayerBase VoicePlayerBase = VoicePlayerBase.Get(component);
            VoicePlayerBase.Enqueue(file, -1);
            VoicePlayerBase.LogDebug = false;
            VoicePlayerBase.BroadcastChannel = VoiceChatChannel.Intercom;
            VoicePlayerBase.Volume = Volume;
            VoicePlayerBase.Loop = Loop;
            VoicePlayerBase.Play(0);
        }
        /// <summary>
        /// 停止播放音乐
        /// <para><paramref name="Id"/> 假人Id</para>
        /// </summary>
        /// <param name="Id">假人Id</param>
        public static void StopAudio(int Id)
        {
            try
            {
                VoicePlayerBase VoicePlayerBase = VoicePlayerBase.Get(List[Id]);
                if (VoicePlayerBase == null)
                    return;
                if (VoicePlayerBase.CurrentPlay != null)
                {
                    VoicePlayerBase.Stoptrack(true);
                    VoicePlayerBase.OnDestroy();
                }
            }
            catch 
            {
            
            }
        }
        /// <summary>
        /// 通过Id删除假人
        /// </summary>
        /// <param name="Id">假人Id</param>
        /// <returns>若返回true 则删除成功</returns>
        public static bool Remove(int Id)
        {
            if (List.TryGetValue(Id, out var hub))
            {
                if (hub.TryGetComponent<VoicePlayerBase>(out var voicePlayerBase))
                {
                    if (voicePlayerBase.CurrentPlay != null)
                    {
                        voicePlayerBase.Stoptrack(true);
                        voicePlayerBase.OnDestroy();
                    }
                }
                NetworkServer.RemovePlayerForConnection(hub.connectionToClient, true);
                List.Remove(Id);
                return true;
            }
            return false;
        }
        /// <summary>
        /// 向服务器添加一个假人
        /// <para>请注意! 该行为可能会导致玩家人数上限+1 请自觉添加人数定子!</para>
        /// <para><paramref name="Id"/> 假人Id</para>
        /// <para><paramref name="Name"/>假人昵称</para>
        /// </summary>
        /// <param name="Id">假人Id</param>
        /// <param name="Name">假人昵称</param>
        /// <returns>若返回为true 则说明成功添加假人</returns>
        public static bool Add(int Id, string Name = "Bot")
        {
            if (List.ContainsKey(Id))
                return false;
            GameObject obj = Object.Instantiate(NetworkManager.singleton.playerPrefab);
            NetworkServer.AddPlayerForConnection(new FakeConnection(Id), obj);
            ReferenceHub component = obj.GetComponent<ReferenceHub>();
            component.nicknameSync.Network_myNickSync = Name;
            List[Id] = component;
            return true;
        }
    }
}
