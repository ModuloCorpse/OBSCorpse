namespace OBSCorpse.Requests
{
    public class OBSGetCurrentPreviewSceneRequest() : AOBSRequest("GetCurrentPreviewScene", null)
    {
        private string m_CurrentPreviewScene = string.Empty;

        public string CurrentPreviewScene => m_CurrentPreviewScene;

        protected override void OnResponse(Response response)
        {
            if (response.Result && response.Data != null &&
                response.Data.TryGet("currentPreviewSceneName", out string? currentPreviewSceneName))
                m_CurrentPreviewScene = currentPreviewSceneName!;
        }
    }
}
