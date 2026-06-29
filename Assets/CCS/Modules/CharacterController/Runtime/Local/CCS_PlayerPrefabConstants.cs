// =============================================================================
// SCRIPT: CCS_PlayerPrefabConstants
// CATEGORY: Modules / CharacterController / Runtime / Local
// PURPOSE: Canonical paths for the shared network-capable player prefab.
// PLACEMENT: Runtime constants. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-19
// NOTES: v0.7.2 production prefab path for solo and multiplayer runtime spawning.
// =============================================================================

using CCS.Modules.CharacterController;

namespace CCS.Modules.CharacterController.Local {
    public static class CCS_PlayerPrefabConstants
    {
        public const string NetworkedPlayerPrefabPath =
            "Assets/CCS/Modules/CharacterController/Prefabs/Player/PF_CCS_CharacterController_Player_Networked.prefab";

        // Negative-validation only. v0.2.4 removed the deprecated solo prefab asset.
        public const string DeprecatedOfflinePlayerPrefabPath =
            "Assets/CCS/Modules/CharacterController/Prefabs/Player/PF_CCS_CharacterController_TestPlayer.prefab";

        // Negative-validation only. v0.7.2 removed Tests/Prefabs networked player path.
        public const string DeprecatedTestsNetworkedPlayerPrefabPath =
            "Assets/CCS/Modules/CharacterController/Tests/Prefabs/PF_CCS_CharacterController_TestPlayer_Networked.prefab";

        // Negative-validation only. Legacy duplicate under Prefabs/Player before v0.7.2 rename.
        public const string DeprecatedNetworkedPlayerDuplicatePrefabPath =
            "Assets/CCS/Modules/CharacterController/Prefabs/Player/PF_CCS_CharacterController_TestPlayer_Networked.prefab";

        public const string DefaultDisplayProfilePath =
            "Assets/CCS/Modules/CharacterController/Runtime/Profiles/Profiles/TestPlayer/CCS_TestPlayerDisplayProfile_Default.asset";

        public const string NetworkedPlayerInstanceName = "PF_CCS_CharacterController_Player_Networked";

        // Negative-validation only. Validation scene must not contain scene-placed legacy solo player roots.
        public const string DeprecatedOfflinePlayerInstanceName = "PF_CCS_CharacterController_TestPlayer";

        public const string NameplateRootObjectName = "NameplateRoot";

        public const string NameplateTextObjectName = "PlayerNameText";

        public const string CapsuleVisualName = "CapsuleVisual";

        public const string GlassesVisualName = "VisualGlasses";

        public const string DefaultDisplayName = "Player";
    }
}
