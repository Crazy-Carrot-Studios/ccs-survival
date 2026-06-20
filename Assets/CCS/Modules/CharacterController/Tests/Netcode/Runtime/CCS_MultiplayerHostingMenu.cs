using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// =============================================================================
// SCRIPT: CCS_MultiplayerHostingMenu
// CATEGORY: Modules / CharacterController / Tests / Netcode / Runtime
// PURPOSE: Simple Name → Host or Join → auto-enter character test flow.
// PLACEMENT: SCN_CCS_MultiplayerHosting Canvas root.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Test-only local hosting. No Relay, Lobby, or internet matchmaking.
// =============================================================================

namespace CCS.Modules.CharacterController.Tests.Netcode
{
    public sealed class CCS_MultiplayerHostingMenu : MonoBehaviour
    {
        #region Variables

        [Header("Network")]
        [SerializeField] private NetworkManager networkManager;
        [SerializeField] private UnityTransport transport;

        [Header("Step 1 - Player Name")]
        [SerializeField] private InputField playerNameInput;
        [SerializeField] private TextMeshProUGUI playerNameWarningText;

        [Header("Step 1 - Server Name")]
        [SerializeField] private InputField serverNameInput;
        [SerializeField] private TextMeshProUGUI serverNameWarningText;

        [Header("Step 2 - Host")]
        [SerializeField] private Button hostAndStartButton;

        [Header("Step 2 - Join")]
        [SerializeField] private RectTransform serverListContainer;
        [SerializeField] private Text emptyServerListText;
        [SerializeField] private Button refreshServersButton;
        [SerializeField] private Button joinSelectedButton;

        [Header("Advanced")]
        [SerializeField] private GameObject advancedManualJoinPanel;
        [SerializeField] private Button advancedManualJoinToggleButton;
        [SerializeField] private InputField manualAddressInput;
        [SerializeField] private InputField manualPortInput;
        [SerializeField] private Button joinManualButton;

        [Header("Diagnostics")]
        [SerializeField] private GameObject diagnosticsPanel;
        [SerializeField] private Text diagnosticsText;

        [Header("Footer")]
        [SerializeField] private Text connectedPlayersText;
        [SerializeField] private Button quitButton;

        private readonly List<CCS_MultiplayerServerListEntry> serverEntries = new List<CCS_MultiplayerServerListEntry>();
        private readonly List<Button> serverListButtons = new List<Button>();
        private int selectedServerIndex = -1;
        private bool subscribedToNetworkEvents;
        private bool advancedPanelVisible;
        private bool pendingClientSceneSync;
        private bool pendingHostSceneLoad;
        private bool subscribedToSceneEvents;

        #endregion

        #region Unity Callbacks

        private void Awake()
        {
            ApplyDefaultUiValues();
            WireButtons();
            RebuildServerList();
        }

        private void OnEnable()
        {
            SubscribeToNetworkEvents();
            RefreshConnectedPlayersText();
            RefreshButtonStates();
        }

        private void OnDisable()
        {
            UnsubscribeFromNetworkEvents();
        }

        private void Update()
        {
            RefreshButtonStates();

            if (networkManager != null && networkManager.IsListening)
            {
                RefreshConnectedPlayersText();
            }
        }

        #endregion

        #region Public Methods

