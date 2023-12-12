namespace OBSCorpse
{
    public class OBSScene(string name, int idx)
    {
        private readonly string m_Name = name;
        private readonly int m_Idx = idx;
        public string Name => m_Name;
        public int Idx => m_Idx;
        public override string ToString() => string.Format("[Name: \"{0}\", Idx: {1}]", m_Name, m_Idx);
    }
}
