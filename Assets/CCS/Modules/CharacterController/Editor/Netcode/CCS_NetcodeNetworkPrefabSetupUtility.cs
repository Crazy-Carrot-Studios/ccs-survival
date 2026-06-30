using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using CCS.Modules.Attributes.Editor;
using CCS.Modules.CharacterController.Editor;
using CCS.Modules.Interaction.Editor;
using CCS.Modules.CharacterController.Netcode;
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
// NOTES: Prefab GameObject references are written via YAML root fileID repair only.
// =============================================================================

namespace CCS.Modules.CharacterController.Netcode.Editor
{
    public static class CCS_NetcodeNetworkPrefabSetupUtility
    {
        #region Public Methods

        public static bool RebuildNetworkPrefabSetup()
        {
            TryEnsureAIBanditPrefabViaReflection();

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

            if (!CCS_InteractionMasterTestBuilder.EnsureMasterTestPickupInteraction())
            {
                Debug.Log("[Netcode Setup] Master test interactable spawn controller already valid.");
            }

            if (!ValidatePlayerPrefabAsset(out string playerError))
            {
                Debug.LogError("[Netcode Setup] " + playerError);
                return false;
            }

            bool changed = false;
            changed |= WriteDefaultNetworkPrefabsListAsset();
            changed |= EnsureEmptyDefaultNetworkPrefabsListYaml();
            changed |= WriteTestNetworkPrefabsListAsset();
            changed |= WriteNetworkTestPrefabsRegistryAsset();
            changed |= WriteTestNetworkManagerPrefabAsset();

            if (changed)
            {
                AssetDatabase.SaveAssets();
            }

            bool yamlChanged = false;
            yamlChanged |= EnsureYamlPrefabRootReference(
                CCS_NetcodeConstants.NetworkManagerPrefabPath,
                "PlayerPrefab",
                CCS_NetcodeConstants.NetworkedPlayerPrefabPath);
            yamlChanged |= EnsureYamlNetworkPrefabsListEntries(
                CCS_NetcodeConstants.TestNetworkPrefabsListPath,
                CCS_NetcodeConstants.RequiredNetworkPrefabPaths);
            yamlChanged |= EnsureYamlPrefabRootReference(
                CCS_NetcodeConstants.NetworkManagerPrefabPath,
                "networkedPlayerPrefabFallback",
                CCS_NetcodeConstants.NetworkedPlayerPrefabPath);
            yamlChanged |= EnsureYamlPrefabRootReference(
                CCS_NetcodeConstants.NetworkManagerPrefabPath,
                "toggleInteractablePrefabFallback",
                CCS_NetcodeConstants.TestPickupInteractablePrefabPath);
            yamlChanged |= EnsureYamlNetworkPrefabsArray(
                CCS_NetcodeConstants.NetworkTestPrefabsRegistryPath,
                "networkPrefabs",
                CCS_NetcodeConstants.RequiredNetworkPrefabPaths);
            changed |= yamlChanged;
            changed |= RefreshHostingSceneNetworkManagerInstance();

            changed |= ForceImportNetworkPrefabAssets();

            if (yamlChanged)
            {
                AssetDatabase.Refresh();
            }

            if (EnsureEmptyDefaultNetworkPrefabsListYaml())
            {
                AssetDatabase.Refresh();
                changed = true;
            }

            LogNetworkPrefabRegistryDiagnostics();
            CCS_NetcodeRegistryAuditUtility.RunFullAudit();

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
                CCS_NetcodeConstants.NetworkedPlayerPrefabPath);
            if (playerPrefab == null)
            {
                errorMessage = "Missing player prefab: " + CCS_NetcodeConstants.NetworkedPlayerPrefabPath;
                return false;
            }

            if (playerPrefab.GetComponent<NetworkObject>() == null)
            {
                errorMessage =
                    CCS_NetcodeConstants.NetworkedPlayerPrefabPath + " is missing NetworkObject.";
                return false;
            }

