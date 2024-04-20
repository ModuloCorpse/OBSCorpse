using CorpseLib;
using CorpseLib.DataNotation;

namespace OBSCorpse
{
    public abstract class AOBSRequest(string type, DataObject? data = null) : IOBSRequest
    {
        public class Response
        {
            private readonly DataObject? m_Data;
            private readonly string m_Comment = string.Empty;
            private readonly RequestStatus m_Code;
            private readonly bool m_Result;

            public DataObject? Data => m_Data;
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

            public Response(DataObject status, DataObject? data)
            {
                m_Data = data;
                m_Result = status.GetOrDefault("result", false);
                m_Code = status.GetOrDefault("code", RequestStatus.Unknown);
                m_Comment = status.GetOrDefault("comment", string.Empty)!;
            }
        }

        public class DataSerializer : ADataSerializer<AOBSRequest>
        {
            protected override OperationResult<AOBSRequest> Deserialize(DataObject reader) => new("No deserialization for AOBSRequest", string.Empty);

            protected override void Serialize(AOBSRequest obj, DataObject writer)
            {
                writer["requestId"] = obj.m_ID;
                writer["requestType"] = obj.m_Type;
                if (obj.m_Data != null)
                    writer["requestData"] = obj.m_Data;
            }
        }

        private Response? m_Response = null;
        private readonly DataObject? m_Data = data;
        private readonly Guid m_ID = Guid.NewGuid();
        private readonly string m_Type = type;
        private volatile bool m_HasResult = false;

        public string ID => m_ID.ToString();
        public bool HasResult => m_HasResult;

        public void ReceivedResponse(DataObject response)
        {
            if (response.TryGet("requestType", out string? type) && m_Type == type &&
                response.TryGet("requestStatus", out DataObject? status))
            {
                m_Response = new(status!, response.GetOrDefault<DataObject?>("responseData", null));
                OnResponse(m_Response);
                m_HasResult = true;
            }
        }

        public Response GetResponse() => m_Response ?? new();

        public void SetResponse() => m_Response = new();

        protected abstract void OnResponse(Response response);
    }

    public class OBSRequest(string type, DataObject? data = null) : AOBSRequest(type, data)
    {
        protected override void OnResponse(Response response) { }
    }
}
