namespace OBSCorpse.Requests
{
    public class OBSGetVersionRequest() : AOBSRequest("GetVersion", null)
    {
        private OBSVersionData? m_VersionData;

        public OBSVersionData VersionData => m_VersionData!;

        protected override void OnResponse(Response response)
        {
            if (response.Result && response.Data != null &&
                response.Data.TryGet("obsWebSocketVersion", out string? websocketVersion) &&
                response.Data.TryGet("obsVersion", out string? obsVersion))
            {
                string[] availableRequests = response.Data.GetArray<string>("available-requests");
                m_VersionData = new(websocketVersion!, obsVersion!, availableRequests!);
            }
        }
    }
}
