namespace AudioApi.AudioCore.EventArgs.Voice
{
    public class TrackSelectingEventArgs
    {
        public VoicePlayerBase VoicePlayerBase { get; }
        public bool DirectPlay { get; }
        public bool IsAllowed { get; set; }
        public TrackSelectingEventArgs(VoicePlayerBase audioPlayerBase, bool directPlay, bool isAllowed)
        {
            VoicePlayerBase = audioPlayerBase;
            DirectPlay = directPlay;
            IsAllowed = isAllowed;
        }
    }
}
