﻿using CorpseLib.DataNotation;

namespace OBSCorpse.Requests
{
    public class OBSGetGroupSceneItemListRequest(string sceneName) : AOBSRequest("GetGroupSceneItemList", new DataObject() { { "sceneName", sceneName } })
    {
        private readonly List<OBSSceneItem> m_SceneItems = [];
        private readonly string m_SceneName = sceneName;

        public OBSSceneItem[] SceneItems => [.. m_SceneItems];

        protected override void OnResponse(Response response)
        {
            m_SceneItems.Clear();
            if (response.Result && response.Data != null)
            {
                List<DataObject> items = response.Data.GetList<DataObject>("sceneItems");
                foreach (DataObject item in items)
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
