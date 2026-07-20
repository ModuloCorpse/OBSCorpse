namespace OBSCorpse.Requests
{
    public class OBSToggleStreamRequest() : AOBSRequest("ToggleStream", null)
    {
        private bool m_OutputActive = false;
        public bool OutputActive => m_OutputActive;
        protected override async Task OnResponse(Response response)
        {
            if (response.Result && response.Data != null &&
                response.Data.TryGet("outputActive", out bool? outputActive))
                m_OutputActive = (bool)outputActive!;
        }
    }
}
