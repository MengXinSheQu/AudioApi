using AudioApi.Compents;
using Mirror;
using System.Collections.Generic;
using UnityEngine;
using VoiceChat;
using Logger = LabApi.Features.Console.Logger;

namespace AudioApi.AudioCore.Dummies
{ 
    public static class VoiceDummy
    {
        /// <summary>
        /// 假人列表
        /// </summary>
        public static Dictionary<int, ReferenceHub> List { get; } = new Dictionary<int, ReferenceHub>();
        /// <summary>
        /// 清理所有假人
        /// </summary>
        public static void Clear()
        {
            try
            {
                foreach (var player in List.Values)
                {
                    if (player.gameObject != null)
                        NetworkServer.Destroy(player.gameObject);
                }
            }
            catch { }
            List.Clear();
        }
        /// <summary>
        /// 对玩家播放音乐
        /// </summary>
        /// <param name="player">玩家Hub</param>
        /// <param name="Id">假人Id</param>
        /// <param name="Paths">路径(仅文件夹)</param>
        /// <param name="MusicName">文件名称(不能加.ogg后缀名)</param>
        /// <param name="Volume">音量大小</param>
        /// <param name="Loop">是否循环</param>
        public static void PlaySound(this ReferenceHub player, int Id, string Paths, string MusicName, float Volume = 50f, bool Loop = false)
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
        /// 全局播放音乐
        /// </summary>
        /// <param name="Id">假人Id</param>
        /// <param name="Paths">路径(仅文件夹)</param>
        /// <param name="MusicName">文件名称(不能加.ogg后缀名)</param>
        /// <param name="Volume">音量大小</param>
        /// <param name="Loop">是否循环</param>
        public static void PlaySound(int Id, string Paths, string MusicName, float Volume = 50f, bool Loop = false)
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
        /// </summary>
        /// <param name="Id"></param>
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
                List[Id] = null;
                Logger.Info($"删除 [{Id}]");
            }
            catch 
            {
            
            }
        }
        /// <summary>
        /// 添加假人
        /// </summary>
        /// <param name="Id">假人Id</param>
        /// <param name="Name">假人昵称</param>
        public static void Add(int Id, string Name = "Bot")
        {
            if (List.ContainsKey(Id) && List[Id] != null)
                return;
            Logger.Info($"添加 [{Id}-{Name}]");
            GameObject obj = Object.Instantiate(NetworkManager.singleton.playerPrefab);
            NetworkServer.AddPlayerForConnection(new FakeConnection(Id), obj);
            ReferenceHub component = obj.GetComponent<ReferenceHub>();
            component.nicknameSync.Network_myNickSync = Name;
            List[Id] = component;
        }
    }
}
