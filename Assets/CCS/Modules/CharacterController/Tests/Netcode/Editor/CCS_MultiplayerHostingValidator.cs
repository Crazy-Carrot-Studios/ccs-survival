using System.Collections.Generic;
using System.IO;
using CCS.Modules.CharacterController;
using CCS.Modules.CharacterController.Tests;
using CCS.Modules.CharacterController.Tests.Netcode;
using CCS.Project;
using UnityEditor;
using UnityEditor.SceneManagement;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

// =============================================================================
// SCRIPT: CCS_MultiplayerHostingValidator
// CATEGORY: Modules / CharacterController / Tests / Netcode / Editor
// PURPOSE: Reports problems in SCN_CCS_MultiplayerHosting and network prefab wiring.
// PLACEMENT: Editor validation utility. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Report-only. Does not rebuild or auto-fix the scene.
// =============================================================================

namespace CCS.Modules.CharacterController.Tests.Netcode.Editor
{
    public static class CCS_MultiplayerHostingValidator
    {
        #region Public Methods

        public static CCS_SurvivalValidationResult ValidateHostingScene()
        {
            List<string> failures = new List<string>();

            AppendIfMissing(
                failures,
                File.Exists(CCS_NetcodeTestConstants.MultiplayerHostingScenePath),
                $"Missing asset: {CCS_NetcodeTestConstants.MultiplayerHostingScenePath}");

            AppendIfMissing(
                failures,
                File.Exists(CCS_NetcodeTestConstants.NetworkManagerPrefabPath),
                $"Missing asset: {CCS_NetcodeTestConstants.NetworkManagerPrefabPath}");

            AppendIfMissing(
                failures,
                File.Exists(CCS_NetcodeTestConstants.NetworkedPlayerPrefabPath),
                $"Missing asset: {CCS_NetcodeTestConstants.NetworkedPlayerPrefabPath}");

            AppendIfMissing(
                failures,
                File.Exists(CCS_NetcodeTestConstants.TestNetworkPrefabsListPath),
                $"Missing asset: {CCS_NetcodeTestConstants.TestNetworkPrefabsListPath}");

            ValidateDefaultNetworkPrefabsList(failures);
            ValidateNetworkManagerPrefab(failures);
            ValidateMasterTestNetworkSpawnPoints(failures);
            ValidateMasterTestDoesNotContainNetworkedPlayerPrefab(failures);
            ValidateHostingSceneContent(failures);

            if (failures.Count > 0)
            {
                return CCS_SurvivalValidationResult.Fail(string.Join(" ", failures));
            }

            return CCS_SurvivalValidationResult.Pass("Multiplayer hosting scene validated.");
        }

        #endregion

        #region Private Methods

        private static void ValidateDefaultNetworkPrefabsList(List<string> failures)
        {
            NetworkPrefabsList defaultList = AssetDatabase.LoadAssetAtPath<NetworkPrefabsList>(
                CCS_NetcodeTestConstants.DefaultNetworkPrefabsListPath);
            AppendIfMissing(
                failures,
                defaultList != null,
                $"Missing asset: {CCS_NetcodeTestConstants.DefaultNetworkPrefabsListPath}.");
            if (defaultList == null)
            {
                return;
            }

            AppendIfMissing(
                failures,
                defaultList.PrefabList.Count == 0,
                $"{CCS_NetcodeTestConstants.DefaultNetworkPrefabsListPath} must stay empty. Register test prefabs only in {CCS_NetcodeTestConstants.TestNetworkPrefabsListPath}.");
        }

        private static void ValidateNetworkManagerPrefab(List<string> failures)
        {
            GameObject managerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                CCS_NetcodeTestConstants.NetworkManagerPrefabPath);
            if (managerPrefab == null)
            {
                failures.Add($"Could not load {CCS_NetcodeTestConstants.NetworkManagerPrefabPath}.");
                return;
            }

            NetworkManager networkManager = managerPrefab.GetComponent<NetworkManager>();
            AppendIfMissing(
                failures,
                networkManager != null,
                $"{CCS_NetcodeTestConstants.NetworkManagerPrefabPath} is missing NetworkManager.");

            if (networkManager == null)
            {
                return;
            }

            GameObject playerPrefab = networkManager.NetworkConfig.PlayerPrefab;
            AppendIfMissing(
                failures,
                playerPrefab != null,
                $"{CCS_NetcodeTestConstants.NetworkManagerPrefabPath} NetworkConfig.PlayerPrefab is null.");

