using CorpseLib;
using CorpseLib.DataNotation;

namespace OBSCorpse
{
    public class OBSRequestBatch : IOBSRequest
    {
        public class DataSerializer : ADataSerializer<OBSRequestBatch>
        {
            protected override OperationResult<OBSRequestBatch> Deserialize(DataObject reader) => new("No deserialization for OBSRequestBatch", string.Empty);

            protected override void Serialize(OBSRequestBatch obj, DataObject writer)
            {
                writer["requestId"] = obj.m_ID;
                writer["haltOnFailure"] = obj.m_HaltOnFailure;
                writer["executionType"] = obj.m_ExecutionType;
                writer["requests"] = obj.m_Requests;
            }
        }

        private readonly List<AOBSRequest> m_Requests = [];
        private readonly Guid m_ID = Guid.NewGuid();
        private readonly RequestBatchExecutionType m_ExecutionType = RequestBatchExecutionType.SerialRealtime;
        private readonly bool m_HaltOnFailure = false;
        private volatile bool m_HasResult = false;

        public string ID => m_ID.ToString();
        public bool HasResult => m_HasResult;

        public void ReceivedResponse(DataObject response)
        {
            List<DataObject> results = response.GetList<DataObject>("results");
            int i = 0;
            foreach (AOBSRequest request in m_Requests)
            {
                if (i == results.Count)
                    request.SetResponse();
                else
                {
                    request.ReceivedResponse(results[i]);
                    ++i;
                }
            }
            m_HasResult = true;
        }

        public void AddRequest(AOBSRequest request) => m_Requests.Add(request);
        public void AddRequests(IEnumerable<AOBSRequest> requests) => m_Requests.AddRange(requests);
    }
}
