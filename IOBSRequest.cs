using CorpseLib.DataNotation;

namespace OBSCorpse
{
    public interface IOBSRequest
    {
        public string ID { get; }
        public Task ReceivedResponse(DataObject response);
    }
}
