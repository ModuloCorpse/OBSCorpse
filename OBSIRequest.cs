using CorpseLib.Json;

namespace OBSCorpse
{
    public abstract class OBSIRequest
    {
        private readonly string m_ID = Guid.NewGuid().ToString();
        private volatile bool m_HaveResponse = false;

        public string ID => m_ID;
        public bool HaveResponse => m_HaveResponse;

        protected void MarkAsResponded() => m_HaveResponse = true;

        public JObject ToJson()
        {
            JObject request = new() { { "requestId", ID } };
            FillJson(ref request);
            return request;
        }

        public void ReceivedResponse(JObject response)
        {
            if (response.TryGet("requestId", out string? id) && ID == id)
                SetResponse(response);
        }

        protected abstract void SetResponse(JObject response);
        protected abstract void FillJson(ref JObject json);
    }
}
