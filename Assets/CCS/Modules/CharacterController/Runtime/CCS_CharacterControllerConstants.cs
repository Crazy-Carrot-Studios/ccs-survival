// =============================================================================
// SCRIPT: CCS_CharacterControllerConstants
// CATEGORY: Modules / CharacterController / Runtime
// PURPOSE: Module paths, input map names, profile IDs, and validation constants.
// PLACEMENT: Static constants. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: v0.2.0 foundation. No gameplay dependencies outside this module.
// =============================================================================

namespace CCS.Modules.CharacterController
{
    public static class CCS_CharacterControllerConstants
    {
        public const string ModuleVersion = "0.2.0";

        public const string ModuleLogCategory = "Character Controller";

        public const string ModuleRootPath = "Assets/CCS/Modules/CharacterController";

        public const string RuntimeAsmdefPath = ModuleRootPath + "/Runtime/CCS.Modules.CharacterController.Runtime.asmdef";

        public const string EditorAsmdefPath = ModuleRootPath + "/Editor/CCS.Modules.CharacterController.Editor.asmdef";

        public const string InputActionsAssetPath =
            ModuleRootPath + "/Content/Input/CCS_CharacterController_InputActions.inputactions";

        public const string DefaultMovementProfilePath =
            ModuleRootPath + "/Profiles/Movement/CCS_CharacterMovementProfile_Default.asset";

        public const string DefaultCameraProfilePath =
            ModuleRootPath + "/Profiles/Camera/CCS_CharacterCameraProfile_ThirdPersonSurvival.asset";

        public const string DefaultCameraProfileSetPath =
            ModuleRootPath + "/Profiles/Camera/CCS_DefaultCharacterCameraProfileSet.asset";

        public const string TestPrefabPath =
            ModuleRootPath + "/Prefabs/PF_CCS_CharacterController_TestPlayer.prefab";

        public const string InputActionMapName = "Gameplay";

        public const string MoveActionName = "Move";

        public const string LookActionName = "Look";

        public const string SprintActionName = "Sprint";

        public const string JumpActionName = "Jump";

        public const string ToggleCursorActionName = "ToggleCursor";

        public const string CameraZoomActionName = "CameraZoom";

        public const string MovementProfileId = "ccs.survival.profile.character.movement.default";

        public const string CameraProfileId = "ccs.survival.profile.character.camera.thirdperson";

        public const string CameraProfileSetId = "ccs.survival.profile.character.camera.set.default";
    }
}
