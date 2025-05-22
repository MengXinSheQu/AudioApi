namespace AudioApi.EventArgs.Voice
{    /// <summary>
     /// 音频加载完成
     /// </summary>
    public class TrackLoadedEventArgs(VoicePlayerBase @base, string track, int queuePos, bool directPlay)
    {
        public VoicePlayerBase VoicePlayerBase { get; } = @base;
        public int QueuePos { get; } = queuePos;
        public string Track { get; } = track;
        public bool DirectPlay { get; } = directPlay;
    }
}
