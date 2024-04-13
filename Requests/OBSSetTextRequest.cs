using CorpseLib.Json;

namespace OBSCorpse.Requests
{
    public class OBSSetTextRequest(OBSSource source, string text) : AOBSRequest("SetInputSettings", new JsonObject() { { "inputName", source.Name }, { "inputSettings", new JsonObject() { { "text", text } } } })
    {
        private bool m_Success = false;
        public bool Success => m_Success;
        protected override void OnResponse(Response response) => m_Success = response.Result;
    }
}
