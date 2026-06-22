// =============================================================================
// SCRIPT: CCS_CharacterControllerConstants
// CATEGORY: Modules / CharacterController / Runtime
// PURPOSE: Module paths, input map names, profile IDs, and validation constants.
// PLACEMENT: Static constants. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: v0.2.4 active module scope. No gameplay dependencies outside this module.
// =============================================================================

using UnityEngine;

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

        public const string ContentAnimationsRootPath = ModuleRootPath + "/Content/Animations";

        public const string LocomotionAnimationsPath = ContentAnimationsRootPath + "/Locomotion";

        public const string InteractionAnimationsPath = ContentAnimationsRootPath + "/Interaction";

        public const string CombatRevolverAnimationsPath = ContentAnimationsRootPath + "/Combat/Revolver";

        public const string AimStrafeAnimationsPath = ContentAnimationsRootPath + "/Combat/AimStrafe";

        public const string AimStrafeWalkFwdClipPath = AimStrafeAnimationsPath + "/CCS_AimStrafe_WalkFwd.anim";

        public const string AimStrafeWalkBwdClipPath = AimStrafeAnimationsPath + "/CCS_AimStrafe_WalkBwd.anim";

        public const string AimStrafeStrafeLeftClipPath = AimStrafeAnimationsPath + "/CCS_AimStrafe_StrafeLeft.anim";

        public const string AimStrafeStrafeRightClipPath = AimStrafeAnimationsPath + "/CCS_AimStrafe_StrafeRight.anim";

        public const string AnimatorAimStrafeLocomotionStateName = "AimStrafe_Locomotion";

        public const string AnimatorAimStrafeBlendTreeName = "AimStrafe_BlendTree";

        public const string MapMainFbxPath = "Assets/MovementAnimsetPro/Animations/MovementAnimsetPro.fbx";

        public const string MapAdditionalsFbxPath = "Assets/MovementAnimsetPro/Animations/MovementAnimsetPro_Additionals.fbx";

        public const string VendorSourceInvectorAnimationsPath = "Assets/VendorSource/Invector/Shooter/Animations";

        public const string InvectorUpperBodyPosesFbxPath =
            VendorSourceInvectorAnimationsPath + "/Shooter_UpperBodyPoses.fbx";

        public const string InvectorShotReloadFbxPath =
            VendorSourceInvectorAnimationsPath + "/Shooter_Shot&Reload.fbx";

        public const string LegacyInvectorAnimationsRootPath =
            "Assets/Invector-3rdPersonController/Shooter/3DModels/Animations";

        public const string RevolverUpperBodyMaskPath =
            CombatRevolverAnimationsPath + "/CCS_Revolver_UpperBody.mask";

        public const string RevolverAimIdleClipPath =
            CombatRevolverAnimationsPath + "/CCS_Revolver_AimIdle_UpperBody.anim";

        public const string RevolverIdlePistolClipPath =
            CombatRevolverAnimationsPath + "/CCS_Revolver_IdlePistol_UpperBody.anim";

        public const string RevolverFireClipPath =
            CombatRevolverAnimationsPath + "/CCS_Revolver_Fire_UpperBody.anim";

        public const string RevolverReloadClipPath =
            CombatRevolverAnimationsPath + "/CCS_Revolver_Reload_UpperBody.anim";

        public const string AnimatorRevolverUpperBodyLayerName = "RevolverUpperBody";

        public const string AnimatorRevolverEmptyStateName = "Revolver_Empty";

        public const string AnimatorRevolverAimIdleStateName = "Revolver_AimIdle";

        public const string AnimatorRevolverFireStateName = "Revolver_Fire";

        public const string AnimatorRevolverReloadStateName = "Revolver_Reload";

        public const string AnimatorRevolverAimHeldParameter = "RevolverAimHeld";

        public const string AnimatorRevolverFireTriggerParameter = "RevolverFireTrigger";

        public const string AnimatorRevolverReloadTriggerParameter = "RevolverReloadTrigger";

        public const string AnimatorRevolverIsReloadingParameter = "RevolverIsReloading";

        public const string PlayerLocomotionAnimatorControllerPath =
            ModuleRootPath + "/Characters/Player/Animations/Controllers/AC_CCS_Player_Locomotion_StarterAssets.controller";

        public const string DefaultMovementProfilePath =
            ModuleRootPath + "/Profiles/Movement/CCS_CharacterMovementProfile_Default.asset";

        public const string DefaultCameraProfilePath =
            ModuleRootPath + "/Profiles/Camera/CCS_CharacterCameraProfile_ThirdPersonSurvival.asset";

        public const string DefaultCameraProfileSetPath =
            ModuleRootPath + "/Profiles/Camera/CCS_DefaultCharacterCameraProfileSet.asset";

        public const string AimCameraProfilePath =
            ModuleRootPath + "/Profiles/Camera/CCS_CharacterCameraProfile_AimOverShoulder.asset";

        public const string CameraRigPrefabPath = ModuleRootPath + "/Prefabs/Camera/PF_CCS_CharacterCameraRig.prefab";

        public const string ThirdPersonCinemachineCameraName = "CinemachineCamera_TP";

        public const string AimCinemachineCameraName = "CinemachineCamera_Aim";

        public const string CameraPitchTargetObjectName = "CameraPitchTarget";

        public const string CameraLookTargetObjectName = "CameraLookTarget";

        public const string CameraFollowAnchorObjectName = "CameraFollowAnchor";

        public const string PlayerLayerName = "Player";

        public const string PlayerTag = "Player";

        public const string InteractableLayerName = "Interactable";

        public const float CameraPitchTargetLocalHeight = 1.48f;

        public static readonly Vector3 CameraLookTargetLocalPosition = new Vector3(0f, 0.10f, 0.25f);

        public const float CameraPitchTargetMinimumLocalHeight = 1.40f;

        public const float CameraPitchTargetMaximumLocalHeight = 1.60f;

        public const float ThirdPersonCameraDistanceTuned = 3.0f;

        public const float ThirdPersonCameraDistanceMinimum = 2.85f;

        public const float ThirdPersonCameraDistanceMaximum = 3.25f;

        public const float AimCameraDistanceTuned = 1.5f;

        public const float AimCameraDistanceMinimum = 1.35f;

        public const float AimCameraDistanceMaximum = 1.75f;

        public const float ThirdPersonVerticalArmLengthMinimum = 0.35f;

        public const float ThirdPersonVerticalArmLengthMaximum = 0.55f;

        public const float AimVerticalArmLengthMinimum = 0.18f;

        public const float AimVerticalArmLengthMaximum = 0.32f;

        public const int ThirdPersonCameraActivePriority = 10;

        public const int AimCameraActivePriority = 20;

        public const int CinemachineCameraInactivePriority = 0;

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

        public const string AimActionName = "Aim";

        public const string FireActionName = "Fire";

        public const string ReloadActionName = "Reload";

        public const string MovementProfileId = "ccs.survival.profile.character.movement.default";

        public const string CameraProfileId = "ccs.survival.profile.character.camera.thirdperson";

        public const string CameraProfileSetId = "ccs.survival.profile.character.camera.set.default";

        public const bool EnableJumpDebugLogs = false;

        public const float DefaultJumpHeight = 1.25f;

        public const float DefaultGravity = -20f;

        public const float DefaultCoyoteTime = 0.1f;

        public const float DefaultJumpBufferTime = 0.1f;

        public const float DefaultAirControl = 0.4f;

        public const string AnimatorIsAimingMovementModeParameter = "IsAimingMovementMode";

        public const string AnimatorAimMoveXParameter = "AimMoveX";

        public const string AnimatorAimMoveYParameter = "AimMoveY";

        public const float DefaultAimMovementSpeedMultiplier = 0.55f;

        public const float DefaultAimRotationSpeedDegrees = 720f;

        public const float DefaultAimStrafeDeadZone = 0.05f;

        public const float DefaultAimBackpedalMultiplier = 0.80f;

        public const float DefaultAimSideStrafeMultiplier = 0.90f;

        public const float MinimumAimRotationSpeedDegrees = 360f;
    }
}
