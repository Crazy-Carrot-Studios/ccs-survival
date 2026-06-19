// =============================================================================
// SCRIPT: CCS_TestPlayerPrefabConstants
// CATEGORY: Modules / CharacterController / Tests / Runtime
// PURPOSE: Canonical paths for the shared network-capable master test player prefab.
// PLACEMENT: Runtime constants. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-19
// NOTES: v0.2.4 canonical path for solo and multiplayer runtime spawning.
// =============================================================================

namespace CCS.Modules.CharacterController.Tests
{
    public static class CCS_TestPlayerPrefabConstants
    {
        public const string NetworkedPlayerPrefabPath =
            "Assets/CCS/Modules/CharacterController/Tests/Prefabs/PF_CCS_CharacterController_TestPlayer_Networked.prefab";

        // Negative-validation only. v0.2.4 removed the deprecated solo prefab asset.
        public const string DeprecatedOfflinePlayerPrefabPath =
            "Assets/CCS/Modules/CharacterController/Prefabs/Player/PF_CCS_CharacterController_TestPlayer.prefab";

        // Negative-validation only. Canonical networked prefab lives under Tests/Prefabs/.
        public const string DeprecatedNetworkedPlayerDuplicatePrefabPath =
            "Assets/CCS/Modules/CharacterController/Prefabs/Player/PF_CCS_CharacterController_TestPlayer_Networked.prefab";

        public const string DefaultDisplayProfilePath =
            "Assets/CCS/Modules/CharacterController/Profiles/TestPlayer/CCS_TestPlayerDisplayProfile_Default.asset";

        public const string NetworkedPlayerInstanceName = "PF_CCS_CharacterController_TestPlayer_Networked";

        // Negative-validation only. Master Test must not contain scene-placed legacy solo player roots.
        public const string DeprecatedOfflinePlayerInstanceName = "PF_CCS_CharacterController_TestPlayer";

        public const string NameplateRootObjectName = "NameplateRoot";

        public const string NameplateTextObjectName = "PlayerNameText";

        public const string CapsuleVisualName = "CapsuleVisual";

        public const string GlassesVisualName = "VisualGlasses";

        public const string DefaultDisplayName = "Player";
    }
}
