using System.IO;
using System.Text.RegularExpressions;
using CCS.Modules.Attributes.Editor;
using CCS.Modules.CharacterController.Editor;
using CCS.Modules.Interaction.Editor;
using CCS.Modules.CharacterController.Tests.Netcode;
using UnityEditor;
using UnityEditor.SceneManagement;
using Unity.Netcode;
using Unity.Netcode.Components;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.SceneManagement;

// =============================================================================
// SCRIPT: CCS_NetcodeNetworkPrefabSetupUtility
// CATEGORY: Modules / CharacterController / Tests / Netcode / Editor
// PURPOSE: Clears and rebuilds NetworkManager / NetworkPrefabsList test wiring.
// PLACEMENT: Editor setup utility. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Uses SerializedObject wiring so prefab assets stay valid at runtime.
// =============================================================================

namespace CCS.Modules.CharacterController.Tests.Netcode.Editor
{
    public static class CCS_NetcodeNetworkPrefabSetupUtility
    {
        #region Public Methods

        public static bool RebuildNetworkPrefabSetup()
        {
            if (!CCS_CharacterControllerPlayerPrefabBuilder.EnsurePlayerPrefabs())
            {
                Debug.LogWarning("[Netcode Setup] Player prefab sync reported no changes or could not run.");
            }

            if (!CCS_AttributesAssetBuilder.EnsureAttributesAssets())
            {
                Debug.Log("[Netcode Setup] Attributes assets already valid.");
            }

            if (!CCS_AttributesTestPlayerPrefabBuilder.EnsureTestPlayerAttributes())
            {
                Debug.LogWarning("[Netcode Setup] Attributes prefab sync reported no changes or could not run.");
            }

            if (!CCS_InteractionTestPlayerPrefabBuilder.EnsureTestPlayerInteractionScanner())
            {
                Debug.LogWarning("[Netcode Setup] Interaction prefab sync reported no changes or could not run.");
            }

            if (!ValidatePlayerPrefabAsset(out string playerError))
            {
                Debug.LogError("[Netcode Setup] " + playerError);
                return false;
            }

            bool changed = false;
            changed |= WriteDefaultNetworkPrefabsListAsset();
            changed |= WriteTestNetworkPrefabsListAsset();
            changed |= WriteTestNetworkManagerPrefabAsset();

            if (changed)
            {
                AssetDatabase.SaveAssets();
            }

            bool yamlChanged = false;
            yamlChanged |= EnsureYamlPrefabRootReference(
                CCS_NetcodeTestConstants.NetworkManagerPrefabPath,
                "PlayerPrefab",
                CCS_NetcodeTestConstants.NetworkedPlayerPrefabPath);
            yamlChanged |= EnsureYamlPrefabRootReference(
                CCS_NetcodeTestConstants.TestNetworkPrefabsListPath,
                "Prefab",
                CCS_NetcodeTestConstants.NetworkedPlayerPrefabPath);
            yamlChanged |= EnsureYamlPrefabRootReference(
                CCS_NetcodeTestConstants.NetworkManagerPrefabPath,
                "networkedPlayerPrefabFallback",
                CCS_NetcodeTestConstants.NetworkedPlayerPrefabPath);
            changed |= yamlChanged;
            changed |= RefreshHostingSceneNetworkManagerInstance();

            if (yamlChanged)
            {
                AssetDatabase.Refresh();
            }

            if (changed)
            {
                Debug.Log("[Netcode Setup] Network prefab wiring rebuilt.");
            }
            else
            {
                Debug.Log("[Netcode Setup] Network prefab wiring already valid.");
            }

            return true;
        }

        #endregion

        #region Private Methods

