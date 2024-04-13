using CorpseLib.Json;

namespace OBSCorpse.Requests
{
    public class OBSSetSceneNameRequest(string sceneName, string newSceneName) : AOBSRequest("SetSceneName", new JsonObject() { { "sceneName", sceneName }, { "newSceneName", newSceneName } })
    {
        private bool m_Success = false;
        public bool Success => m_Success;
        protected override void OnResponse(Response response) => m_Success = response.Result;
    }
}
