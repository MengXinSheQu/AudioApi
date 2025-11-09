using CentralAuth;

namespace AudioApi.Extensions
{
    /// <summary>
    /// 杂项扩展
    /// </summary>
    public static class MiscExtension
    {
        /// <summary>
        /// 检测玩家是否连接服务器 且玩家为真玩家
        /// </summary>
        /// <param name="hub">玩家Hub</param>
        public static bool IsConnected(this ReferenceHub hub)
        {
            return hub.authManager.InstanceMode == ClientInstanceMode.ReadyClient &&
                   hub.nicknameSync.NickSet &&
                   !hub.isLocalPlayer &&
                   !string.IsNullOrEmpty(hub.authManager.UserId) &&
                   !hub.authManager.UserId.Contains("Dummy");
        }
        /// <summary>
        /// 通过玩家ReferenceHub获取VoicePlayerBase
        /// </summary>
        /// <param name="hub">玩家的ReferenceHub</param>
        /// <param name="voicePlayerBase">VoicePlayerBase输出</param>
        /// <returns>如果为true，则说明能正常返回</returns>
        public static bool TryGetVoicePlayer(this ReferenceHub hub,out VoicePlayerBase? voicePlayerBase)
        {
            if(VoicePlayerBase.AudioPlayers.TryGetValue(hub, out var result))
            {
                voicePlayerBase = result;
                return true;
            }
            voicePlayerBase = null;
            return false;
        }
    }
}
