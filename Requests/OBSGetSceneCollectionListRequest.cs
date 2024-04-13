namespace OBSCorpse.Requests
{
    public class OBSGetSceneCollectionListRequest() : AOBSRequest("GetSceneCollectionList", null)
    {
        private OBSSceneCollectionList? m_SceneCollectionList;

        public OBSSceneCollectionList SceneCollectionList => m_SceneCollectionList!;

        protected override void OnResponse(Response response)
        {
            if (response.Result && response.Data != null &&
                response.Data.TryGet("currentSceneCollectionName", out string? currentSceneCollectionName))
                m_SceneCollectionList = new(currentSceneCollectionName!, response.Data.GetList<string>("sceneCollections"));
        }
    }
}
