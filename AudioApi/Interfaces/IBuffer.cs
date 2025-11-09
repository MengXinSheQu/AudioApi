using NVorbis;
using System.Collections.Generic;

namespace AudioApi.Interfaces
{
    public interface IBuffer
    {
        /// <summary>
        /// 读取的Stream流
        /// </summary>
        Queue<float> StreamBuffer { get; }
        /// <summary>
        /// NVorbis提供的Reader
        /// </summary>
        public VorbisReader VorbisReader { get; set; }
        /// <summary>
        /// 发送的字节流
        /// </summary>
        float[] SendBuffer { get; set; }
        /// <summary>
        /// 读取的字节流
        /// </summary>
        float[] ReadBuffer { get; set; }
    }
}
