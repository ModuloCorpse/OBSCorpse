using System.Collections.ObjectModel;

namespace OBSCorpse
{
    public class OBSSceneList
    {
        private readonly string m_CurrentProgramScene;
        private readonly string m_CurrentPreviewScene;
        private readonly List<OBSScene> m_ScenesList;

        public string CurrentProgramScene => m_CurrentProgramScene;
        public string CurrentPreviewScene => m_CurrentPreviewScene;
        public ReadOnlyCollection<OBSScene> ScenesList => m_ScenesList.AsReadOnly();

        public OBSSceneList(string currentProgramScene, string currentPreviewScene, List<OBSScene> scenesList)
        {
            m_CurrentProgramScene = currentProgramScene;
            m_CurrentPreviewScene = currentPreviewScene;
            m_ScenesList = scenesList;
        }
    }
}
