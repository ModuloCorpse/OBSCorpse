using CorpseLib.Json;

namespace OBSCorpse
{
    public class OBSRequest : OBSIRequest
    {
        public class Response
        {
            private readonly JObject? m_Data;
            private readonly string m_Comment = string.Empty;
            private readonly RequestStatus m_Code;
            private readonly bool m_Result;

            public JObject? Data => m_Data;
            public string Comment => m_Comment;
            public RequestStatus Code => m_Code;
            public bool Result => m_Result;

            public Response()
            {
                m_Data = null;
                m_Result = false;
                m_Code = RequestStatus.Unknown;
                m_Comment = string.Empty;
            }

            public Response(JObject status, JObject? data)
            {
                m_Data = data;
                m_Result = status.GetOrDefault("result", false);
                m_Code = status.GetOrDefault("code", RequestStatus.Unknown);
                m_Comment = status.GetOrDefault("comment", string.Empty)!;
            }
        }

        private Response? m_Response = null;
        private readonly JObject? m_Data;
        private readonly string m_Type;

        public Response GetResponse() => m_Response ?? new();

        public OBSRequest(string type, JObject? data = null)
        {
            m_Data = data;
            m_Type = type;
        }

        public void SetResponse()
        {
            m_Response = new();
            MarkAsResponded();
        }

        protected override void SetResponse(JObject response)
        {
            if (response.TryGet("requestType", out string? type) && m_Type == type &&
                response.TryGet("requestStatus", out JObject? status))
            {
                m_Response = new(status!, response.GetOrDefault<JObject?>("responseData", null));
                MarkAsResponded();
            }
        }

        protected override void FillJson(ref JObject request)
        {
            request.Add("requestType", m_Type);
            if (m_Data != null)
                request.Add("requestData", m_Data);
        }
    }
}
