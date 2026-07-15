using CorpseLib.DataNotation;

namespace OBSCorpse.Requests
{
    public abstract class AOBSCallVendorRequestRequest : AOBSRequest
    {
        protected AOBSCallVendorRequestRequest(string vendor, string request, DataObject data) : base("CallVendorRequest", new () { { "vendorName", vendor }, { "requestType", request }, { "requestData", data } }) { }
        protected AOBSCallVendorRequestRequest(string vendor, string request) : base("CallVendorRequest", new() { { "vendorName", vendor }, { "requestType", request } }) { }
    }
}
