// =============================================================================
// SCRIPT: CCS_InteractionConstants// CATEGORY: Modules / Interaction / Runtime
// PURPOSE: Canonical paths, IDs, and version metadata for the Interaction module.
// PLACEMENT: Runtime constants. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: v0.4.0 foundation — scanner profile and test interactable assets.
// =============================================================================

using UnityEngine;

namespace CCS.Modules.Interaction{
    public static class CCS_InteractionConstants
    {
        public const string ModuleVersion = "0.4.0";

        public const string ModuleLogCategory = "Interaction";

        public const string ModuleRootPath = "Assets/CCS/Modules/Interaction";

        public const string ScannerProfilePath =
            ModuleRootPath + "/Tests/Profiles/CCS_InteractionScannerProfile_Default.asset";

        public const string ScannerProfileId = "ccs.survival.profile.interaction.scanner.default";

        public const string TestToggleInteractablePrefabPath =
            ModuleRootPath + "/Tests/Prefabs/PF_CCS_TestInteractable_ToggleCube.prefab";

        public const string TestToggleInteractableInstanceName = "PF_CCS_TestInteractable_ToggleCube";

        public const string NetworkedTestPlayerPrefabPath =
            "Assets/CCS/Modules/CharacterController/Tests/Prefabs/PF_CCS_CharacterController_TestPlayer_Networked.prefab";

        public const string MasterTestScenePath =
            "Assets/CCS/Scenes/CharacterController/SCN_CCS_CharacterController_MasterTest.unity";

        public static readonly Vector3 TestToggleInteractablePosition = new Vector3(24f, 0f, 20f);
    }
}
