using CorpseLib.Json;

namespace OBSCorpse.Requests
{
    public class OBSGetSceneSourceRequest : AOBSRequest
    {
        private OBSSource m_Source = new(string.Empty, string.Empty, -1);
        private readonly string m_SceneName;
        private string m_SourceName = string.Empty;
        private int m_ID = -1;

        public OBSSource Source => m_Source;

        public OBSGetSceneSourceRequest(string sceneName, string sourceName) : base("GetSceneItemId", new JsonObject() { { "sceneName", sceneName }, { "sourceName", sourceName } })
        {
            m_SceneName = sceneName;
            m_SourceName = sourceName;
        }

        public OBSGetSceneSourceRequest(string sceneName, int sourceID) : base("GetSceneItemSource", new JsonObject() { { "sceneName", sceneName }, { "sceneItemId", sourceID } })
        {
            m_SceneName = sceneName;
            m_ID = sourceID;
        }

        protected override void OnResponse(Response response)
        {
            if (response.Result && response.Data != null)
            {
                if (response.Data.TryGet("sceneItemId", out int? sceneItemId))
                    m_ID = (int)sceneItemId!;
                else if (response.Data.TryGet("sourceName", out string? sourceName))
                    m_SourceName = sourceName!;
                m_Source = new(m_SceneName, m_SourceName, m_ID);
            }
        }
    }
}
