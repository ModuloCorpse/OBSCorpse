using CorpseLib;
using CorpseLib.Json;
using CorpseLib.Network;
using CorpseLib.Web;
using System.Text;
using Version = CorpseLib.Version;

namespace OBSCorpse
{
    public class OBSProtocol : WebSocketProtocol
    {
        private static readonly Version MINIMUM_REQUIRED = new(5, 1, 0);

        public static OBSProtocol? NewConnection(string password, URI url)
        {
            OBSProtocol protocol = new(password);
            TCPAsyncClient obsClient = new(protocol, url);
            obsClient.OnDisconnect += (ATCPClient _) => protocol.OnDisconnect?.Invoke(protocol, EventArgs.Empty);
            if (obsClient.Start())
            {
                while (!protocol.Identified && protocol.IsConnected())
                    Thread.Sleep(100);
                return (protocol.Identified) ? protocol : null;
            }
            return null;
        }
        public static OBSProtocol? NewConnection(URI url) => NewConnection(string.Empty, url);
        public static OBSProtocol? NewConnection(string host, int port = 4455) => NewConnection(string.Empty, URI.Build("ws").Host(host).Port(port).Path("/").Build());
        public static OBSProtocol? NewConnection(string password, string host, int port = 4455) => NewConnection(password, URI.Build("ws").Host(host).Port(port).Path("/").Build());

        public event EventHandler? OnDisconnect;
        public event EventHandler<string>? OnSceneChanged;
        public event EventHandler<Tuple<bool, string>>? OnStreamStatusChanged;

        private readonly Dictionary<string, FileCountdownTimeAction> m_Timers = [];
        private readonly Dictionary<string, OBSIRequest> m_PendingRequests = [];
        private readonly object m_Lock = new();
        private readonly string m_Password;
        private bool m_Identified = false;

        public bool Identified => m_Identified;

        private OBSProtocol(string password) => m_Password = password;

        protected override void OnWSMessage(string message)
        {
            try
            {
                JsonObject messageJson = JsonParser.Parse(message);
                if (messageJson.TryGet("op", out WebSocketOpCode? op) && messageJson.TryGet("d", out JsonObject? data))
                    HandleOBSMessage((WebSocketOpCode)op!, data!);
            }
            catch (Exception ex) { Console.WriteLine(ex.ToString()); }
        }

        private void SendMessage(WebSocketOpCode opCode, JsonObject data)
        {
            JsonObject obj = new() { { "op", opCode }, { "d", data } };
            Send(obj.ToNetworkString());
        }

        private void SetRequestResponse(JsonObject requestResponse)
        {
            OBSIRequest? request = null;
            if (requestResponse.TryGet("requestId", out string? id))
            {
                lock (m_Lock)
                {
                    if (m_PendingRequests.TryGetValue(id!, out request))
                        m_PendingRequests.Remove(id!);
                }
            }
            request?.ReceivedResponse(requestResponse);
        }

        private void SendRequest(OBSIRequest request)
        {
            lock (m_Lock)
            {
                m_PendingRequests[request.ID] = request;
            }
            SendMessage(WebSocketOpCode.Request, request.ToJson());
        }

        private OBSRequest SendRequest(string type, JsonObject? data)
        {
            OBSRequest request = new(type, data);
            SendRequest(request);
            return request;
        }

        private OBSRequest.Response SendAndAwaitRequest(string type, JsonObject? data)
        {
            OBSRequest request = SendRequest(type, data);
            AwaitRequest(request);
            return request.GetResponse();
        }

        private void AwaitRequest(OBSIRequest request)
        {
            while (!request.HaveResponse && IsConnected())
                Thread.Sleep(10);
        }

