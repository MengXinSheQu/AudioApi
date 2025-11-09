namespace AudioApi.EventArgs.Voice
{    /// <summary>
     /// 音频选择完成
     /// </summary>
    public class TrackSelectedEventArgs(VoicePlayerBase playerBase, bool directPlay, int queuePos)
    {
        public VoicePlayerBase VoicePlayerBase { get; } = playerBase;
        public bool DirectPlay { get; } = directPlay;
        public int QueuePos { get; } = queuePos;
    }
}
