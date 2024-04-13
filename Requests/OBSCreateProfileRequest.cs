using CorpseLib.Json;

namespace OBSCorpse.Requests
{
    public class OBSCreateProfileRequest(string profileName) : AOBSRequest("CreateProfile", new JsonObject() { { "profileName", profileName } })
    {
        private bool m_Success = false;
        public bool Success => m_Success;
        protected override void OnResponse(Response response) => m_Success = response.Result;
    }
}
