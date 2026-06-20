using Unity.Netcode;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

// =============================================================================
// SCRIPT: CCS_HostingApplicationQuitUtility
// CATEGORY: Modules / CharacterController / Tests / Netcode / Runtime
// PURPOSE: Shuts down test netcode sessions and exits the player or editor play mode.
// PLACEMENT: Runtime utility. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-19
// NOTES: Quit must not reload hosting UI or behave like Back.
// =============================================================================

namespace CCS.Modules.CharacterController.Tests.Netcode
{
    public static class CCS_HostingApplicationQuitUtility
    {
        #region Public Methods

        public static void QuitApplication(NetworkManager networkManager = null)
        {
            CCS_LocalMultiplayerPlayerNameCache.Clear();
            CCS_LocalMultiplayerHostSessionBeacon.StopBeacon();

            NetworkManager manager = networkManager != null ? networkManager : NetworkManager.Singleton;
            if (manager != null && manager.IsListening)
            {
                manager.Shutdown();
            }

#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        #endregion
    }
}
