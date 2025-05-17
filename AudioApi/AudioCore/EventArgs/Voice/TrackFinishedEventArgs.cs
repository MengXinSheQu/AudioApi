namespace AudioApi.AudioCore.EventArgs.Voice
{
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
