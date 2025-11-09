namespace AudioApi.Interfaces
{
    public interface ILogControl
    {
        /// <summary>
        /// Debug日志
        /// </summary>
        public bool LogDebug { get; set; }
        /// <summary>
        /// 消息日志
        /// </summary>
        public bool LogInfo { get; set; }
    }
}
