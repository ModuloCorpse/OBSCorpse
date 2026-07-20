using CorpseLib.DataNotation;

namespace OBSCorpse.Requests
{
    public class OBSCreateSceneCollectionRequest(string sceneCollectionName) : AOBSRequest("CreateSceneCollection", new DataObject() { { "sceneCollectionName", sceneCollectionName } })
    {
        private bool m_Success = false;
        public bool Success => m_Success;
        protected override async Task OnResponse(Response response) => m_Success = response.Result;
    }
}
