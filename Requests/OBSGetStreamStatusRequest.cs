namespace OBSCorpse.Requests
{
    public class OBSGetStreamStatusRequest() : AOBSRequest("GetStreamStatus", null)
    {
        private OBSStreamStatus? m_StreamStatus;

        public OBSStreamStatus StreamStatus => m_StreamStatus!;

        protected override void OnResponse(Response response)
        {
            if (response.Result && response.Data != null &&
                response.Data.TryGet("outputActive", out bool? outputActive) &&
                response.Data.TryGet("outputReconnecting", out bool? outputReconnecting) &&
                response.Data.TryGet("outputTimecode", out string? outputTimecode) &&
                response.Data.TryGet("outputDuration", out long? outputDuration) &&
                response.Data.TryGet("outputCongestion", out long? outputCongestion) &&
                response.Data.TryGet("outputBytes", out long? outputBytes) &&
                response.Data.TryGet("outputSkippedFrames", out long? outputSkippedFrames) &&
                response.Data.TryGet("outputTotalFrames", out long? outputTotalFrames))
                m_StreamStatus = new((bool)outputActive!, (bool)outputReconnecting!, outputTimecode!, (long)outputDuration!, (long)outputCongestion!, (long)outputBytes!, (long)outputSkippedFrames!, (long)outputTotalFrames!);
        }
    }
}
