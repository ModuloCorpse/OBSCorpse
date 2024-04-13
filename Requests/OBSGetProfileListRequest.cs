namespace OBSCorpse.Requests
{
    public class OBSGetProfileListRequest() : AOBSRequest("GetProfileList", null)
    {
        private string[] m_ProfilesList = [];
        private string m_CurrentProfile = string.Empty;

        public string CurrentProfile => m_CurrentProfile;
        public string[] ProfilesList => m_ProfilesList;

        protected override void OnResponse(Response response)
        {
            if (response.Result && response.Data != null &&
                response.Data.TryGet("currentProfileName", out string? currentProfileName))
            {
                m_ProfilesList = [..response.Data.GetList<string>("profiles")];
                m_CurrentProfile = currentProfileName!;
            }
        }
    }
}
