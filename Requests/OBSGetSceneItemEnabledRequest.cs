using CorpseLib.Json;

namespace OBSCorpse.Requests
{
    public class OBSGetSceneItemEnabledRequest(OBSSource source) : AOBSRequest("GetSceneItemEnabled", new JsonObject() { { "sceneName", source.Scene }, { "sceneItemId", source.ID } })
    {
        private bool m_SceneItemEnabled = false;

        public bool SceneItemEnabled => m_SceneItemEnabled;

        protected override void OnResponse(Response response)
        {
            if (response.Result && response.Data != null &&
                response.Data.TryGet("sceneItemEnabled", out bool? sceneItemEnabled))
                m_SceneItemEnabled = (bool)sceneItemEnabled!;
        }
    }
}
