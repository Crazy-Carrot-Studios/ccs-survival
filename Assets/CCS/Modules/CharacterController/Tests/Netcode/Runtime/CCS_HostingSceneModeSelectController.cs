using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// =============================================================================
// SCRIPT: CCS_HostingSceneModeSelectController
// CATEGORY: Modules / CharacterController / Tests / Netcode / Runtime
// PURPOSE: Hosting scene entry flow — Mode Select before the multiplayer network panel.
// PLACEMENT: SCN_CCS_MultiplayerHosting Canvas alongside CCS_MultiplayerHostingMenu.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Single Player loads Master Test offline. Multiplayer reveals existing Host/Join UI.
// =============================================================================

namespace CCS.Modules.CharacterController.Tests.Netcode
{
    public sealed class CCS_HostingSceneModeSelectController : MonoBehaviour
    {
        #region Variables

        [Header("Panels")]
        [SerializeField] private GameObject modeSelectPanel;

        [SerializeField] private GameObject networkingPanel;

        [Header("Mode Select")]
        [SerializeField] private Button singlePlayerButton;

        [SerializeField] private Button multiplayerButton;

        [SerializeField] private Button quitButton;

        [Header("Networking")]
        [SerializeField] private Button backButton;

        [Header("Network")]
        [SerializeField] private NetworkManager networkManager;

        #endregion

        #region Unity Callbacks

        private void Awake()
        {
            WireButtons();
            ShowModeSelect();
        }

        #endregion

        #region Public Methods

        public void StartSinglePlayer()
        {
            OnSinglePlayerClicked();
        }

        public void OnSinglePlayerClicked()
        {
            EnsureNetworkIsNotListening();
            CCS_LocalMultiplayerPlayerNameCache.Clear();
            SceneManager.LoadScene(CCS_NetcodeTestConstants.MasterTestSceneName, LoadSceneMode.Single);
        }

        public void OnMultiplayerClicked()
        {
            ShowNetworkingPanel();
        }

        public void OnBackClicked()
        {
            if (IsNetworkSessionActive())
            {
                return;
            }

            ShowModeSelect();
        }

        public void OnQuitClicked()
        {
            CCS_HostingApplicationQuitUtility.QuitApplication(networkManager);
        }

        public void ShowModeSelect()
        {
            if (modeSelectPanel != null)
            {
                modeSelectPanel.SetActive(true);
            }

            if (networkingPanel != null)
            {
                networkingPanel.SetActive(false);
            }
        }

        public void ShowNetworkingPanel()
        {
            if (modeSelectPanel != null)
            {
                modeSelectPanel.SetActive(false);
            }

            if (networkingPanel != null)
            {
                networkingPanel.SetActive(true);
            }
        }

        #endregion

        #region Private Methods

        private void WireButtons()
        {
            if (singlePlayerButton != null)
            {
                singlePlayerButton.onClick.AddListener(OnSinglePlayerClicked);
            }

            if (multiplayerButton != null)
            {
                multiplayerButton.onClick.AddListener(OnMultiplayerClicked);
            }

            if (backButton != null)
            {
                backButton.onClick.AddListener(OnBackClicked);
            }

            if (quitButton != null)
            {
                quitButton.onClick.AddListener(OnQuitClicked);
            }
        }

        private void EnsureNetworkIsNotListening()
        {
            NetworkManager manager = networkManager != null ? networkManager : NetworkManager.Singleton;
            if (manager != null && manager.IsListening)
            {
                manager.Shutdown();
            }
        }

        private bool IsNetworkSessionActive()
        {
            NetworkManager manager = networkManager != null ? networkManager : NetworkManager.Singleton;
            return manager != null && manager.IsListening;
        }

        #endregion
    }
}
