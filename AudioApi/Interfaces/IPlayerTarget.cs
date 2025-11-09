using System.Collections.Generic;
using System;

namespace AudioApi.Interfaces
{
    public interface IPlayerTarget
    {
        /// <summary>
        /// 所有接受播放的玩家Hub
        /// </summary>
        List<ReferenceHub> BroadcastTo { get; }
        /// <summary>
        /// 一个Func，符合该Func的玩家作为播放对象，为null则全部允许
        /// </summary>
        Func<ReferenceHub, bool> BroadcastFunc { get; }
    }
}