        public void OnHostAndStartClicked()
        {
            if (!TryValidateForHost(out string validationError))
            {
                LogHostFlow(validationError);
                return;
            }

            LogHostFlow("Host button clicked");

            if (!TryResolveNetworkReferences(out NetworkManager manager, out UnityTransport resolvedTransport))
            {
                LogHostFlow("Network manager or transport is missing.");
                SetDiagnostics("Network manager or transport is missing.");
                return;
            }

            LogHostFlow($"NetworkManager.Singleton exists: {(NetworkManager.Singleton != null).ToString()}");
            LogHostFlow($"IsListening before StartHost: {manager.IsListening.ToString()}");

            GameObject playerPrefab = manager.NetworkConfig != null ? manager.NetworkConfig.PlayerPrefab : null;
            bool playerPrefabValid = CCS_NetcodeNetworkConfigValidationUtility.HasValidNetworkObjectPrefab(playerPrefab);
            LogHostFlow($"PlayerPrefab valid: {playerPrefabValid.ToString()}");
            LogHostFlow(
                $"PlayerPrefab has NetworkObject: {(playerPrefab != null && playerPrefabValid).ToString()}");

            string targetSceneName = CCS_NetcodeTestConstants.MasterTestSceneName;
            bool masterTestInBuild = CCS_HostingSceneBuildUtility.IsSceneInBuildSettings(targetSceneName);
            LogHostFlow($"Build has Master Test scene: {masterTestInBuild.ToString()}");
            LogHostFlow($"Target scene name: {targetSceneName}");

            CCS_NetcodeRegistryUtility.TryLogNetworkConfigDiagnostics(manager);

            if (!masterTestInBuild)
            {
                string buildError =
                    $"Scene '{targetSceneName}' is missing from Build Settings. Run Setup And Validate Multiplayer Hosting Scene.";
                LogHostFlow(buildError);
                SetDiagnostics(buildError);
                return;
            }

            if (manager.IsListening)
            {
                LogHostFlow("Session is already running.");
                SetDiagnostics("Session is already running.");
                return;
            }

            CacheLocalPlayerName();

            if (!TryApplyDefaultHostTransport(resolvedTransport, out string errorMessage))
            {
                LogHostFlow(errorMessage);
                SetDiagnostics(errorMessage);
                return;
            }

            if (!CCS_NetcodeNetworkConfigValidationUtility.TryValidateForStart(manager, out string networkError))
            {
                LogHostFlow(networkError);
                SetDiagnostics(networkError);
                return;
            }

            LogHostFlow("Calling StartHost");
            bool hostStarted = manager.StartHost();
            LogHostFlow($"StartHost returned: {hostStarted.ToString()}");
            LogHostFlow($"IsListening after StartHost: {manager.IsListening.ToString()}");
            LogHostFlow(
                $"IsServer / IsHost / IsClient after StartHost: {manager.IsServer.ToString()} / "
                + $"{manager.IsHost.ToString()} / {manager.IsClient.ToString()}");

            if (!hostStarted)
            {
                string hostError = "Failed to start host. Check Player.log for [Hosting Flow] and [Netcode] lines.";
                LogHostFlow(hostError);
                SetDiagnostics(hostError);
                return;
            }

            pendingHostSceneLoad = true;
            string serverDisplayName = GetSanitizedServerName();
            CCS_LocalMultiplayerHostSessionBeacon.StartBeacon(
                serverDisplayName,
                CCS_NetcodeTestConstants.DefaultLocalhostAddress,
                CCS_NetcodeTestConstants.DefaultServerPort);
            SetDiagnostics($"Hosting as {CCS_LocalMultiplayerPlayerNameCache.PendingLocalDisplayName}. Entering test...");
            TryLoadPendingHostScene();
        }

        public void OnJoinSelectedClicked()
        {
            if (!TryValidatePlayerNameForJoin(out string playerNameError))
            {
                LogJoinFlow(playerNameError);
                return;
            }

            LogJoinFlow("Join button clicked");

            bool hasSelectedHost = selectedServerIndex >= 0 && selectedServerIndex < serverEntries.Count;
            LogJoinFlow($"Selected host exists: {hasSelectedHost.ToString()}");

            if (!hasSelectedHost)
            {
                LogJoinFlow("No host selected from join list.");
                SetDiagnostics("Select a host from the list, or refresh after a player hosts.");
                return;
            }

            CCS_MultiplayerServerListEntry entry = serverEntries[selectedServerIndex];

            LogJoinFlow($"Selected host address: {entry.Address}");
            LogJoinFlow($"Selected host port: {entry.Port.ToString(CultureInfo.InvariantCulture)}");
            TryJoinAddress(entry.Address, entry.Port, entry.GetListLabel());
        }

        public void OnJoinManualClicked()
        {
            if (!TryValidatePlayerNameForJoin(out _))
            {
                return;
            }

            string address = manualAddressInput != null ? manualAddressInput.text.Trim() : string.Empty;
            if (string.IsNullOrWhiteSpace(address))
            {
                SetDiagnostics("Enter a manual address.");
                return;
            }

            if (!TryParsePort(manualPortInput, out ushort port, out string portError))
            {
                SetDiagnostics(portError);
                return;
            }

            TryJoinAddress(address, port, $"{address}:{port.ToString(CultureInfo.InvariantCulture)}");
        }

        public void OnRefreshServersClicked()
        {
            RebuildServerList();
            LogJoinFlow($"Refresh clicked; host list count={serverEntries.Count.ToString(CultureInfo.InvariantCulture)}");
            SetDiagnostics("Host list refreshed.");
        }

