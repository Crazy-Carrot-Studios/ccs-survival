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

        public const string MasterTestSceneName = "SCN_CCS_CharacterController_MasterTest";

        public const string MasterTestScenePath =
            "Assets/CCS/Scenes/CharacterController/SCN_CCS_CharacterController_MasterTest.unity";

        public const string NetworkManagerPrefabPath =
            "Assets/CCS/Modules/CharacterController/Prefabs/Network/PF_CCS_TestNetworkManager.prefab";

        public const string NetworkedPlayerPrefabPath =
            CCS_TestPlayerPrefabConstants.NetworkedPlayerPrefabPath;

        public const string TestNetworkPrefabsListPath =
            "Assets/CCS/Modules/CharacterController/Prefabs/Network/CCS_TestNetworkPrefabsList.asset";

        public const string DefaultNetworkPrefabsListPath = "Assets/DefaultNetworkPrefabs.asset";

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

        public const string PlayerSetupPanelObjectName = "PlayerSetupPanel";

        public const string MainContentPanelObjectName = "MainContentPanel";

        public const string FooterPanelObjectName = "FooterPanel";

        public const string HostGamePanelObjectName = "HostGamePanel";

        public const string JoinGamePanelObjectName = "JoinGamePanel";

        public const string PlayerNameStatusTextObjectName = "PlayerNameStatusText";

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
