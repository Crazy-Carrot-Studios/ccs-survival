using Unity.Netcode;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_NetcodeRegistryUtility
// CATEGORY: Modules / CharacterController / Tests / Netcode / Runtime
// PURPOSE: Optional NetworkConfig diagnostics for host/join troubleshooting.
// PLACEMENT: Runtime utility. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-19
// NOTES: Diagnostics never throw and never load Resources registry at runtime.
// =============================================================================

namespace CCS.Modules.CharacterController.Tests.Netcode
{
    public static class CCS_NetcodeRegistryUtility
    {
        #region Public Methods

        public static void TryLogNetworkConfigDiagnostics(NetworkManager networkManager)
        {
            try
            {
                LogNetworkConfigDiagnostics(networkManager);
            }
            catch (System.Exception exception)
            {
                Debug.LogWarning(
                    $"[Netcode Registry] Diagnostic skipped due to invalid entry: {exception.Message}");
            }
        }

        public static void LogRegisteredPrefabs(NetworkManager networkManager)
        {
            try
            {
                LogRegisteredPrefabsInternal(networkManager);
            }
            catch (System.Exception exception)
            {
                Debug.LogWarning(
                    $"[Netcode Registry] Diagnostic skipped due to invalid entry: {exception.Message}");
            }
        }

        public static bool TryFindRegisteredPrefabHash(NetworkManager networkManager, uint hash, out string prefabName)
        {
            prefabName = string.Empty;
            if (networkManager == null || networkManager.NetworkConfig?.Prefabs?.NetworkPrefabsLists == null)
            {
                return false;
            }

            try
            {
                for (int listIndex = 0; listIndex < networkManager.NetworkConfig.Prefabs.NetworkPrefabsLists.Count; listIndex++)
                {
                    NetworkPrefabsList prefabsList = networkManager.NetworkConfig.Prefabs.NetworkPrefabsLists[listIndex];
                    if (prefabsList?.PrefabList == null)
                    {
                        continue;
                    }

                    for (int entryIndex = 0; entryIndex < prefabsList.PrefabList.Count; entryIndex++)
                    {
                        GameObject prefab = prefabsList.PrefabList[entryIndex].Prefab;
                        if (!CCS_NetworkTestPrefabsRegistry.TryResolvePrefab(
                                prefab,
                                out GameObject validPrefab,
                                out NetworkObject networkObject))
                        {
                            continue;
                        }

                        if (CCS_NetcodeNetworkObjectHashUtility.GetHash(networkObject) == hash)
                        {
                            prefabName = validPrefab.name;
                            return true;
                        }
                    }
                }
            }
            catch (System.Exception exception)
            {
                Debug.LogWarning(
                    $"[Netcode Registry] Diagnostic skipped due to invalid entry: {exception.Message}");
                return false;
            }

            Debug.LogWarning($"[Netcode Registry] Missing hash: {hash}");
            return false;
        }

#if UNITY_EDITOR
        public static void LogEditorRegistryDiagnostics()
        {
            try
            {
                CCS_NetworkTestPrefabsRegistry registry = Resources.Load<CCS_NetworkTestPrefabsRegistry>(
                    CCS_NetcodeTestConstants.NetworkTestPrefabsRegistryResourceName);
                LogEditorRegistryDiagnostics(registry);
            }
            catch (System.Exception exception)
            {
                Debug.LogWarning(
                    $"[Netcode Registry] Editor registry diagnostic skipped: {exception.Message}");
            }
        }

        public static void LogEditorRegistryDiagnostics(CCS_NetworkTestPrefabsRegistry registry)
        {
            if (!IsAliveUnityObject(registry))
            {
                Debug.LogWarning("[Netcode Registry] Editor registry asset is missing.");
                return;
            }

            int entryCount;
            try
            {
                entryCount = registry.Count;
            }
            catch (System.Exception exception)
            {
                Debug.LogWarning(
                    $"[Netcode Registry] Diagnostic skipped due to invalid entry: {exception.Message}");
                return;
            }

            for (int i = 0; i < entryCount; i++)
            {
                GameObject candidate = null;
                try
                {
                    candidate = registry.GetPrefab(i);
                }
                catch (System.Exception exception)
                {
                    Debug.LogWarning(
                        $"[Netcode Registry] Diagnostic skipped due to invalid entry at index {i}: {exception.Message}");
                    continue;
                }

                if (CCS_NetworkTestPrefabsRegistry.TryResolvePrefab(
                        candidate,
                        out GameObject prefab,
                        out NetworkObject networkObject))
                {
                    Debug.Log(
                        $"[Netcode Registry] Editor registry prefab[{i}]={prefab.name} "
                        + $"hash={CCS_NetcodeNetworkObjectHashUtility.GetHash(networkObject)}");
                }
                else
                {
                    Debug.LogWarning($"[Netcode Registry] Skipped invalid registry entry at index {i}");
                }
            }
        }
#endif

