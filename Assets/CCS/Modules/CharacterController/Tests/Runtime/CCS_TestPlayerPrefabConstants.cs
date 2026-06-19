// =============================================================================
// SCRIPT: CCS_TestPlayerPrefabConstants
// CATEGORY: Modules / CharacterController / Tests / Runtime
// PURPOSE: Canonical paths for the shared network-capable master test player prefab.
// PLACEMENT: Runtime constants. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-19
// NOTES: v0.2.2 single prefab path for solo and multiplayer runtime spawning.
// =============================================================================

namespace CCS.Modules.CharacterController.Tests
{
    public static class CCS_TestPlayerPrefabConstants
    {
        public const string NetworkedPlayerPrefabPath =
            "Assets/CCS/Modules/CharacterController/Tests/Prefabs/PF_CCS_CharacterController_TestPlayer_Networked.prefab";

        public const string DeprecatedOfflinePlayerPrefabPath =
            "Assets/CCS/Modules/CharacterController/Prefabs/Player/PF_CCS_CharacterController_TestPlayer.prefab";

        public const string DefaultDisplayProfilePath =
            "Assets/CCS/Modules/CharacterController/Profiles/TestPlayer/CCS_TestPlayerDisplayProfile_Default.asset";

        public const string NetworkedPlayerInstanceName = "PF_CCS_CharacterController_TestPlayer_Networked";

        public const string DeprecatedOfflinePlayerInstanceName = "PF_CCS_CharacterController_TestPlayer";
    }
}