            if (playerPrefab != null)
            {
                string playerPath = AssetDatabase.GetAssetPath(playerPrefab);
                AppendIfMissing(
                    failures,
                    playerPath == CCS_NetcodeTestConstants.NetworkedPlayerPrefabPath,
                    $"{CCS_NetcodeTestConstants.NetworkManagerPrefabPath} must use {CCS_NetcodeTestConstants.NetworkedPlayerPrefabPath}.");
            }

            List<NetworkPrefabsList> prefabLists = networkManager.NetworkConfig.Prefabs.NetworkPrefabsLists;
            AppendIfMissing(
                failures,
                prefabLists != null && prefabLists.Count == 1 && prefabLists[0] != null,
                $"{CCS_NetcodeTestConstants.NetworkManagerPrefabPath} must reference exactly one NetworkPrefabsList.");

            if (prefabLists != null && prefabLists.Count == 1 && prefabLists[0] != null)
            {
                string listPath = AssetDatabase.GetAssetPath(prefabLists[0]);
                AppendIfMissing(
                    failures,
                    listPath == CCS_NetcodeTestConstants.TestNetworkPrefabsListPath,
                    $"{CCS_NetcodeTestConstants.NetworkManagerPrefabPath} must reference {CCS_NetcodeTestConstants.TestNetworkPrefabsListPath}.");
            }

            NetworkPrefabsList testList = AssetDatabase.LoadAssetAtPath<NetworkPrefabsList>(
                CCS_NetcodeTestConstants.TestNetworkPrefabsListPath);
            if (testList == null)
            {
                failures.Add($"Could not load {CCS_NetcodeTestConstants.TestNetworkPrefabsListPath}.");
                return;
            }

            AppendIfMissing(
                failures,
                testList.PrefabList.Count == 1,
                $"{CCS_NetcodeTestConstants.TestNetworkPrefabsListPath} must contain exactly one player prefab entry.");

            GameObject registeredPlayerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                CCS_NetcodeTestConstants.NetworkedPlayerPrefabPath);
            AppendIfMissing(
                failures,
                registeredPlayerPrefab != null && registeredPlayerPrefab.GetComponent<NetworkObject>() != null,
                $"{CCS_NetcodeTestConstants.NetworkedPlayerPrefabPath} must contain NetworkObject.");
            AppendIfMissing(
                failures,
                registeredPlayerPrefab != null
                && registeredPlayerPrefab.GetComponent<CCS_ClientOwnerNetworkTransform>() != null,
                $"{CCS_NetcodeTestConstants.NetworkedPlayerPrefabPath} must contain CCS_ClientOwnerNetworkTransform.");
            NetworkTransform registeredNetworkTransform = registeredPlayerPrefab != null
                ? registeredPlayerPrefab.GetComponent<NetworkTransform>()
                : null;
            AppendIfMissing(
                failures,
                registeredNetworkTransform != null
                && registeredNetworkTransform.AuthorityMode == NetworkTransform.AuthorityModes.Owner,
                $"{CCS_NetcodeTestConstants.NetworkedPlayerPrefabPath} NetworkTransform must use Owner authority mode.");
            AppendIfMissing(
                failures,
                registeredPlayerPrefab != null
                && registeredPlayerPrefab.GetComponent<CCS_ControllerTestNetworkPlayerBehaviour>() != null,
                $"{CCS_NetcodeTestConstants.NetworkedPlayerPrefabPath} must contain CCS_ControllerTestNetworkPlayerBehaviour.");
            AppendIfMissing(
                failures,
                registeredPlayerPrefab != null && registeredPlayerPrefab.GetComponent<CCS_NetworkPlayerNameplate>() != null,
                $"{CCS_NetcodeTestConstants.NetworkedPlayerPrefabPath} must contain CCS_NetworkPlayerNameplate.");
            AppendIfMissing(
                failures,
                registeredPlayerPrefab != null
                && registeredPlayerPrefab.GetComponent<CCS_TestPlayerOfflineBootstrap>() != null,
                $"{CCS_NetcodeTestConstants.NetworkedPlayerPrefabPath} must contain CCS_TestPlayerOfflineBootstrap.");

