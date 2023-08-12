namespace OBSCorpse
{
    public class OBSStreamStatus
    {
        private readonly string m_Timecode;
        private readonly long m_Duration;
        private readonly long m_Congestion;
        private readonly long m_Bytes;
        private readonly long m_SkippedFrames;
        private readonly long m_TotalFrames;
        private readonly bool m_Active;
        private readonly bool m_Reconnecting;

        public string Timecode => m_Timecode;
        public long Duration => m_Duration;
        public long Congestion => m_Congestion;
        public long Bytes => m_Bytes;
        public long SkippedFrames => m_SkippedFrames;
        public long TotalFrames => m_TotalFrames;
        public bool Active => m_Active;
        public bool Reconnecting => m_Reconnecting;

        public OBSStreamStatus(bool active, bool reconnecting, string timecode, long duration, long congestion, long bytes, long skippedFrames, long totalFrames)
        {
            m_Timecode = timecode;
            m_Duration = duration;
            m_Congestion = congestion;
            m_Bytes = bytes;
            m_SkippedFrames = skippedFrames;
            m_TotalFrames = totalFrames;
            m_Active = active;
            m_Reconnecting = reconnecting;
        }
    }
}
