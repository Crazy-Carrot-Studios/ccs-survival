// =============================================================================
// SCRIPT: CCS_NetcodeTestConstants
// CATEGORY: Modules / CharacterController / Tests / Netcode / Runtime
// PURPOSE: Paths and shared constants for Character Controller netcode smoke tests.
// PLACEMENT: Runtime constants. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Test-only. No production lobby or account integration.
// =============================================================================

using UnityEngine;

namespace CCS.Modules.CharacterController.Tests.Netcode
{
    public static class CCS_NetcodeTestConstants
    {
        public const string NetcodeRuntimeAsmdefPath =
            "Assets/CCS/Modules/CharacterController/Tests/Netcode/Runtime/CCS.Modules.CharacterController.Tests.Netcode.Runtime.asmdef";

        public const string NetcodeEditorAsmdefPath =
            "Assets/CCS/Modules/CharacterController/Tests/Netcode/Editor/CCS.Modules.CharacterController.Tests.Netcode.Editor.asmdef";

        public const string MultiplayerHostingScenePath =
            "Assets/CCS/Scenes/Network/SCN_CCS_MultiplayerHosting.unity";

        public const string MultiplayerHostingSceneName = "SCN_CCS_MultiplayerHosting";

        public const string MasterTestSceneName = "SCN_CCS_CharacterController_MasterTest";

        public const string MasterTestScenePath =
            "Assets/CCS/Scenes/CharacterController/SCN_CCS_CharacterController_MasterTest.unity";

        public const string NetworkManagerPrefabPath =
            "Assets/CCS/Modules/CharacterController/Prefabs/Network/PF_CCS_TestNetworkManager.prefab";

        public const string NetworkedPlayerPrefabPath =
            CCS_TestPlayerPrefabConstants.NetworkedPlayerPrefabPath;

        public const string TestToggleInteractablePrefabPath =
            "Assets/CCS/Modules/Interaction/Tests/Prefabs/PF_CCS_TestInteractable_ToggleCube.prefab";

        public static readonly string[] RequiredNetworkPrefabPaths =
        {
            NetworkedPlayerPrefabPath,
            TestToggleInteractablePrefabPath,
        };

        public const string TestNetworkPrefabsListPath =
            "Assets/CCS/Modules/CharacterController/Prefabs/Network/CCS_TestNetworkPrefabsList.asset";

        public const string DefaultNetworkPrefabsListPath = "Assets/DefaultNetworkPrefabs.asset";

        public const string NetworkTestPrefabsRegistryPath =
            "Assets/CCS/Modules/CharacterController/Tests/Netcode/Resources/CCS_NetworkTestPrefabsRegistry.asset";

        public const string NetworkTestPrefabsRegistryResourceName = "CCS_NetworkTestPrefabsRegistry";

        public const string NetworkPlayerPrefabRegistryPath = NetworkTestPrefabsRegistryPath;

        public const string NetworkPlayerPrefabRegistryResourceName = NetworkTestPrefabsRegistryResourceName;

        public const string BlackGlassesMaterialPath =
            "Assets/CCS/Modules/CharacterController/Materials/Player/M_CCS_TestPlayerBlack.mat";

        public const string SceneCameraRigName = "PF_CCS_CharacterCameraRig";

        public const string OfflineTestPlayerSceneName = CCS_TestPlayerPrefabConstants.NetworkedPlayerInstanceName;

        public const string NameplateRootObjectName = "NameplateRoot";

        public const string NameplateTextObjectName = "PlayerNameText";

        public const string LegacyNameplateTextObjectName = "NameplateText";

        public const string CapsuleVisualName = "CapsuleVisual";

        public const string GlassesVisualName = "VisualGlasses";

        public const string HeaderPanelObjectName = "HeaderPanel";

        public const string ModeSelectPanelObjectName = "ModeSelectPanel";

        public const string ModeSelectCardObjectName = "ModeSelectCard";

        public const string NetworkingPanelObjectName = "NetworkingPanel";

        public const string NetworkingCardObjectName = "NetworkingCard";

        public const string NetworkingContentObjectName = NetworkingCardObjectName;

        public const string SinglePlayerButtonObjectName = "SinglePlayerButton";

        public const string MultiplayerButtonObjectName = "MultiplayerButton";

        public const string BackButtonObjectName = "BackButton";

        public const string QuitButtonObjectName = "QuitButton";

        public const string ModeSelectQuitButtonObjectName = "ModeSelectQuitButton";

