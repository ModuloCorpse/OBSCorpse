using CorpseLib.Json;

namespace OBSCorpse
{
    public interface IOBSRequest
    {
        public string ID { get; }
        public void ReceivedResponse(JsonObject response);
    }
}
