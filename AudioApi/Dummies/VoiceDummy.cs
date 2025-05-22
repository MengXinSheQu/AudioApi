using AudioApi.Compents;
using AudioApi.Enums;
using Mirror;
using System.Collections.Generic;
using UnityEngine;
using VoiceChat;
using Logger = LabApi.Features.Console.Logger;

namespace AudioApi.Dummies
{ 
    /// <summary>
    /// 假人生成器
    /// </summary>
    public static class VoiceDummy
    {
        /// <summary>
        /// 假人列表
        /// </summary>
        public static Dictionary<int, ReferenceHub> List { get; } = [];
        /// <summary>
        /// 清理全部假人
        /// <para><paramref name="type"/>清理的类型 默认仅清除GameObject</para>
        /// </summary>
        /// <param name="type">清理类型</param>
        /// <returns>若返回true 则成功清理所有假人</returns>
        public static bool Clear(ClearType type = ClearType.GameObject)
        {
            try
            {
                foreach (var player in List.Values)
                {
                    if (type == ClearType.GameObject)
                    {
                        NetworkServer.Destroy(player.gameObject);
                    }
                    else
                    {
                        try
                        {
                            NetworkServer.DestroyPlayerForConnection(player.connectionToClient);
                            NetworkServer.Destroy(player.gameObject);
                        }
                        catch { NetworkServer.Destroy(player.gameObject); }
                    }
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
        /// <para><paramref name="Paths"/> 文件的父路径</para>
        /// <para><paramref name="MusicName"/>音频名称 无需添加后缀名</para>
        /// <para><paramref name="Volume"/>音频大小 100为100% 默认为50f</para>
        /// <para><paramref name="Loop"/>是否循环 默认为false</para>
        /// </summary>
        /// <para><paramref name="player"/> 玩家的Hub</para>
        /// <param name="Id">假人Id 若没有会创建他</param>
        /// <param name="Paths">文件的父路径</param>
        /// <param name="MusicName">音频名称 无需添加后缀名</param>
        /// <param name="Volume">音频大小 100为100%</param>
        /// <param name="Loop">是否循环</param>
        public static void PlayToPlayer(this ReferenceHub player, int Id, string Paths, string MusicName, float Volume = 50f, bool Loop = false)
        {
            if (!List.ContainsKey(Id))
                Add(Id, "Bot");
            ReferenceHub component = List[Id];
            VoicePlayerBase VoicePlayerBase = VoicePlayerBase.Get(component);
            VoicePlayerBase.Enqueue(Paths + "\\" + MusicName + ".ogg", -1);
            VoicePlayerBase.LogDebug = false;
            VoicePlayerBase.BroadcastTo.Add(player.PlayerId);
            VoicePlayerBase.Volume = Volume;
            VoicePlayerBase.Loop = Loop;
            VoicePlayerBase.Play(0);
        }
        /// <summary>
        /// 向全体玩家播放音乐
        /// <para><paramref name="Id"/> 假人Id 若没有会创建他</para>
        /// <para><paramref name="Paths"/> 完整路径 具体查看参数注释</para>
        /// <para><paramref name="type"/>音频读取方式</para>
        /// <para><paramref name="Volume"/>音频大小 100为100% 默认为50f</para>
        /// <para><paramref name="Loop"/>是否循环 默认为false</para>
        /// </summary>
        /// <param name="Id">假人Id 若没有会创建他</param>
        /// <param name="Paths">完整路径
        /// <para>根据<see cref="FileReaderType"/>选择</para>
        /// <para>若为<see cref="FileReaderType.Default"/> 则需要添加.ogg后缀名</para>
        /// <para>若为<see cref="FileReaderType.HasExtension"/> 则无需后缀名</para></param>
        /// <param name="type">音频读取方式</param>
        /// <param name="Volume">音频大小 100为100%</param>
        /// <param name="Loop">是否循环</param>
        public static void Play(int Id, string Paths, FileReaderType type, float Volume = 50f, bool Loop = false)
        {
            if (!List.ContainsKey(Id))
                Add(Id, "Bot");
            Logger.Info($"播放音乐[{Paths}]");
            ReferenceHub component = List[Id];
            VoicePlayerBase VoicePlayerBase = VoicePlayerBase.Get(component);
            string str = Paths;
            switch (type)
            {
                case FileReaderType.HasExtension:
                    str = Paths + ".ogg";
                    break;
            }
            VoicePlayerBase.Enqueue(str, -1);
            VoicePlayerBase.LogDebug = false;
            VoicePlayerBase.BroadcastChannel = VoiceChatChannel.Intercom;
            VoicePlayerBase.Volume = Volume;
            VoicePlayerBase.Loop = Loop;
            VoicePlayerBase.Play(0);
        }
        /// <summary>
        /// 向全体玩家播放音乐
        /// <para><paramref name="Id"/> 假人Id 若没有会创建他</para>
        /// <para><paramref name="Paths"/> 文件的父路径</para>
        /// <para><paramref name="MusicName"/>音频名称 无需添加后缀名</para>
        /// <para><paramref name="Volume"/>音频大小 100为100% 默认为50f</para>
        /// <para><paramref name="Loop"/>是否循环 默认为false</para>
        /// </summary>
        /// <param name="Id">假人Id 若没有会创建他</param>
        /// <param name="Paths">文件的父路径</param>
        /// <param name="MusicName">音频名称 无需添加后缀名</param>
        /// <param name="Volume">音频大小 100为100%</param>
        /// <param name="Loop">是否循环</param>
        public static void Play(int Id, string Paths, string MusicName, float Volume = 50f, bool Loop = false)
        {
            if (!List.ContainsKey(Id))
                Add(Id, "Bot");
            Logger.Info($"播放音乐[{MusicName}]");
            ReferenceHub component = List[Id];
            VoicePlayerBase VoicePlayerBase = VoicePlayerBase.Get(component);
            string str = Paths + "\\" + MusicName + ".ogg";
            VoicePlayerBase.Enqueue(str, -1);
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
        /// 删除假人
        /// <para><paramref name="Id"/> 假人Id</para>
        /// </summary>
        /// <param name="Id">假人Id</param>
        public static void Remove(int Id)
        {
            try
            {
                VoicePlayerBase VoicePlayerBase = VoicePlayerBase.Get(List[Id]);
                if (VoicePlayerBase != null && VoicePlayerBase.CurrentPlay != null)
                {
                    VoicePlayerBase.Stoptrack(true);
                    VoicePlayerBase.OnDestroy();
                }
                NetworkServer.Destroy(List[Id].gameObject);
                List.Remove(Id);
                Logger.Info($"删除 [{Id}]");
            }
            catch 
            {
            
            }
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
            Logger.Info($"添加 [{Id}-{Name}]");
            GameObject obj = Object.Instantiate(NetworkManager.singleton.playerPrefab);
            NetworkServer.AddPlayerForConnection(new FakeConnection(Id), obj);
            ReferenceHub component = obj.GetComponent<ReferenceHub>();
            component.nicknameSync.Network_myNickSync = Name;
            List[Id] = component;
            return true;
        }
    }
}
