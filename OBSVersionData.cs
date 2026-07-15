namespace OBSCorpse
{
    public class OBSVersionData(string websocketVersion, string obsVersion, string[] availableRequests)
    {
        private readonly string[] m_AvailableRequests = availableRequests;
        private readonly string m_WebsocketVersion = websocketVersion;
        private readonly string m_OBSVersion = obsVersion;

        public string[] AvailableRequests => m_AvailableRequests;
        public string WebsocketVersion => m_WebsocketVersion;
        public string OBSVersion => m_OBSVersion;
    }
}
