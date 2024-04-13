namespace OBSCorpse.Requests
{
    public class OBSStopRecordRequest() : AOBSRequest("StopRecord", null)
    {
        private string m_OutputPath = string.Empty;

        public string OutputPath => m_OutputPath;

        protected override void OnResponse(Response response)
        {
            if (response.Result && response.Data != null &&
                response.Data.TryGet("outputPath", out string? outputPath))
                m_OutputPath = outputPath!;
        }
    }
}