        private static bool EnsureYamlPrefabRootReference(
            string assetPath,
            string fieldName,
            string prefabAssetPath)
        {
            if (!File.Exists(assetPath) || string.IsNullOrEmpty(prefabAssetPath))
            {
                return false;
            }

            string prefabGuid = AssetDatabase.AssetPathToGUID(prefabAssetPath);
            if (string.IsNullOrEmpty(prefabGuid))
            {
                return false;
            }

            string yaml = File.ReadAllText(assetPath);
            string pattern =
                $@"(\s*{Regex.Escape(fieldName)}: \{{)fileID: \d+, guid: {prefabGuid}, type: 3\}}";
            string replacement = $"$1fileID: 100100000, guid: {prefabGuid}, type: 3}}";
            string updatedYaml = Regex.Replace(yaml, pattern, replacement);
            if (updatedYaml == yaml)
            {
                return false;
            }

            File.WriteAllText(assetPath, updatedYaml);
            return true;
        }

        private static bool ValidatePlayerPrefabAsset(out string errorMessage)
        {
            errorMessage = string.Empty;
            GameObject playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                CCS_NetcodeTestConstants.NetworkedPlayerPrefabPath);
            if (playerPrefab == null)
            {
                errorMessage = "Missing player prefab: " + CCS_NetcodeTestConstants.NetworkedPlayerPrefabPath;
                return false;
            }

            if (playerPrefab.GetComponent<NetworkObject>() == null)
            {
                errorMessage =
                    CCS_NetcodeTestConstants.NetworkedPlayerPrefabPath + " is missing NetworkObject.";
                return false;
            }

            if (playerPrefab.GetComponent<CCS_ClientOwnerNetworkTransform>() == null)
            {
                errorMessage =
                    CCS_NetcodeTestConstants.NetworkedPlayerPrefabPath + " is missing CCS_ClientOwnerNetworkTransform.";
                return false;
            }

            NetworkTransform networkTransform = playerPrefab.GetComponent<NetworkTransform>();
            if (networkTransform == null || networkTransform.AuthorityMode != NetworkTransform.AuthorityModes.Owner)
            {
                errorMessage =
                    CCS_NetcodeTestConstants.NetworkedPlayerPrefabPath
                    + " must use owner-authoritative NetworkTransform.";
                return false;
            }

            if (playerPrefab.GetComponent<CCS_ControllerTestNetworkPlayerBehaviour>() == null)
            {
                errorMessage =
                    CCS_NetcodeTestConstants.NetworkedPlayerPrefabPath
                    + " is missing CCS_ControllerTestNetworkPlayerBehaviour.";
                return false;
            }

            if (playerPrefab.GetComponent<CCS_NetworkPlayerNameplate>() == null)
            {
                errorMessage =
                    CCS_NetcodeTestConstants.NetworkedPlayerPrefabPath
                    + " is missing CCS_NetworkPlayerNameplate.";
                return false;
            }

            return true;
        }

        private static bool WriteDefaultNetworkPrefabsListAsset()
        {
            return EnsureNetworkPrefabsListAsset(
                CCS_NetcodeTestConstants.DefaultNetworkPrefabsListPath,
                "DefaultNetworkPrefabs",
                isDefault: true,
                playerPrefab: null);
        }

