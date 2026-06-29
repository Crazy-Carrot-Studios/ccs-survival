using System.Collections;
using CCS.Modules.CharacterController;
using UnityEngine;
using UnityEngine.SceneManagement;

// =============================================================================
// SCRIPT: CCS_MasterTestPlayerOfflineBootstrapper
// CATEGORY: Modules / CharacterController / Tests / Runtime / Managers
// PURPOSE: Master Test scene-level offline player bootstrap (replaces prefab bootstrap).
// PLACEMENT: CCS_TestingManager on SCN_CCS_CharacterController_MasterTest only.
// AUTHOR: James Schilz
// CREATED: 2026-06-29
// NOTES: No-ops outside Master Test or when Netcode is active. Does not affect hosting flow.
// =============================================================================

namespace CCS.Modules.CharacterController.Tests
{
    [DefaultExecutionOrder(-140)]
    public sealed class CCS_MasterTestPlayerOfflineBootstrapper : MonoBehaviour
    {
        #region Variables

        [SerializeField] private CCS_TestPlayerDisplayProfile displayProfile;

        [SerializeField] private bool enableBootstrap = true;

        private bool attemptedBootstrap;

        #endregion

        #region Unity Callbacks

        private void Start()
        {
            if (!enableBootstrap || !IsMasterTestSceneContext())
            {
                return;
            }

            StartCoroutine(TryBootstrapWhenPlayerReady());
        }

        #endregion

        #region Private Methods

        private static bool IsMasterTestSceneContext()
        {
            return SceneManager.GetActiveScene().name == CCS_MasterTestUiConstants.MasterTestSceneName;
        }

        private IEnumerator TryBootstrapWhenPlayerReady()
        {
            const int maxFrames = 180;
            for (int frame = 0; frame < maxFrames; frame++)
            {
                if (attemptedBootstrap || CCS_MasterTestNetworkSessionUtility.IsNetworkSessionActive())
                {
                    yield break;
                }

                GameObject playerRoot = FindSpawnedTestPlayerRoot();
                if (playerRoot != null)
                {
                    attemptedBootstrap = true;
                    CCS_CharacterCameraController sceneCamera = ResolveSceneCameraController();
                    CCS_TestPlayerDisplayProfile profile = ResolveDisplayProfile();
                    bool configured = CCS_TestPlayerLocalSessionConfigurator.TryConfigureOfflinePlayer(
                        playerRoot,
                        profile,
                        sceneCamera);
                    LogVerbose(
                        configured
                            ? "Configured offline test player from scene bootstrapper."
                            : "Offline bootstrapper found player but configuration was already applied or skipped.");
                    yield break;
                }

                yield return null;
            }

            LogVerbose("Timed out waiting for spawned test player for offline bootstrap.");
        }

        private static GameObject FindSpawnedTestPlayerRoot()
        {
            GameObject[] roots = SceneManager.GetActiveScene().GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                GameObject root = roots[i];
                if (root != null && root.name == CCS_TestPlayerPrefabConstants.NetworkedPlayerInstanceName)
                {
                    return root;
                }
            }

            return null;
        }

        private CCS_TestPlayerDisplayProfile ResolveDisplayProfile()
        {
            if (displayProfile != null)
            {
                return displayProfile;
            }

            CCS_MasterTestSpawnController spawnController = FindFirstObjectByType<CCS_MasterTestSpawnController>();
            if (spawnController != null && spawnController.DisplayProfile != null)
            {
                return spawnController.DisplayProfile;
            }

            return null;
        }

        private static CCS_CharacterCameraController ResolveSceneCameraController()
        {
            CCS_CharacterCameraController[] cameraControllers =
                FindObjectsByType<CCS_CharacterCameraController>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (int i = 0; i < cameraControllers.Length; i++)
            {
                CCS_CharacterCameraController candidate = cameraControllers[i];
                if (candidate == null || candidate.GetComponentInParent<CCS_CharacterMotor>() != null)
                {
                    continue;
                }

                return candidate;
            }

            return null;
        }

        private static void LogVerbose(string message)
        {
            CCS_CharacterControllerTestingManager manager = CCS_CharacterControllerTestingManager.ActiveInstance;
            if (manager != null && manager.EnableVerboseLogs)
            {
                Debug.Log("[Master Test Offline Bootstrapper] " + message);
            }
        }

        #endregion
    }
}
