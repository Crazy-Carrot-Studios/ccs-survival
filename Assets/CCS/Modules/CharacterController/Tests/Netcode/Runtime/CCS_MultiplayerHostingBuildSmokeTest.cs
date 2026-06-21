using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

// =============================================================================
// SCRIPT: CCS_MultiplayerHostingBuildSmokeTest
// CATEGORY: Modules / CharacterController / Tests / Netcode / Runtime
// PURPOSE: Optional build smoke test that auto-starts host from the hosting scene.
// PLACEMENT: Runtime bootstrap. Activated with -ccsNetcodeHostSmoke.
// AUTHOR: James Schilz
// CREATED: 2026-06-20
// NOTES: Used by batchmode/build validation only. Does not affect normal play.
// =============================================================================

namespace CCS.Modules.CharacterController.Tests.Netcode
{
    public sealed class CCS_MultiplayerHostingBuildSmokeTest : MonoBehaviour
    {
        private const string SmokeArgument = "-ccsNetcodeHostSmoke";
        private const float SmokeTimeoutSeconds = 8f;

        #region Public Methods

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        public static void BootstrapSmokeTest()
        {
            if (!IsSmokeTestRequested())
            {
                return;
            }

            GameObject runnerObject = new GameObject(nameof(CCS_MultiplayerHostingBuildSmokeTest));
            runnerObject.AddComponent<CCS_MultiplayerHostingBuildSmokeTest>();
            DontDestroyOnLoad(runnerObject);
        }

        #endregion

        #region Private Methods

        private static bool IsSmokeTestRequested()
        {
            string[] arguments = Environment.GetCommandLineArgs();
            for (int i = 0; i < arguments.Length; i++)
            {
                if (string.Equals(arguments[i], SmokeArgument, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        private IEnumerator Start()
        {
            Debug.Log("[Hosting Smoke] Build host smoke test started.");
            yield return null;

            float elapsedSeconds = 0f;
            while (SceneManager.GetActiveScene().name != CCS_NetcodeTestConstants.MultiplayerHostingSceneName
                   && elapsedSeconds < SmokeTimeoutSeconds)
            {
                elapsedSeconds += Time.unscaledDeltaTime;
                yield return null;
            }

            if (SceneManager.GetActiveScene().name != CCS_NetcodeTestConstants.MultiplayerHostingSceneName)
            {
                Debug.LogError(
                    $"[Hosting Smoke] Expected scene {CCS_NetcodeTestConstants.MultiplayerHostingSceneName} "
                    + $"but found {SceneManager.GetActiveScene().name}.");
                Quit(1);
                yield break;
            }

            NetworkManager manager = NetworkManager.Singleton != null
                ? NetworkManager.Singleton
                : FindAnyObjectByType<NetworkManager>();
            if (manager == null)
            {
                Debug.LogError("[Hosting Smoke] NetworkManager was not found.");
                Quit(1);
                yield break;
            }

            CCS_NetcodeRegistryUtility.TryLogNetworkConfigDiagnostics(manager);

            if (!CCS_HostingSceneBuildUtility.IsSceneInBuildSettings(CCS_NetcodeTestConstants.MasterTestSceneName))
            {
                Debug.LogError("[Hosting Smoke] Master Test scene is missing from Build Settings.");
                Quit(1);
                yield break;
            }

            if (!CCS_NetcodeNetworkConfigValidationUtility.TryValidateForStart(manager, out string networkError))
            {
                Debug.LogError("[Hosting Smoke] Network validation failed: " + networkError);
                Quit(1);
                yield break;
            }

            Debug.Log("[Hosting Flow] Calling StartHost");
            bool hostStarted = manager.StartHost();
            Debug.Log($"[Hosting Flow] StartHost returned: {hostStarted.ToString()}");

            if (hostStarted)
            {
                Debug.Log("[Hosting Smoke] StartHost returned: True");
                Quit(0);
                yield break;
            }

            Debug.LogError("[Hosting Smoke] StartHost returned: False");
            Quit(1);
        }

        private static void Quit(int exitCode)
        {
#if UNITY_EDITOR
            Debug.Log($"[Hosting Smoke] Smoke test complete. exitCode={exitCode}");
#else
            Application.Quit(exitCode);
#endif
        }

        #endregion
    }
}
