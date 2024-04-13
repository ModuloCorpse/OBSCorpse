namespace OBSCorpse.Requests
{
    public class OBSGetRecordStatusRequest() : AOBSRequest("GetRecordStatus", null)
    {
        private OBSRecordStatus? m_RecordStatus;

        public OBSRecordStatus RecordStatus => m_RecordStatus!;

        protected override void OnResponse(Response response)
        {
            if (response.Result && response.Data != null &&
                response.Data.TryGet("outputActive", out bool? outputActive) &&
                response.Data.TryGet("outputPaused", out bool? outputPaused) &&
                response.Data.TryGet("outputTimecode", out string? outputTimecode) &&
                response.Data.TryGet("outputDuration", out long? outputDuration) &&
                response.Data.TryGet("outputBytes", out long? outputBytes))
                m_RecordStatus = new((bool)outputActive!, (bool)outputPaused!, outputTimecode!, (long)outputDuration!, (long)outputBytes!);
        }
    }
}
