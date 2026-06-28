// =============================================================================
// SCRIPT: CCS_PlayerPrefabConstants
// CATEGORY: Modules / CharacterController / Runtime
// PURPOSE: Canonical paths and hierarchy names for production and test player prefabs.
// PLACEMENT: Runtime constants. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-25
// NOTES: v0.8.0 production player prefab architecture.
// =============================================================================

namespace CCS.Modules.CharacterController
{
    public static class CCS_PlayerPrefabConstants
    {
        public const string ProductionPlayerPrefabPath =
            "Assets/CCS/Prefabs/Player/PF_CCS_Player_Networked_Runtime.prefab";

        public const string TestHarnessPlayerPrefabPath =
            "Assets/CCS/Prefabs/Player/PF_CCS_Player_Networked_TestHarness.prefab";

        public const string LegacyMasterTestPlayerPrefabPath =
            "Assets/CCS/Modules/CharacterController/Tests/Prefabs/PF_CCS_CharacterController_TestPlayer_Networked.prefab";

        public const string ProductionPlayerInstanceName = "PF_CCS_Player_Networked_Runtime";

        public const string TestHarnessPlayerInstanceName = "PF_CCS_Player_Networked_TestHarness";

        public const string RuntimeSystemsObjectName = "RuntimeSystems";

        public const string PresentationObjectName = "Presentation";

        public const string PlayerLocalUiObjectName = "PlayerLocalUI";

        public const string TestVisualsObjectName = "TestVisuals";

        public const string RealCharacterModelObjectName = "RealCharacterModel";

        public const string PlayerVisualPrefabInstanceName = "PF_CCS_Player_Visual";

        public const int ProductionRootComponentHardTarget = 8;

        public const int ProductionRootComponentTransitionalTarget = 12;
    }
}
