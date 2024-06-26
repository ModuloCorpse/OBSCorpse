﻿namespace OBSCorpse
{
    public class OBSProfileList(string currentProfile, List<string> profilesList)
    {
        private readonly string m_CurrentProfile = currentProfile;
        private readonly List<string> m_ProfilesList = profilesList;
        public string CurrentProfile => m_CurrentProfile;
        public string[] ProfilesList => [..m_ProfilesList];
    }
}
