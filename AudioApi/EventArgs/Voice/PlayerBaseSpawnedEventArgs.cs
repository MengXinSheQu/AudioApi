namespace AudioApi.EventArgs.Voice
{
    /// <summary>
    /// VoicePlayerBase生成
    /// </summary>
    public class PlayerBaseSpawnedEventArgs(VoicePlayerBase @base)
    {
        public VoicePlayerBase VoicePlayerBase { get; } = @base;
    }
}
