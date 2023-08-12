namespace OBSCorpse
{
    public class OBSScene
    {
        private readonly string m_Name;
        private readonly int m_Idx;

        public string Name => m_Name;
        public int Idx => m_Idx;

        public OBSScene(string name, int idx)
        {
            m_Name = name;
            m_Idx = idx;
        }

        public override string ToString() => string.Format("[Name: \"{0}\", Idx: {1}]", m_Name, m_Idx);
    }
}
