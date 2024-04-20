using CorpseLib.DataNotation;

namespace OBSCorpse
{
    public interface IOBSRequest
    {
        public string ID { get; }
        public void ReceivedResponse(DataObject response);
    }
}
