namespace AudioApi.Enums
{
    /// <summary>
    /// 文件读取方式
    /// </summary>
    public enum FileReaderType
    {
        /// <summary>
        /// 没有.ogg后缀名 文件需要完整拼写
        /// </summary>
        Default = 0,
        /// <summary>
        /// 拥有.ogg后缀名 选择此项文件可不添加.ogg
        /// </summary>
        HasExtension
    }
}
