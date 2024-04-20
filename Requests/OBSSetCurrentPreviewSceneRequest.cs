using CorpseLib.DataNotation;

namespace OBSCorpse.Requests
{
    public class OBSSetCurrentPreviewSceneRequest(string sceneName) : AOBSRequest("SetCurrentPreviewScene", new DataObject() { { "sceneName", sceneName } })
    {
        private bool m_Success = false;
        public bool Success => m_Success;
        protected override void OnResponse(Response response) => m_Success = response.Result;
    }
}
