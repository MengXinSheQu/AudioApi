namespace AudioApi.AudioCore.EventArgs.Voice
{
    public class TrackLoadedEventArgs
    {
        public VoicePlayerBase VoicePlayerBase { get; }
        public int QueuePos { get; }
        public string Track { get; }
        public bool DirectPlay { get; }
        public TrackLoadedEventArgs(VoicePlayerBase @base, string track, int queuePos, bool directPlay)
        {
            VoicePlayerBase = @base;
            Track = track;
            QueuePos = queuePos;
            DirectPlay = directPlay;
        }
    }
}
