namespace AudioApi.EventArgs.Voice
{
    /// <summary>
    /// 音频播放完毕
    /// </summary>
    public class TrackFinishedEventArgs
    {
        public VoicePlayerBase VoicePlayerBase { get; }
        public string Track { get; }
        public bool DirectPlay { get; }
        public TrackFinishedEventArgs(VoicePlayerBase playerBase, string track, bool directPlay)        {
            VoicePlayerBase = playerBase;
            Track = track;
            DirectPlay = directPlay;
        }
    }
}
