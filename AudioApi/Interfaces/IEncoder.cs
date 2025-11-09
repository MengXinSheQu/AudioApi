using VoiceChat.Codec;

namespace AudioApi.Interfaces
{    /// <summary>
     /// OpusEncoder接口，提供了OpusEncoder
     /// </summary>
    public interface IEncoder
    {
        /// <summary>
        /// Encoder，用于音频转译
        /// </summary>
        OpusEncoder Encoder { get; }
    }
}