            if (registeredPlayerPrefab != null)
            {
                CCS_CharacterMotor motor = registeredPlayerPrefab.GetComponent<CCS_CharacterMotor>();
                CCS_SurvivalValidationResult jumpValidation =
                    CCS_CharacterControllerValidationUtility.ValidatePlayerJumpConfiguration(
                        motor,
                        CCS_NetcodeTestConstants.NetworkedPlayerPrefabPath);
                if (!jumpValidation.IsSuccess)
                {
                    failures.Add(jumpValidation.Message);
                }

                CCS_CharacterInputActionProvider inputProvider =
                    registeredPlayerPrefab.GetComponent<CCS_CharacterInputActionProvider>();
                if (inputProvider != null && inputProvider.InputActionsAsset != null)
                {
                    CCS_SurvivalValidationResult inputValidation =
                        CCS_CharacterControllerValidationUtility.ValidateInputActionsAsset(
                            inputProvider.InputActionsAsset);
                    if (!inputValidation.IsSuccess)
                    {
                        failures.Add(inputValidation.Message);
                    }
                }
            }

            AppendIfMissing(
                failures,
                UsesYamlPrefabRootReference(
                    CCS_NetcodeTestConstants.NetworkManagerPrefabPath,
                    "PlayerPrefab",
                    CCS_NetcodeTestConstants.NetworkedPlayerPrefabPath),
                $"{CCS_NetcodeTestConstants.NetworkManagerPrefabPath} PlayerPrefab must use prefab asset root reference (fileID 100100000).");

            string testListPrefabPath = GetNetworkPrefabListEntryPath(testList);
            AppendIfMissing(
                failures,
                testListPrefabPath == CCS_NetcodeTestConstants.NetworkedPlayerPrefabPath,
                $"{CCS_NetcodeTestConstants.TestNetworkPrefabsListPath} must reference {CCS_NetcodeTestConstants.NetworkedPlayerPrefabPath}.");
            AppendIfMissing(
                failures,
                UsesYamlPrefabRootReference(
                    CCS_NetcodeTestConstants.TestNetworkPrefabsListPath,
                    "Prefab",
                    CCS_NetcodeTestConstants.NetworkedPlayerPrefabPath),
                $"{CCS_NetcodeTestConstants.TestNetworkPrefabsListPath} Prefab must use prefab asset root reference (fileID 100100000).");

