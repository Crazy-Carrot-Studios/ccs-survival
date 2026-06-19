// =============================================================================
// SCRIPT: CCS_CharacterControllerConstants
// CATEGORY: Modules / CharacterController / Runtime
// PURPOSE: Module paths, input map names, profile IDs, and validation constants.
// PLACEMENT: Static constants. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: v0.2.4 active module scope. No gameplay dependencies outside this module.
// =============================================================================

namespace CCS.Modules.CharacterController
{
    public static class CCS_CharacterControllerConstants
    {
        public const string ModuleVersion = "0.2.4";

        public const string ModuleLogCategory = "Character Controller";

        public const string ModuleRootPath = "Assets/CCS/Modules/CharacterController";

        public const string RuntimeAsmdefPath = ModuleRootPath + "/Runtime/CCS.Modules.CharacterController.Runtime.asmdef";

        public const string EditorAsmdefPath = ModuleRootPath + "/Editor/CCS.Modules.CharacterController.Editor.asmdef";

        public const string InputActionsAssetPath =
            ModuleRootPath + "/Content/Input/CCS_CharacterController_InputActions.inputactions";

        public const string LookActionReferencePath =
            ModuleRootPath + "/Content/Input/CCS_CharacterController_LookActionReference.asset";

        public const string DefaultMovementProfilePath =
            ModuleRootPath + "/Profiles/Movement/CCS_CharacterMovementProfile_Default.asset";

        public const string DefaultCameraProfilePath =
            ModuleRootPath + "/Profiles/Camera/CCS_CharacterCameraProfile_ThirdPersonSurvival.asset";

        public const string DefaultCameraProfileSetPath =
            ModuleRootPath + "/Profiles/Camera/CCS_DefaultCharacterCameraProfileSet.asset";

        public const string TestPrefabPath =
            "Assets/CCS/Modules/CharacterController/Tests/Prefabs/PF_CCS_CharacterController_TestPlayer_Networked.prefab";

        public const string MasterTestScenePath =
            "Assets/CCS/Scenes/CharacterController/SCN_CCS_CharacterController_MasterTest.unity";

        public const string TestScenePath =
            "Assets/CCS/Scenes/CharacterController/SCN_CCS_CharacterController_Test.unity";

        public const string TestGroundPrefabPath =
            ModuleRootPath + "/Prefabs/Environment/PF_CCS_TestGround_OneMeterGrid.prefab";

        public const string TestGroundGridTexturePath =
            ModuleRootPath + "/Materials/Environment/T_CCS_TestGround_1mGrid.png";

        public const string TestGroundGridMaterialPath =
            ModuleRootPath + "/Materials/Environment/M_CCS_TestGround_1mGrid.mat";

        public const string TestGroundObjectName = "CCS_TestGround_OneMeterGrid";

        public const string TestGroundPrefabName = "PF_CCS_TestGround_OneMeterGrid";

        public const string TestSceneLabelObjectName = "CCS_TestSceneLabel";

        public const float TestGroundPlaneScale = 20f;

        public const float TestGroundGridMetersPerCell = 1f;

        public const float TestGroundTextureMetersPerRepeat = 10f;

        public const float TestGroundMaterialTiling = 20f;

        public const string InputActionMapName = "Gameplay";

        public const string MoveActionName = "Move";

        public const string LookActionName = "Look";

        public const string LookOrbitHorizontalAxisName = "Look Orbit X";

        public const string LookOrbitVerticalAxisName = "Look Orbit Y";

        public const string SprintActionName = "Sprint";

        public const string JumpActionName = "Jump";

        public const string ToggleCursorActionName = "ToggleCursor";

        public const string CameraZoomActionName = "CameraZoom";

        public const string MovementProfileId = "ccs.survival.profile.character.movement.default";

        public const string CameraProfileId = "ccs.survival.profile.character.camera.thirdperson";

        public const string CameraProfileSetId = "ccs.survival.profile.character.camera.set.default";

        public const bool EnableJumpDebugLogs = false;

        public const float DefaultJumpHeight = 1.25f;

        public const float DefaultGravity = -20f;

        public const float DefaultCoyoteTime = 0.1f;

        public const float DefaultJumpBufferTime = 0.1f;

        public const float DefaultAirControl = 0.4f;
    }
}
