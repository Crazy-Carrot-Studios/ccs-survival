using System.Collections.Generic;
using System.IO;
using CCS.Modules.CharacterController.Tests.Netcode;
using Unity.Netcode;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// =============================================================================
// SCRIPT: CCS_NetcodeRegistryAuditUtility
// CATEGORY: Modules / CharacterController / Tests / Netcode / Editor
// PURPOSE: Audits NetworkObject prefabs/scenes and maps runtime hashes for diagnostics.
// PLACEMENT: Editor utility. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-19
// NOTES: Logs use prefix [Netcode Registry Audit].
// =============================================================================

namespace CCS.Modules.CharacterController.Tests.Netcode.Editor
{
    public static class CCS_NetcodeRegistryAuditUtility
    {
        #region Public Methods

        public static void RunFullAudit()
        {
            Debug.Log("[Netcode Registry Audit] Starting full network object audit.");

            NetworkPrefabsList testList = AssetDatabase.LoadAssetAtPath<NetworkPrefabsList>(
                CCS_NetcodeTestConstants.TestNetworkPrefabsListPath);
            HashSet<string> registeredPaths = BuildRegisteredPathSet(testList);

            for (int i = 0; i < CCS_NetcodeTestConstants.RequiredNetworkPrefabPaths.Length; i++)
            {
                AuditPrefabAsset(
                    CCS_NetcodeTestConstants.RequiredNetworkPrefabPaths[i],
                    registeredPaths);
            }

            AuditSceneNetworkObjects(
                CCS_NetcodeTestConstants.MasterTestScenePath,
                registeredPaths,
                allowScenePlacedNetworkObjects: false);
            AuditSceneNetworkObjects(
                CCS_NetcodeTestConstants.MultiplayerHostingScenePath,
                registeredPaths,
                allowScenePlacedNetworkObjects: true);

            AuditHashTargets();
            Debug.Log("[Netcode Registry Audit] Audit complete.");
        }

        public static bool ValidateAuditRules(List<string> failures)
        {
            bool passed = true;
            NetworkPrefabsList testList = AssetDatabase.LoadAssetAtPath<NetworkPrefabsList>(
                CCS_NetcodeTestConstants.TestNetworkPrefabsListPath);
            HashSet<string> registeredPaths = BuildRegisteredPathSet(testList);

            for (int i = 0; i < CCS_NetcodeTestConstants.RequiredNetworkPrefabPaths.Length; i++)
            {
                string path = CCS_NetcodeTestConstants.RequiredNetworkPrefabPaths[i];
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (!PrefabHasSingleRootNetworkObject(prefab, out string prefabError))
                {
                    failures.Add(prefabError);
                    passed = false;
                }

                if (!registeredPaths.Contains(path))
                {
                    failures.Add($"{path} is not registered in {CCS_NetcodeTestConstants.TestNetworkPrefabsListPath}.");
                    passed = false;
                }
            }

            if (!SceneHasExpectedNetworkObjects(
                    CCS_NetcodeTestConstants.MasterTestScenePath,
                    allowScenePlacedNetworkObjects: false,
                    out string masterTestError))
            {
                failures.Add(masterTestError);
                passed = false;
            }

            return passed;
        }

        #endregion

        #region Private Methods

        private static HashSet<string> BuildRegisteredPathSet(NetworkPrefabsList testList)
        {
            HashSet<string> registeredPaths = new HashSet<string>();
            if (testList?.PrefabList == null)
            {
                return registeredPaths;
            }

            for (int i = 0; i < testList.PrefabList.Count; i++)
            {
                GameObject prefab = testList.PrefabList[i].Prefab;
                if (prefab == null)
                {
                    continue;
                }

                string path = AssetDatabase.GetAssetPath(prefab);
                if (!string.IsNullOrEmpty(path))
                {
                    registeredPaths.Add(path);
                }
            }

            return registeredPaths;
        }

        private static void AuditPrefabAsset(string assetPath, HashSet<string> registeredPaths)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (prefab == null)
            {
                Debug.LogWarning($"[Netcode Registry Audit] Missing prefab asset: {assetPath}");
                return;
            }

            NetworkObject[] networkObjects = prefab.GetComponentsInChildren<NetworkObject>(true);
            for (int i = 0; i < networkObjects.Length; i++)
            {
                NetworkObject networkObject = networkObjects[i];
                bool isRoot = networkObject.gameObject == prefab;
                uint hash = CCS_NetcodeNetworkObjectHashUtility.GetHash(networkObject);
                Debug.Log(
                    $"[Netcode Registry Audit] Prefab={prefab.name} path={assetPath} object={networkObject.name} "
                    + $"root={isRoot.ToString()} nested={(!isRoot).ToString()} active={networkObject.gameObject.activeSelf.ToString()} "
                    + $"hash={hash} registered={registeredPaths.Contains(assetPath).ToString()}");
            }
        }