            AppendIfMissing(
                failures,
                managerPrefab.GetComponent<CCS_LocalMultiplayerTestLauncher>() != null,
                $"{CCS_NetcodeTestConstants.NetworkManagerPrefabPath} is missing CCS_LocalMultiplayerTestLauncher.");
        }

        private static void ValidateHostingSceneContent(List<string> failures)
        {
            if (!File.Exists(CCS_NetcodeTestConstants.MultiplayerHostingScenePath))
            {
                return;
            }

            UnityEngine.SceneManagement.Scene hostingScene = EditorSceneManager.OpenScene(
                CCS_NetcodeTestConstants.MultiplayerHostingScenePath,
                OpenSceneMode.Single);
            if (!hostingScene.IsValid())
            {
                failures.Add($"Could not open {CCS_NetcodeTestConstants.MultiplayerHostingScenePath}.");
                return;
            }

            AppendIfMissing(
                failures,
                GameObject.Find("PF_CCS_TestNetworkManager") != null,
                $"{CCS_NetcodeTestConstants.MultiplayerHostingScenePath} is missing PF_CCS_TestNetworkManager.");

            CCS_MultiplayerHostingMenu menu = Object.FindFirstObjectByType<CCS_MultiplayerHostingMenu>();
            AppendIfMissing(
                failures,
                menu != null,
                $"{CCS_NetcodeTestConstants.MultiplayerHostingScenePath} is missing CCS_MultiplayerHostingMenu.");

            if (menu != null)
            {
                SerializedObject serializedMenu = new SerializedObject(menu);
                AppendIfMissing(
                    failures,
                    serializedMenu.FindProperty("playerNameInput")?.objectReferenceValue != null,
                    "CCS_MultiplayerHostingMenu.playerNameInput is not assigned.");
                AppendIfMissing(
                    failures,
                    serializedMenu.FindProperty("hostAndStartButton")?.objectReferenceValue != null,
                    "CCS_MultiplayerHostingMenu.hostAndStartButton is not assigned.");
                AppendIfMissing(
                    failures,
                    serializedMenu.FindProperty("joinSelectedButton")?.objectReferenceValue != null,
                    "CCS_MultiplayerHostingMenu.joinSelectedButton is not assigned.");
                AppendIfMissing(
                    failures,
                    serializedMenu.FindProperty("refreshServersButton")?.objectReferenceValue != null,
                    "CCS_MultiplayerHostingMenu.refreshServersButton is not assigned.");
                AppendIfMissing(
                    failures,
                    serializedMenu.FindProperty("serverListContainer")?.objectReferenceValue != null,
                    "CCS_MultiplayerHostingMenu.serverListContainer is not assigned.");
                AppendIfMissing(
                    failures,
                    serializedMenu.FindProperty("emptyServerListText")?.objectReferenceValue != null,
                    "CCS_MultiplayerHostingMenu.emptyServerListText is not assigned.");
                AppendIfMissing(
                    failures,
                    serializedMenu.FindProperty("advancedManualJoinPanel")?.objectReferenceValue != null,
                    "CCS_MultiplayerHostingMenu.advancedManualJoinPanel is not assigned.");
                AppendIfMissing(
                    failures,
                    serializedMenu.FindProperty("exitButton")?.objectReferenceValue != null,
                    "CCS_MultiplayerHostingMenu.exitButton is not assigned.");
                AppendIfMissing(
                    failures,
                    serializedMenu.FindProperty("networkManager")?.objectReferenceValue != null,
                    "CCS_MultiplayerHostingMenu.networkManager is not assigned. Run Setup Multiplayer Hosting Scene.");
                AppendIfMissing(
                    failures,
                    serializedMenu.FindProperty("transport")?.objectReferenceValue != null,
                    "CCS_MultiplayerHostingMenu.transport is not assigned. Run Setup Multiplayer Hosting Scene.");

                ValidateHostingMenuFlow(failures);
                ValidateNetworkPrefabReferences(failures);
            }

            EventSystem eventSystem = Object.FindFirstObjectByType<EventSystem>();
            AppendIfMissing(
                failures,
                eventSystem != null,
                $"{CCS_NetcodeTestConstants.MultiplayerHostingScenePath} is missing EventSystem.");
            AppendIfMissing(
                failures,
                eventSystem != null && eventSystem.GetComponent<InputSystemUIInputModule>() != null,
                $"{CCS_NetcodeTestConstants.MultiplayerHostingScenePath} EventSystem must use InputSystemUIInputModule.");
            AppendIfMissing(
                failures,
                eventSystem == null || eventSystem.GetComponent<StandaloneInputModule>() == null,
                $"{CCS_NetcodeTestConstants.MultiplayerHostingScenePath} must not use StandaloneInputModule.");
        }

        private static void ValidateNetworkPrefabReferences(List<string> failures)
        {
            NetworkPrefabsList testList = AssetDatabase.LoadAssetAtPath<NetworkPrefabsList>(
                CCS_NetcodeTestConstants.TestNetworkPrefabsListPath);
            if (testList == null)
            {
                failures.Add($"Missing asset: {CCS_NetcodeTestConstants.TestNetworkPrefabsListPath}");
                return;
            }

            if (testList.PrefabList == null || testList.PrefabList.Count != 1)
            {
                failures.Add($"{CCS_NetcodeTestConstants.TestNetworkPrefabsListPath} must contain exactly one player prefab entry.");
                return;
            }

            GameObject registeredPrefab = testList.PrefabList[0].Prefab;
            if (registeredPrefab == null || !registeredPrefab)
            {
                failures.Add(
                    $"{CCS_NetcodeTestConstants.TestNetworkPrefabsListPath} player prefab reference is missing or destroyed. Run Setup Multiplayer Hosting Scene.");
                return;
            }

            if (registeredPrefab.GetComponent<NetworkObject>() == null)
            {
                failures.Add($"{CCS_NetcodeTestConstants.TestNetworkPrefabsListPath} registered prefab is missing NetworkObject.");
            }

            GameObject managerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                CCS_NetcodeTestConstants.NetworkManagerPrefabPath);
            NetworkManager networkManager = managerPrefab != null ? managerPrefab.GetComponent<NetworkManager>() : null;
            if (networkManager == null)
            {
                failures.Add($"{CCS_NetcodeTestConstants.NetworkManagerPrefabPath} is missing NetworkManager.");
                return;
            }

            GameObject playerPrefab = networkManager.NetworkConfig.PlayerPrefab;
            if (playerPrefab == null || !playerPrefab)
            {
                failures.Add(
                    $"{CCS_NetcodeTestConstants.NetworkManagerPrefabPath} PlayerPrefab reference is missing or destroyed. Run Setup Multiplayer Hosting Scene.");
            }
        }

        private static void ValidateHostingMenuFlow(List<string> failures)
        {
            const string menuPath =
                "Assets/CCS/Modules/CharacterController/Tests/Netcode/Runtime/CCS_MultiplayerHostingMenu.cs";
            if (File.Exists(menuPath))
            {
                string source = File.ReadAllText(menuPath);
                AppendIfMissing(
                    failures,
                    source.Contains("LoadMasterTestSceneAsNetworkSession"),
                    "CCS_MultiplayerHostingMenu must auto-load the character test scene after host/join.");
                AppendIfMissing(
                    failures,
                    source.Contains("OnHostAndStartClicked"),
                    "CCS_MultiplayerHostingMenu must expose Host & Start flow.");
                AppendIfMissing(
                    failures,
                    source.Contains("advancedManualJoinPanel"),
                    "CCS_MultiplayerHostingMenu must keep manual join behind an advanced panel.");
            }

            Transform hostingRoot = GameObject.Find("HostingUiRoot")?.transform;
            Transform content = hostingRoot != null ? hostingRoot.Find("Content") : null;
            AppendIfMissing(
                failures,
                hostingRoot != null && content != null,
                "SCN_CCS_MultiplayerHosting must contain HostingUiRoot/Content. Run Setup Multiplayer Hosting Scene.");

            if (content != null)
            {
                AppendIfMissing(
                    failures,
                    content.Find("NamePanel/PlayerNameInput") != null,
                    "Hosting UI must contain the player name field.");
                AppendIfMissing(
                    failures,
                    content.Find("HostJoinContainer/HostCard/HostAndStartButton") != null,
                    "Hosting UI must contain Host & Start.");
                AppendIfMissing(
                    failures,
                    content.Find("HostJoinContainer/JoinCard/JoinButtons/JoinSelectedButton") != null,
                    "Hosting UI must contain Join Selected.");
                AppendIfMissing(
                    failures,
                    content.Find("HostJoinContainer/JoinCard/JoinButtons/RefreshServersButton") != null,
                    "Hosting UI must contain Refresh.");
                AppendIfMissing(
                    failures,
                    content.Find("HostJoinContainer/JoinCard/ServerListScroll/Viewport/ServerListContainer") != null,
                    "Hosting UI must contain a server list container.");
                AppendIfMissing(
                    failures,
                    content.Find("AdvancedManualJoinPanel") != null,
                    "Hosting UI must contain Advanced Manual Join panel.");
                AppendIfMissing(
                    failures,
                    content.Find("HostJoinContainer/HostCard") != null
                        && content.Find("HostJoinContainer/JoinCard") != null,
                    "Hosting UI must contain side-by-side Host Game and Join Game cards.");
            }

            AppendIfMissing(
                failures,
                GameObject.Find("PlayerSetupPanel") == null && GameObject.Find("MainContentPanel") == null,
                "Hosting UI still contains legacy panels. Run Setup Multiplayer Hosting Scene.");

            Canvas hostingCanvas = Object.FindFirstObjectByType<Canvas>();
            if (hostingCanvas != null)
            {
                AppendIfMissing(
                    failures,
                    hostingCanvas.GetComponent<VerticalLayoutGroup>() == null
                        && hostingCanvas.GetComponent<HorizontalLayoutGroup>() == null,
                    "Canvas must not use a root Layout Group. Run Rebuild Multiplayer Hosting UI.");
            }

            CCS_MultiplayerHostingMenu menu = Object.FindFirstObjectByType<CCS_MultiplayerHostingMenu>();
            if (menu != null)
            {
                SerializedObject serializedMenu = new SerializedObject(menu);
                AppendIfMissing(
                    failures,
                    serializedMenu.FindProperty("hostAndStartButton")?.objectReferenceValue != null,
                    "CCS_MultiplayerHostingMenu.hostAndStartButton is not wired.");
                AppendIfMissing(
                    failures,
                    serializedMenu.FindProperty("joinSelectedButton")?.objectReferenceValue != null,
                    "CCS_MultiplayerHostingMenu.joinSelectedButton is not wired.");
            }
        }

        private static bool UsesYamlPrefabRootReference(
            string assetPath,
            string fieldName,
            string expectedPrefabPath)
        {
            if (!File.Exists(assetPath))
            {
                return false;
            }

            string prefabGuid = AssetDatabase.AssetPathToGUID(expectedPrefabPath);
            string expectedSnippet = $"{fieldName}: {{fileID: 100100000, guid: {prefabGuid}, type: 3}}";
            return File.ReadAllText(assetPath).Contains(expectedSnippet);
        }

        private static string GetNetworkPrefabListEntryPath(NetworkPrefabsList list)
        {
            if (list == null)
            {
                return string.Empty;
            }

            SerializedObject serializedList = new SerializedObject(list);
            SerializedProperty listProperty = serializedList.FindProperty("List");
            if (listProperty == null || listProperty.arraySize != 1)
            {
                return string.Empty;
            }

            Object prefabReference = listProperty
                .GetArrayElementAtIndex(0)
                .FindPropertyRelative("Prefab")
                .objectReferenceValue;
            return prefabReference != null ? AssetDatabase.GetAssetPath(prefabReference) : string.Empty;
        }

        private static void ValidateMasterTestNetworkSpawnPoints(List<string> failures)
        {
            if (!File.Exists(CCS_NetcodeTestConstants.MasterTestScenePath))
            {
                failures.Add($"Missing asset: {CCS_NetcodeTestConstants.MasterTestScenePath}");
                return;
            }

            UnityEngine.SceneManagement.Scene masterScene = EditorSceneManager.OpenScene(
                CCS_NetcodeTestConstants.MasterTestScenePath,
                OpenSceneMode.Additive);
            if (!masterScene.IsValid())
            {
                failures.Add(
                    $"Could not open {CCS_NetcodeTestConstants.MasterTestScenePath} for network spawn validation.");
                return;
            }

            Transform testPoints = GameObject.Find("TestPoints")?.transform;
            AppendIfMissing(
                failures,
                testPoints != null,
                $"{CCS_NetcodeTestConstants.MasterTestScenePath} must contain TestPoints for network spawns.");

            string[] spawnPointNames = CCS_NetcodeTestConstants.MasterTestSpawnPointObjectNames;
            for (int i = 0; i < spawnPointNames.Length; i++)
            {
                Transform spawnPoint = testPoints != null ? testPoints.Find(spawnPointNames[i]) : null;
                AppendIfMissing(
                    failures,
                    spawnPoint != null,
                    $"{CCS_NetcodeTestConstants.MasterTestScenePath} must contain {spawnPointNames[i]}.");

                if (spawnPoint != null)
                {
                    AppendIfMissing(
                        failures,
                        !CCS_MultiplayerTestSpawnUtility.IsNearWorldOrigin(spawnPoint.position),
                        $"{CCS_NetcodeTestConstants.MasterTestScenePath} spawn point {spawnPointNames[i]} must not be at world origin.");
                }
            }

            AppendIfMissing(
                failures,
                !CCS_MultiplayerTestSpawnUtility.IsNearWorldOrigin(
                    CCS_NetcodeTestConstants.MasterTestFallbackSpawnPosition),
                "Network spawn fallback position must not be world origin.");

            EditorSceneManager.CloseScene(masterScene, removeScene: false);
        }

        private static void ValidateMasterTestDoesNotContainNetworkedPlayerPrefab(List<string> failures)
        {
            if (!File.Exists(CCS_NetcodeTestConstants.MasterTestScenePath))
            {
                return;
            }

            UnityEngine.SceneManagement.Scene masterScene = EditorSceneManager.OpenScene(
                CCS_NetcodeTestConstants.MasterTestScenePath,
                OpenSceneMode.Additive);
            if (!masterScene.IsValid())
            {
                return;
            }

            GameObject networkedPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                CCS_NetcodeTestConstants.NetworkedPlayerPrefabPath);
            string networkedPrefabName = networkedPrefab != null ? networkedPrefab.name : string.Empty;
            if (!string.IsNullOrEmpty(networkedPrefabName))
            {
                GameObject[] sceneRoots = masterScene.GetRootGameObjects();
                for (int i = 0; i < sceneRoots.Length; i++)
                {
                    Transform[] transforms = sceneRoots[i].GetComponentsInChildren<Transform>(true);
                    for (int j = 0; j < transforms.Length; j++)
                    {
                        if (transforms[j] != null && transforms[j].name == networkedPrefabName)
                        {
                            failures.Add(
                                $"{CCS_NetcodeTestConstants.MasterTestScenePath} must not contain placed {networkedPrefabName} instances.");
                            EditorSceneManager.CloseScene(masterScene, removeScene: false);
                            return;
                        }
                    }
                }
            }

            EditorSceneManager.CloseScene(masterScene, removeScene: false);
        }

        private static void AppendIfMissing(List<string> failures, bool condition, string message)
        {
            if (!condition)
            {
                failures.Add(message);
            }
        }

        #endregion
    }
}