        public const float ModeSelectQuitButtonWidth = 260f;

        public const float ModeSelectCardWidth = 1250f;

        public const float ModeSelectCardHeight = 820f;

        public const float ModeSelectMainTitleMinWidth = 1100f;

        public const float ModeSelectMainTitleWidth = 1150f;

        public const float ModeSelectMenuButtonWidth = 720f;

        public const float ModeSelectMenuButtonHeight = 110f;

        public const float ModeSelectButtonSpacing = 40f;

        public const float ModeSelectMenuButtonMinWidth = 680f;

        public const float ModeSelectMenuButtonMaxWidth = 740f;

        public const float ModeSelectMenuButtonMinHeight = 100f;

        public const float ModeSelectMenuButtonMaxHeight = 120f;

        public const string ModeSelectTopAccentObjectName = "TopAccentLine";

        public const string ModeSelectBottomAccentObjectName = "BottomAccentDivider";

        public const float NetworkingCardWidth = 1480f;

        public const float NetworkingCardHeight = 900f;

        public const float HostCardWidth = 600f;

        public const float HostCardHeight = 320f;

        public const float NetworkingNamePanelWidth = 1220f;

        public const float NetworkingNamePanelHeight = 130f;

        public const float NetworkingNamePanelTopOffset = 245f;

        public const float NetworkingHostJoinCardCenterYOffset = -145f;

        public const float NetworkingHostJoinCardCenterXOffset = 320f;

        public const float NetworkingMinNamePanelBodyGap = 35f;

        public const float NetworkingHostJoinButtonBottomOffset = 55f;

        public const float NetworkingHostJoinButtonHeight = 58f;

        public const float NetworkingFooterDividerBottomOffset = 95f;

        public const float NetworkingFooterButtonBottomOffset = 65f;

        public const float NetworkingMinFooterBodyGap = 45f;

        public const float NetworkingBackButtonWidth = 230f;

        public const float NetworkingPlayersPanelWidth = 260f;

        public const float NetworkingExitButtonWidth = 230f;

        public const float HostStartButtonMaxWidth = 520f;

        public const float RefreshButtonMaxWidth = 240f;

        public const float JoinSelectedButtonMaxWidth = 320f;

        public const float FooterButtonMaxWidth = 240f;

        public const float HostingPrimaryButtonMinHeight = 56f;

        public const float HostingPrimaryButtonMaxHeight = 70f;

        public const float HostingFooterButtonMinHeight = 48f;

        public const float HostingFooterButtonMaxHeight = 64f;

        public const float HostingMenuButtonMinHeight = HostingPrimaryButtonMinHeight;

        public const float HostingMenuButtonMaxHeight = HostingPrimaryButtonMaxHeight;

        public const string ModeSelectDividerObjectName = "CenterDivider";

        public const string PlayerSetupPanelObjectName = "PlayerSetupPanel";

        public const string MainContentPanelObjectName = "MainContentPanel";

        public const string FooterPanelObjectName = "FooterPanel";

        public const string HostGamePanelObjectName = "HostGamePanel";

        public const string JoinGamePanelObjectName = "JoinGamePanel";

        public const string PlayerNameStatusTextObjectName = "PlayerNameStatusText";

        public const string PlayerNameWarningTextObjectName = "PlayerNameWarningText";

        public const string PlayerNameRequiredWarningMessage =
            "Enter a player name before hosting or joining.";

        public const string ServerListContainerObjectName = "ServerListContainer";

        public const string DefaultLocalhostServerDisplayName = "Localhost";

        public const string DefaultLocalhostAddress = "127.0.0.1";

        public const string DefaultServerName = "Local Server";

        public const int MaxServerNameLength = 32;

        public const int MaxDisplayNameLength = 20;

        public const string DefaultDisplayName = "Player";

        public const ushort DefaultServerPort = 7777;

        public const int DefaultMaxPlayers = 3;

        public static readonly string[] MasterTestSpawnPointObjectNames =
        {
            "TP_Spawn_Host",
            "TP_Spawn_Client_01",
            "TP_Spawn_Client_02",
        };

        public static readonly Vector3 MasterTestFallbackSpawnPosition = new Vector3(22f, 0f, 18f);

        public const bool EnableControllerAuditLogs = false;

        public const bool EnableMotorMoveAuditLogs = false;

        public const bool EnableInputAuditLogs = false;

        public const bool EnableJumpAuditLogs = false;

        public const bool DisableNetworkTransformForAudit = false;
    }
}