        public void OnAdvancedManualJoinToggleClicked()
        {
            advancedPanelVisible = !advancedPanelVisible;
            if (advancedManualJoinPanel != null)
            {
                advancedManualJoinPanel.SetActive(advancedPanelVisible);
            }

            if (advancedManualJoinToggleButton != null)
            {
                Text label = advancedManualJoinToggleButton.GetComponentInChildren<Text>();
                if (label != null)
                {
                    label.text = advancedPanelVisible ? "Hide Advanced Manual Join" : "Advanced Manual Join";
                }
            }
        }

        public void OnQuitClicked()
        {
            pendingClientSceneSync = false;
            pendingHostSceneLoad = false;
            CCS_LocalMultiplayerHostSessionBeacon.StopBeacon();
            CCS_HostingApplicationQuitUtility.QuitApplication(networkManager);
        }

        #endregion

        #region Private Methods

        private void ApplyDefaultUiValues()
        {
            if (playerNameInput != null)
            {
                playerNameInput.text = string.Empty;
            }

            ApplyDefaultServerName();

            if (manualAddressInput != null && string.IsNullOrWhiteSpace(manualAddressInput.text))
            {
                manualAddressInput.text = CCS_NetcodeTestConstants.DefaultLocalhostAddress;
            }

            if (manualPortInput != null && string.IsNullOrWhiteSpace(manualPortInput.text))
            {
                manualPortInput.text = CCS_NetcodeTestConstants.DefaultServerPort.ToString(CultureInfo.InvariantCulture);
            }

            if (advancedManualJoinPanel != null)
            {
                advancedManualJoinPanel.SetActive(false);
            }

            if (diagnosticsPanel != null)
            {
                diagnosticsPanel.SetActive(false);
            }

            advancedPanelVisible = false;
            SetDiagnostics(string.Empty);
            ClearPlayerNameWarning();
            ClearServerNameWarning();
            RefreshConnectedPlayersText();
        }

        private void WireButtons()
        {
            if (playerNameInput != null)
            {
                playerNameInput.onValueChanged.AddListener(HandlePlayerNameInputChanged);
            }

            if (serverNameInput != null)
            {
                serverNameInput.onValueChanged.AddListener(HandleServerNameInputChanged);
            }

            if (hostAndStartButton != null)
            {
                hostAndStartButton.onClick.AddListener(OnHostAndStartClicked);
            }

            if (joinSelectedButton != null)
            {
                joinSelectedButton.onClick.AddListener(OnJoinSelectedClicked);
            }

            if (joinManualButton != null)
            {
                joinManualButton.onClick.AddListener(OnJoinManualClicked);
            }

            if (refreshServersButton != null)
            {
                refreshServersButton.onClick.AddListener(OnRefreshServersClicked);
            }

            if (advancedManualJoinToggleButton != null)
            {
                advancedManualJoinToggleButton.onClick.AddListener(OnAdvancedManualJoinToggleClicked);
            }

            if (quitButton != null)
            {
                quitButton.onClick.AddListener(OnQuitClicked);
            }
        }

        private void RebuildServerList()
        {
            ClearServerListUi();
            serverEntries.Clear();
            serverEntries.AddRange(CCS_LocalMultiplayerHostDiscovery.DiscoverLocalHosts());
            LogJoinFlow(
                $"Host list rebuilt with {serverEntries.Count.ToString(CultureInfo.InvariantCulture)} discovered host(s).");

            for (int i = 0; i < serverEntries.Count; i++)
            {
                int capturedIndex = i;
                Button rowButton = CreateServerListRow(serverEntries[i]);
                rowButton.onClick.AddListener(() => SelectServer(capturedIndex));
                serverListButtons.Add(rowButton);
            }

            bool hasServers = serverEntries.Count > 0;
            if (emptyServerListText != null)
            {
                emptyServerListText.gameObject.SetActive(!hasServers);
                emptyServerListText.text = CCS_NetcodeTestConstants.EmptyServerListMessage;
            }

            if (hasServers)
            {
                SelectServer(0);
            }
            else
            {
                selectedServerIndex = -1;
            }
        }

        private void ClearServerListUi()
        {
            for (int i = 0; i < serverListButtons.Count; i++)
            {
                if (serverListButtons[i] != null)
                {
                    Destroy(serverListButtons[i].gameObject);
                }
            }

            serverListButtons.Clear();
            selectedServerIndex = -1;
        }