        private void HandleHello(JsonObject data)
        {
            if (data.TryGet("rpcVersion", out int? rpc) &&
                data.TryGet("obsWebSocketVersion", out string? websocketVersion) &&
                new Version(websocketVersion!) >= MINIMUM_REQUIRED)
            {
                JsonObject response = new() { { "rpcVersion", rpc! } };
                if (data.TryGet("authentication", out JsonObject? authentication) &&
                    authentication!.TryGet("challenge", out string? challenge) &&
                    authentication!.TryGet("salt", out string? salt))
                {
                    string base64_secret = Convert.ToBase64String(System.Security.Cryptography.SHA256.HashData(Encoding.UTF8.GetBytes(m_Password + salt)));
                    string auth = Convert.ToBase64String(System.Security.Cryptography.SHA256.HashData(Encoding.UTF8.GetBytes(base64_secret + challenge)));
                    response.Add("authentification", auth);
                }
                SendMessage(WebSocketOpCode.Identify, response);
            }
        }

        private void HandleSceneChange(JsonObject data)
        {
            if (data.TryGet("sceneName", out string? sceneName))
                OnSceneChanged?.Invoke(this, sceneName!);
        }

        private void HandleStreamStateChanged(JsonObject data)
        {
            if (data.TryGet("outputActive", out bool? outputActive) && data.TryGet("outputState", out string? outputState))
                OnStreamStatusChanged?.Invoke(this, new((bool)outputActive!, outputState!));
        }

        private void HandleEvent(JsonObject data)
        {
            if (data.TryGet("eventType", out string? eventType))
            {
                JsonObject eventData = data.GetOrDefault("eventData", new JsonObject())!;
                switch (eventType)
                {
                    case "CurrentProgramSceneChanged": HandleSceneChange(eventData); break;
                    case "StreamStateChanged": HandleStreamStateChanged(eventData); break;
                }
            }
        }

        private void HandleOBSMessage(WebSocketOpCode op, JsonObject data)
        {
            switch (op)
            {
                case WebSocketOpCode.Hello: HandleHello(data); break;
                case WebSocketOpCode.Identified: m_Identified = true; break;
                case WebSocketOpCode.Event: HandleEvent(data); break;
                case WebSocketOpCode.RequestResponse: SetRequestResponse(data); break;
                case WebSocketOpCode.RequestBatchResponse: SetRequestResponse(data); break;
                default: Console.WriteLine("[{0}] {1}", op, data); break;
            }
        }

        public void StartTimer(long durationInSeconds, string scene, string path) => StartTimer(durationInSeconds, string.Empty, scene, path);

        public void StartTimer(long durationInSeconds, string endMessage, string scene, string path)
        {
            if (m_Timers.TryGetValue(path, out var oldTimer))
                oldTimer.Stop();
            FileCountdownTimeAction timer = (string.IsNullOrEmpty(endMessage)) ? new(path, durationInSeconds) : new(path, endMessage, durationInSeconds);
            timer.OnFinish += (object? sender, EventArgs e) => m_Timers.Remove(path);
            m_Timers[path] = timer;
            timer.Start();
            if (!string.IsNullOrEmpty(scene))
                SetCurrentProgramScene(scene);
        }

        //====================GENERAL====================\\
        /*
            GetVersion
            GetStats
            BroadcastCustomEvent
            CallVendorRequest
            GetHotkeyList
            TriggerHotkeyByName
            TriggerHotkeyByKeySequence
            Sleep
        */

