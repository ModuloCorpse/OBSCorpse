using System.Collections.ObjectModel;

namespace OBSCorpse
{
    public class OBSSceneCollectionList
    {
        private readonly string m_CurrentSceneCollection;
        private readonly List<string> m_SceneCollectionList;

        public string CurrentSceneCollection => m_CurrentSceneCollection;
        public ReadOnlyCollection<string> SceneCollectionList => m_SceneCollectionList.AsReadOnly();

        public OBSSceneCollectionList(string currentSceneCollection, List<string> sceneCollectionList)
        {
            m_CurrentSceneCollection = currentSceneCollection;
            m_SceneCollectionList = sceneCollectionList;
        }
    }
}