        private Button CreateServerListRow(CCS_MultiplayerServerListEntry entry)
        {
            Transform parent = serverListContainer != null ? serverListContainer : transform;
            GameObject rowObject = new GameObject(
                $"ServerRow_{entry.DisplayName}",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image),
                typeof(Button),
                typeof(LayoutElement));

            rowObject.transform.SetParent(parent, false);

            LayoutElement layoutElement = rowObject.GetComponent<LayoutElement>();
            layoutElement.preferredHeight = 44f;
            layoutElement.minHeight = 44f;

            Image background = rowObject.GetComponent<Image>();
            background.color = new Color(0.1f, 0.14f, 0.2f, 1f);

            GameObject labelObject = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            labelObject.transform.SetParent(rowObject.transform, false);
            RectTransform labelRect = labelObject.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = new Vector2(14f, 6f);
            labelRect.offsetMax = new Vector2(-14f, -6f);

            Text label = labelObject.GetComponent<Text>();
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.fontSize = 18;
            label.alignment = TextAnchor.MiddleLeft;
            label.color = new Color(0.91f, 0.93f, 0.97f, 1f);
            label.text = entry.GetListLabel();

            Button button = rowObject.GetComponent<Button>();
            StyleListButton(button);
            return button;
        }

        private static void StyleListButton(Button button)
        {
            ColorBlock colors = button.colors;
            colors.normalColor = new Color(0.1f, 0.14f, 0.2f, 1f);
            colors.highlightedColor = new Color(0.2f, 0.45f, 0.78f, 1f);
            colors.pressedColor = new Color(0.16f, 0.38f, 0.68f, 1f);
            colors.selectedColor = new Color(0.2f, 0.45f, 0.78f, 1f);
            button.colors = colors;
        }

        private void SelectServer(int index)
        {
            selectedServerIndex = index;

            for (int i = 0; i < serverListButtons.Count; i++)
            {
                if (serverListButtons[i] == null)
                {
                    continue;
                }

                Image image = serverListButtons[i].GetComponent<Image>();
                if (image == null)
                {
                    continue;
                }

                image.color = i == selectedServerIndex
                    ? new Color(0.2f, 0.45f, 0.78f, 1f)
                    : new Color(0.1f, 0.14f, 0.2f, 1f);
            }
        }

        private void TryJoinAddress(string address, ushort port, string displayTarget)
        {
            LogJoinFlow($"Active scene before join: {SceneManager.GetActiveScene().name}");

            if (!TryResolveNetworkReferences(out NetworkManager manager, out UnityTransport resolvedTransport))
            {
                LogJoinFlow("Network manager or transport is missing.");
                SetDiagnostics("Network manager or transport is missing.");
                return;
            }

            LogJoinFlow($"NetworkManager.Singleton exists: {(NetworkManager.Singleton != null).ToString()}");
            LogJoinFlow($"IsListening before StartClient: {manager.IsListening.ToString()}");

            GameObject playerPrefab = manager.NetworkConfig != null ? manager.NetworkConfig.PlayerPrefab : null;
            bool playerPrefabValid = CCS_NetcodeNetworkConfigValidationUtility.HasValidNetworkObjectPrefab(playerPrefab);
            LogJoinFlow($"PlayerPrefab valid: {playerPrefabValid.ToString()}");
            LogJoinFlow($"NetworkPrefabsList valid: {HasValidNetworkPrefabsList(manager).ToString()}");

            CCS_NetcodeRegistryUtility.TryLogNetworkConfigDiagnostics(manager);

            if (manager.IsListening)
            {
                LogJoinFlow("Already connected.");
                SetDiagnostics("Already connected.");
                return;
            }

            CacheLocalPlayerName();
            pendingClientSceneSync = true;

            LogJoinFlow($"Transport type: {resolvedTransport.GetType().Name}");
            resolvedTransport.SetConnectionData(address, port);
            LogTransportConnectionData(resolvedTransport, "before StartClient");

            if (!CCS_NetcodeNetworkConfigValidationUtility.TryValidateForStart(manager, out string networkError))
            {
                pendingClientSceneSync = false;
                LogJoinFlow(networkError);
                SetDiagnostics(networkError);
                return;
            }

            LogJoinFlow("Calling StartClient");
            bool clientStarted = manager.StartClient();
            LogJoinFlow($"StartClient returned: {clientStarted.ToString()}");
            LogJoinFlow($"IsListening after StartClient: {manager.IsListening.ToString()}");
            LogJoinFlow(
                $"IsServer / IsHost / IsClient after StartClient: {manager.IsServer.ToString()} / "
                + $"{manager.IsHost.ToString()} / {manager.IsClient.ToString()}");

            if (!clientStarted)
            {
                pendingClientSceneSync = false;
                string clientError = "Failed to start client. Check Console for [Join Flow] lines.";
                LogJoinFlow(clientError);
                SetDiagnostics(clientError);
                return;
            }

            TrySubscribeToSceneEvents();
            SetDiagnostics($"Joining {displayTarget}...");
        }