        private static bool WriteTestNetworkPrefabsListAsset()
        {
            GameObject playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                CCS_NetcodeTestConstants.NetworkedPlayerPrefabPath);
            return EnsureNetworkPrefabsListAsset(
                CCS_NetcodeTestConstants.TestNetworkPrefabsListPath,
                "CCS_TestNetworkPrefabsList",
                isDefault: false,
                playerPrefab: playerPrefab);
        }

        private static bool EnsureNetworkPrefabsListAsset(
            string assetPath,
            string assetName,
            bool isDefault,
            GameObject playerPrefab)
        {
            NetworkPrefabsList list = AssetDatabase.LoadAssetAtPath<NetworkPrefabsList>(assetPath);
            bool created = false;
            if (list == null)
            {
                list = ScriptableObject.CreateInstance<NetworkPrefabsList>();
                list.name = assetName;
                string directory = Path.GetDirectoryName(assetPath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                AssetDatabase.CreateAsset(list, assetPath);
                created = true;
            }

            SerializedObject serializedList = new SerializedObject(list);
            SerializedProperty isDefaultProperty = serializedList.FindProperty("IsDefault");
            if (isDefaultProperty != null && isDefaultProperty.boolValue != isDefault)
            {
                isDefaultProperty.boolValue = isDefault;
            }

            SerializedProperty entries = serializedList.FindProperty("List");
            if (entries == null)
            {
                Debug.LogError("[Netcode Setup] NetworkPrefabsList.List property was not found.");
                return created;
            }

            bool changed = created;
            int expectedCount = playerPrefab != null ? 1 : 0;
            if (entries.arraySize != expectedCount)
            {
                entries.arraySize = expectedCount;
                changed = true;
            }

            if (playerPrefab != null)
            {
                SerializedProperty entry = entries.GetArrayElementAtIndex(0);
                SerializedProperty overrideProperty = entry.FindPropertyRelative("Override");
                SerializedProperty prefabProperty = entry.FindPropertyRelative("Prefab");
                SerializedProperty sourcePrefabProperty = entry.FindPropertyRelative("SourcePrefabToOverride");
                SerializedProperty sourceHashProperty = entry.FindPropertyRelative("SourceHashToOverride");
                SerializedProperty targetPrefabProperty = entry.FindPropertyRelative("OverridingTargetPrefab");

                if (overrideProperty != null && overrideProperty.enumValueIndex != 0)
                {
                    overrideProperty.enumValueIndex = 0;
                    changed = true;
                }

                changed |= ForceRebindPrefabReference(prefabProperty, playerPrefab);

                if (sourcePrefabProperty != null && sourcePrefabProperty.objectReferenceValue != null)
                {
                    sourcePrefabProperty.objectReferenceValue = null;
                    changed = true;
                }

                if (sourceHashProperty != null && sourceHashProperty.intValue != 0)
                {
                    sourceHashProperty.intValue = 0;
                    changed = true;
                }

                if (targetPrefabProperty != null && targetPrefabProperty.objectReferenceValue != null)
                {
                    targetPrefabProperty.objectReferenceValue = null;
                    changed = true;
                }
            }

            if (changed)
            {
                serializedList.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(list);
            }

            return changed;
        }

        private static bool WriteTestNetworkManagerPrefabAsset()
        {
            GameObject managerPrefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(
                CCS_NetcodeTestConstants.NetworkManagerPrefabPath);
            GameObject playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                CCS_NetcodeTestConstants.NetworkedPlayerPrefabPath);
            NetworkPrefabsList prefabsList = AssetDatabase.LoadAssetAtPath<NetworkPrefabsList>(
                CCS_NetcodeTestConstants.TestNetworkPrefabsListPath);

            if (managerPrefabAsset == null || playerPrefab == null || prefabsList == null)
            {
                Debug.LogError("[Netcode Setup] Missing NetworkManager, player prefab, or prefabs list asset.");
                return false;
            }

            NetworkManager networkManager = managerPrefabAsset.GetComponent<NetworkManager>();
            if (networkManager == null)
            {
                Debug.LogError("[Netcode Setup] PF_CCS_TestNetworkManager is missing NetworkManager.");
                return false;
            }

            bool changed = false;
            SerializedObject serializedManager = new SerializedObject(networkManager);
            SerializedProperty networkConfig = serializedManager.FindProperty("NetworkConfig");
            if (networkConfig == null)
            {
                Debug.LogError("[Netcode Setup] NetworkManager.NetworkConfig property was not found.");
                return false;
            }

            SerializedProperty playerPrefabProperty = networkConfig.FindPropertyRelative("PlayerPrefab");
            changed |= ForceRebindPrefabReference(playerPrefabProperty, playerPrefab);
            changed |= EnsureNetworkPrefabReferenceGuard(managerPrefabAsset, playerPrefab);

            SerializedProperty prefabsProperty = networkConfig.FindPropertyRelative("Prefabs");
            SerializedProperty prefabListsProperty = prefabsProperty?.FindPropertyRelative("NetworkPrefabsLists");
            if (prefabListsProperty != null)
            {
                if (prefabListsProperty.arraySize != 1
                    || prefabListsProperty.GetArrayElementAtIndex(0).objectReferenceValue != prefabsList)
                {
                    prefabListsProperty.arraySize = 1;
                    prefabListsProperty.GetArrayElementAtIndex(0).objectReferenceValue = prefabsList;
                    changed = true;
                }
            }

            SerializedProperty oldPrefabListProperty = networkConfig.FindPropertyRelative("OldPrefabList");
            if (oldPrefabListProperty != null && oldPrefabListProperty.arraySize > 0)
            {
                oldPrefabListProperty.arraySize = 0;
                changed = true;
            }

            if (changed)
            {
                serializedManager.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(networkManager);
                PrefabUtility.SavePrefabAsset(managerPrefabAsset);
            }

            return changed;
        }

        private static bool RefreshHostingSceneNetworkManagerInstance()
        {
            Scene hostingScene = EditorSceneManager.OpenScene(
                CCS_NetcodeTestConstants.MultiplayerHostingScenePath,
                OpenSceneMode.Single);
            if (!hostingScene.IsValid())
            {
                Debug.LogError(
                    "[Netcode Setup] Could not open "
                    + CCS_NetcodeTestConstants.MultiplayerHostingScenePath);
                return false;
            }

            GameObject existingManager = GameObject.Find("PF_CCS_TestNetworkManager");
            if (existingManager != null
                && SceneNetworkManagerReferencesAreValid(existingManager, out _))
            {
                WireHostingMenuNetworkReferences(existingManager);
                return false;
            }

            if (existingManager != null)
            {
                Object.DestroyImmediate(existingManager);
            }

            GameObject managerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                CCS_NetcodeTestConstants.NetworkManagerPrefabPath);
            if (managerPrefab == null)
            {
                Debug.LogError(
                    "[Netcode Setup] Missing prefab: "
                    + CCS_NetcodeTestConstants.NetworkManagerPrefabPath);
                return false;
            }

            GameObject instance = PrefabUtility.InstantiatePrefab(managerPrefab, hostingScene) as GameObject;
            if (instance == null)
            {
                Debug.LogError("[Netcode Setup] Failed to instantiate PF_CCS_TestNetworkManager.");
                return false;
            }

            instance.name = "PF_CCS_TestNetworkManager";
            WireHostingMenuNetworkReferences(instance);
            EditorSceneManager.MarkSceneDirty(hostingScene);
            EditorSceneManager.SaveScene(hostingScene);
            return true;
        }

        public static bool SceneNetworkManagerReferencesAreValid(
            GameObject networkManagerObject,
            out string errorMessage)
        {
            errorMessage = string.Empty;
            if (networkManagerObject == null)
            {
                errorMessage = "NetworkManager scene object is missing.";
                return false;
            }

            NetworkManager networkManager = networkManagerObject.GetComponent<NetworkManager>();
            if (networkManager == null)
            {
                errorMessage = "PF_CCS_TestNetworkManager is missing NetworkManager.";
                return false;
            }

            GameObject playerPrefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(
                CCS_NetcodeTestConstants.NetworkedPlayerPrefabPath);
            NetworkPrefabsList prefabsListAsset = AssetDatabase.LoadAssetAtPath<NetworkPrefabsList>(
                CCS_NetcodeTestConstants.TestNetworkPrefabsListPath);
            if (playerPrefabAsset == null || prefabsListAsset == null)
            {
                errorMessage = "Network player prefab or test prefabs list asset is missing.";
                return false;
            }

            NetworkConfig networkConfig = networkManager.NetworkConfig;
            if (networkConfig == null)
            {
                errorMessage = "NetworkManager.NetworkConfig is null.";
                return false;
            }

            if (!EditorPrefabReferenceIsValid(networkConfig.PlayerPrefab, playerPrefabAsset))
            {
                errorMessage = "NetworkConfig.PlayerPrefab is missing, destroyed, or not a project prefab asset.";
                return false;
            }

            if (networkConfig.Prefabs == null
                || networkConfig.Prefabs.NetworkPrefabsLists == null
                || networkConfig.Prefabs.NetworkPrefabsLists.Count != 1
                || networkConfig.Prefabs.NetworkPrefabsLists[0] != prefabsListAsset)
            {
                errorMessage = "NetworkConfig.Prefabs.NetworkPrefabsLists is not wired to CCS_TestNetworkPrefabsList.";
                return false;
            }

            if (prefabsListAsset.PrefabList == null
                || prefabsListAsset.PrefabList.Count != 1
                || !EditorPrefabReferenceIsValid(prefabsListAsset.PrefabList[0].Prefab, playerPrefabAsset))
            {
                errorMessage = "CCS_TestNetworkPrefabsList contains an invalid player prefab entry.";
                return false;
            }

            if (networkManagerObject.GetComponent<CCS_NetworkPrefabReferenceGuard>() == null)
            {
                errorMessage = "PF_CCS_TestNetworkManager is missing CCS_NetworkPrefabReferenceGuard.";
                return false;
            }

            return true;
        }

        private static bool EditorPrefabReferenceIsValid(GameObject prefabReference, GameObject expectedAsset)
        {
            if (prefabReference == null || expectedAsset == null)
            {
                return false;
            }

            if (!EditorUtility.IsPersistent(prefabReference) || prefabReference != expectedAsset)
            {
                return false;
            }

            return EditorPrefabHasNetworkObject(prefabReference);
        }

        private static bool ForceRebindPrefabReference(SerializedProperty prefabProperty, GameObject playerPrefab)
        {
            if (prefabProperty == null || playerPrefab == null)
            {
                return false;
            }

            if (prefabProperty.objectReferenceValue == playerPrefab
                && EditorPrefabHasNetworkObject(playerPrefab))
            {
                return false;
            }

            prefabProperty.objectReferenceValue = null;
            prefabProperty.objectReferenceValue = playerPrefab;
            return true;
        }

        private static bool EnsureNetworkPrefabReferenceGuard(GameObject managerPrefabAsset, GameObject playerPrefab)
        {
            CCS_NetworkPrefabReferenceGuard guard = managerPrefabAsset.GetComponent<CCS_NetworkPrefabReferenceGuard>();
            bool changed = false;
            if (guard == null)
            {
                guard = managerPrefabAsset.AddComponent<CCS_NetworkPrefabReferenceGuard>();
                changed = true;
            }

            SerializedObject serializedGuard = new SerializedObject(guard);
            SerializedProperty fallbackProperty = serializedGuard.FindProperty("networkedPlayerPrefabFallback");
            if (fallbackProperty != null && fallbackProperty.objectReferenceValue != playerPrefab)
            {
                fallbackProperty.objectReferenceValue = playerPrefab;
                changed = true;
            }

            if (changed)
            {
                serializedGuard.ApplyModifiedPropertiesWithoutUndo();
            }

            return changed;
        }

        private static bool EditorPrefabHasNetworkObject(GameObject prefabReference)
        {
            if (prefabReference == null)
            {
                return false;
            }

            try
            {
                return prefabReference.GetComponent<NetworkObject>() != null;
            }
            catch (MissingReferenceException)
            {
                return false;
            }
        }

        private static void WireHostingMenuNetworkReferences(GameObject networkManagerInstance)
        {
            CCS_MultiplayerHostingMenu menu = Object.FindFirstObjectByType<CCS_MultiplayerHostingMenu>();
            if (menu == null || networkManagerInstance == null)
            {
                return;
            }

            NetworkManager networkManager = networkManagerInstance.GetComponent<NetworkManager>();
            UnityTransport transport = networkManagerInstance.GetComponent<UnityTransport>();
            SerializedObject serializedMenu = new SerializedObject(menu);
            SerializedProperty managerProperty = serializedMenu.FindProperty("networkManager");
            SerializedProperty transportProperty = serializedMenu.FindProperty("transport");
            if (managerProperty != null)
            {
                managerProperty.objectReferenceValue = networkManager;
            }

            if (transportProperty != null)
            {
                transportProperty.objectReferenceValue = transport;
            }

            serializedMenu.ApplyModifiedPropertiesWithoutUndo();
        }

        #endregion
    }
}
