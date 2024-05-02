namespace OBSCorpse
{
    public interface IOBSHandler
    {
        public void OnDisconnect();
        public void OnSceneChanged(string newScene);
        public void OnSceneItemEnableStateChanged(string sceneName, int sceneItemID, bool enabled);
        public void OnStreamStatusChanged(bool newStatus, string outputState);
        public void OnRecordStatusChanged(bool newStatus, string outputState, string outputPath);
    }
}
