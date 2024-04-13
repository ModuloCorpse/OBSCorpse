using CorpseLib.Json;

namespace OBSCorpse.Requests
{
    public class OBSRemoveProfileRequest(string profileName) : AOBSRequest("RemoveProfile", new JsonObject() { { "profileName", profileName } })
    {
        private bool m_Success = false;
        public bool Success => m_Success;
        protected override void OnResponse(Response response) => m_Success = response.Result;
    }
}
