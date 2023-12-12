using System.Collections.ObjectModel;

namespace OBSCorpse
{
    public class OBSSceneList(string currentProgramScene, string currentPreviewScene, List<OBSScene> scenesList)
    {
        private readonly string m_CurrentProgramScene = currentProgramScene;
        private readonly string m_CurrentPreviewScene = currentPreviewScene;
        private readonly List<OBSScene> m_ScenesList = scenesList;
        public string CurrentProgramScene => m_CurrentProgramScene;
        public string CurrentPreviewScene => m_CurrentPreviewScene;
        public ReadOnlyCollection<OBSScene> ScenesList => m_ScenesList.AsReadOnly();
    }
}
