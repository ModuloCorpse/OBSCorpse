namespace OBSCorpse.Requests
{
    public class OBSCreateRecordChapterRequest() : AOBSRequest("CreateRecordChapter", null)
    {
        private bool m_Success = false;
        public bool Success => m_Success;
        protected override async Task OnResponse(Response response) => m_Success = response.Result;
    }
}
