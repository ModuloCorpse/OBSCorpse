namespace OBSCorpse.Requests
{
    public class OBSGetCurrentProgramSceneRequest() : AOBSRequest("GetCurrentProgramScene", null)
    {
        private string m_CurrentProgramScene = string.Empty;

        public string CurrentProgramScene => m_CurrentProgramScene;

        protected override void OnResponse(Response response)
        {
            if (response.Result && response.Data != null &&
                response.Data.TryGet("currentProgramSceneName", out string? currentProgramSceneName))
                m_CurrentProgramScene = currentProgramSceneName!;
        }
    }
}