        private static void AuditSceneNetworkObjects(
            string scenePath,
            HashSet<string> registeredPaths,
            bool allowScenePlacedNetworkObjects)
        {
            if (!File.Exists(scenePath))
            {
                Debug.LogWarning($"[Netcode Registry Audit] Missing scene: {scenePath}");
                return;
            }

            Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
            if (!scene.IsValid())
            {
                Debug.LogWarning($"[Netcode Registry Audit] Could not open scene: {scenePath}");
                return;
            }

            NetworkObject[] sceneObjects = Object.FindObjectsByType<NetworkObject>(
                FindObjectsInactive.Include);
            for (int i = 0; i < sceneObjects.Length; i++)
            {
                NetworkObject sceneObject = sceneObjects[i];
                if (sceneObject == null)
                {
                    continue;
                }

                GameObject sourcePrefab = PrefabUtility.GetCorrespondingObjectFromSource(sceneObject.gameObject);
                string sourcePath = sourcePrefab != null ? AssetDatabase.GetAssetPath(sourcePrefab) : string.Empty;
                uint hash = CCS_NetcodeNetworkObjectHashUtility.GetHash(sceneObject);
                bool registered = !string.IsNullOrEmpty(sourcePath) && registeredPaths.Contains(sourcePath);
                Debug.Log(
                    $"[Netcode Registry Audit] Scene={scene.name} object={sceneObject.name} path={sourcePath} "
                    + $"scenePlaced={(!string.IsNullOrEmpty(sourcePath)).ToString()} active={sceneObject.gameObject.activeSelf.ToString()} "
                    + $"hash={hash} registered={registered.ToString()} allowed={allowScenePlacedNetworkObjects.ToString()}");
            }

            EditorSceneManager.CloseScene(scene, removeScene: false);
        }

        private static void AuditHashTargets()
        {
            uint[] targetHashes = { 1858026642u, 2092802458u };
            for (int i = 0; i < targetHashes.Length; i++)
            {
                uint targetHash = targetHashes[i];
                bool mapped = false;

                for (int pathIndex = 0; pathIndex < CCS_NetcodeTestConstants.RequiredNetworkPrefabPaths.Length; pathIndex++)
                {
                    GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                        CCS_NetcodeTestConstants.RequiredNetworkPrefabPaths[pathIndex]);
                    NetworkObject networkObject = prefab != null ? prefab.GetComponent<NetworkObject>() : null;
                    if (networkObject != null
                        && CCS_NetcodeNetworkObjectHashUtility.GetHash(networkObject) == targetHash)
                    {
                        Debug.Log(
                            $"[Netcode Registry Audit] Hash {targetHash} maps to prefab asset {prefab.name} "
                            + $"at {CCS_NetcodeTestConstants.RequiredNetworkPrefabPaths[pathIndex]}");
                        mapped = true;
                    }
                }

                Scene masterScene = EditorSceneManager.OpenScene(
                    CCS_NetcodeTestConstants.MasterTestScenePath,
                    OpenSceneMode.Additive);
                if (masterScene.IsValid())
                {
                    NetworkObject[] sceneObjects = Object.FindObjectsByType<NetworkObject>(
                        FindObjectsInactive.Include);
                    for (int sceneIndex = 0; sceneIndex < sceneObjects.Length; sceneIndex++)
                    {
                        NetworkObject sceneObject = sceneObjects[sceneIndex];
                        if (sceneObject != null
                            && CCS_NetcodeNetworkObjectHashUtility.GetHash(sceneObject) == targetHash)
                        {
                            Debug.Log(
                                $"[Netcode Registry Audit] Hash {targetHash} maps to scene object {sceneObject.name} "
                                + $"in {CCS_NetcodeTestConstants.MasterTestScenePath}");
                            mapped = true;
                        }
                    }

                    EditorSceneManager.CloseScene(masterScene, removeScene: false);
                }

                if (!mapped)
                {
                    Debug.LogWarning(
                        $"[Netcode Registry Audit] Hash {targetHash} is not mapped to a current prefab asset or saved scene object. "
                        + "Treat as stale runtime/scene-sync hash until reproduced in play mode.");
                }
            }
        }

        private static bool PrefabHasSingleRootNetworkObject(GameObject prefabRoot, out string errorMessage)
        {
            errorMessage = string.Empty;
            if (prefabRoot == null)
            {
                errorMessage = "Network prefab asset is missing.";
                return false;
            }

            NetworkObject rootNetworkObject = prefabRoot.GetComponent<NetworkObject>();
            if (rootNetworkObject == null)
            {
                errorMessage = $"{AssetDatabase.GetAssetPath(prefabRoot)} is missing a root NetworkObject.";
                return false;
            }

            NetworkObject[] allNetworkObjects = prefabRoot.GetComponentsInChildren<NetworkObject>(true);
            if (allNetworkObjects.Length > 1)
            {
                errorMessage = $"{AssetDatabase.GetAssetPath(prefabRoot)} has nested NetworkObject components.";
                return false;
            }

            return true;
        }

        private static bool SceneHasExpectedNetworkObjects(
            string scenePath,
            bool allowScenePlacedNetworkObjects,
            out string errorMessage)
        {
            errorMessage = string.Empty;
            if (!File.Exists(scenePath))
            {
                errorMessage = $"Missing scene: {scenePath}";
                return false;
            }

            Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
            if (!scene.IsValid())
            {
                errorMessage = $"Could not open scene: {scenePath}";
                return false;
            }

            NetworkObject[] sceneObjects = Object.FindObjectsByType<NetworkObject>(
                FindObjectsInactive.Include);
            if (!allowScenePlacedNetworkObjects && sceneObjects.Length > 0)
            {
                errorMessage =
                    $"{scenePath} must not contain scene-placed NetworkObjects. Use runtime spawners instead.";
                EditorSceneManager.CloseScene(scene, removeScene: false);
                return false;
            }

            EditorSceneManager.CloseScene(scene, removeScene: false);
            return true;
        }

        #endregion
    }
}
