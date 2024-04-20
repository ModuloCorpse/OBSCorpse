using CorpseLib.DataNotation;
using CorpseLib.Json;
using CorpseLib.Logging;
using CorpseLib.Network;
using CorpseLib.Web;
using System.Collections.Concurrent;
using System.Text;
using Version = CorpseLib.Version;

namespace OBSCorpse
{
    public class OBSProtocol : WebSocketProtocol
    {
        private static readonly Version MINIMUM_REQUIRED = new(5, 1, 0);

        public static readonly Logger OBS_LOG = new("[${d}-${M}-${y} ${h}:${m}:${s}.${ms}] ${log}") { new LogInFile("./log/${y}${M}${d}${h}-OBS.log") };
        public static void StartLogging() => OBS_LOG.Start();
        public static void StopLogging() => OBS_LOG.Stop();

        static OBSProtocol()
        {
            DataHelper.RegisterSerializer(new OBSScene.DataSerializer());
            DataHelper.RegisterSerializer(new AOBSRequest.DataSerializer());
            DataHelper.RegisterSerializer(new OBSRequestBatch.DataSerializer());
        }

        private static OBSProtocol? CreateNewConnection(string password, URI url, IOBSHandler? handler)
        {
            OBSProtocol protocol = new(password, handler);
            TCPAsyncClient obsClient = new(protocol, url);
            if (obsClient.Start())
            {
                while (!protocol.Identified && protocol.IsConnected())
                    Thread.Sleep(100);
                return (protocol.Identified) ? protocol : null;
            }
            return null;
        }

        public static OBSProtocol? NewConnection(string password, URI url, IOBSHandler handler) => CreateNewConnection(password, url, handler);
        public static OBSProtocol? NewConnection(string password, URI url) => CreateNewConnection(password, url, null);
        public static OBSProtocol? NewConnection(URI url, IOBSHandler handler) => CreateNewConnection(string.Empty, url, handler);
        public static OBSProtocol? NewConnection(URI url) => CreateNewConnection(string.Empty, url, null);

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

        protected override void OnClientDisconnected()
        {
            m_Handler?.OnDisconnect();
        }

        protected override void OnWSMessage(string message)
        {
            try
            {
                DataObject messageJson = JsonParser.Parse(message);
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
                        default: OBS_LOG.Log(string.Format("[{0}] {1}", op, data)); break;
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
                Send(JsonParser.NetStr(new DataObject() { { "op", WebSocketOpCode.Identify }, { "d", response } }));
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
                Send(JsonParser.NetStr(new DataObject() { { "op", WebSocketOpCode.Request }, { "d", request } }));
                while (!request.HasResult && IsConnected())
                    Thread.Sleep(10);
            }
        }

        public void Send(IEnumerable<AOBSRequest> requests)
        {
            OBSRequestBatch requestBatch = new();
            requestBatch.AddRequests(requests);
            if (m_PendingRequests.TryAdd(requestBatch.ID, requestBatch))
            {
                Send(JsonParser.NetStr(new DataObject() { { "op", WebSocketOpCode.RequestBatch }, { "d", requestBatch } }));
                while (!requestBatch.HasResult && IsConnected())
                    Thread.Sleep(10);
            }
        }
    }
}
