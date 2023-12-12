namespace OBSCorpse
{
    public class OBSStreamStatus(bool active, bool reconnecting, string timecode, long duration, long congestion, long bytes, long skippedFrames, long totalFrames)
    {
        private readonly string m_Timecode = timecode;
        private readonly long m_Duration = duration;
        private readonly long m_Congestion = congestion;
        private readonly long m_Bytes = bytes;
        private readonly long m_SkippedFrames = skippedFrames;
        private readonly long m_TotalFrames = totalFrames;
        private readonly bool m_Active = active;
        private readonly bool m_Reconnecting = reconnecting;
        public string Timecode => m_Timecode;
        public long Duration => m_Duration;
        public long Congestion => m_Congestion;
        public long Bytes => m_Bytes;
        public long SkippedFrames => m_SkippedFrames;
        public long TotalFrames => m_TotalFrames;
        public bool Active => m_Active;
        public bool Reconnecting => m_Reconnecting;
    }
}
