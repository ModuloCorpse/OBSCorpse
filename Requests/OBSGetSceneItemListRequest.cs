using CorpseLib.Json;

namespace OBSCorpse.Requests
{
    public class OBSGetSceneItemListRequest(string sceneName) : AOBSRequest("GetSceneItemList", new JsonObject() { { "sceneName", sceneName } })
    {
        private readonly List<OBSSceneItem> m_SceneItems = [];
        private readonly string m_SceneName = sceneName;

        public OBSSceneItem[] SceneItems => [.. m_SceneItems];

        protected override void OnResponse(Response response)
        {
            m_SceneItems.Clear();
            if (response.Result && response.Data != null)
            {
                List<JsonObject> items = response.Data.GetList<JsonObject>("sceneItems");
                foreach (JsonObject item in items)
                {
                    if (item.TryGet("sourceName", out string? sourceName) &&
                        item.TryGet("sceneItemId", out int? sceneItemId) &&
                        item.TryGet("isGroup", out bool? isGroup))
                        m_SceneItems.Add(new(new(m_SceneName, sourceName!, (int)sceneItemId!), isGroup == true));
                }
            }
        }
    }
}
