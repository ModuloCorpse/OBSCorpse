using CorpseLib.Json;

namespace OBSCorpse.Requests
{
    public class OBSCreateSceneRequest(string sceneName) : AOBSRequest("CreateScene", new JsonObject() { { "sceneName", sceneName } })
    {
        private bool m_Success = false;
        public bool Success => m_Success;
        protected override void OnResponse(Response response) => m_Success = response.Result;
    }
}
