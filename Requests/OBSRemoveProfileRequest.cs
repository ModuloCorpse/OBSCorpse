using CorpseLib.DataNotation;

namespace OBSCorpse.Requests
{
    public class OBSRemoveProfileRequest(string profileName) : AOBSRequest("RemoveProfile", new DataObject() { { "profileName", profileName } })
    {
        private bool m_Success = false;
        public bool Success => m_Success;
        protected override async Task OnResponse(Response response) => m_Success = response.Result;
    }
}
