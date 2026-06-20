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

                InputField playerNameInput = serializedMenu.FindProperty("playerNameInput")?.objectReferenceValue as InputField;
                AppendIfMissing(
                    failures,
                    playerNameInput != null && playerNameInput.interactable,
                    "CCS_MultiplayerHostingMenu.playerNameInput must be assigned and interactable.");

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

            if (testList.PrefabList == null || testList.PrefabList.Count != 1)
            {
                failures.Add($"{CCS_NetcodeTestConstants.TestNetworkPrefabsListPath} must contain exactly one player prefab entry.");
                return;
            }

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
                    networkingCard.Find("HostCard") != null && networkingCard.Find("JoinCard") != null,
                    "Networking card must contain HostCard and JoinCard.");
                ValidateMatchingCardSizes(
                    failures,
                    networkingCard.Find("HostCard") as RectTransform,
                    networkingCard.Find("JoinCard") as RectTransform);
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
                    CCS_NetcodeTestConstants.HostingPrimaryButtonMinHeight,
                    CCS_NetcodeTestConstants.HostingPrimaryButtonMaxHeight);
                ValidateAnchoredButtonSize(
                    failures,
                    networkingCard.Find("JoinCard/JoinButtons/RefreshServersButton") as RectTransform,
                    "Refresh",
                    180f,
                    CCS_NetcodeTestConstants.RefreshButtonMaxWidth,
                    CCS_NetcodeTestConstants.HostingPrimaryButtonMinHeight,
                    CCS_NetcodeTestConstants.HostingPrimaryButtonMaxHeight);
                ValidateAnchoredButtonSize(
                    failures,
                    networkingCard.Find("JoinCard/JoinButtons/JoinSelectedButton") as RectTransform,
                    "Join Selected",
                    240f,
                    CCS_NetcodeTestConstants.JoinSelectedButtonMaxWidth,
                    CCS_NetcodeTestConstants.HostingPrimaryButtonMinHeight,
                    CCS_NetcodeTestConstants.HostingPrimaryButtonMaxHeight);
                ValidateAnchoredButtonSize(
                    failures,
                    networkingCard.Find(CCS_NetcodeTestConstants.BackButtonObjectName) as RectTransform,
                    "Back",
                    180f,
                    CCS_NetcodeTestConstants.FooterButtonMaxWidth,
                    CCS_NetcodeTestConstants.HostingFooterButtonMinHeight,
                    CCS_NetcodeTestConstants.HostingFooterButtonMaxHeight);
                ValidateAnchoredButtonSize(
                    failures,
                    networkingCard.Find("ExitButton") as RectTransform,
                    "Exit",
                    180f,
                    CCS_NetcodeTestConstants.FooterButtonMaxWidth,
                    CCS_NetcodeTestConstants.HostingFooterButtonMinHeight,
                    CCS_NetcodeTestConstants.HostingFooterButtonMaxHeight);

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
                    serializedMenu.FindProperty("exitButton")?.objectReferenceValue != null,
                    "CCS_MultiplayerHostingMenu.exitButton is not wired.");
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
