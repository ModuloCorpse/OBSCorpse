namespace OBSCorpse
{
    public class OBSSceneItem(OBSSource source, bool isGroup)
    {
        private readonly OBSSource m_Source = source;
        private readonly bool m_IsGroup = isGroup;
        public OBSSource Source => m_Source;
        public bool IsGroup => m_IsGroup;
    }
}