            if (playerPrefab.GetComponent<CCS_ClientOwnerNetworkTransform>() == null)
            {
                errorMessage =
                    CCS_NetcodeConstants.NetworkedPlayerPrefabPath + " is missing CCS_ClientOwnerNetworkTransform.";
                return false;
            }

            NetworkTransform networkTransform = playerPrefab.GetComponent<NetworkTransform>();
            if (networkTransform == null || networkTransform.AuthorityMode != NetworkTransform.AuthorityModes.Owner)
            {
                errorMessage =
                    CCS_NetcodeConstants.NetworkedPlayerPrefabPath
                    + " must use owner-authoritative NetworkTransform.";
                return false;
            }

            if (playerPrefab.GetComponent<CCS_NetworkPlayerController>() == null)
            {
                errorMessage =
                    CCS_NetcodeConstants.NetworkedPlayerPrefabPath
                    + " is missing CCS_NetworkPlayerController.";
                return false;
            }

            if (playerPrefab.GetComponent<CCS_NetworkPlayerNameplate>() == null)
            {
                errorMessage =
                    CCS_NetcodeConstants.NetworkedPlayerPrefabPath
                    + " is missing CCS_NetworkPlayerNameplate.";
                return false;
            }

            return true;
        }

        private static bool WriteDefaultNetworkPrefabsListAsset()
        {
            return EnsureNetworkPrefabsListAsset(
                CCS_NetcodeConstants.DefaultNetworkPrefabsListPath,
                "DefaultNetworkPrefabs",
                isDefault: true,
                prefabPaths: System.Array.Empty<string>());
        }

        private static bool EnsureEmptyDefaultNetworkPrefabsListYaml()
        {
            string assetPath = CCS_NetcodeConstants.DefaultNetworkPrefabsListPath;
            if (!File.Exists(assetPath))
            {
                return false;
            }

            string yaml = File.ReadAllText(assetPath);
            if (yaml.Contains("  List: []"))
            {
                return false;
            }

            string pattern = @"  List:\r?\n(?:  - .*\r?\n(?:    .*\r?\n)*)*";
            string updatedYaml = Regex.Replace(yaml, pattern, "  List: []\r\n");
            if (updatedYaml == yaml)
            {
                return false;
            }

            File.WriteAllText(assetPath, updatedYaml);
            return true;
        }

        private static bool WriteTestNetworkPrefabsListAsset()
        {
            return EnsureNetworkPrefabsListAsset(
                CCS_NetcodeConstants.TestNetworkPrefabsListPath,
                "CCS_NetworkPrefabsList",
                isDefault: false,
                prefabPaths: CCS_NetcodeConstants.RequiredNetworkPrefabPaths);
        }

        private static bool EnsureNetworkPrefabsListAsset(
            string assetPath,
            string assetName,
            bool isDefault,
            string[] prefabPaths)
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

            string[] safePrefabPaths = prefabPaths ?? System.Array.Empty<string>();
            bool changed = created;
            int expectedCount = safePrefabPaths.Length;
            bool listHasEntries = list.PrefabList != null && list.PrefabList.Count > 0;
            if (entries.arraySize != expectedCount || (expectedCount == 0 && listHasEntries))
            {
                entries.arraySize = expectedCount;
                changed = true;
            }

            for (int i = 0; i < expectedCount; i++)
            {
                SerializedProperty entry = entries.GetArrayElementAtIndex(i);
                SerializedProperty prefabProperty = entry.FindPropertyRelative("Prefab");
                GameObject prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(safePrefabPaths[i]);
                if (prefabAsset == null)
                {
                    Debug.LogError("[Netcode Setup] Missing prefab for list entry: " + safePrefabPaths[i]);
                    continue;
                }

                if (prefabProperty != null && prefabProperty.objectReferenceValue != prefabAsset)
                {
                    prefabProperty.objectReferenceValue = prefabAsset;
                    changed = true;
                }

                SerializedProperty overrideProperty = entry.FindPropertyRelative("Override");
                SerializedProperty sourcePrefabProperty = entry.FindPropertyRelative("SourcePrefabToOverride");
                SerializedProperty sourceHashProperty = entry.FindPropertyRelative("SourceHashToOverride");
                SerializedProperty targetPrefabProperty = entry.FindPropertyRelative("OverridingTargetPrefab");

                if (overrideProperty != null && overrideProperty.enumValueIndex != 0)
                {
                    overrideProperty.enumValueIndex = 0;
                    changed = true;
                }

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
                AssetDatabase.SaveAssetIfDirty(list);
            }

