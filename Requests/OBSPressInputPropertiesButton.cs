using CorpseLib.DataNotation;

namespace OBSCorpse.Requests
{
    public class OBSPressInputPropertiesButton(string inputName, OBSPressInputPropertiesButton.Properties properties) : AOBSRequest("PressInputPropertiesButton", new DataObject() { { "inputName", inputName }, { "propertyName", GetPropertiesName(properties) } })
    {
        public enum Properties
        {
            RefreshNoCache = 0
        }

        private bool m_Success = false;
        public bool Success => m_Success;

        private static string GetPropertiesName(Properties properties) => properties switch
        {
            Properties.RefreshNoCache => "refreshnocache",
            _ => throw new ArgumentOutOfRangeException(nameof(properties), properties, null)
        };

        protected override void OnResponse(Response response) => m_Success = response.Result;
    }
}
