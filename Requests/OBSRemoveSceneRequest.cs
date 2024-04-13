using CorpseLib.Json;

namespace OBSCorpse.Requests
{
    public class OBSRemoveSceneRequest(string sceneName) : AOBSRequest("RemoveScene", new JsonObject() { { "sceneName", sceneName } })
    {
        private bool m_Success = false;
        public bool Success => m_Success;
        protected override void OnResponse(Response response) => m_Success = response.Result;
    }
}
