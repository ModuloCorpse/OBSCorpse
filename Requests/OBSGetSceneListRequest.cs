namespace OBSCorpse.Requests
{
    public class OBSGetSceneListRequest() : AOBSRequest("GetSceneList", null)
    {
        private OBSSceneList? m_SceneList;

        public OBSSceneList SceneList => m_SceneList!;

        protected override void OnResponse(Response response)
        {
            if (response.Result && response.Data != null &&
                response.Data.TryGet("currentProgramSceneName", out string? currentProgramSceneName) &&
                response.Data.TryGet("currentPreviewSceneName", out string? currentPreviewSceneName))
                m_SceneList = new(currentProgramSceneName!, currentPreviewSceneName!, response.Data.GetList<OBSScene>("scenes"));
        }
    }
}
