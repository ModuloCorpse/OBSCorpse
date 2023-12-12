using System.Collections.ObjectModel;

namespace OBSCorpse
{
    public class OBSSceneCollectionList(string currentSceneCollection, List<string> sceneCollectionList)
    {
        private readonly string m_CurrentSceneCollection = currentSceneCollection;
        private readonly List<string> m_SceneCollectionList = sceneCollectionList;
        public string CurrentSceneCollection => m_CurrentSceneCollection;
        public ReadOnlyCollection<string> SceneCollectionList => m_SceneCollectionList.AsReadOnly();
    }
}
