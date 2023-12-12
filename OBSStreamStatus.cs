namespace OBSCorpse
{
    public class OBSRecordStatus(bool active, bool paused, string timecode, long duration, long bytes)
    {
        private readonly string m_Timecode = timecode;
        private readonly long m_Duration = duration;
        private readonly long m_Bytes = bytes;
        private readonly bool m_Active = active;
        private readonly bool m_Paused = paused;
        public string Timecode => m_Timecode;
        public long Duration => m_Duration;
        public long Bytes => m_Bytes;
        public bool Active => m_Active;
        public bool Paused => m_Paused;
    }
}
