namespace AudioApi.AudioCore.EventArgs.Voice
{
    public class TrackSelectedEventArgs
    {
        public VoicePlayerBase VoicePlayerBase { get; }
        public bool DirectPlay { get; }
        public int QueuePos { get; }
        public TrackSelectedEventArgs(VoicePlayerBase playerBase, bool directPlay, int queuePos)
        {
            VoicePlayerBase = playerBase;
            DirectPlay = directPlay;
            QueuePos = queuePos;
        }
    }
}