        //====================CONFIG====================\\
        /*
            GetPersistentData
            SetPersistentData
        */
        public OBSSceneCollectionList GetSceneCollectionList()
        {
            string currentSceneCollection = string.Empty;
            List<string> sceneCollectionList = [];
            OBSRequest.Response response = SendAndAwaitRequest("GetSceneCollectionList", null);
            if (response.Result && response.Data != null &&
                response.Data.TryGet("currentSceneCollectionName", out string? currentSceneCollectionName) &&
                response.Data.TryGet("sceneCollections", out List<string>? collection))
            {
                currentSceneCollection = currentSceneCollectionName!;
                sceneCollectionList.AddRange(collection!);
            }
            return new(currentSceneCollection, sceneCollectionList);
        }
        public bool SetCurrentSceneCollection(string sceneCollectionName) => SendAndAwaitRequest("SetCurrentSceneCollection", new JsonObject() { { "sceneCollectionName", sceneCollectionName } }).Result;
        public bool CreateSceneCollection(string sceneCollectionName) => SendAndAwaitRequest("CreateSceneCollection", new JsonObject() { { "sceneCollectionName", sceneCollectionName } }).Result;
        public OBSProfileList GetProfileList()
        {
            string currentProfile = string.Empty;
            List<string> profilesList = [];
            OBSRequest.Response response = SendAndAwaitRequest("GetProfileList", null);
            if (response.Result && response.Data != null &&
                response.Data.TryGet("currentProfileName", out string? currentProfileName) &&
                response.Data.TryGet("profiles", out List<string>? profiles))
            {
                currentProfile = currentProfileName!;
                profilesList.AddRange(profiles!);
            }
            return new(currentProfile, profilesList);
        }
        public bool SetCurrentProfile(string profileName) => SendAndAwaitRequest("SetCurrentProfile", new JsonObject() { { "profileName", profileName } }).Result;
        public bool CreateProfile(string profileName) => SendAndAwaitRequest("CreateProfile", new JsonObject() { { "profileName", profileName   } }).Result;
        public bool RemoveProfile(string profileName) => SendAndAwaitRequest("RemoveProfile", new JsonObject() { { "profileName", profileName } }).Result;
        /*
            GetProfileParameter
            SetProfileParameter
            GetVideoSettings
            SetVideoSettings
            GetStreamServiceSettings
            SetStreamServiceSettings
        */
        public string GetRecordDirectory()
        {
            OBSRequest.Response response = SendAndAwaitRequest("GetRecordDirectory", null);
            if (response.Result && response.Data != null &&
                response.Data.TryGet("recordDirectory", out string? recordDirectory))
                return recordDirectory!;
            return string.Empty;
        }

        //====================SOURCES====================\\
        /*
            GetSourceActive
            GetSourceScreenshot
            SaveSourceScreenshot
        */

        //====================SCENES====================\\
        public OBSSceneList GetSceneList()
        {
            string currentProgramScene = string.Empty;
            string currentPreviewScene = string.Empty;
            List<OBSScene> scenesList = [];
            OBSRequest.Response response = SendAndAwaitRequest("GetSceneList", null);
            if (response.Result && response.Data != null &&
                response.Data.TryGet("currentProgramSceneName", out string? currentProgramSceneName) &&
                response.Data.TryGet("currentPreviewSceneName", out string? currentPreviewSceneName) &&
                response.Data.TryGet("scenes", out List<JsonObject>? scenes))
            {
                currentProgramScene = currentProgramSceneName!;
                currentPreviewScene = currentPreviewSceneName ?? string.Empty;
                foreach (JsonObject sceneObj in scenes!)
                {
                    if (sceneObj.TryGet("sceneName", out string? sceneName) && sceneObj.TryGet("sceneIndex", out int? sceneIndex))
                        scenesList.Add(new(sceneName!, (int)sceneIndex!));
                }
            }
            return new(currentProgramScene, currentPreviewScene, scenesList);
        }
        public List<string> GetGroupList()
        {
            List<string> groupsList = [];
            OBSRequest.Response response = SendAndAwaitRequest("GetGroupList", null);
            if (response.Result && response.Data != null &&
                response.Data.TryGet("groups", out List<string>? groups))
                groupsList.AddRange(groups!);
            return groupsList;
        }
        public string GetCurrentProgramScene()
        {
            OBSRequest.Response response = SendAndAwaitRequest("GetCurrentProgramScene", null);
            if (response.Result && response.Data != null &&
                response.Data.TryGet("currentProgramSceneName", out string? currentProgramSceneName))
                return currentProgramSceneName!;
            return string.Empty;
        }
        public bool SetCurrentProgramScene(string sceneName) => SendAndAwaitRequest("SetCurrentProgramScene", new JsonObject() { { "sceneName", sceneName } }).Result;
        public string GetCurrentPreviewScene()
        {
            OBSRequest.Response response = SendAndAwaitRequest("GetCurrentPreviewScene", null);
            if (response.Result && response.Data != null &&
                response.Data.TryGet("currentPreviewSceneName", out string? currentPreviewSceneName))
                return currentPreviewSceneName!;
            return string.Empty;
        }
        public bool SetCurrentPreviewScene(string sceneName) => SendAndAwaitRequest("SetCurrentPreviewScene", new JsonObject() { { "sceneName", sceneName } }).Result;
        public bool CreateScene(string sceneName) => SendAndAwaitRequest("CreateScene", new JsonObject() { { "sceneName", sceneName } }).Result;
        public bool RemoveScene(string sceneName) => SendAndAwaitRequest("RemoveScene", new JsonObject() { { "sceneName", sceneName } }).Result;
        public bool SetSceneName(string sceneName, string newSceneName) => SendAndAwaitRequest("SetSceneName", new JsonObject() { { "sceneName", sceneName }, { "newSceneName", newSceneName } }).Result;
        /*
            GetSceneSceneTransitionOverride
            SetSceneSceneTransitionOverride
        */

