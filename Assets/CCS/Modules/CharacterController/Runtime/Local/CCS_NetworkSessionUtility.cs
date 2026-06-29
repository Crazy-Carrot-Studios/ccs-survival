using CCS.Modules.CharacterController.Validation;
using UnityEngine;
using UnityEngine.SceneManagement;

// =============================================================================
// SCRIPT: CCS_NetworkSessionUtility
// CATEGORY: Modules / CharacterController / Tests / Runtime
// PURPOSE: Detects when the master test scene is running as an active Netcode session.
// PLACEMENT: Runtime static utility. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Used to gate network-only UI such as the join notification feed.
// =============================================================================

namespace CCS.Modules.CharacterController.Local {
    public static class CCS_NetworkSessionUtility
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
            return activeScene.name == CCS_ValidationUiConstants.MasterTestSceneName
                || activeScene.path == CCS_ValidationUiConstants.MasterTestScenePath;
        }

        public static bool CanProcessJoinNotifications()
        {
            return IsNetworkSessionActive() && IsMasterTestSceneActive();
        }

        #endregion
    }
}
