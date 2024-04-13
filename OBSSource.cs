namespace OBSCorpse
{
    public class OBSSource(string sceneName, string name, int id)
    {
        private readonly string m_SceneName = sceneName;
        private readonly string m_Name = name;
        private readonly int m_ID = id;

        public string Scene => m_SceneName;
        public string Name => m_Name;
        public int ID => m_ID;
    }
}
