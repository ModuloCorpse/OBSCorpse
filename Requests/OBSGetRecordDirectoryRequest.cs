namespace OBSCorpse.Requests
{
    public class OBSGetRecordDirectoryRequest() : AOBSRequest("GetRecordDirectory", null)
    {
        private string m_RecordDirectory = string.Empty;

        public string RecordDirectory => m_RecordDirectory;

        protected override void OnResponse(Response response)
        {
            if (response.Result && response.Data != null &&
                response.Data.TryGet("recordDirectory", out string? recordDirectory))
                m_RecordDirectory = recordDirectory!;
        }
    }
}