        private void LoadMasterTestSceneAsNetworkSession()
        {
            if (networkManager == null || !networkManager.IsListening)
            {
                LogHostFlow("LoadMasterTestSceneAsNetworkSession skipped: NetworkManager is not listening.");
                return;
            }

            string sceneName = CCS_NetcodeTestConstants.MasterTestSceneName;
            if (!CCS_HostingSceneBuildUtility.IsSceneInBuildSettings(sceneName))
            {
                LogHostFlow($"Target scene '{sceneName}' is missing from build settings.");
                SetDiagnostics($"Scene '{sceneName}' is not in Build Settings.");
                return;
            }

            if (networkManager.IsServer)
            {
                if (networkManager.NetworkConfig.EnableSceneManagement && networkManager.SceneManager != null)
                {
                    LogHostFlow("Loading Master Test via Netcode SceneManager");
                    SceneEventProgressStatus loadStatus = networkManager.SceneManager.LoadScene(
                        sceneName,
                        LoadSceneMode.Single);
                    LogHostFlow($"Netcode scene load returned status: {loadStatus}");
                    if (loadStatus == SceneEventProgressStatus.Started)
                    {
                        LogHostFlow("Fallback SceneManager load used: no");
                        return;
                    }

                    LogHostFlow(
                        $"Netcode scene load did not start ({loadStatus}). Using SceneManager fallback for local host.");
                }
                else
                {
                    LogHostFlow("Scene management disabled or unavailable; using SceneManager fallback for local host.");
                }

                LogHostFlow("Fallback SceneManager load used: yes");
                SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
                LogActiveSceneAfterLoad();
                return;
            }

            LogHostFlow($"Client fallback SceneManager.LoadScene('{sceneName}', Single).");
            LogHostFlow("Fallback SceneManager load used: yes");
            SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
            LogActiveSceneAfterLoad();
        }

        private void LogActiveSceneAfterLoad()
        {
            LogHostFlow($"Active scene after load: {SceneManager.GetActiveScene().name}");
        }

        private void TryLoadPendingHostScene()
        {
            if (!pendingHostSceneLoad)
            {
                return;
            }

            if (networkManager == null || !networkManager.IsListening || !networkManager.IsServer)
            {
                return;
            }

            pendingHostSceneLoad = false;
            LoadMasterTestSceneAsNetworkSession();
        }

        private static void LogHostFlow(string message)
        {
            Debug.Log("[Hosting Flow] " + message);
        }

        private static void LogJoinFlow(string message)
        {
            Debug.Log("[Join Flow] " + message);
        }

        private static void LogTransportConnectionData(UnityTransport resolvedTransport, string stageLabel)
        {
            if (resolvedTransport == null)
            {
                LogJoinFlow($"Transport connection data {stageLabel}: transport is null.");
                return;
            }

            UnityTransport.ConnectionAddressData connectionData = resolvedTransport.ConnectionData;
            LogJoinFlow(
                $"Transport connection data {stageLabel}: {connectionData.Address}:"
                + $"{connectionData.Port.ToString(CultureInfo.InvariantCulture)} "
                + $"(listen={connectionData.ServerListenAddress})");
        }

