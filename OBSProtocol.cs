using CorpseLib;
using CorpseLib.DataNotation;
using CorpseLib.Json;
using CorpseLib.Logging;
using CorpseLib.Network.WebSocket;
using System.Collections.Concurrent;
using System.Text;
using Version = CorpseLib.Version;

namespace OBSCorpse
{
    public class OBSProtocol : AWebSocketProtocol
    {
        private static readonly Version MINIMUM_REQUIRED = new(5, 1, 0);

        public static readonly Logger OBS_LOG = new("[${d}-${M}-${y} ${h}:${m}:${s}.${ms}] ${log}");
        public static void StartLogging() => OBS_LOG.Start();
        public static void StopLogging() => OBS_LOG.Stop();

        static OBSProtocol()
        {
            DataHelper.RegisterSerializer(new OBSScene.DataSerializer());
            DataHelper.RegisterSerializer(new AOBSRequest.DataSerializer());
            DataHelper.RegisterSerializer(new OBSRequestBatch.DataSerializer());
        }

        private static OBSProtocol? CreateNewConnection(string password, URI uri, IOBSHandler? handler)
        {
            OBSProtocol obsClient = new(password, handler);
            WebSocketClient? webSocket = WebSocketClient.Connect(uri, obsClient);
            if (webSocket == null)
                return null;
            while (!obsClient.Identified && obsClient.IsConnected())
                Thread.Sleep(100);
            return (obsClient.Identified) ? obsClient : null;
        }

        public static OBSProtocol? NewConnection(URI url, string password, IOBSHandler handler) => CreateNewConnection(password, URI.Build("ws").Host(url.Host).Port(url.Port).Path(url.Path).Build(), handler);
        public static OBSProtocol? NewConnection(URI url, string password) => CreateNewConnection(password, URI.Build("ws").Host(url.Host).Port(url.Port).Path(url.Path).Build(), null);
        public static OBSProtocol? NewConnection(URI url, IOBSHandler handler) => CreateNewConnection(string.Empty, URI.Build("ws").Host(url.Host).Port(url.Port).Path(url.Path).Build(), handler);
        public static OBSProtocol? NewConnection(URI url) => CreateNewConnection(string.Empty, URI.Build("ws").Host(url.Host).Port(url.Port).Path(url.Path).Build(), null);
        public static OBSProtocol? NewConnection(string host, int port, string password, IOBSHandler handler) => CreateNewConnection(password, URI.Build("ws").Host(host).Port(port).Build(), handler);
        public static OBSProtocol? NewConnection(string host, int port, string password) => CreateNewConnection(password, URI.Build("ws").Host(host).Port(port).Build(), null);
        public static OBSProtocol? NewConnection(string host, int port, IOBSHandler handler) => CreateNewConnection(string.Empty, URI.Build("ws").Host(host).Port(port).Build(), handler);
        public static OBSProtocol? NewConnection(string host, int port) => CreateNewConnection(string.Empty, URI.Build("ws").Host(host).Port(port).Build(), null);
        public static OBSProtocol? NewConnection(string password, IOBSHandler handler) => CreateNewConnection(password, URI.Build("ws").Host("localhost").Port(4455).Build(), handler);
        public static OBSProtocol? NewConnection(string password) => CreateNewConnection(password, URI.Build("ws").Host("localhost").Port(4455).Build(), null);
        public static OBSProtocol? NewConnection(IOBSHandler handler) => CreateNewConnection(string.Empty, URI.Build("ws").Host("localhost").Port(4455).Build(), handler);
        public static OBSProtocol? NewConnection() => CreateNewConnection(string.Empty, URI.Build("ws").Host("localhost").Port(4455).Build(), null);

        private readonly IOBSHandler? m_Handler = null;
        private readonly ConcurrentDictionary<string, IOBSRequest> m_PendingRequests = [];
        private readonly string m_Password;
        private bool m_Identified = false;

        public bool Identified => m_Identified;

        private OBSProtocol(string password, IOBSHandler? handler)
        {
            m_Password = password;
            m_Handler = handler;
        }

        public override void OnOpen() { }

        public override void OnClose(int status, string message) => m_Handler?.OnDisconnect();

        public override void OnError(Exception ex) => OBS_LOG.Log(ex.ToString());

        public override void HandleMessage(string message)
        {
            try
            {
                DataObject messageJson = JsonParser.Parse(message);
                OBS_LOG.Log("Received: ${0}", JsonParser.Str(messageJson));
                if (messageJson.TryGet("op", out WebSocketOpCode? op) &&
                    messageJson.TryGet("d", out DataObject? data) && data != null)
                {
                    switch (op)
                    {
                        case WebSocketOpCode.Hello: HandleHello(data); break;
                        case WebSocketOpCode.Identified: m_Identified = true; break;
                        case WebSocketOpCode.Event: HandleEvent(data); break;
                        case WebSocketOpCode.RequestResponse: SetRequestResponse(data); break;
                        case WebSocketOpCode.RequestBatchResponse: SetRequestResponse(data); break;
                        default: OBS_LOG.Log("[${0}] ${1}", op!, data); break;
                    }
                }
            }
            catch (Exception ex) { OBS_LOG.Log(ex.ToString()); }
        }

