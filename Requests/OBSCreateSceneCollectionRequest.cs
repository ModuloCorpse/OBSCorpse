using CorpseLib.Json;

namespace OBSCorpse.Requests
{
    public class OBSCreateSceneCollectionRequest(string sceneCollectionName) : AOBSRequest("CreateSceneCollection", new JsonObject() { { "sceneCollectionName", sceneCollectionName } })
    {
        private bool m_Success = false;
        public bool Success => m_Success;
        protected override void OnResponse(Response response) => m_Success = response.Result;
    }
}
