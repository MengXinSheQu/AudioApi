using VoiceChat.Networking;

namespace AudioApi.Interfaces
{
    /// <summary>
    /// PlaybackBuffer接口，提供了PlaybackBuffer
    /// </summary>
    public interface IPlaybackBuffer
    {
        /// <summary>
        /// PlaybackBuffer，用于音频转换
        /// </summary>
        PlaybackBuffer PlaybackBuffer { get; }
    }
}