        private void SetRequestResponse(DataObject requestResponse)
        {
            if (requestResponse.TryGet("requestId", out string? id) &&
                m_PendingRequests.TryRemove(id!, out IOBSRequest? request))
                request.ReceivedResponse(requestResponse);
        }

        private void HandleHello(DataObject data)
        {
            if (data.TryGet("rpcVersion", out int? rpc) &&
                data.TryGet("obsWebSocketVersion", out string? websocketVersion) &&
                new Version(websocketVersion!) >= MINIMUM_REQUIRED)
            {
                DataObject response = new() { { "rpcVersion", rpc! } };
                if (data.TryGet("authentication", out DataObject? authentication) &&
                    authentication!.TryGet("challenge", out string? challenge) &&
                    authentication!.TryGet("salt", out string? salt))
                {
                    string base64_secret = Convert.ToBase64String(System.Security.Cryptography.SHA256.HashData(Encoding.UTF8.GetBytes(m_Password + salt)));
                    string auth = Convert.ToBase64String(System.Security.Cryptography.SHA256.HashData(Encoding.UTF8.GetBytes(base64_secret + challenge)));
                    response.Add("authentification", auth);
                }
                DataObject identifyData = new() { { "op", WebSocketOpCode.Identify }, { "d", response } };
                OBS_LOG.Log("Sending: ${0}", JsonParser.Str(identifyData));
                Send(JsonParser.NetStr(identifyData));
            }
        }

        private void HandleEvent(DataObject data)
        {
            if (data.TryGet("eventType", out string? eventType))
            {
                DataObject eventData = data.GetOrDefault("eventData", new DataObject())!;
                switch (eventType)
                {
                    case "CurrentProgramSceneChanged": HandleSceneChange(eventData); break;
                    case "StreamStateChanged": HandleStreamStateChanged(eventData); break;
                    case "RecordStateChanged": HandleRecordStateChanged(eventData); break;
                    case "SceneItemEnableStateChanged": HandleSceneItemEnableStateChanged(eventData); break;
                }
            }
        }

        private void HandleSceneChange(DataObject data)
        {
            if (data.TryGet("sceneName", out string? sceneName))
                m_Handler?.OnSceneChanged(sceneName!);
        }

        private void HandleStreamStateChanged(DataObject data)
        {
            if (data.TryGet("outputActive", out bool? outputActive) && data.TryGet("outputState", out string? outputState))
                m_Handler?.OnStreamStatusChanged((bool)outputActive!, outputState!);
        }

        private void HandleRecordStateChanged(DataObject data)
        {
            if (data.TryGet("outputActive", out bool? outputActive) &&
                data.TryGet("outputState", out string? outputState) &&
                data.TryGet("outputPath", out string? outputPath))
                m_Handler?.OnRecordStatusChanged((bool)outputActive!, outputState!, outputPath ?? string.Empty);
        }

        private void HandleSceneItemEnableStateChanged(DataObject data)
        {
            if (data.TryGet("sceneItemEnabled", out bool? sceneItemEnabled) &&
                data.TryGet("sceneName", out string? sceneName) &&
                data.TryGet("sceneItemId", out int? sceneItemId))
                m_Handler?.OnSceneItemEnableStateChanged(sceneName!, (int)sceneItemId!, (bool)sceneItemEnabled!);
        }

        public void Send(AOBSRequest request)
        {
            if (m_PendingRequests.TryAdd(request.ID, request))
            {
                DataObject requestData = new() { { "op", WebSocketOpCode.Request }, { "d", request } };
                OBS_LOG.Log("Sending: ${0}", JsonParser.Str(requestData));
                Send(JsonParser.NetStr(requestData));
                while (!request.HasResult && IsConnected())
                    Thread.Sleep(10);
            }
        }

        public void Send(OBSRequestBatch requestBatch)
        {
            if (m_PendingRequests.TryAdd(requestBatch.ID, requestBatch))
            {
                DataObject requestBatchData = new() { { "op", WebSocketOpCode.RequestBatch }, { "d", requestBatch } };
                OBS_LOG.Log("Sending: ${0}", JsonParser.Str(requestBatchData));
                Send(JsonParser.NetStr(requestBatchData));
                while (!requestBatch.HasResult && IsConnected())
                    Thread.Sleep(10);
            }
        }

        public void Send(IEnumerable<AOBSRequest> requests)
        {
            OBSRequestBatch requestBatch = new();
            requestBatch.AddRequests(requests);
            Send(requestBatch);
        }
    }
}
