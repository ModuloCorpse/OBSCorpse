namespace OBSCorpse.Requests
{
    public class OBSGetGroupListRequest() : AOBSRequest("GetGroupList", null)
    {
        private List<string> m_GroupList = [];

        public List<string> GroupList => m_GroupList!;

        protected override void OnResponse(Response response)
        {
            if (response.Result && response.Data != null)
                m_GroupList = response.Data.GetList<string>("groups");
        }
    }
}