        //====================INPUTS====================\\
        /*
            GetInputList
            GetInputKindList
            GetSpecialInputs
            CreateInput
            RemoveInput
            SetInputName
            GetInputDefaultSettings
            GetInputSettings
            SetInputSettings
            GetInputMute
            SetInputMute
            ToggleInputMute
            GetInputVolume
            SetInputVolume
            GetInputAudioBalance
            SetInputAudioBalance
            GetInputAudioSyncOffset
            SetInputAudioSyncOffset
            GetInputAudioMonitorType
            SetInputAudioMonitorType
            GetInputAudioTracks
            SetInputAudioTracks
            GetInputPropertiesListPropertyItems
            PressInputPropertiesButton
        */

        //====================TRANSITIONS====================\\
        /*
            GetTransitionKindList
            GetSceneTransitionList
            GetCurrentSceneTransition
            SetCurrentSceneTransition
            SetCurrentSceneTransitionDuration
            SetCurrentSceneTransitionSettings
            GetCurrentSceneTransitionCursor
            TriggerStudioModeTransition
            SetTBarPosition
        */

        //====================FILTERS====================\\
        /*
            GetSourceFilterList
            GetSourceFilterDefaultSettings
            CreateSourceFilter
            RemoveSourceFilter
            SetSourceFilterName
            GetSourceFilter
            SetSourceFilterIndex
            SetSourceFilterSettings
            SetSourceFilterEnabled
        */

        //====================SCENE ITEMS====================\\
        /*
            GetSceneItemList
            GetGroupSceneItemList
            GetSceneItemId
            CreateSceneItem
            RemoveSceneItem
            DuplicateSceneItem
            GetSceneItemTransform
            SetSceneItemTransform
            GetSceneItemEnabled
            SetSceneItemEnabled
            GetSceneItemLocked
            SetSceneItemLocked
            GetSceneItemIndex
            SetSceneItemIndex
            GetSceneItemBlendMode
            SetSceneItemBlendMode
        */

        //====================OUTPUTS====================\\
        /*
            GetVirtualCamStatus
            ToggleVirtualCam
            StartVirtualCam
            StopVirtualCam
            GetReplayBufferStatus
            ToggleReplayBuffer
            StartReplayBuffer
            StopReplayBuffer
            SaveReplayBuffer
            GetLastReplayBufferReplay
            GetOutputList
            GetOutputStatus
            ToggleOutput
            StartOutput
            StopOutput
            GetOutputSettings
            SetOutputSettings
        */

