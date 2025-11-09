using System.Collections.Generic;
using System.IO;
using VoiceChat;

namespace AudioApi.Interfaces
{
    /// <summary>
    /// PlayerBase接口，提供了大量的功能
    /// </summary>
    public interface IPlayerBase : IBuffer , IPlayerTarget, IEncoder, IPlaybackBuffer,ILogControl
    {       
        /// <summary>  
        /// 码率头，不建议自行修改    
        /// </summary>
        int HeadSamples { get; set; }
        /// <summary>
        /// 已经转译的流
        /// </summary>
        byte[] EncodedBuffer { get; }
        /// <summary>
        /// 包含音频的数据流
        /// </summary>
        MemoryStream CurrentPlayStream { get; set; }
        /// <summary>
        /// 正在播放的音乐路径
        /// </summary>
        string CurrentPlay { get; set; }
        /// <summary>
        /// 音频是否循环
        /// </summary>
        bool Loop { get; set; }
        /// <summary>
        /// 是否随机播放
        /// </summary>
        bool Shuffle { get; set; }
        /// <summary>
        /// 是否应在当前音频结束后继续切换播放
        /// </summary>
        bool Continue { get; set; }
        /// <summary>
        /// 是否播放音频
        /// </summary>
        bool ShouldPlay { get; set; }
        /// <summary>
        /// 所有播放的音乐
        /// </summary>
        List<string> AudioToPlay { get; set; }
        /// <summary>
        /// 音频大小 百分比
        /// </summary>
        float Volume { get; set; }
        /// <summary>
        /// 播放的类型，推荐Intercom
        /// </summary>
        VoiceChatChannel BroadcastChannel { get; set; }
        /// <summary>
        /// 音频是否播放完成
        /// </summary>
        bool IsFinished { get; set; }

        /// <summary>
        /// 播放后清理音频
        /// </summary>
        bool ClearOnFinish { get; set; }
        /// <summary>
        /// 播放音频
        /// </summary>
        /// <param name="queuePos">音频列表位置</param>
        void Play(int queuePos);
        /// <summary>
        /// 停止播放音频
        /// </summary>
        /// <param name="Clear">如果为true 下次播放从头播放</param>
        void Stoptrack(bool clear);
        /// <summary>
        /// 向列表加入音频
        /// </summary>
        /// <param name="audio">路径</param>
        /// <param name="pos">音频播放位置，使用-1转到队列末尾</param>
        void Enqueue(string audio, int pos);
        /// <summary>
        /// 音频处理核心
        /// </summary>
        /// <param name="position">播放坐标</param>
        /// <returns></returns>
        IEnumerator<float> Playback(int position);
    }
}
