using UnityEngine;
using UnityEngine.SceneManagement;

// =============================================================================
// SCRIPT: CCS_MasterTestNetworkSessionUtility
// CATEGORY: Modules / CharacterController / Tests / Runtime
// PURPOSE: Detects when the master test scene is running as an active Netcode session.
// PLACEMENT: Runtime static utility. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Used to gate network-only UI such as the join notification feed.
// =============================================================================

namespace CCS.Modules.CharacterController.Tests
{
    public static class CCS_MasterTestNetworkSessionUtility
    {
        #region Public Methods

        public static bool IsNetworkSessionActive()
        {
            return Unity.Netcode.NetworkManager.Singleton != null
                && Unity.Netcode.NetworkManager.Singleton.IsListening;
        }

        public static bool IsMasterTestSceneActive()
        {
            Scene activeScene = SceneManager.GetActiveScene();
            return activeScene.name == CCS_MasterTestUiConstants.MasterTestSceneName
                || activeScene.path == CCS_MasterTestUiConstants.MasterTestScenePath;
        }

        public static bool CanProcessJoinNotifications()
        {
            return IsNetworkSessionActive() && IsMasterTestSceneActive();
        }

        #endregion
    }
}
