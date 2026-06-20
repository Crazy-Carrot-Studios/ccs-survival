using System.Collections.Generic;
using System.IO;
using CCS.Modules.CharacterController;
using CCS.Modules.CharacterController.Tests;
using CCS.Modules.CharacterController.Tests.Netcode;
using CCS.Project;
using TMPro;
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

            AppendIfMissing(
                failures,
                File.Exists(CCS_NetcodeTestConstants.NetworkPlayerPrefabRegistryPath),
                $"Missing asset: {CCS_NetcodeTestConstants.NetworkPlayerPrefabRegistryPath}");

            ValidateDefaultNetworkPrefabsList(failures);
            ValidateBuildSettingsScenes(failures);
            ValidateNetworkManagerPrefab(failures);
            ValidateMasterTestNetworkSpawnPoints(failures);
            ValidateMasterTestDoesNotContainNetworkedPlayerPrefab(failures);
            if (!CCS_NetcodeRegistryAuditUtility.ValidateAuditRules(failures))
            {
                Debug.LogWarning("[Validation] Netcode registry audit rules reported failures.");
            }

            ValidateHostingSceneContent(failures);

            if (failures.Count > 0)
            {
                return CCS_SurvivalValidationResult.Fail(string.Join(" ", failures));
            }

            return CCS_SurvivalValidationResult.Pass("Multiplayer hosting scene validated.");
        }

        #endregion

        #region Private Methods

        private static void ValidateBuildSettingsScenes(List<string> failures)
        {
            AppendIfMissing(
                failures,
                IsSceneEnabledInBuildSettings(CCS_NetcodeTestConstants.MultiplayerHostingScenePath),
                $"{CCS_NetcodeTestConstants.MultiplayerHostingScenePath} must be enabled in Build Settings.");
            AppendIfMissing(
                failures,
                IsSceneEnabledInBuildSettings(CCS_NetcodeTestConstants.MasterTestScenePath),
                $"{CCS_NetcodeTestConstants.MasterTestScenePath} must be enabled in Build Settings.");
            AppendIfMissing(
                failures,
                SceneNameMatchesAsset(
                    CCS_NetcodeTestConstants.MultiplayerHostingScenePath,
                    CCS_NetcodeTestConstants.MultiplayerHostingSceneName),
                $"Runtime scene name '{CCS_NetcodeTestConstants.MultiplayerHostingSceneName}' must match the hosting scene asset name.");
            AppendIfMissing(
                failures,
                SceneNameMatchesAsset(
                    CCS_NetcodeTestConstants.MasterTestScenePath,
                    CCS_NetcodeTestConstants.MasterTestSceneName),
                $"Runtime scene name '{CCS_NetcodeTestConstants.MasterTestSceneName}' must match the master test scene asset name.");
            AppendIfMissing(
                failures,
                CCS_NetcodeTestConstants.MasterTestSceneName == "SCN_CCS_CharacterController_MasterTest",
                "Runtime host scene name must be exactly SCN_CCS_CharacterController_MasterTest.");
        }

        private static bool IsSceneEnabledInBuildSettings(string scenePath)
        {
            EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
            for (int i = 0; i < scenes.Length; i++)
            {
                if (scenes[i].path == scenePath)
                {
                    return scenes[i].enabled;
                }
            }

            return false;
        }

        private static bool SceneNameMatchesAsset(string scenePath, string expectedSceneName)
        {
            if (!File.Exists(scenePath))
            {
                return false;
            }

            string assetName = Path.GetFileNameWithoutExtension(scenePath);
            return assetName == expectedSceneName;
        }

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
                YamlNetworkPrefabsListIsEmpty(CCS_NetcodeTestConstants.DefaultNetworkPrefabsListPath),
                $"{CCS_NetcodeTestConstants.DefaultNetworkPrefabsListPath} must stay empty. Register test prefabs only in {CCS_NetcodeTestConstants.TestNetworkPrefabsListPath}.");
        }

        private static bool YamlNetworkPrefabsListIsEmpty(string assetPath)
        {
            if (!File.Exists(assetPath))
            {
                return false;
            }

            return File.ReadAllText(assetPath).Contains("  List: []");
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
            AppendIfMissing(
                failures,
                EditorSerializedPrefabMatchesCanonical(
                    playerPrefab,
                    CCS_NetcodeTestConstants.NetworkedPlayerPrefabPath),
                $"{CCS_NetcodeTestConstants.NetworkManagerPrefabPath} NetworkConfig.PlayerPrefab is missing, destroyed, or lacks NetworkObject.");

            if (playerPrefab != null)
            {
                string playerPath = AssetDatabase.GetAssetPath(playerPrefab);
                AppendIfMissing(
                    failures,
                    playerPath == CCS_NetcodeTestConstants.NetworkedPlayerPrefabPath,
                    $"{CCS_NetcodeTestConstants.NetworkManagerPrefabPath} must use {CCS_NetcodeTestConstants.NetworkedPlayerPrefabPath}.");
            }

            AppendIfMissing(
                failures,
                networkManager.NetworkConfig.EnableSceneManagement,
                $"{CCS_NetcodeTestConstants.NetworkManagerPrefabPath} NetworkConfig.EnableSceneManagement must be enabled.");

            AppendIfMissing(
                failures,
                !networkManager.NetworkConfig.ForceSamePrefabs,
                $"{CCS_NetcodeTestConstants.NetworkManagerPrefabPath} NetworkConfig.ForceSamePrefabs must be disabled for editor/build local join tests.");

            CCS_NetworkPrefabReferenceGuard guard = managerPrefab.GetComponent<CCS_NetworkPrefabReferenceGuard>();
            AppendIfMissing(
                failures,
                guard != null,
                $"{CCS_NetcodeTestConstants.NetworkManagerPrefabPath} is missing CCS_NetworkPrefabReferenceGuard.");
            if (guard != null)
            {
                AppendIfMissing(
                    failures,
                    UsesYamlPrefabRootReference(
                        CCS_NetcodeTestConstants.NetworkManagerPrefabPath,
                        "networkedPlayerPrefabFallback",
                        CCS_NetcodeTestConstants.NetworkedPlayerPrefabPath),
                    $"{CCS_NetcodeTestConstants.NetworkManagerPrefabPath} networkedPlayerPrefabFallback must use prefab asset root reference (fileID 100100000).");
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
                testList.PrefabList.Count == CCS_NetcodeTestConstants.RequiredNetworkPrefabPaths.Length,
                $"{CCS_NetcodeTestConstants.TestNetworkPrefabsListPath} must contain {CCS_NetcodeTestConstants.RequiredNetworkPrefabPaths.Length} registered network prefab entries.");

            ValidateRequiredNetworkPrefabListEntries(failures, testList);

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

            ValidateRequiredNetworkPrefabYamlReferences(failures);

            CCS_NetworkTestPrefabsRegistry registry = AssetDatabase.LoadAssetAtPath<CCS_NetworkTestPrefabsRegistry>(
                CCS_NetcodeTestConstants.NetworkTestPrefabsRegistryPath);
            AppendIfMissing(
                failures,
                registry != null,
                $"Could not load {CCS_NetcodeTestConstants.NetworkTestPrefabsRegistryPath}.");
            if (registry != null)
            {
                string[] requiredPaths = CCS_NetcodeTestConstants.RequiredNetworkPrefabPaths;
                AppendIfMissing(
                    failures,
                    registry.Count == requiredPaths.Length,
                    $"{CCS_NetcodeTestConstants.NetworkTestPrefabsRegistryPath} must contain exactly {requiredPaths.Length} network prefabs.");

                int validEntryCount = 0;
                for (int i = 0; i < requiredPaths.Length; i++)
                {
                    GameObject registryPrefab = registry.GetPrefab(i);
                    bool entryValid = EditorSerializedPrefabMatchesCanonical(registryPrefab, requiredPaths[i]);
                    if (entryValid)
                    {
                        validEntryCount++;
                    }

                    AppendIfMissing(
                        failures,
                        entryValid,
                        $"{CCS_NetcodeTestConstants.NetworkTestPrefabsRegistryPath} networkPrefabs[{i}] is missing, destroyed, or lacks NetworkObject.");
                    AppendIfMissing(
                        failures,
                        registryPrefab != null
                        && AssetDatabase.GetAssetPath(registryPrefab) == requiredPaths[i],
                        $"{CCS_NetcodeTestConstants.NetworkTestPrefabsRegistryPath} networkPrefabs[{i}] must reference {requiredPaths[i]}.");
                }

                AppendIfMissing(
                    failures,
                    validEntryCount == requiredPaths.Length,
                    $"{CCS_NetcodeTestConstants.NetworkTestPrefabsRegistryPath} contains destroyed or null registry entries.");
            }

            AppendIfMissing(
                failures,
                managerPrefab.GetComponent<CCS_NetworkPrefabReferenceGuard>() != null,
                $"{CCS_NetcodeTestConstants.NetworkManagerPrefabPath} is missing CCS_NetworkPrefabReferenceGuard.");

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

            CCS_HostingSceneModeSelectController modeController =
                Object.FindFirstObjectByType<CCS_HostingSceneModeSelectController>();
            AppendIfMissing(
                failures,
                modeController != null,
                $"{CCS_NetcodeTestConstants.MultiplayerHostingScenePath} is missing CCS_HostingSceneModeSelectController.");

            if (menu != null)
            {
                SerializedObject serializedMenu = new SerializedObject(menu);
                AppendIfMissing(
                    failures,
                    serializedMenu.FindProperty("playerNameInput")?.objectReferenceValue != null,
                    "CCS_MultiplayerHostingMenu.playerNameInput is not assigned.");
                AppendIfMissing(
                    failures,
                    serializedMenu.FindProperty("playerNameWarningText")?.objectReferenceValue != null,
                    "CCS_MultiplayerHostingMenu.playerNameWarningText is not assigned.");
                AppendIfMissing(
                    failures,
                    serializedMenu.FindProperty("serverNameInput")?.objectReferenceValue != null,
                    "CCS_MultiplayerHostingMenu.serverNameInput is not assigned.");
                AppendIfMissing(
                    failures,
                    serializedMenu.FindProperty("serverNameWarningText")?.objectReferenceValue != null,
                    "CCS_MultiplayerHostingMenu.serverNameWarningText is not assigned.");
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
                    serializedMenu.FindProperty("quitButton")?.objectReferenceValue != null,
                    "CCS_MultiplayerHostingMenu.quitButton is not assigned.");
                AppendIfMissing(
                    failures,
                    serializedMenu.FindProperty("networkManager")?.objectReferenceValue != null,
                    "CCS_MultiplayerHostingMenu.networkManager is not assigned. Run Setup Multiplayer Hosting Scene.");
                AppendIfMissing(
                    failures,
                    serializedMenu.FindProperty("transport")?.objectReferenceValue != null,
                    "CCS_MultiplayerHostingMenu.transport is not assigned. Run Setup Multiplayer Hosting Scene.");

                InputField playerNameInput = serializedMenu.FindProperty("playerNameInput")?.objectReferenceValue as InputField;
                AppendIfMissing(
                    failures,
                    playerNameInput != null && playerNameInput.interactable,
                    "CCS_MultiplayerHostingMenu.playerNameInput must be assigned and interactable.");

                InputField serverNameInput = serializedMenu.FindProperty("serverNameInput")?.objectReferenceValue as InputField;
                AppendIfMissing(
                    failures,
                    serverNameInput != null && serverNameInput.interactable,
                    "CCS_MultiplayerHostingMenu.serverNameInput must be assigned and interactable.");

                ValidateHostingMenuFlow(failures);
                ValidateModeSelectFlow(failures, modeController);
                ValidateNetworkPrefabReferences(failures);
            }

            Canvas hostingCanvas = Object.FindFirstObjectByType<Canvas>();
            if (hostingCanvas != null)
            {
                AppendIfMissing(
                    failures,
                    hostingCanvas.GetComponent<GraphicRaycaster>() != null,
                    $"{CCS_NetcodeTestConstants.MultiplayerHostingScenePath} Canvas must have GraphicRaycaster.");
                AppendIfMissing(
                    failures,
                    hostingCanvas.GetComponent<VerticalLayoutGroup>() == null
                        && hostingCanvas.GetComponent<HorizontalLayoutGroup>() == null,
                    "Canvas must not use a root Layout Group. Run Rebuild Multiplayer Hosting UI.");
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

            if (testList.PrefabList == null
                || testList.PrefabList.Count != CCS_NetcodeTestConstants.RequiredNetworkPrefabPaths.Length)
            {
                failures.Add(
                    $"{CCS_NetcodeTestConstants.TestNetworkPrefabsListPath} must contain {CCS_NetcodeTestConstants.RequiredNetworkPrefabPaths.Length} registered network prefab entries.");
                return;
            }

            ValidateRequiredNetworkPrefabListEntries(failures, testList);

            GameObject registeredPrefab = testList.PrefabList[0].Prefab;
            GameObject expectedPlayerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                CCS_NetcodeTestConstants.NetworkedPlayerPrefabPath);
            if (!EditorPrefabHasNetworkObject(registeredPrefab)
                && EditorPrefabHasNetworkObject(expectedPlayerPrefab))
            {
                registeredPrefab = expectedPlayerPrefab;
            }

            if (!EditorPrefabHasNetworkObject(registeredPrefab))
            {
                failures.Add(
                    $"{CCS_NetcodeTestConstants.TestNetworkPrefabsListPath} player prefab reference is missing, destroyed, or lacks NetworkObject. Run Setup Multiplayer Hosting Scene.");
                return;
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
            if (!EditorPrefabHasNetworkObject(playerPrefab)
                && EditorPrefabHasNetworkObject(expectedPlayerPrefab))
            {
                playerPrefab = expectedPlayerPrefab;
            }

            if (!EditorPrefabHasNetworkObject(playerPrefab))
            {
                failures.Add(
                    $"{CCS_NetcodeTestConstants.NetworkManagerPrefabPath} PlayerPrefab reference is missing, destroyed, or lacks NetworkObject. Run Setup Multiplayer Hosting Scene.");
            }

            AppendIfMissing(
                failures,
                managerPrefab.GetComponent<CCS_NetworkPrefabReferenceGuard>() != null,
                $"{CCS_NetcodeTestConstants.NetworkManagerPrefabPath} must contain CCS_NetworkPrefabReferenceGuard.");
        }

        private static void ValidateModeSelectFlow(
            List<string> failures,
            CCS_HostingSceneModeSelectController modeController)
        {
            const string controllerPath =
                "Assets/CCS/Modules/CharacterController/Tests/Netcode/Runtime/CCS_HostingSceneModeSelectController.cs";
            if (File.Exists(controllerPath))
            {
                string source = File.ReadAllText(controllerPath);
                AppendIfMissing(
                    failures,
                    source.Contains("OnSinglePlayerClicked"),
                    "CCS_HostingSceneModeSelectController must expose Single Player flow.");
                AppendIfMissing(
                    failures,
                    source.Contains("OnMultiplayerClicked"),
                    "CCS_HostingSceneModeSelectController must expose Multiplayer flow.");
                AppendIfMissing(
                    failures,
                    source.Contains("EnsureNetworkIsNotListening"),
                    "CCS_HostingSceneModeSelectController must not start Netcode for Single Player.");
                AppendIfMissing(
                    failures,
                    source.Contains("MasterTestSceneName"),
                    "CCS_HostingSceneModeSelectController must load Master Test for Single Player.");
                AppendIfMissing(
                    failures,
                    source.Contains("OnQuitClicked"),
                    "CCS_HostingSceneModeSelectController must expose Quit flow.");
                AppendIfMissing(
                    failures,
                    source.Contains("CCS_HostingApplicationQuitUtility"),
                    "CCS_HostingSceneModeSelectController must quit through CCS_HostingApplicationQuitUtility.");
                AppendIfMissing(
                    failures,
                    !source.Contains("UnityEditor"),
                    "CCS_HostingSceneModeSelectController must not reference UnityEditor in runtime code.");
            }

            Transform hostingRoot = GameObject.Find("HostingUiRoot")?.transform;
            Transform modeSelectPanel = hostingRoot != null
                ? hostingRoot.Find(CCS_NetcodeTestConstants.ModeSelectPanelObjectName)
                : null;
            Transform networkingPanel = hostingRoot != null
                ? hostingRoot.Find(CCS_NetcodeTestConstants.NetworkingPanelObjectName)
                : null;
            Transform networkingCard = networkingPanel != null
                ? networkingPanel.Find(CCS_NetcodeTestConstants.NetworkingCardObjectName)
                : null;

            AppendIfMissing(
                failures,
                modeSelectPanel != null,
                "Hosting UI must contain Mode Select panel. Run Setup Multiplayer Hosting Scene.");
            AppendIfMissing(
                failures,
                networkingPanel != null && networkingCard != null,
                "Hosting UI must contain Networking panel/card. Run Setup Multiplayer Hosting Scene.");

            if (modeSelectPanel != null)
            {
                AppendIfMissing(
                    failures,
                    modeSelectPanel.gameObject.activeSelf,
                    "Mode Select panel must start visible in the hosting scene.");
                AppendIfMissing(
                    failures,
                    modeSelectPanel.Find(
                        $"{CCS_NetcodeTestConstants.ModeSelectCardObjectName}/{CCS_NetcodeTestConstants.SinglePlayerButtonObjectName}") != null,
                    "Mode Select must contain Single Player button.");
                AppendIfMissing(
                    failures,
                    modeSelectPanel.Find(
                        $"{CCS_NetcodeTestConstants.ModeSelectCardObjectName}/{CCS_NetcodeTestConstants.MultiplayerButtonObjectName}") != null,
                    "Mode Select must contain Multiplayer button.");
                AppendIfMissing(
                    failures,
                    modeSelectPanel.Find(
                        $"{CCS_NetcodeTestConstants.ModeSelectCardObjectName}/MainTitleText") != null,
                    "Mode Select must contain the main title.");
                AppendIfMissing(
                    failures,
                    modeSelectPanel.Find(
                        $"{CCS_NetcodeTestConstants.ModeSelectCardObjectName}/LocalTestSessionText") != null,
                    "Mode Select must contain the local test session subtitle.");
                AppendIfMissing(
                    failures,
                    modeSelectPanel.Find(
                        $"{CCS_NetcodeTestConstants.ModeSelectCardObjectName}/{CCS_NetcodeTestConstants.ModeSelectTopAccentObjectName}") != null,
                    "Mode Select must contain the top accent line.");
                AppendIfMissing(
                    failures,
                    modeSelectPanel.Find(
                        $"{CCS_NetcodeTestConstants.ModeSelectCardObjectName}/{CCS_NetcodeTestConstants.ModeSelectDividerObjectName}") != null,
                    "Mode Select must contain the center divider.");
                AppendIfMissing(
                    failures,
                    modeSelectPanel.Find(
                        $"{CCS_NetcodeTestConstants.ModeSelectCardObjectName}/{CCS_NetcodeTestConstants.ModeSelectBottomAccentObjectName}") != null,
                    "Mode Select must contain the bottom accent divider.");
                AppendIfMissing(
                    failures,
                    modeSelectPanel.Find(
                        $"{CCS_NetcodeTestConstants.ModeSelectCardObjectName}/{CCS_NetcodeTestConstants.ModeSelectQuitButtonObjectName}") != null,
                    "Mode Select must contain a Quit button.");

                Transform modeSelectCard = modeSelectPanel.Find(CCS_NetcodeTestConstants.ModeSelectCardObjectName);
                if (modeSelectCard != null)
                {
                    RectTransform cardRect = modeSelectCard as RectTransform;
                    ValidateAnchoredWidth(
                        failures,
                        cardRect,
                        "Mode Select card",
                        CCS_NetcodeTestConstants.ModeSelectCardWidth,
                        60f);
                    ValidateAnchoredHeight(
                        failures,
                        cardRect,
                        "Mode Select card",
                        CCS_NetcodeTestConstants.ModeSelectCardHeight,
                        60f);
                    ValidateAnchoredButtonSize(
                        failures,
                        modeSelectCard.Find(CCS_NetcodeTestConstants.SinglePlayerButtonObjectName) as RectTransform,
                        "Single Player",
                        CCS_NetcodeTestConstants.ModeSelectMenuButtonMinWidth,
                        CCS_NetcodeTestConstants.ModeSelectMenuButtonMaxWidth,
                        CCS_NetcodeTestConstants.ModeSelectMenuButtonMinHeight,
                        CCS_NetcodeTestConstants.ModeSelectMenuButtonMaxHeight);
                    ValidateAnchoredButtonSize(
                        failures,
                        modeSelectCard.Find(CCS_NetcodeTestConstants.MultiplayerButtonObjectName) as RectTransform,
                        "Multiplayer",
                        CCS_NetcodeTestConstants.ModeSelectMenuButtonMinWidth,
                        CCS_NetcodeTestConstants.ModeSelectMenuButtonMaxWidth,
                        CCS_NetcodeTestConstants.ModeSelectMenuButtonMinHeight,
                        CCS_NetcodeTestConstants.ModeSelectMenuButtonMaxHeight);

                    Transform studioTitle = modeSelectCard.Find("StudioTitleText");
                    AppendIfMissing(
                        failures,
                        studioTitle != null && studioTitle.GetComponent<TextMeshProUGUI>() != null,
                        "Mode Select studio title must use TextMeshPro.");

                    Transform mainTitle = modeSelectCard.Find("MainTitleText");
                    if (mainTitle is RectTransform mainTitleRect)
                    {
                        TextMeshProUGUI mainTitleText = mainTitle.GetComponent<TextMeshProUGUI>();
                        AppendIfMissing(
                            failures,
                            mainTitleText != null,
                            "Mode Select main title must use TextMeshPro.");
                        AppendIfMissing(
                            failures,
                            mainTitleText != null
                                && mainTitleText.text == "CHARACTER CONTROLLER TEST",
                            "Mode Select main title text must be CHARACTER CONTROLLER TEST.");
                        AppendIfMissing(
                            failures,
                            mainTitleRect.sizeDelta.x >= CCS_NetcodeTestConstants.ModeSelectMainTitleMinWidth,
                            $"Mode Select main title width must be at least {CCS_NetcodeTestConstants.ModeSelectMainTitleMinWidth:0.#}.");
                        AppendIfMissing(
                            failures,
                            mainTitleText == null
                                || mainTitleText.overflowMode != TextOverflowModes.Ellipsis,
                            "Mode Select main title must not truncate with ellipsis.");
                    }
                    else
                    {
                        failures.Add("Mode Select main title RectTransform is missing.");
                    }

                    Transform modeSelectQuit = modeSelectCard.Find(CCS_NetcodeTestConstants.ModeSelectQuitButtonObjectName);
                    AppendIfMissing(
                        failures,
                        ButtonLabelEquals(modeSelectQuit?.GetComponentInChildren<Text>(), "QUIT"),
                        "Mode Select Quit button text must be QUIT.");
                    ValidateAnchoredButtonSize(
                        failures,
                        modeSelectQuit as RectTransform,
                        "Mode Select Quit",
                        CCS_NetcodeTestConstants.ModeSelectQuitButtonWidth - 20f,
                        CCS_NetcodeTestConstants.ModeSelectQuitButtonWidth + 20f,
                        CCS_NetcodeTestConstants.NetworkingHostJoinButtonHeight - 6f,
                        CCS_NetcodeTestConstants.NetworkingHostJoinButtonHeight + 6f);
                }
            }

            if (networkingPanel != null)
            {
                AppendIfMissing(
                    failures,
                    !networkingPanel.gameObject.activeSelf,
                    "Networking panel must start hidden in the hosting scene.");
            }

            if (modeController != null)
            {
                SerializedObject serializedController = new SerializedObject(modeController);
                AppendIfMissing(
                    failures,
                    serializedController.FindProperty("modeSelectPanel")?.objectReferenceValue != null,
                    "CCS_HostingSceneModeSelectController.modeSelectPanel is not wired.");
                AppendIfMissing(
                    failures,
                    serializedController.FindProperty("networkingPanel")?.objectReferenceValue != null,
                    "CCS_HostingSceneModeSelectController.networkingPanel is not wired.");
                AppendIfMissing(
                    failures,
                    serializedController.FindProperty("singlePlayerButton")?.objectReferenceValue != null,
                    "CCS_HostingSceneModeSelectController.singlePlayerButton is not wired.");
                AppendIfMissing(
                    failures,
                    serializedController.FindProperty("multiplayerButton")?.objectReferenceValue != null,
                    "CCS_HostingSceneModeSelectController.multiplayerButton is not wired.");
                AppendIfMissing(
                    failures,
                    serializedController.FindProperty("backButton")?.objectReferenceValue != null,
                    "CCS_HostingSceneModeSelectController.backButton is not wired.");
                AppendIfMissing(
                    failures,
                    serializedController.FindProperty("quitButton")?.objectReferenceValue != null,
                    "CCS_HostingSceneModeSelectController.quitButton is not wired.");
            }

            if (networkingCard != null)
            {
                RectTransform networkingCardRect = networkingCard as RectTransform;
                ValidateCenterAnchoredCard(
                    failures,
                    networkingCardRect,
                    CCS_NetcodeTestConstants.NetworkingCardWidth,
                    CCS_NetcodeTestConstants.NetworkingCardHeight);
                AppendIfMissing(
                    failures,
                    networkingCard.Find("NamePanel/PlayerNameInput") != null,
                    "Networking card must contain the player name field.");
                AppendIfMissing(
                    failures,
                    networkingCard.Find("NameHintText") != null,
                    "Networking card must contain NameHintText below the header divider.");
                AppendIfMissing(
                    failures,
                    networkingCard.Find("NamePanel/NameHintText") == null,
                    "NameHintText must live on NetworkingCard, not inside NamePanel.");
                AppendIfMissing(
                    failures,
                    networkingCard.Find($"NamePanel/{CCS_NetcodeTestConstants.PlayerNameWarningTextObjectName}") != null,
                    "Networking card must contain the player name warning label.");
                AppendIfMissing(
                    failures,
                    networkingCard.Find("ServerNamePanel/ServerNameInput") != null,
                    "Networking card must contain the server name field.");
                AppendIfMissing(
                    failures,
                    networkingCard.Find($"ServerNamePanel/{CCS_NetcodeTestConstants.ServerNameWarningTextObjectName}") != null,
                    "Networking card must contain the server name warning label.");
                AppendIfMissing(
                    failures,
                    networkingCard.Find("HostCard") != null && networkingCard.Find("JoinCard") != null,
                    "Networking card must contain HostCard and JoinCard.");
                ValidateMatchingCardSizes(
                    failures,
                    networkingCard.Find("HostCard") as RectTransform,
                    networkingCard.Find("JoinCard") as RectTransform);
                ValidateNetworkingPanelVerticalLayout(
                    failures,
                    networkingCardRect,
                    networkingCard.Find("NamePanel") as RectTransform,
                    networkingCard.Find(CCS_NetcodeTestConstants.ServerNamePanelObjectName) as RectTransform,
                    networkingCard.Find("HostCard") as RectTransform,
                    networkingCard.Find("JoinCard") as RectTransform);
                ValidateNetworkingFooterLayout(
                    failures,
                    networkingCardRect,
                    networkingCard.Find("HostCard") as RectTransform,
                    networkingCard.Find("JoinCard") as RectTransform);
                ValidateJoinCardListLayout(
                    failures,
                    networkingCard.Find("JoinCard") as RectTransform);
                ValidateInputWarningLayout(
                    failures,
                    networkingCard.Find("NamePanel") as RectTransform,
                    "NamePanel");
                ValidateInputWarningLayout(
                    failures,
                    networkingCard.Find(CCS_NetcodeTestConstants.ServerNamePanelObjectName) as RectTransform,
                    "ServerNamePanel");
                AppendIfMissing(
                    failures,
                    networkingCard.Find("AdvancedManualJoinPanel") == null,
                    "Advanced Manual Join panel must be removed from the hosting UI.");
                AppendIfMissing(
                    failures,
                    networkingCard.Find("AdvancedManualJoinToggleButton") == null,
                    "Advanced Manual Join toggle must be removed from the hosting UI.");

                ValidateAnchoredButtonSize(
                    failures,
                    networkingCard.Find("HostCard/HostAndStartButton") as RectTransform,
                    "Host & Start",
                    300f,
                    CCS_NetcodeTestConstants.HostStartButtonMaxWidth,
                    CCS_NetcodeTestConstants.NetworkingHostJoinButtonHeight - 6f,
                    CCS_NetcodeTestConstants.NetworkingHostJoinButtonHeight + 6f);
                ValidateAnchoredButtonSize(
                    failures,
                    networkingCard.Find("JoinCard/JoinButtons/RefreshServersButton") as RectTransform,
                    "Refresh",
                    180f,
                    CCS_NetcodeTestConstants.RefreshButtonMaxWidth,
                    CCS_NetcodeTestConstants.NetworkingHostJoinButtonHeight - 6f,
                    CCS_NetcodeTestConstants.NetworkingHostJoinButtonHeight + 6f);
                ValidateAnchoredButtonSize(
                    failures,
                    networkingCard.Find("JoinCard/JoinButtons/JoinSelectedButton") as RectTransform,
                    "Join Selected",
                    240f,
                    CCS_NetcodeTestConstants.JoinSelectedButtonMaxWidth,
                    CCS_NetcodeTestConstants.NetworkingHostJoinButtonHeight - 6f,
                    CCS_NetcodeTestConstants.NetworkingHostJoinButtonHeight + 6f);
                ValidateAnchoredButtonSize(
                    failures,
                    networkingCard.Find(CCS_NetcodeTestConstants.BackButtonObjectName) as RectTransform,
                    "Back",
                    CCS_NetcodeTestConstants.NetworkingBackButtonWidth - 20f,
                    CCS_NetcodeTestConstants.NetworkingBackButtonWidth + 20f,
                    CCS_NetcodeTestConstants.NetworkingHostJoinButtonHeight - 6f,
                    CCS_NetcodeTestConstants.NetworkingHostJoinButtonHeight + 6f);
                ValidateAnchoredButtonSize(
                    failures,
                    networkingCard.Find(CCS_NetcodeTestConstants.QuitButtonObjectName) as RectTransform,
                    "Quit",
                    CCS_NetcodeTestConstants.NetworkingExitButtonWidth - 20f,
                    CCS_NetcodeTestConstants.NetworkingExitButtonWidth + 20f,
                    CCS_NetcodeTestConstants.NetworkingHostJoinButtonHeight - 6f,
                    CCS_NetcodeTestConstants.NetworkingHostJoinButtonHeight + 6f);

                GameObject diagnosticsPanel = networkingCard.Find("DiagnosticsPanel")?.gameObject;
                AppendIfMissing(
                    failures,
                    diagnosticsPanel == null || !diagnosticsPanel.activeSelf,
                    "Diagnostics panel must stay hidden in the polished hosting UI.");
            }
        }

        private static void ValidateCenterAnchoredCard(
            List<string> failures,
            RectTransform cardRect,
            float expectedWidth,
            float expectedHeight)
        {
            if (cardRect == null)
            {
                failures.Add("Networking card RectTransform is missing.");
                return;
            }

            AppendIfMissing(
                failures,
                Mathf.Approximately(cardRect.anchorMin.x, 0.5f)
                    && Mathf.Approximately(cardRect.anchorMin.y, 0.5f)
                    && Mathf.Approximately(cardRect.anchorMax.x, 0.5f)
                    && Mathf.Approximately(cardRect.anchorMax.y, 0.5f),
                "Networking card must be center anchored.");
            ValidateAnchoredWidth(failures, cardRect, "Networking card", expectedWidth, 40f);
            ValidateAnchoredHeight(failures, cardRect, "Networking card", expectedHeight, 40f);
        }

        private static void ValidateMatchingCardSizes(
            List<string> failures,
            RectTransform hostCard,
            RectTransform joinCard)
        {
            if (hostCard == null || joinCard == null)
            {
                failures.Add("HostCard or JoinCard RectTransform is missing.");
                return;
            }

            ValidateAnchoredWidth(
                failures,
                hostCard,
                "Host card",
                CCS_NetcodeTestConstants.HostCardWidth,
                20f);
            ValidateAnchoredHeight(
                failures,
                hostCard,
                "Host card",
                CCS_NetcodeTestConstants.HostCardHeight,
                20f);
            AppendIfMissing(
                failures,
                Mathf.Approximately(hostCard.sizeDelta.x, joinCard.sizeDelta.x)
                    && Mathf.Approximately(hostCard.sizeDelta.y, joinCard.sizeDelta.y),
                "HostCard and JoinCard must use matching sizes.");
        }

        private static void ValidateNetworkingPanelVerticalLayout(
            List<string> failures,
            RectTransform networkingCard,
            RectTransform namePanel,
            RectTransform serverNamePanel,
            RectTransform hostCard,
            RectTransform joinCard)
        {
            if (networkingCard == null)
            {
                failures.Add("Networking card RectTransform is missing for vertical layout validation.");
                return;
            }

            if (namePanel == null || serverNamePanel == null || hostCard == null || joinCard == null)
            {
                failures.Add("Networking NamePanel, ServerNamePanel, HostCard, or JoinCard is missing.");
                return;
            }

            ValidateAnchoredWidth(
                failures,
                namePanel,
                "Name panel",
                CCS_NetcodeTestConstants.NetworkingNamePanelWidth,
                20f);
            ValidateAnchoredHeight(
                failures,
                namePanel,
                "Name panel",
                CCS_NetcodeTestConstants.NetworkingNamePanelHeight,
                10f);
            AppendIfMissing(
                failures,
                Mathf.Approximately(namePanel.anchorMin.y, 1f)
                    && Mathf.Approximately(namePanel.anchorMax.y, 1f),
                "NamePanel must use top-center anchoring inside NetworkingCard.");
            AppendIfMissing(
                failures,
                Mathf.Abs(namePanel.anchoredPosition.y + CCS_NetcodeTestConstants.NetworkingNamePanelTopOffset) <= 8f,
                $"NamePanel must sit near Y -{CCS_NetcodeTestConstants.NetworkingNamePanelTopOffset:0.#} from the card top.");

            ValidateAnchoredWidth(
                failures,
                serverNamePanel,
                "Server name panel",
                CCS_NetcodeTestConstants.NetworkingServerNamePanelWidth,
                20f);
            ValidateAnchoredHeight(
                failures,
                serverNamePanel,
                "Server name panel",
                CCS_NetcodeTestConstants.NetworkingServerNamePanelHeight,
                10f);
            AppendIfMissing(
                failures,
                Mathf.Approximately(serverNamePanel.anchorMin.y, 1f)
                    && Mathf.Approximately(serverNamePanel.anchorMax.y, 1f),
                "ServerNamePanel must use top-center anchoring inside NetworkingCard.");
            AppendIfMissing(
                failures,
                Mathf.Abs(serverNamePanel.anchoredPosition.y + CCS_NetcodeTestConstants.NetworkingServerNamePanelTopOffset) <= 8f,
                $"ServerNamePanel must sit near Y -{CCS_NetcodeTestConstants.NetworkingServerNamePanelTopOffset:0.#} from the card top.");

            AppendIfMissing(
                failures,
                Mathf.Approximately(hostCard.anchorMin.x, 0.5f)
                    && Mathf.Approximately(hostCard.anchorMin.y, 0.5f)
                    && Mathf.Approximately(hostCard.anchorMax.x, 0.5f)
                    && Mathf.Approximately(hostCard.anchorMax.y, 0.5f),
                "HostCard must use center anchoring inside NetworkingCard.");
            AppendIfMissing(
                failures,
                Mathf.Approximately(joinCard.anchorMin.x, 0.5f)
                    && Mathf.Approximately(joinCard.anchorMin.y, 0.5f)
                    && Mathf.Approximately(joinCard.anchorMax.x, 0.5f)
                    && Mathf.Approximately(joinCard.anchorMax.y, 0.5f),
                "JoinCard must use center anchoring inside NetworkingCard.");
            AppendIfMissing(
                failures,
                Mathf.Abs(hostCard.anchoredPosition.x + CCS_NetcodeTestConstants.NetworkingHostJoinCardCenterXOffset) <= 12f
                    && Mathf.Abs(hostCard.anchoredPosition.y - CCS_NetcodeTestConstants.NetworkingHostJoinCardCenterYOffset) <= 12f,
                $"HostCard must sit near ({-CCS_NetcodeTestConstants.NetworkingHostJoinCardCenterXOffset:0.#}, {CCS_NetcodeTestConstants.NetworkingHostJoinCardCenterYOffset:0.#}).");
            AppendIfMissing(
                failures,
                Mathf.Abs(joinCard.anchoredPosition.x - CCS_NetcodeTestConstants.NetworkingHostJoinCardCenterXOffset) <= 12f
                    && Mathf.Abs(joinCard.anchoredPosition.y - CCS_NetcodeTestConstants.NetworkingHostJoinCardCenterYOffset) <= 12f,
                $"JoinCard must sit near ({CCS_NetcodeTestConstants.NetworkingHostJoinCardCenterXOffset:0.#}, {CCS_NetcodeTestConstants.NetworkingHostJoinCardCenterYOffset:0.#}).");

            Bounds nameBounds = GetRelativeBounds(networkingCard, namePanel);
            Bounds serverBounds = GetRelativeBounds(networkingCard, serverNamePanel);
            Bounds hostBounds = GetRelativeBounds(networkingCard, hostCard);
            Bounds joinBounds = GetRelativeBounds(networkingCard, joinCard);

            float nameToServerGap = nameBounds.min.y - serverBounds.max.y;
            AppendIfMissing(
                failures,
                nameToServerGap >= 10f,
                "ServerNamePanel overlaps NamePanel or sits too close to the player name section.");

            float serverToHostGap = serverBounds.min.y - hostBounds.max.y;
            float serverToJoinGap = serverBounds.min.y - joinBounds.max.y;
            AppendIfMissing(
                failures,
                serverToHostGap >= CCS_NetcodeTestConstants.NetworkingMinNamePanelBodyGap,
                "HostCard overlaps ServerNamePanel or sits too close to the server name section.");
            AppendIfMissing(
                failures,
                serverToJoinGap >= CCS_NetcodeTestConstants.NetworkingMinNamePanelBodyGap,
                "JoinCard overlaps ServerNamePanel or sits too close to the server name section.");

            InputField playerNameInput = namePanel.GetComponentInChildren<InputField>(true);
            AppendIfMissing(
                failures,
                playerNameInput != null && playerNameInput.interactable,
                "NamePanel player name input must remain interactable.");

            InputField serverNameInput = serverNamePanel.GetComponentInChildren<InputField>(true);
            AppendIfMissing(
                failures,
                serverNameInput != null && serverNameInput.interactable,
                "ServerNamePanel server name input must remain interactable.");
        }

        private static void ValidateJoinCardListLayout(List<string> failures, RectTransform joinCard)
        {
            if (joinCard == null)
            {
                failures.Add("JoinCard RectTransform is missing for join list layout validation.");
                return;
            }

            RectTransform serverListScroll = joinCard.Find("ServerListScroll") as RectTransform;
            RectTransform buttonRow = joinCard.Find("JoinButtons") as RectTransform;
            if (serverListScroll == null || buttonRow == null)
            {
                failures.Add("JoinCard server list scroll or button row is missing.");
                return;
            }

            ValidateAnchoredWidth(
                failures,
                serverListScroll,
                "Join host list",
                CCS_NetcodeTestConstants.NetworkingJoinHostListWidth,
                20f);
            ValidateAnchoredHeight(
                failures,
                serverListScroll,
                "Join host list",
                CCS_NetcodeTestConstants.NetworkingJoinHostListHeight,
                10f);
            AppendIfMissing(
                failures,
                Mathf.Abs(serverListScroll.anchoredPosition.y - CCS_NetcodeTestConstants.NetworkingJoinHostListCenterYOffset) <= 12f,
                $"Join host list must sit near Y {CCS_NetcodeTestConstants.NetworkingJoinHostListCenterYOffset:0.#} inside JoinCard.");
            AppendIfMissing(
                failures,
                Mathf.Abs(buttonRow.anchoredPosition.y - CCS_NetcodeTestConstants.NetworkingJoinButtonBottomOffset) <= 8f,
                $"Join button row must sit near Y {CCS_NetcodeTestConstants.NetworkingJoinButtonBottomOffset:0.#} from the JoinCard bottom.");

            Bounds listBounds = GetRelativeBounds(joinCard, serverListScroll);
            Bounds buttonBounds = GetRelativeBounds(joinCard, buttonRow);
            float listToButtonGap = listBounds.min.y - buttonBounds.max.y;
            AppendIfMissing(
                failures,
                listToButtonGap >= CCS_NetcodeTestConstants.NetworkingJoinHostListGapAboveButtons,
                "Join empty host message must not overlap Refresh / Join Selected buttons.");
        }

        private static void ValidateInputWarningLayout(
            List<string> failures,
            RectTransform panel,
            string panelLabel)
        {
            if (panel == null)
            {
                failures.Add($"Hosting UI panel '{panelLabel}' is missing for warning layout validation.");
                return;
            }

            RectTransform warningRect = panel.Find(CCS_NetcodeTestConstants.PlayerNameWarningTextObjectName) as RectTransform;
            if (panelLabel == "ServerNamePanel")
            {
                warningRect = panel.Find(CCS_NetcodeTestConstants.ServerNameWarningTextObjectName) as RectTransform;
            }

            if (warningRect == null)
            {
                failures.Add($"{panelLabel} warning text is missing.");
                return;
            }

            AppendIfMissing(
                failures,
                warningRect.anchoredPosition.x >= CCS_NetcodeTestConstants.NetworkingInputWarningLeftOffset - 20f,
                $"{panelLabel} warning text must sit to the right of the input field.");
            AppendIfMissing(
                failures,
                warningRect.sizeDelta.x >= CCS_NetcodeTestConstants.NetworkingInputWarningWidth - 40f,
                $"{panelLabel} warning text must use a wide warning area.");
            AppendIfMissing(
                failures,
                warningRect.sizeDelta.y >= CCS_NetcodeTestConstants.NetworkingInputWarningHeight - 8f,
                $"{panelLabel} warning text must use a tall warning area.");
        }

        private static void ValidateNetworkingFooterLayout(
            List<string> failures,
            RectTransform networkingCard,
            RectTransform hostCard,
            RectTransform joinCard)
        {
            if (networkingCard == null)
            {
                failures.Add("Networking card RectTransform is missing for footer layout validation.");
                return;
            }

            RectTransform footerDivider = networkingCard.Find("FooterDivider") as RectTransform;
            RectTransform backButton = networkingCard.Find(CCS_NetcodeTestConstants.BackButtonObjectName) as RectTransform;
            RectTransform playersPanel = networkingCard.Find("ConnectedPlayersPanel") as RectTransform;
            RectTransform quitButton = networkingCard.Find(CCS_NetcodeTestConstants.QuitButtonObjectName) as RectTransform;

            if (footerDivider == null || backButton == null || playersPanel == null || quitButton == null)
            {
                failures.Add("Networking footer divider or footer controls are missing.");
                return;
            }

            Bounds hostBounds = GetRelativeBounds(networkingCard, hostCard);
            Bounds joinBounds = GetRelativeBounds(networkingCard, joinCard);
            Bounds footerBounds = GetRelativeBounds(networkingCard, footerDivider);
            float bodyBottom = Mathf.Min(hostBounds.min.y, joinBounds.min.y);
            float footerGap = bodyBottom - footerBounds.max.y;
            AppendIfMissing(
                failures,
                footerGap >= CCS_NetcodeTestConstants.NetworkingMinFooterBodyGap,
                $"Footer divider must leave at least {CCS_NetcodeTestConstants.NetworkingMinFooterBodyGap:0.#}px below Host/Join cards.");
            AppendIfMissing(
                failures,
                hostBounds.min.y > footerBounds.max.y,
                "HostCard overlaps FooterDivider.");
            AppendIfMissing(
                failures,
                joinBounds.min.y > footerBounds.max.y,
                "JoinCard overlaps FooterDivider.");

            ValidateBottomAnchoredYOffset(
                failures,
                footerDivider,
                "Footer divider",
                CCS_NetcodeTestConstants.NetworkingFooterDividerBottomOffset,
                8f);
            ValidateBottomAnchoredYOffset(
                failures,
                backButton,
                "Back button",
                CCS_NetcodeTestConstants.NetworkingFooterButtonBottomOffset,
                10f);
            ValidateBottomAnchoredYOffset(
                failures,
                playersPanel,
                "Players panel",
                CCS_NetcodeTestConstants.NetworkingFooterButtonBottomOffset,
                10f);
            ValidateBottomAnchoredYOffset(
                failures,
                quitButton,
                "Quit button",
                CCS_NetcodeTestConstants.NetworkingFooterButtonBottomOffset,
                10f);

            AppendIfMissing(
                failures,
                backButton.anchoredPosition.y < footerDivider.anchoredPosition.y,
                "Back button must sit below the footer divider band.");
            AppendIfMissing(
                failures,
                quitButton.anchoredPosition.y < footerDivider.anchoredPosition.y,
                "Quit button must sit below the footer divider band.");
        }

        private static Bounds GetRelativeBounds(RectTransform parent, RectTransform child)
        {
            if (parent == null || child == null)
            {
                return new Bounds();
            }

            return RectTransformUtility.CalculateRelativeRectTransformBounds(parent, child);
        }

        private static void ValidateBottomAnchoredYOffset(
            List<string> failures,
            RectTransform rect,
            string label,
            float expectedYOffset,
            float tolerance)
        {
            if (rect == null)
            {
                failures.Add($"Hosting UI element '{label}' is missing for footer layout validation.");
                return;
            }

            AppendIfMissing(
                failures,
                Mathf.Approximately(rect.anchorMin.y, 0f) && Mathf.Approximately(rect.anchorMax.y, 0f),
                $"Hosting UI element '{label}' must use bottom anchoring.");
            AppendIfMissing(
                failures,
                Mathf.Abs(rect.anchoredPosition.y - expectedYOffset) <= tolerance,
                $"Hosting UI element '{label}' must sit near {expectedYOffset:0.#}px from the card bottom.");
        }

        private static void ValidateAnchoredWidth(
            List<string> failures,
            RectTransform rect,
            string label,
            float expectedWidth,
            float tolerance)
        {
            if (rect == null)
            {
                failures.Add($"Hosting UI element '{label}' is missing a RectTransform width.");
                return;
            }

            AppendIfMissing(
                failures,
                Mathf.Abs(rect.sizeDelta.x - expectedWidth) <= tolerance,
                $"Hosting UI element '{label}' width must be near {expectedWidth:0.#} (±{tolerance:0.#}).");
        }

        private static void ValidateAnchoredHeight(
            List<string> failures,
            RectTransform rect,
            string label,
            float expectedHeight,
            float tolerance)
        {
            if (rect == null)
            {
                failures.Add($"Hosting UI element '{label}' is missing a RectTransform height.");
                return;
            }

            AppendIfMissing(
                failures,
                Mathf.Abs(rect.sizeDelta.y - expectedHeight) <= tolerance,
                $"Hosting UI element '{label}' height must be near {expectedHeight:0.#} (±{tolerance:0.#}).");
        }

        private static void ValidateAnchoredButtonSize(
            List<string> failures,
            RectTransform rect,
            string label,
            float minWidth,
            float maxWidth,
            float minHeight,
            float maxHeight)
        {
            if (rect == null)
            {
                failures.Add($"Hosting UI button '{label}' is missing a RectTransform.");
                return;
            }

            AppendIfMissing(
                failures,
                rect.sizeDelta.x >= minWidth && rect.sizeDelta.x <= maxWidth,
                $"Hosting UI button '{label}' width must be between {minWidth:0.#} and {maxWidth:0.#}.");
            AppendIfMissing(
                failures,
                rect.sizeDelta.y >= minHeight && rect.sizeDelta.y <= maxHeight,
                $"Hosting UI button '{label}' height must be between {minHeight:0.#} and {maxHeight:0.#}.");
        }

        private static bool ButtonLabelEquals(Text label, string expectedText)
        {
            return label != null && label.text == expectedText;
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
                    source.Contains("OnJoinSelectedClicked"),
                    "CCS_MultiplayerHostingMenu must expose Join Selected flow.");
                AppendIfMissing(
                    failures,
                    source.Contains("TryValidateForHost"),
                    "CCS_MultiplayerHostingMenu must validate player and server name before host.");
                AppendIfMissing(
                    failures,
                    source.Contains("TryValidateForJoin"),
                    "CCS_MultiplayerHostingMenu must validate player name and selected host before join.");
                AppendIfMissing(
                    failures,
                    source.Contains("playerNameWarningText"),
                    "CCS_MultiplayerHostingMenu must expose the player name warning label.");
                AppendIfMissing(
                    failures,
                    source.Contains("serverNameWarningText"),
                    "CCS_MultiplayerHostingMenu must expose the server name warning label.");
                AppendIfMissing(
                    failures,
                    source.Contains("PlayerNameWarningDisplayMessage"),
                    "CCS_MultiplayerHostingMenu must use the player name warning display message.");
                AppendIfMissing(
                    failures,
                    source.Contains("ServerNameRequiredWarningMessage"),
                    "CCS_MultiplayerHostingMenu must use the required server name warning message.");
                AppendIfMissing(
                    failures,
                    source.Contains("NoLocalHostSelectedWarningMessage"),
                    "CCS_MultiplayerHostingMenu must use the no-local-host join warning message.");
                AppendIfMissing(
                    failures,
                    source.Contains("[Hosting Validation]"),
                    "CCS_MultiplayerHostingMenu must log hosting validation failures.");
                AppendIfMissing(
                    failures,
                    source.Contains("[Join Validation]"),
                    "CCS_MultiplayerHostingMenu must log join validation failures.");
                AppendIfMissing(
                    failures,
                    source.Contains("[Join Selection]"),
                    "CCS_MultiplayerHostingMenu must log join selection changes.");
                AppendIfMissing(
                    failures,
                    source.Contains("ClearSelectedHost"),
                    "CCS_MultiplayerHostingMenu must clear join selection explicitly.");
                AppendIfMissing(
                    failures,
                    source.Contains("RefreshJoinSelectedButtonState"),
                    "CCS_MultiplayerHostingMenu must gate Join Selected on explicit host selection.");
                AppendIfMissing(
                    failures,
                    source.Contains("OnNetworkingPanelShown"),
                    "CCS_MultiplayerHostingMenu must reset join selection when the networking panel opens.");
                AppendIfMissing(
                    failures,
                    !source.Contains("SelectServer(0)"),
                    "CCS_MultiplayerHostingMenu must not auto-select the first discovered host.");
                AppendIfMissing(
                    failures,
                    !source.Contains("ApplyDefaultServerName"),
                    "CCS_MultiplayerHostingMenu must not prefill server name before host validation.");
                AppendIfMissing(
                    failures,
                    source.Contains("CCS_LocalMultiplayerHostDiscovery.DiscoverLocalHosts"),
                    "CCS_MultiplayerHostingMenu must discover local hosts instead of seeding fake localhost.");
                AppendIfMissing(
                    failures,
                    source.Contains("EmptyServerListMessage"),
                    "CCS_MultiplayerHostingMenu must use the empty join-list message constant.");
                AppendIfMissing(
                    failures,
                    !source.Contains("CreateLocalhostDefault()"),
                    "CCS_MultiplayerHostingMenu must not seed fake localhost in the join list.");
                AppendIfMissing(
                    failures,
                    source.Contains("OnQuitClicked"),
                    "CCS_MultiplayerHostingMenu must expose Quit flow.");
                AppendIfMissing(
                    failures,
                    source.Contains("CCS_HostingApplicationQuitUtility"),
                    "CCS_MultiplayerHostingMenu must quit through CCS_HostingApplicationQuitUtility.");
                AppendIfMissing(
                    failures,
                    source.Contains("CCS_HostingSceneBuildUtility"),
                    "CCS_MultiplayerHostingMenu must validate build settings at runtime.");
                AppendIfMissing(
                    failures,
                    !source.Contains("MultiplayerHostingScenePath"),
                    "CCS_MultiplayerHostingMenu must not load scenes by editor asset path at runtime.");
                AppendIfMissing(
                    failures,
                    !source.Contains("UnityEditor"),
                    "CCS_MultiplayerHostingMenu must not reference UnityEditor in runtime code.");
                AppendIfMissing(
                    failures,
                    !source.Contains("OnExitClicked"),
                    "CCS_MultiplayerHostingMenu must not use legacy Exit behavior.");
            }

            Transform hostingRoot = GameObject.Find("HostingUiRoot")?.transform;
            Transform networkingCard = hostingRoot != null
                ? hostingRoot.Find($"{CCS_NetcodeTestConstants.NetworkingPanelObjectName}/{CCS_NetcodeTestConstants.NetworkingCardObjectName}")
                : null;
            AppendIfMissing(
                failures,
                hostingRoot != null && networkingCard != null,
                "SCN_CCS_MultiplayerHosting must contain HostingUiRoot/NetworkingPanel/NetworkingCard. Run Setup Multiplayer Hosting Scene.");

            if (networkingCard != null)
            {
                AppendIfMissing(
                    failures,
                    networkingCard.Find("NamePanel/PlayerNameInput") != null,
                    "Hosting UI must contain the player name field.");
                AppendIfMissing(
                    failures,
                    networkingCard.Find("NameHintText") != null,
                    "Hosting UI must contain NameHintText below the header divider.");
                AppendIfMissing(
                    failures,
                    networkingCard.Find("NamePanel/NameHintText") == null,
                    "NameHintText must live on NetworkingCard, not inside NamePanel.");
                AppendIfMissing(
                    failures,
                    networkingCard.Find($"NamePanel/{CCS_NetcodeTestConstants.PlayerNameWarningTextObjectName}") != null,
                    "Hosting UI must contain the player name warning label.");
                AppendIfMissing(
                    failures,
                    networkingCard.Find("ServerNamePanel/ServerNameInput") != null,
                    "Hosting UI must contain the server name field.");
                AppendIfMissing(
                    failures,
                    networkingCard.Find($"ServerNamePanel/{CCS_NetcodeTestConstants.ServerNameWarningTextObjectName}") != null,
                    "Hosting UI must contain the server name warning label.");
                AppendIfMissing(
                    failures,
                    networkingCard.Find("HostCard/HostAndStartButton") != null,
                    "Hosting UI must contain Host & Start.");
                AppendIfMissing(
                    failures,
                    networkingCard.Find("JoinCard/JoinButtons/JoinSelectedButton") != null,
                    "Hosting UI must contain Join Selected.");
                AppendIfMissing(
                    failures,
                    networkingCard.Find("JoinCard/JoinButtons/RefreshServersButton") != null,
                    "Hosting UI must contain Refresh.");
                AppendIfMissing(
                    failures,
                    networkingCard.Find("JoinCard/ServerListScroll/Viewport/ServerListContainer") != null,
                    "Hosting UI must contain a server list container.");
                AppendIfMissing(
                    failures,
                    networkingCard.Find("HostCard") != null && networkingCard.Find("JoinCard") != null,
                    "Hosting UI must contain side-by-side Host Game and Join Game cards.");
                AppendIfMissing(
                    failures,
                    networkingCard.Find(CCS_NetcodeTestConstants.BackButtonObjectName) != null,
                    "Hosting UI must contain Back button on the networking panel.");
                AppendIfMissing(
                    failures,
                    networkingCard.Find(CCS_NetcodeTestConstants.QuitButtonObjectName) != null,
                    "Hosting UI must contain Quit button on the networking panel.");
                AppendIfMissing(
                    failures,
                    ButtonLabelEquals(
                        networkingCard.Find(CCS_NetcodeTestConstants.QuitButtonObjectName)?.GetComponentInChildren<Text>(),
                        "QUIT"),
                    "Networking Quit button text must be QUIT.");
                AppendIfMissing(
                    failures,
                    networkingCard.Find("AdvancedManualJoinPanel") == null,
                    "Hosting UI must not expose Advanced Manual Join in the polished layout.");
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
                AppendIfMissing(
                    failures,
                    serializedMenu.FindProperty("refreshServersButton")?.objectReferenceValue != null,
                    "CCS_MultiplayerHostingMenu.refreshServersButton is not wired.");
                AppendIfMissing(
                    failures,
                    serializedMenu.FindProperty("quitButton")?.objectReferenceValue != null,
                    "CCS_MultiplayerHostingMenu.quitButton is not wired.");
                AppendIfMissing(
                    failures,
                    serializedMenu.FindProperty("advancedManualJoinPanel")?.objectReferenceValue == null,
                    "CCS_MultiplayerHostingMenu.advancedManualJoinPanel must remain unassigned in the simple UI.");
                AppendIfMissing(
                    failures,
                    serializedMenu.FindProperty("advancedManualJoinToggleButton")?.objectReferenceValue == null,
                    "CCS_MultiplayerHostingMenu.advancedManualJoinToggleButton must remain unassigned in the simple UI.");
            }

            AppendIfMissing(
                failures,
                GameObject.Find("AdvancedManualJoinPanel") == null,
                "Hosting scene must not contain AdvancedManualJoinPanel.");
            AppendIfMissing(
                failures,
                GameObject.Find("AdvancedManualJoinToggleButton") == null,
                "Hosting scene must not contain AdvancedManualJoinToggleButton.");
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
            if (string.IsNullOrEmpty(prefabGuid))
            {
                return false;
            }

            string[] lines = File.ReadAllLines(assetPath);
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                if (!line.Contains(fieldName + ":"))
                {
                    continue;
                }

                if (line.Contains("fileID: 100100000")
                    && line.Contains($"guid: {prefabGuid}")
                    && line.Contains("type: 3"))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool EditorSerializedPrefabMatchesCanonical(
            GameObject prefabReference,
            string canonicalPrefabPath)
        {
            if (prefabReference == null || string.IsNullOrEmpty(canonicalPrefabPath))
            {
                return false;
            }

            try
            {
                string resolvedPath = AssetDatabase.GetAssetPath(prefabReference);
                if (resolvedPath != canonicalPrefabPath)
                {
                    return false;
                }

                GameObject canonicalPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(canonicalPrefabPath);
                return canonicalPrefab != null && canonicalPrefab.GetComponent<NetworkObject>() != null;
            }
            catch (MissingReferenceException)
            {
                return false;
            }
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

        private static void ValidateRequiredNetworkPrefabListEntries(
            List<string> failures,
            NetworkPrefabsList testList)
        {
            string[] requiredPaths = CCS_NetcodeTestConstants.RequiredNetworkPrefabPaths;
            for (int i = 0; i < requiredPaths.Length; i++)
            {
                GameObject expectedPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(requiredPaths[i]);
                AppendIfMissing(
                    failures,
                    expectedPrefab != null && expectedPrefab.GetComponent<NetworkObject>() != null,
                    $"{requiredPaths[i]} must exist and contain NetworkObject.");

                bool found = false;
                if (testList?.PrefabList != null)
                {
                    for (int entryIndex = 0; entryIndex < testList.PrefabList.Count; entryIndex++)
                    {
                        GameObject registeredPrefab = testList.PrefabList[entryIndex].Prefab;
                        if (!EditorPrefabHasNetworkObject(registeredPrefab)
                            && EditorPrefabHasNetworkObject(expectedPrefab))
                        {
                            registeredPrefab = expectedPrefab;
                        }

                        if (EditorPrefabHasNetworkObject(registeredPrefab)
                            && AssetDatabase.GetAssetPath(registeredPrefab) == requiredPaths[i])
                        {
                            NetworkObject networkObject = registeredPrefab.GetComponent<NetworkObject>();
                            Debug.Log(
                                $"[Netcode Registry] Registered prefab: {registeredPrefab.name} path={requiredPaths[i]} hash={CCS_NetcodeNetworkObjectHashUtility.GetHash(networkObject)}");
                            found = true;
                            break;
                        }
                    }
                }

                AppendIfMissing(
                    failures,
                    found,
                    $"{CCS_NetcodeTestConstants.TestNetworkPrefabsListPath} must register {requiredPaths[i]}.");
            }
        }

        private static void ValidateRequiredNetworkPrefabYamlReferences(List<string> failures)
        {
            string[] requiredPaths = CCS_NetcodeTestConstants.RequiredNetworkPrefabPaths;
            for (int i = 0; i < requiredPaths.Length; i++)
            {
                AppendIfMissing(
                    failures,
                    YamlNetworkPrefabsListContainsPrefabRootReference(
                        CCS_NetcodeTestConstants.TestNetworkPrefabsListPath,
                        requiredPaths[i]),
                    $"{CCS_NetcodeTestConstants.TestNetworkPrefabsListPath} must contain prefab root reference (fileID 100100000) for {requiredPaths[i]}.");
            }
        }

        private static bool YamlNetworkPrefabsListContainsPrefabRootReference(
            string assetPath,
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
            string expectedReference = $"fileID: 100100000, guid: {prefabGuid}, type: 3";
            return yaml.Contains(expectedReference);
        }

        private static void ValidateMasterTestSceneNetworkPrefabRegistration(List<string> failures)
        {
            if (!File.Exists(CCS_NetcodeTestConstants.MasterTestScenePath))
            {
                failures.Add($"Missing asset: {CCS_NetcodeTestConstants.MasterTestScenePath}");
                return;
            }

            NetworkPrefabsList testList = AssetDatabase.LoadAssetAtPath<NetworkPrefabsList>(
                CCS_NetcodeTestConstants.TestNetworkPrefabsListPath);
            if (testList == null)
            {
                failures.Add($"Missing asset: {CCS_NetcodeTestConstants.TestNetworkPrefabsListPath}");
                return;
            }

            UnityEngine.SceneManagement.Scene masterScene = EditorSceneManager.OpenScene(
                CCS_NetcodeTestConstants.MasterTestScenePath,
                OpenSceneMode.Additive);
            if (!masterScene.IsValid())
            {
                failures.Add(
                    $"Could not open {CCS_NetcodeTestConstants.MasterTestScenePath} for network prefab registration validation.");
                return;
            }

            NetworkObject[] sceneObjects = Object.FindObjectsByType<NetworkObject>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);
            for (int i = 0; i < sceneObjects.Length; i++)
            {
                NetworkObject sceneObject = sceneObjects[i];
                if (sceneObject == null)
                {
                    continue;
                }

                Debug.Log(
                    $"[Netcode Registry] Scene NetworkObject: {sceneObject.name} hash={CCS_NetcodeNetworkObjectHashUtility.GetHash(sceneObject)}");

                GameObject sourcePrefab = PrefabUtility.GetCorrespondingObjectFromSource(sceneObject.gameObject);
                if (sourcePrefab == null)
                {
                    continue;
                }

                string sourcePath = AssetDatabase.GetAssetPath(sourcePrefab);
                if (string.IsNullOrEmpty(sourcePath))
                {
                    continue;
                }

                bool registered = false;
                for (int entryIndex = 0; entryIndex < testList.PrefabList.Count; entryIndex++)
                {
                    GameObject registeredPrefab = testList.PrefabList[entryIndex].Prefab;
                    if (registeredPrefab != null && AssetDatabase.GetAssetPath(registeredPrefab) == sourcePath)
                    {
                        registered = true;
                        break;
                    }
                }

                AppendIfMissing(
                    failures,
                    registered,
                    $"{CCS_NetcodeTestConstants.MasterTestScenePath} scene object '{sceneObject.name}' uses unregistered network prefab {sourcePath}.");
            }

            EditorSceneManager.CloseScene(masterScene, removeScene: false);
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
