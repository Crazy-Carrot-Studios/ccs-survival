using UnityEngine;

// =============================================================================
// SCRIPT: CCS_ValidationUiConstants
// CATEGORY: Modules / CharacterController / Tests / Runtime
// PURPOSE: Shared master test UI object names and join notification defaults.
// PLACEMENT: Runtime constants. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Used by feed runtime, scene builder, and validators.
// =============================================================================

namespace CCS.Modules.CharacterController.Validation {
    public static class CCS_ValidationUiConstants
    {
        public const string MasterTestSceneName = "SCN_CCS_CharacterController_Validation";

        public const string MasterTestScenePath =
            "Assets/CCS/Modules/CharacterController/Scenes/Validation/SCN_CCS_CharacterController_Validation.unity";

        public const string MasterTestUiCanvasObjectName = "MasterTestUiCanvas";

        public const string JoinNotificationFeedObjectName = "CCS_PlayerJoinNotificationFeed";

        public const string JoinNotificationPanelObjectName = "JoinNotificationPanel";

        public const string JoinNotificationTitleObjectName = "JoinNotificationTitle";

        public const string JoinNotificationEntriesObjectName = "JoinNotificationEntries";

        public const string JoinNotificationEntryTemplateObjectName = "JoinNotificationEntryTemplate";

        public const string JoinNotificationHeaderText = "Session";

        public const int JoinNotificationMaxEntries = 5;

        public const float JoinNotificationEntryLifetimeSeconds = 5f;

        public const float JoinNotificationPanelVisibleSeconds = 5f;

        public const float JoinNotificationPanelWidth = 360f;

        public const float JoinNotificationPanelMinHeight = 180f;

        public const float JoinNotificationPanelMargin = 32f;

        public const string DefaultJoinPlayerDisplayName = "Player";

        public static bool EnableJoinFeedDebugLogs = false;

        public static bool EnableOfflineBootstrapVerboseLogs = false;

        public static readonly Color JoinPanelColor = new Color(0.08f, 0.12f, 0.18f, 0.92f);

        public static readonly Color JoinTitleTextColor = new Color(0.93f, 0.95f, 0.98f, 1f);

        public static readonly Color JoinEntryTextColor = new Color(0.72f, 0.78f, 0.86f, 1f);
    }
}
