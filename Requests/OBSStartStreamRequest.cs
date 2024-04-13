namespace OBSCorpse.Requests
{
    public class OBSStartStreamRequest() : AOBSRequest("StartStream", null)
    {
        private bool m_Success = false;
        public bool Success => m_Success;
        protected override void OnResponse(Response response) => m_Success = response.Result;
    }
}
