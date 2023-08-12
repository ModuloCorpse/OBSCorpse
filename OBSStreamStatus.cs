namespace OBSCorpse
{
    public class OBSRecordStatus
    {
        private readonly string m_Timecode;
        private readonly long m_Duration;
        private readonly long m_Bytes;
        private readonly bool m_Active;
        private readonly bool m_Paused;

        public string Timecode => m_Timecode;
        public long Duration => m_Duration;
        public long Bytes => m_Bytes;
        public bool Active => m_Active;
        public bool Paused => m_Paused;

        public OBSRecordStatus(bool active, bool paused, string timecode, long duration, long bytes)
        {
            m_Timecode = timecode;
            m_Duration = duration;
            m_Bytes = bytes;
            m_Active = active;
            m_Paused = paused;
        }
    }
}