            return changed;
        }

        private static bool EnsureYamlNetworkPrefabsListEntries(string assetPath, string[] prefabPaths)
        {
            if (!File.Exists(assetPath) || prefabPaths == null || prefabPaths.Length == 0)
            {
                return false;
            }

            StringBuilder listBuilder = new StringBuilder();
            listBuilder.AppendLine("  List:");
            for (int i = 0; i < prefabPaths.Length; i++)
            {
                string prefabGuid = AssetDatabase.AssetPathToGUID(prefabPaths[i]);
                if (string.IsNullOrEmpty(prefabGuid))
                {
                    Debug.LogError("[Netcode Setup] Missing prefab for list entry: " + prefabPaths[i]);
                    return false;
                }

                listBuilder.AppendLine("  - Override: 0");
                listBuilder.AppendLine(
                    $"    Prefab: {{fileID: 100100000, guid: {prefabGuid}, type: 3}}");
                listBuilder.AppendLine("    SourcePrefabToOverride: {fileID: 0}");
                listBuilder.AppendLine("    SourceHashToOverride: 0");
                listBuilder.AppendLine("    OverridingTargetPrefab: {fileID: 0}");
            }

            string yaml = File.ReadAllText(assetPath);
            string pattern = @"  List:\r?\n(?:  - .*\r?\n(?:    .*\r?\n)*)*";
            string replacement = listBuilder.ToString().TrimEnd() + "\r\n";
            string updatedYaml = Regex.Replace(yaml, pattern, replacement);
            if (updatedYaml == yaml)
            {
                return false;
            }

            File.WriteAllText(assetPath, updatedYaml);
            return true;
        }

        private static void LogNetworkPrefabRegistryDiagnostics()
        {
            string[] requiredPaths = CCS_NetcodeConstants.RequiredNetworkPrefabPaths;
            for (int i = 0; i < requiredPaths.Length; i++)
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(requiredPaths[i]);
                if (prefab == null)
                {
                    Debug.LogWarning($"[Netcode Registry] Registered prefab entry {i} is missing at {requiredPaths[i]}.");
                    continue;
                }

                NetworkObject networkObject = null;
                try
                {
                    networkObject = prefab.GetComponent<NetworkObject>();
                }
                catch (MissingReferenceException)
                {
                    Debug.LogWarning($"[Netcode Registry] Registered prefab entry {i} has a destroyed reference at {requiredPaths[i]}.");
                    continue;
                }

                uint hash = CCS_NetcodeNetworkObjectHashUtility.GetHash(networkObject);
                Debug.Log($"[Netcode Registry] Registered prefab: {prefab.name} path={requiredPaths[i]} hash={hash}");
            }

            if (!File.Exists(CCS_NetcodeConstants.MasterTestScenePath))
            {
                return;
            }

            UnityEngine.SceneManagement.Scene masterScene = EditorSceneManager.OpenScene(
                CCS_NetcodeConstants.MasterTestScenePath,
                OpenSceneMode.Additive);
            if (!masterScene.IsValid())
            {
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

                Debug.Log(
                    $"[Netcode Registry] Scene NetworkObject: {sceneObject.name} hash={CCS_NetcodeNetworkObjectHashUtility.GetHash(sceneObject)}");
            }

            EditorSceneManager.CloseScene(masterScene, removeScene: false);
        }

        private static bool WriteNetworkTestPrefabsRegistryAsset()
        {
            string assetPath = CCS_NetcodeConstants.NetworkTestPrefabsRegistryPath;
            bool created = false;
            CCS_NetworkPrefabsRegistry registry =
                AssetDatabase.LoadAssetAtPath<CCS_NetworkPrefabsRegistry>(assetPath);
            if (registry == null)
            {
                registry = ScriptableObject.CreateInstance<CCS_NetworkPrefabsRegistry>();
                string directory = Path.GetDirectoryName(assetPath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                AssetDatabase.CreateAsset(registry, assetPath);
                created = true;
            }

            SerializedObject serializedRegistry = new SerializedObject(registry);
            SerializedProperty prefabsProperty = serializedRegistry.FindProperty("networkPrefabs");
            bool referencesChanged = false;
            if (prefabsProperty != null)
            {
                string[] requiredPaths = CCS_NetcodeConstants.RequiredNetworkPrefabPaths;
                if (prefabsProperty.arraySize != requiredPaths.Length)
                {
                    prefabsProperty.arraySize = requiredPaths.Length;
                    referencesChanged = true;
                }

                for (int i = 0; i < requiredPaths.Length; i++)
                {
                    GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(requiredPaths[i]);
                    SerializedProperty element = prefabsProperty.GetArrayElementAtIndex(i);
                    if (prefab == null)
                    {
                        Debug.LogError("[Netcode Setup] Missing registry prefab: " + requiredPaths[i]);
                        continue;
                    }

                    if (element.objectReferenceValue != prefab)
                    {
                        element.objectReferenceValue = prefab;
                        referencesChanged = true;
                    }
                }
            }

            if (created || referencesChanged)
            {
                serializedRegistry.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(registry);
                AssetDatabase.SaveAssetIfDirty(registry);
            }

            bool yamlChanged = EnsureYamlNetworkPrefabsArray(
                assetPath,
                "networkPrefabs",
                CCS_NetcodeConstants.RequiredNetworkPrefabPaths);

            if (yamlChanged)
            {
                AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
            }

            if (created || yamlChanged || referencesChanged)
            {
                EditorUtility.SetDirty(registry);
            }

            return created || yamlChanged || referencesChanged;
        }

        private static bool EnsureYamlNetworkPrefabsArray(
            string assetPath,
            string fieldName,
            string[] prefabPaths)
        {
            if (!File.Exists(assetPath) || prefabPaths == null || prefabPaths.Length == 0)
            {
                return false;
            }

            StringBuilder arrayBuilder = new StringBuilder();
            arrayBuilder.AppendLine($"  {fieldName}:");
            for (int i = 0; i < prefabPaths.Length; i++)
            {
                string prefabGuid = AssetDatabase.AssetPathToGUID(prefabPaths[i]);
                if (string.IsNullOrEmpty(prefabGuid))
                {
                    Debug.LogError("[Netcode Setup] Missing prefab for registry entry: " + prefabPaths[i]);
                    return false;
                }

                arrayBuilder.AppendLine(
                    $"  - {{fileID: 100100000, guid: {prefabGuid}, type: 3}}");
            }

            string yaml = File.ReadAllText(assetPath);
            string pattern =
                $@"  {Regex.Escape(fieldName)}:\r?\n(?:  - .*\r?\n)*";
            string replacement = arrayBuilder.ToString().TrimEnd() + "\r\n";
            string updatedYaml = Regex.Replace(yaml, pattern, replacement);
            if (updatedYaml == yaml)
            {
                return false;
            }

            File.WriteAllText(assetPath, updatedYaml);
            return true;
        }

        private static bool WriteTestNetworkManagerPrefabAsset()
        {
            GameObject managerPrefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(
                CCS_NetcodeConstants.NetworkManagerPrefabPath);
            GameObject playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                CCS_NetcodeConstants.NetworkedPlayerPrefabPath);
            GameObject togglePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                CCS_NetcodeConstants.TestPickupInteractablePrefabPath);
            NetworkPrefabsList prefabsList = AssetDatabase.LoadAssetAtPath<NetworkPrefabsList>(
                CCS_NetcodeConstants.TestNetworkPrefabsListPath);

            if (managerPrefabAsset == null || playerPrefab == null || togglePrefab == null || prefabsList == null)
            {
                Debug.LogError("[Netcode Setup] Missing NetworkManager, player prefab, toggle prefab, or prefabs list asset.");
                return false;
            }

            NetworkManager networkManager = managerPrefabAsset.GetComponent<NetworkManager>();
            if (networkManager == null)
            {
                Debug.LogError("[Netcode Setup] PF_CCS_NetworkManager is missing NetworkManager.");
                return false;
            }

            bool changed = false;

            SerializedObject serializedManager = new SerializedObject(networkManager);
            SerializedProperty serializedNetworkConfig = serializedManager.FindProperty("NetworkConfig");
            if (serializedNetworkConfig == null)
            {
                Debug.LogError("[Netcode Setup] NetworkManager.NetworkConfig property was not found.");
                return false;
            }

            SerializedProperty playerPrefabProperty = serializedNetworkConfig.FindPropertyRelative("PlayerPrefab");
            if (playerPrefabProperty != null && playerPrefabProperty.objectReferenceValue != playerPrefab)
            {
                playerPrefabProperty.objectReferenceValue = playerPrefab;
                changed = true;
            }

            SerializedProperty enableSceneManagementProperty =
                serializedNetworkConfig.FindPropertyRelative("EnableSceneManagement");
            if (enableSceneManagementProperty != null && !enableSceneManagementProperty.boolValue)
            {
                enableSceneManagementProperty.boolValue = true;
                changed = true;
            }

            SerializedProperty forceSamePrefabsProperty =
                serializedNetworkConfig.FindPropertyRelative("ForceSamePrefabs");
            if (forceSamePrefabsProperty != null && forceSamePrefabsProperty.boolValue)
            {
                forceSamePrefabsProperty.boolValue = false;
                changed = true;
            }

            SerializedProperty prefabsProperty = serializedNetworkConfig.FindPropertyRelative("Prefabs");
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

            SerializedProperty oldPrefabListProperty = serializedNetworkConfig.FindPropertyRelative("OldPrefabList");
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

            changed |= EnsureNetworkPrefabReferenceGuard(managerPrefabAsset);

            return changed;
        }

        private static bool ForceImportNetworkPrefabAssets()
        {
            string[] assetPaths =
            {
                CCS_NetcodeConstants.NetworkManagerPrefabPath,
                CCS_NetcodeConstants.TestNetworkPrefabsListPath,
                CCS_NetcodeConstants.NetworkTestPrefabsRegistryPath,
                CCS_NetcodeConstants.NetworkedPlayerPrefabPath,
                CCS_NetcodeConstants.TestPickupInteractablePrefabPath,
                CCS_NetcodeConstants.AIBanditPrefabPath,
            };

            bool importedAny = false;
            for (int i = 0; i < assetPaths.Length; i++)
            {
                if (string.IsNullOrEmpty(assetPaths[i]) || !File.Exists(assetPaths[i]))
                {
                    continue;
                }

                AssetDatabase.ImportAsset(assetPaths[i], ImportAssetOptions.ForceUpdate);
                importedAny = true;
            }

            if (importedAny)
            {
                AssetDatabase.SaveAssets();
            }

            return importedAny;
        }

        private static bool RefreshHostingSceneNetworkManagerInstance()
        {
            Scene hostingScene = EditorSceneManager.OpenScene(
                CCS_NetcodeConstants.MultiplayerHostingScenePath,
                OpenSceneMode.Single);
            if (!hostingScene.IsValid())
            {
                Debug.LogError(
                    "[Netcode Setup] Could not open "
                    + CCS_NetcodeConstants.MultiplayerHostingScenePath);
                return false;
            }

            bool changed = false;
            GameObject networkManagerObject = GameObject.Find("PF_CCS_NetworkManager");
            if (networkManagerObject == null)
            {
                GameObject managerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                    CCS_NetcodeConstants.NetworkManagerPrefabPath);
                if (managerPrefab == null)
                {
                    Debug.LogError(
                        "[Netcode Setup] Missing prefab: "
                        + CCS_NetcodeConstants.NetworkManagerPrefabPath);
                    return false;
                }

                networkManagerObject = PrefabUtility.InstantiatePrefab(managerPrefab, hostingScene) as GameObject;
                if (networkManagerObject == null)
                {
                    Debug.LogError("[Netcode Setup] Failed to instantiate PF_CCS_NetworkManager.");
                    return false;
                }

                networkManagerObject.name = "PF_CCS_NetworkManager";
                changed = true;
            }

            if (PrefabUtility.IsPartOfPrefabInstance(networkManagerObject))
            {
                PrefabUtility.UnpackPrefabInstance(
                    networkManagerObject,
                    PrefabUnpackMode.Completely,
                    InteractionMode.AutomatedAction);
                changed = true;
            }

            changed |= ApplySceneNetworkManagerSerializedReferences(networkManagerObject);
            WireHostingMenuNetworkReferences(networkManagerObject);

            if (changed)
            {
                EditorSceneManager.MarkSceneDirty(hostingScene);
                EditorSceneManager.SaveScene(hostingScene);
            }

            return changed;
        }

        private static bool ApplySceneNetworkManagerSerializedReferences(GameObject networkManagerObject)
        {
            if (networkManagerObject == null)
            {
                return false;
            }

            GameObject playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                CCS_NetcodeConstants.NetworkedPlayerPrefabPath);
            GameObject togglePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                CCS_NetcodeConstants.TestPickupInteractablePrefabPath);
            NetworkPrefabsList prefabsList = AssetDatabase.LoadAssetAtPath<NetworkPrefabsList>(
                CCS_NetcodeConstants.TestNetworkPrefabsListPath);
            if (playerPrefab == null || togglePrefab == null || prefabsList == null)
            {
                Debug.LogError("[Netcode Setup] Missing scene wiring assets for NetworkManager.");
                return false;
            }

            NetworkManager networkManager = networkManagerObject.GetComponent<NetworkManager>();
            if (networkManager == null)
            {
                Debug.LogError("[Netcode Setup] Scene PF_CCS_NetworkManager is missing NetworkManager.");
                return false;
            }

            bool changed = false;

            CCS_NetworkPrefabReferenceGuard guard = networkManagerObject.GetComponent<CCS_NetworkPrefabReferenceGuard>();
            if (guard == null)
            {
                guard = networkManagerObject.AddComponent<CCS_NetworkPrefabReferenceGuard>();
                changed = true;
            }

            SerializedObject serializedManager = new SerializedObject(networkManager);
            SerializedProperty serializedNetworkConfig = serializedManager.FindProperty("NetworkConfig");
            if (serializedNetworkConfig == null)
            {
                Debug.LogError("[Netcode Setup] Scene NetworkManager.NetworkConfig property was not found.");
                return false;
            }

            SerializedProperty playerPrefabProperty = serializedNetworkConfig.FindPropertyRelative("PlayerPrefab");
            if (playerPrefabProperty != null && playerPrefabProperty.objectReferenceValue != playerPrefab)
            {
                playerPrefabProperty.objectReferenceValue = playerPrefab;
                changed = true;
            }

            SerializedProperty enableSceneManagementProperty =
                serializedNetworkConfig.FindPropertyRelative("EnableSceneManagement");
            if (enableSceneManagementProperty != null && !enableSceneManagementProperty.boolValue)
            {
                enableSceneManagementProperty.boolValue = true;
                changed = true;
            }

            SerializedProperty forceSamePrefabsProperty =
                serializedNetworkConfig.FindPropertyRelative("ForceSamePrefabs");
            if (forceSamePrefabsProperty != null && forceSamePrefabsProperty.boolValue)
            {
                forceSamePrefabsProperty.boolValue = false;
                changed = true;
            }

            SerializedProperty prefabsProperty = serializedNetworkConfig.FindPropertyRelative("Prefabs");
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

            SerializedProperty oldPrefabListProperty = serializedNetworkConfig.FindPropertyRelative("OldPrefabList");
            if (oldPrefabListProperty != null && oldPrefabListProperty.arraySize > 0)
            {
                oldPrefabListProperty.arraySize = 0;
                changed = true;
            }

            if (guard != null)
            {
                SerializedObject serializedGuard = new SerializedObject(guard);
                SerializedProperty playerFallbackProperty =
                    serializedGuard.FindProperty("networkedPlayerPrefabFallback");
                SerializedProperty toggleFallbackProperty =
                    serializedGuard.FindProperty("toggleInteractablePrefabFallback");
                if (playerFallbackProperty != null && playerFallbackProperty.objectReferenceValue != playerPrefab)
                {
                    playerFallbackProperty.objectReferenceValue = playerPrefab;
                    changed = true;
                }

                if (toggleFallbackProperty != null && toggleFallbackProperty.objectReferenceValue != togglePrefab)
                {
                    toggleFallbackProperty.objectReferenceValue = togglePrefab;
                    changed = true;
                }

                if (changed)
                {
                    serializedGuard.ApplyModifiedPropertiesWithoutUndo();
                }
            }

            if (changed)
            {
                serializedManager.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(networkManager);
            }

            return changed;
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
                errorMessage = "PF_CCS_NetworkManager is missing NetworkManager.";
                return false;
            }

            GameObject playerPrefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(
                CCS_NetcodeConstants.NetworkedPlayerPrefabPath);
            NetworkPrefabsList prefabsListAsset = AssetDatabase.LoadAssetAtPath<NetworkPrefabsList>(
                CCS_NetcodeConstants.TestNetworkPrefabsListPath);
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

            if (!PrefabReferenceResolvesWithNetworkObject(networkConfig.PlayerPrefab)
                || AssetDatabase.GetAssetPath(networkConfig.PlayerPrefab)
                    != CCS_NetcodeConstants.NetworkedPlayerPrefabPath)
            {
                errorMessage = "NetworkConfig.PlayerPrefab is missing, destroyed, or not a project prefab asset.";
                return false;
            }

            if (networkConfig.Prefabs == null
                || networkConfig.Prefabs.NetworkPrefabsLists == null
                || networkConfig.Prefabs.NetworkPrefabsLists.Count != 1
                || networkConfig.Prefabs.NetworkPrefabsLists[0] != prefabsListAsset)
            {
                errorMessage = "NetworkConfig.Prefabs.NetworkPrefabsLists is not wired to CCS_NetworkPrefabsList.";
                return false;
            }

            if (prefabsListAsset.PrefabList == null
                || prefabsListAsset.PrefabList.Count != CCS_NetcodeConstants.RequiredNetworkPrefabPaths.Length)
            {
                errorMessage = "CCS_NetworkPrefabsList does not contain all required network prefab entries.";
                return false;
            }

            if (!NetworkPrefabsListContainsRequiredEntries(prefabsListAsset, out string listError))
            {
                errorMessage = listError;
                return false;
            }

            if (networkManagerObject.GetComponent<CCS_NetworkPrefabReferenceGuard>() == null)
            {
                errorMessage = "PF_CCS_NetworkManager is missing CCS_NetworkPrefabReferenceGuard.";
                return false;
            }

            if (PrefabUtility.IsPartOfPrefabInstance(networkManagerObject))
            {
                errorMessage =
                    "PF_CCS_NetworkManager must be unpacked in SCN_CCS_MultiplayerHosting for build-safe network prefab references.";
                return false;
            }

            return true;
        }

        private static bool NetworkPrefabsListContainsRequiredEntries(
            NetworkPrefabsList prefabsList,
            out string errorMessage)
        {
            errorMessage = string.Empty;
            if (prefabsList?.PrefabList == null)
            {
                errorMessage = "CCS_NetworkPrefabsList.PrefabList is null.";
                return false;
            }

            string[] requiredPaths = CCS_NetcodeConstants.RequiredNetworkPrefabPaths;
            for (int i = 0; i < requiredPaths.Length; i++)
            {
                bool found = false;
                for (int entryIndex = 0; entryIndex < prefabsList.PrefabList.Count; entryIndex++)
                {
                    GameObject prefab = prefabsList.PrefabList[entryIndex].Prefab;
                    if (!PrefabReferenceResolvesWithNetworkObject(prefab))
                    {
                        continue;
                    }

                    if (AssetDatabase.GetAssetPath(prefab) == requiredPaths[i])
                    {
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    errorMessage = $"CCS_NetworkPrefabsList is missing required prefab: {requiredPaths[i]}";
                    return false;
                }
            }

            return true;
        }

        private static bool PrefabReferenceResolvesWithNetworkObject(GameObject prefabReference)
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

        private static bool EnsureNetworkPrefabReferenceGuard(GameObject managerPrefabAsset)
        {
            CCS_NetworkPrefabReferenceGuard guard =
                managerPrefabAsset.GetComponent<CCS_NetworkPrefabReferenceGuard>();
            bool changed = false;
            if (guard == null)
            {
                guard = managerPrefabAsset.AddComponent<CCS_NetworkPrefabReferenceGuard>();
                changed = true;
            }

            GameObject playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                CCS_NetcodeConstants.NetworkedPlayerPrefabPath);
            GameObject togglePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                CCS_NetcodeConstants.TestPickupInteractablePrefabPath);
            GameObject banditPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                CCS_NetcodeConstants.AIBanditPrefabPath);
            SerializedObject serializedGuard = new SerializedObject(guard);
            SerializedProperty playerProperty = serializedGuard.FindProperty("networkedPlayerPrefabFallback");
            SerializedProperty toggleProperty = serializedGuard.FindProperty("toggleInteractablePrefabFallback");
            SerializedProperty banditProperty = serializedGuard.FindProperty("aiBanditPrefabFallback");
            if (playerProperty != null && playerProperty.objectReferenceValue != playerPrefab)
            {
                playerProperty.objectReferenceValue = playerPrefab;
                changed = true;
            }

            if (toggleProperty != null && toggleProperty.objectReferenceValue != togglePrefab)
            {
                toggleProperty.objectReferenceValue = togglePrefab;
                changed = true;
            }

            if (banditProperty != null && banditProperty.objectReferenceValue != banditPrefab)
            {
                banditProperty.objectReferenceValue = banditPrefab;
                changed = true;
            }

            if (changed)
            {
                serializedGuard.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(guard);
                PrefabUtility.SavePrefabAsset(managerPrefabAsset);
            }

            bool yamlChanged = false;
            yamlChanged |= EnsureYamlPrefabRootReference(
                CCS_NetcodeConstants.NetworkManagerPrefabPath,
                "networkedPlayerPrefabFallback",
                CCS_NetcodeConstants.NetworkedPlayerPrefabPath);
            yamlChanged |= EnsureYamlPrefabRootReference(
                CCS_NetcodeConstants.NetworkManagerPrefabPath,
                "toggleInteractablePrefabFallback",
                CCS_NetcodeConstants.TestPickupInteractablePrefabPath);

            return changed || yamlChanged;
        }

        private static void TryEnsureAIBanditPrefabViaReflection()
        {
            System.Type builderType = System.Type.GetType(
                "CCS.Modules.AI.Editor.CCS_AIBanditPrefabBuilder, CCS.Modules.AI.Editor");
            if (builderType == null)
            {
                return;
            }

            System.Reflection.MethodInfo ensureMethod = builderType.GetMethod(
                "EnsureAIBanditPrefab",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            if (ensureMethod == null)
            {
                return;
            }

            ensureMethod.Invoke(null, null);
        }

        private static void WireHostingMenuNetworkReferences(GameObject networkManagerInstance)
        {
            CCS_MultiplayerHostingMenu menu = Object.FindAnyObjectByType<CCS_MultiplayerHostingMenu>();
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