        //====================STREAM====================\\
        public OBSStreamStatus? GetStreamStatus()
        {
            OBSRequest.Response response = SendAndAwaitRequest("GetStreamStatus", null);
            if (response.Result && response.Data != null &&
                response.Data.TryGet("outputActive", out bool? outputActive) &&
                response.Data.TryGet("outputReconnecting", out bool? outputReconnecting) &&
                response.Data.TryGet("outputTimecode", out string? outputTimecode) &&
                response.Data.TryGet("outputDuration", out long? outputDuration) &&
                response.Data.TryGet("outputCongestion", out long? outputCongestion) &&
                response.Data.TryGet("outputBytes", out long? outputBytes) &&
                response.Data.TryGet("outputSkippedFrames", out long? outputSkippedFrames) &&
                response.Data.TryGet("outputTotalFrames", out long? outputTotalFrames))
            {
                return new((bool)outputActive!,
                    (bool)outputReconnecting!,
                    outputTimecode!,
                    (long)outputDuration!,
                    (long)outputCongestion!,
                    (long)outputBytes!,
                    (long)outputSkippedFrames!,
                    (long)outputTotalFrames!);
            }
            return null;
        }
        public OperationResult<bool> ToggleStream()
        {
            OBSRequest.Response response = SendAndAwaitRequest("ToggleStream", null);
            if (response.Result && response.Data != null &&
                response.Data.TryGet("outputActive", out bool? outputActive))
                return new((bool)outputActive!);
            return new("Bad response", string.Empty);
        }
        public bool StartStream() => SendAndAwaitRequest("StartStream", null).Result;
        public bool StopStream() => SendAndAwaitRequest("StopStream", null).Result;
        public bool SendStreamCaption(string captionText) => SendAndAwaitRequest("SendStreamCaption", new JsonObject() { { "captionText", captionText } }).Result;

        //====================RECORD====================\\
        public OBSRecordStatus? GetRecordStatus()
        {
            OBSRequest.Response response = SendAndAwaitRequest("GetRecordStatus", null);
            if (response.Result && response.Data != null &&
                response.Data.TryGet("outputActive", out bool? outputActive) &&
                response.Data.TryGet("outputPaused", out bool? outputPaused) &&
                response.Data.TryGet("outputTimecode", out string? outputTimecode) &&
                response.Data.TryGet("outputDuration", out long? outputDuration) &&
                response.Data.TryGet("outputBytes", out long? outputBytes))
            {
                return new((bool)outputActive!,
                    (bool)outputPaused!,
                    outputTimecode!,
                    (long)outputDuration!,
                    (long)outputBytes!);
            }
            return null;
        }
        public bool ToggleRecord() => SendAndAwaitRequest("ToggleRecord", null).Result;
        public bool StartRecord() => SendAndAwaitRequest("StartRecord", null).Result;
        public bool ToggleRecordPause() => SendAndAwaitRequest("ToggleRecordPause", null).Result;
        public bool PauseRecord() => SendAndAwaitRequest("PauseRecord", null).Result;
        public bool ResumeRecord() => SendAndAwaitRequest("ResumeRecord", null).Result;
        public string StopRecord()
        {
            OBSRequest.Response response = SendAndAwaitRequest("StopRecord", null);
            if (response.Result && response.Data != null &&
                response.Data.TryGet("outputPath", out string? outputPath))
                return outputPath!;
            return string.Empty;
        }

        //====================MEDIA INPUTS====================\\
        /*
            GetMediaInputStatus
            SetMediaInputCursor
            OffsetMediaInputCursor
            TriggerMediaInputAction
        */

        //====================UI====================\\
        /*
            GetStudioModeEnabled
            SetStudioModeEnabled
            OpenInputPropertiesDialog
            OpenInputFiltersDialog
            OpenInputInteractDialog
            GetMonitorList
            OpenVideoMixProjector
            OpenSourceProjector
        */
    }
}
