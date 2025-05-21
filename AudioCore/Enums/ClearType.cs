namespace AudioApi.AudioCore.Enums
{
    /// <summary>
    /// 假人清除方式
    /// </summary>
    public enum ClearType
    {
        /// <summary>
        /// 仅从NetworkServer删除游戏对象
        /// </summary>
        GameObject = 0,
        /// <summary>
        /// 仅从NetworkServer删除连接 这可能会导致很多BUG!
        /// </summary>
        Connection,
    }
}
