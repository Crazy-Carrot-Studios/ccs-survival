using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
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
            if (!HasValidPlayerName(out string playerNameError))
            {
                LogHostFlow(playerNameError);
                SetDiagnostics(playerNameError);
                return;
            }

            if (!TryResolveNetworkReferences(out NetworkManager manager, out UnityTransport resolvedTransport))
            {
                LogHostFlow("Network manager or transport is missing.");
                SetDiagnostics("Network manager or transport is missing.");
                return;
            }

            LogHostFlow(
                $"Pre-host: manager={(manager != null ? manager.name : "null")}, "
                + $"singleton={(NetworkManager.Singleton != null ? NetworkManager.Singleton.name : "null")}, "
                + $"listening={manager.IsListening}");

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

            LogHostFlow(
                $"Starting host for scene '{CCS_NetcodeTestConstants.MasterTestSceneName}' "
                + $"(build included: {CCS_HostingSceneBuildUtility.IsSceneInBuildSettings(CCS_NetcodeTestConstants.MasterTestSceneName)}).");

            bool hostStarted = manager.StartHost();
            LogHostFlow($"StartHost returned {hostStarted.ToString()}.");

            if (!hostStarted)
            {
                SetDiagnostics("Failed to start host.");
                return;
            }

            pendingHostSceneLoad = true;
            SetDiagnostics($"Hosting as {CCS_LocalMultiplayerPlayerNameCache.PendingLocalDisplayName}. Entering test...");
            TryLoadPendingHostScene();
        }

        public void OnJoinSelectedClicked()
        {
            if (!HasValidPlayerName(out string playerNameError))
            {
                SetDiagnostics(playerNameError);
                return;
            }

            if (selectedServerIndex < 0 || selectedServerIndex >= serverEntries.Count)
            {
                SetDiagnostics("Select a host from the list.");
                return;
            }

            CCS_MultiplayerServerListEntry entry = serverEntries[selectedServerIndex];
            TryJoinAddress(entry.Address, entry.Port, entry.GetListLabel());
        }

        public void OnJoinManualClicked()
        {
            if (!HasValidPlayerName(out string playerNameError))
            {
                SetDiagnostics(playerNameError);
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
            RefreshConnectedPlayersText();
        }

        private void WireButtons()
        {
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
            serverEntries.Add(CCS_MultiplayerServerListEntry.CreateLocalhostDefault());

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
                emptyServerListText.text =
                    "No local hosts found. Ask a player to host, then refresh.";
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
            if (!TryResolveNetworkReferences(out NetworkManager manager, out UnityTransport resolvedTransport))
            {
                SetDiagnostics("Network manager or transport is missing.");
                return;
            }

            if (manager.IsListening)
            {
                SetDiagnostics("Already connected.");
                return;
            }

            CacheLocalPlayerName();
            pendingClientSceneSync = true;

            resolvedTransport.SetConnectionData(address, port);
            if (!CCS_NetcodeNetworkConfigValidationUtility.TryValidateForStart(manager, out string networkError))
            {
                pendingClientSceneSync = false;
                SetDiagnostics(networkError);
                return;
            }

            if (!manager.StartClient())
            {
                pendingClientSceneSync = false;
                SetDiagnostics("Failed to start client.");
                return;
            }

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
                    LogHostFlow($"NetworkSceneManager.LoadScene('{sceneName}', Single).");
                    networkManager.SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
                    return;
                }

                LogHostFlow($"Scene management disabled; SceneManager.LoadScene('{sceneName}', Single).");
                SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
                return;
            }

            LogHostFlow($"Client fallback SceneManager.LoadScene('{sceneName}', Single).");
            SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
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
        }

        private void HandleServerStarted()
        {
            RefreshConnectedPlayersText();
            LogHostFlow("OnServerStarted received.");
            TryLoadPendingHostScene();
        }

        private void HandleClientConnected(ulong clientId)
        {
            RefreshConnectedPlayersText();

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
                if (pendingClientSceneSync)
                {
                    pendingClientSceneSync = false;
                    SetDiagnostics("Connected. Entering character test...");
                }
            }
        }

        private void HandleClientDisconnected(ulong clientId)
        {
            if (networkManager != null && clientId == networkManager.LocalClientId)
            {
                pendingClientSceneSync = false;
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
            bool hasValidPlayerName = HasValidPlayerName(out _);
            bool isListening = networkManager != null && networkManager.IsListening;
            bool hasSelectedServer = selectedServerIndex >= 0 && selectedServerIndex < serverEntries.Count;

            if (hostAndStartButton != null)
            {
                hostAndStartButton.interactable = hasValidPlayerName && !isListening;
            }

            if (joinSelectedButton != null)
            {
                joinSelectedButton.interactable = hasValidPlayerName && hasSelectedServer && !isListening;
            }

            if (joinManualButton != null)
            {
                joinManualButton.interactable = hasValidPlayerName && !isListening;
            }

            if (refreshServersButton != null)
            {
                refreshServersButton.interactable = !isListening;
            }
        }

        private bool HasValidPlayerName(out string errorMessage)
        {
            string rawName = playerNameInput != null ? playerNameInput.text : string.Empty;
            if (string.IsNullOrWhiteSpace(rawName))
            {
                errorMessage = "Enter your player name before hosting or joining.";
                return false;
            }

            errorMessage = string.Empty;
            return true;
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
