using System.Collections.ObjectModel;

namespace OBSCorpse
{
    public class OBSProfileList
    {
        private readonly string m_CurrentProfile;
        private readonly List<string> m_ProfilesList;

        public string CurrentProfile => m_CurrentProfile;
        public ReadOnlyCollection<string> ProfilesList => m_ProfilesList.AsReadOnly();

        public OBSProfileList(string currentProfile, List<string> profilesList)
        {
            m_CurrentProfile = currentProfile;
            m_ProfilesList = profilesList;
        }
    }
}