        private static bool HasValidNetworkPrefabsList(NetworkManager manager)
        {
            if (manager == null || manager.NetworkConfig?.Prefabs?.NetworkPrefabsLists == null)
            {
                return false;
            }

            List<NetworkPrefabsList> prefabLists = manager.NetworkConfig.Prefabs.NetworkPrefabsLists;
            if (prefabLists.Count == 0)
            {
                return false;
            }

            for (int i = 0; i < prefabLists.Count; i++)
            {
                NetworkPrefabsList prefabsList = prefabLists[i];
                if (prefabsList == null || prefabsList.PrefabList == null || prefabsList.PrefabList.Count == 0)
                {
                    return false;
                }

                for (int j = 0; j < prefabsList.PrefabList.Count; j++)
                {
                    if (!CCS_NetcodeNetworkConfigValidationUtility.HasValidNetworkObjectPrefab(
                            prefabsList.PrefabList[j].Prefab))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private void SubscribeToNetworkEvents()
        {
            if (subscribedToNetworkEvents || networkManager == null)
            {
                return;
            }

            networkManager.OnClientConnectedCallback += HandleClientConnected;
            networkManager.OnClientDisconnectCallback += HandleClientDisconnected;
            networkManager.OnServerStarted += HandleServerStarted;
            subscribedToNetworkEvents = true;
            SubscribeToSceneEvents();
        }

        private void UnsubscribeFromNetworkEvents()
        {
            if (!subscribedToNetworkEvents || networkManager == null)
            {
                return;
            }

            networkManager.OnClientConnectedCallback -= HandleClientConnected;
            networkManager.OnClientDisconnectCallback -= HandleClientDisconnected;
            networkManager.OnServerStarted -= HandleServerStarted;
            subscribedToNetworkEvents = false;
            UnsubscribeFromSceneEvents();
        }

        private void SubscribeToSceneEvents()
        {
            TrySubscribeToSceneEvents();
        }

        private void TrySubscribeToSceneEvents()
        {
            if (subscribedToSceneEvents || networkManager == null || networkManager.SceneManager == null)
            {
                return;
            }

            networkManager.SceneManager.OnSceneEvent += HandleNetworkSceneEvent;
            subscribedToSceneEvents = true;
            LogJoinFlow("Subscribed to Netcode SceneManager.OnSceneEvent.");
        }

        private void UnsubscribeFromSceneEvents()
        {
            if (!subscribedToSceneEvents || networkManager == null || networkManager.SceneManager == null)
            {
                return;
            }

            networkManager.SceneManager.OnSceneEvent -= HandleNetworkSceneEvent;
            subscribedToSceneEvents = false;
        }

        private void HandleNetworkSceneEvent(SceneEvent sceneEvent)
        {
            string sceneLabel = GetSceneEventLabel(sceneEvent);
            LogJoinFlow(
                $"Netcode scene event: {sceneEvent.SceneEventType} scene={sceneLabel} clientId={sceneEvent.ClientId}");

            if (networkManager == null)
            {
                return;
            }

            bool isLocalClientEvent = sceneEvent.ClientId == networkManager.LocalClientId;
            if (networkManager.IsClient && !networkManager.IsServer && isLocalClientEvent)
            {
                if (sceneEvent.SceneEventType == SceneEventType.Synchronize
                    || sceneEvent.SceneEventType == SceneEventType.SynchronizeComplete
                    || sceneEvent.SceneEventType == SceneEventType.LoadEventCompleted
                    || sceneEvent.SceneEventType == SceneEventType.LoadComplete)
                {
                    LogJoinFlow($"Active scene after sync/load: {SceneManager.GetActiveScene().name}");
                    if (pendingClientSceneSync
                        && (sceneEvent.SceneEventType == SceneEventType.SynchronizeComplete
                            || sceneEvent.SceneEventType == SceneEventType.LoadEventCompleted
                            || sceneEvent.SceneEventType == SceneEventType.LoadComplete
                            || SceneManager.GetActiveScene().name == CCS_NetcodeTestConstants.MasterTestSceneName))
                    {
                        pendingClientSceneSync = false;
                        SetDiagnostics("Connected. Entering character test...");
                    }
                }
            }

            if (sceneEvent.SceneEventType == SceneEventType.LoadComplete
                || sceneEvent.SceneEventType == SceneEventType.LoadEventCompleted)
            {
                LogActiveSceneAfterLoad();
            }
        }

        private static string GetSceneEventLabel(SceneEvent sceneEvent)
        {
            if (!string.IsNullOrEmpty(sceneEvent.SceneName))
            {
                return sceneEvent.SceneName;
            }

            return SceneManager.GetActiveScene().name;
        }

        private void HandleServerStarted()
        {
            RefreshConnectedPlayersText();
            LogHostFlow("OnServerStarted received.");
            SubscribeToSceneEvents();
            TryLoadPendingHostScene();
        }

        private void HandleClientConnected(ulong clientId)
        {
            RefreshConnectedPlayersText();
            LogJoinFlow($"OnClientConnected callback: clientId={clientId.ToString(CultureInfo.InvariantCulture)}");

            if (networkManager == null)
            {
                return;
            }

            if (networkManager.IsServer && clientId == networkManager.LocalClientId)
            {
                return;
            }

            if (networkManager.IsClient && !networkManager.IsServer && clientId == networkManager.LocalClientId)
            {
                LogJoinFlow(
                    $"Local client connected. Active scene: {SceneManager.GetActiveScene().name}. "
                    + $"Pending scene sync: {pendingClientSceneSync.ToString()}");
                TrySubscribeToSceneEvents();

                if (pendingClientSceneSync
                    && SceneManager.GetActiveScene().name == CCS_NetcodeTestConstants.MasterTestSceneName)
                {
                    pendingClientSceneSync = false;
                    SetDiagnostics("Connected. Entering character test...");
                    LogJoinFlow($"Active scene after load: {SceneManager.GetActiveScene().name}");
                }
            }
        }

        private void HandleClientDisconnected(ulong clientId)
        {
            bool isLocalDisconnect = networkManager != null && clientId == networkManager.LocalClientId;
            LogJoinFlow(
                $"OnClientDisconnect callback: clientId={clientId.ToString(CultureInfo.InvariantCulture)} "
                + $"localDisconnect={isLocalDisconnect.ToString()}");

            if (isLocalDisconnect)
            {
                pendingClientSceneSync = false;
                LogJoinFlow(
                    $"Disconnect reason: isListening={networkManager.IsListening.ToString()} "
                    + $"isClient={networkManager.IsClient.ToString()}");
                SetDiagnostics("Disconnected from host.");
            }

            RefreshConnectedPlayersText();
        }

        private void RefreshConnectedPlayersText()
        {
            if (connectedPlayersText == null)
            {
                return;
            }

            int connectedCount = networkManager != null && networkManager.IsListening
                ? networkManager.ConnectedClientsIds.Count
                : 0;

            connectedPlayersText.text =
                $"Players: {connectedCount.ToString(CultureInfo.InvariantCulture)} / {CCS_NetcodeTestConstants.DefaultMaxPlayers.ToString(CultureInfo.InvariantCulture)}";
        }

        private void RefreshButtonStates()
        {
            bool isListening = networkManager != null && networkManager.IsListening;

            if (hostAndStartButton != null)
            {
                hostAndStartButton.interactable = !isListening;
            }

            if (joinSelectedButton != null)
            {
                joinSelectedButton.interactable = !isListening;
            }

            if (joinManualButton != null)
            {
                joinManualButton.interactable = !isListening;
            }

            if (refreshServersButton != null)
            {
                refreshServersButton.interactable = !isListening;
            }
        }

        private bool TryValidateForHost(out string errorMessage)
        {
            string rawPlayerName = playerNameInput != null ? playerNameInput.text : string.Empty;
            if (string.IsNullOrWhiteSpace(rawPlayerName))
            {
                errorMessage = CCS_NetcodeTestConstants.PlayerNameRequiredForHostWarningMessage;
                ClearServerNameWarning();
                ShowPlayerNameWarning(CCS_NetcodeTestConstants.PlayerNameRequiredForHostWarningMessage);
                return false;
            }

            ClearPlayerNameWarning();

            string rawServerName = serverNameInput != null ? serverNameInput.text : string.Empty;
            if (string.IsNullOrWhiteSpace(rawServerName))
            {
                errorMessage = CCS_NetcodeTestConstants.ServerNameRequiredWarningMessage;
                ShowServerNameWarning();
                return false;
            }

            ClearServerNameWarning();
            errorMessage = string.Empty;
            return true;
        }

        private bool TryValidatePlayerNameForJoin(out string errorMessage)
        {
            if (HasValidPlayerName(out errorMessage))
            {
                ClearPlayerNameWarning();
                return true;
            }

            errorMessage = CCS_NetcodeTestConstants.PlayerNameRequiredForJoinWarningMessage;
            ShowPlayerNameWarning(CCS_NetcodeTestConstants.PlayerNameRequiredForJoinWarningMessage);
            return false;
        }

        private bool HasValidPlayerName(out string errorMessage)
        {
            string rawName = playerNameInput != null ? playerNameInput.text : string.Empty;
            if (string.IsNullOrWhiteSpace(rawName))
            {
                errorMessage = CCS_NetcodeTestConstants.PlayerNameRequiredForJoinWarningMessage;
                return false;
            }

            errorMessage = string.Empty;
            return true;
        }

        private void HandlePlayerNameInputChanged(string _)
        {
            if (HasValidPlayerName(out _))
            {
                ClearPlayerNameWarning();
            }
        }

        private void HandleServerNameInputChanged(string _)
        {
            if (HasValidServerName(out _))
            {
                ClearServerNameWarning();
            }
        }

        private bool HasValidServerName(out string errorMessage)
        {
            string rawName = serverNameInput != null ? serverNameInput.text : string.Empty;
            if (string.IsNullOrWhiteSpace(rawName))
            {
                errorMessage = CCS_NetcodeTestConstants.ServerNameRequiredWarningMessage;
                return false;
            }

            errorMessage = string.Empty;
            return true;
        }

        private void ShowServerNameWarning()
        {
            if (serverNameWarningText != null)
            {
                serverNameWarningText.text = CCS_NetcodeTestConstants.ServerNameRequiredWarningMessage;
                serverNameWarningText.gameObject.SetActive(true);
            }

            FocusServerNameInput();
        }

        private void ClearServerNameWarning()
        {
            if (serverNameWarningText == null)
            {
                return;
            }

            serverNameWarningText.text = string.Empty;
            serverNameWarningText.gameObject.SetActive(false);
        }

        private void FocusServerNameInput()
        {
            if (serverNameInput == null)
            {
                return;
            }

            serverNameInput.Select();
            serverNameInput.ActivateInputField();
        }

        private void ApplyDefaultServerName()
        {
            if (serverNameInput == null)
            {
                return;
            }

            string playerName = playerNameInput != null ? playerNameInput.text : string.Empty;
            serverNameInput.text = CCS_MultiplayerServerNameUtility.CreateDefaultServerName(playerName);
        }

        private string GetSanitizedServerName()
        {
            string rawName = serverNameInput != null ? serverNameInput.text : string.Empty;
            return CCS_MultiplayerServerNameUtility.Sanitize(rawName);
        }

        private void ShowPlayerNameWarning(string warningMessage)
        {
            if (playerNameWarningText != null)
            {
                playerNameWarningText.text = warningMessage;
                playerNameWarningText.gameObject.SetActive(true);
            }

            FocusPlayerNameInput();
        }

        private void ClearPlayerNameWarning()
        {
            if (playerNameWarningText == null)
            {
                return;
            }

            playerNameWarningText.text = string.Empty;
            playerNameWarningText.gameObject.SetActive(false);
        }

        private void FocusPlayerNameInput()
        {
            if (playerNameInput == null)
            {
                return;
            }

            playerNameInput.Select();
            playerNameInput.ActivateInputField();
        }

        private void CacheLocalPlayerName()
        {
            string rawName = playerNameInput != null ? playerNameInput.text : string.Empty;
            CCS_LocalMultiplayerPlayerNameCache.SetPendingLocalDisplayName(rawName);
        }

        private bool TryResolveNetworkReferences(out NetworkManager manager, out UnityTransport resolvedTransport)
        {
            manager = networkManager != null ? networkManager : NetworkManager.Singleton;
            resolvedTransport = transport;
            if (resolvedTransport == null && manager != null)
            {
                resolvedTransport = manager.NetworkConfig.NetworkTransport as UnityTransport;
            }

            return manager != null && resolvedTransport != null;
        }

        private bool TryApplyDefaultHostTransport(UnityTransport resolvedTransport, out string errorMessage)
        {
            ushort port = CCS_NetcodeTestConstants.DefaultServerPort;
            resolvedTransport.SetConnectionData("0.0.0.0", port, "0.0.0.0");
            errorMessage = string.Empty;
            return true;
        }

        private static bool TryParsePort(InputField portInput, out ushort port, out string errorMessage)
        {
            errorMessage = string.Empty;
            port = CCS_NetcodeTestConstants.DefaultServerPort;

            string rawPort = portInput != null ? portInput.text.Trim() : string.Empty;
            if (string.IsNullOrWhiteSpace(rawPort))
            {
                rawPort = CCS_NetcodeTestConstants.DefaultServerPort.ToString(CultureInfo.InvariantCulture);
            }

            if (!ushort.TryParse(rawPort, NumberStyles.Integer, CultureInfo.InvariantCulture, out port))
            {
                errorMessage = "Port must be a valid number between 1 and 65535.";
                return false;
            }

            return true;
        }

        private void SetDiagnostics(string message)
        {
            bool hasMessage = !string.IsNullOrWhiteSpace(message);

            if (diagnosticsPanel != null)
            {
                diagnosticsPanel.SetActive(hasMessage);
            }

            if (diagnosticsText != null)
            {
                diagnosticsText.text = message ?? string.Empty;
            }
        }

        #endregion
    }
}
