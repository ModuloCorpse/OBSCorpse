using CorpseLib.Json;

namespace OBSCorpse.Requests
{
    public class OBSSetSceneItemEnabledRequest(OBSSource source, bool enable) : AOBSRequest("SetSceneItemEnabled", new JsonObject() { { "sceneName", source.Scene }, { "sceneItemId", source.ID }, { "sceneItemEnabled", enable } })
    {
        private bool m_Success = false;
        public bool Success => m_Success;
        protected override void OnResponse(Response response) => m_Success = response.Result;
    }
}
