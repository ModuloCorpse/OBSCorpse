﻿using CorpseLib.DataNotation;

namespace OBSCorpse.Requests
{
    public class OBSSendStreamCaptionRequest(string captionText) : AOBSRequest("SendStreamCaption", new DataObject() { { "captionText", captionText } })
    {
        private bool m_Success = false;
        public bool Success => m_Success;
        protected override void OnResponse(Response response) => m_Success = response.Result;
    }
}