        #endregion

        #region Private Methods

        private static void LogNetworkConfigDiagnostics(NetworkManager networkManager)
        {
            if (networkManager == null || networkManager.NetworkConfig == null)
            {
                Debug.LogWarning("[Netcode Registry] NetworkManager or NetworkConfig is missing.");
                return;
            }

            LogRegisteredPrefabsInternal(networkManager);

            int requiredCount = CCS_NetcodeTestConstants.RequiredNetworkPrefabPaths.Length;
            int registeredCount = CountRegisteredPrefabs(networkManager);
            Debug.Log(
                $"[Netcode Registry] Required prefab count={requiredCount} registered prefab count={registeredCount} "
                + $"EnableSceneManagement={networkManager.NetworkConfig.EnableSceneManagement.ToString()} "
                + $"activeScene={UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}");

            if (CCS_NetworkTestPrefabsRegistry.TryResolvePrefab(
                    networkManager.NetworkConfig.PlayerPrefab,
                    out GameObject playerPrefab,
                    out NetworkObject playerNetworkObject))
            {
                Debug.Log(
                    $"[Netcode Registry] Player prefab={playerPrefab.name} "
                    + $"hash={CCS_NetcodeNetworkObjectHashUtility.GetHash(playerNetworkObject)}");
            }
            else
            {
                Debug.LogWarning(
                    "[Netcode Registry] Player prefab is null, destroyed, or missing NetworkObject.");
            }

            int sceneCount = UnityEngine.SceneManagement.SceneManager.sceneCountInBuildSettings;
            for (int sceneIndex = 0; sceneIndex < sceneCount; sceneIndex++)
            {
                string scenePath = UnityEngine.SceneManagement.SceneUtility.GetScenePathByBuildIndex(sceneIndex);
                Debug.Log($"[Netcode Registry] Build scene[{sceneIndex}]={scenePath}");
            }
        }

        private static void LogRegisteredPrefabsInternal(NetworkManager networkManager)
        {
            if (networkManager == null || networkManager.NetworkConfig == null)
            {
                Debug.LogWarning("[Netcode Registry] NetworkManager or NetworkConfig is missing.");
                return;
            }

            NetworkPrefabs prefabs = networkManager.NetworkConfig.Prefabs;
            if (prefabs == null || prefabs.NetworkPrefabsLists == null)
            {
                Debug.LogWarning("[Netcode Registry] NetworkPrefabsLists is missing.");
                return;
            }

            for (int listIndex = 0; listIndex < prefabs.NetworkPrefabsLists.Count; listIndex++)
            {
                NetworkPrefabsList prefabsList = prefabs.NetworkPrefabsLists[listIndex];
                if (prefabsList == null || prefabsList.PrefabList == null)
                {
                    continue;
                }

                for (int entryIndex = 0; entryIndex < prefabsList.PrefabList.Count; entryIndex++)
                {
                    GameObject prefab = prefabsList.PrefabList[entryIndex].Prefab;
                    if (!CCS_NetworkTestPrefabsRegistry.TryResolvePrefab(
                            prefab,
                            out GameObject validPrefab,
                            out NetworkObject networkObject))
                    {
                        Debug.LogWarning(
                            $"[Netcode Registry] Diagnostic skipped due to invalid entry at index {entryIndex}");
                        continue;
                    }

                    uint hash = CCS_NetcodeNetworkObjectHashUtility.GetHash(networkObject);
                    Debug.Log($"[Netcode Registry] Registered prefab: {validPrefab.name} hash={hash}");
                }
            }
        }

        private static int CountRegisteredPrefabs(NetworkManager networkManager)
        {
            int registeredCount = 0;
            if (networkManager.NetworkConfig?.Prefabs?.NetworkPrefabsLists == null)
            {
                return registeredCount;
            }

            for (int listIndex = 0; listIndex < networkManager.NetworkConfig.Prefabs.NetworkPrefabsLists.Count; listIndex++)
            {
                NetworkPrefabsList list = networkManager.NetworkConfig.Prefabs.NetworkPrefabsLists[listIndex];
                if (list?.PrefabList != null)
                {
                    registeredCount += list.PrefabList.Count;
                }
            }

            return registeredCount;
        }

        private static bool IsAliveUnityObject(Object unityObject)
        {
            if (ReferenceEquals(unityObject, null))
            {
                return false;
            }

            return unityObject != null;
        }

        #endregion
    }
}
