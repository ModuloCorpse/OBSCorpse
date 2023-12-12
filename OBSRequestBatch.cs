using CorpseLib.Json;

namespace OBSCorpse
{
    public class OBSRequestBatch : OBSIRequest
    {
        private readonly List<OBSRequest> m_Requests = [];
        private readonly RequestBatchExecutionType m_ExecutionType = RequestBatchExecutionType.SerialRealtime;
        private readonly bool m_HaltOnFailure = false;

        public void AddRequest(OBSRequest request) => m_Requests.Add(request);

        protected override void SetResponse(JObject response)
        {
            if (response.TryGet("results", out List<JObject>? tmp))
            {
                List<JObject> results = tmp!;
                int i = 0;
                foreach (OBSRequest request in m_Requests)
                {
                    if (i == results.Count)
                        request.SetResponse();
                    else
                    {
                        request.ReceivedResponse(results[i]);
                        ++i;
                    }
                }
                MarkAsResponded();
            }
        }

        protected override void FillJson(ref JObject request)
        {
            JArray requests = [];
            foreach (OBSRequest requestToAdd in m_Requests)
                requests.Add(requestToAdd.ToJson());
            request.Add("haltOnFailure", m_HaltOnFailure);
            request.Add("executionType", m_ExecutionType);
            request.Add("requests", requests);
        }
    }
}
