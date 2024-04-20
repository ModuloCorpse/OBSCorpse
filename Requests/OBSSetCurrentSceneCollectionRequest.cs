using CorpseLib.DataNotation;

namespace OBSCorpse.Requests
{
    public class OBSSetCurrentSceneCollectionRequest(string sceneCollectionName) : AOBSRequest("SetCurrentSceneCollection", new DataObject() { { "sceneCollectionName", sceneCollectionName } })
    {
        private bool m_Success = false;
        public bool Success => m_Success;
        protected override void OnResponse(Response response) => m_Success = response.Result;
    }
}
