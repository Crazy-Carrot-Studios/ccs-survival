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

        public const string RevolverAimAnimationsPath = ContentAnimationsRootPath + "/Combat/Aiming/Revolver";

        public const string RevolverAimMasksPath = ContentAnimationsRootPath + "/Masks";

        public const string RevolverAimRightArmMaskLegacyPath =
            RevolverAimMasksPath + "/AM_CCS_Revolver_RightArm_Aim.mask";

        public const string RevolverAimRightArmMaskPath =
            RevolverAimMasksPath + "/AM_CCS_Revolver_UpperBodyRightArm_Aim.mask";

        public const string RevolverIdleToAimClipPath =
            RevolverAimAnimationsPath + "/CCS_WW_Revolver_IdleToAim.anim";

        public const string RevolverAimIdleFullDrawClipPath =
            RevolverAimAnimationsPath + "/CCS_WW_Revolver_AimIdle_FullDraw.anim";

        public const string RevolverReloadIdleRhFutureClipPath =
            RevolverAimAnimationsPath + "/CCS_WW_Revolver_Reload_Idle_RH.anim";

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

        public const string WildWestAnimationPackRootPath = "Assets/YashMakesGames/Wild West Animation Pack";

        public const string WildWestRevolverAnimationsPath = ContentAnimationsRootPath + "/Revolver/WildWest";

        public const string WildWestRevolverRightArmMaskPath =
            WildWestRevolverAnimationsPath + "/CCS_Revolver_RightArm_UpperBody.mask";

        public const string WildWestRevolverAimIdleFullDrawClipPath = RevolverAimIdleFullDrawClipPath;

        public const string WildWestRevolverAimWalkClipPath =
            WildWestRevolverAnimationsPath + "/CCS_WW_Revolver_AimWalk.anim";

        public const string WildWestRevolverIdleToAimClipPath = RevolverIdleToAimClipPath;

        public const string WildWestRevolverAimToIdleClipPath =
            WildWestRevolverAnimationsPath + "/CCS_WW_Revolver_AimToIdle.anim";

        public const string WildWestRevolverWalkToAimWalkClipPath =
            WildWestRevolverAnimationsPath + "/CCS_WW_Revolver_WalkToAimWalk.anim";

        public const string WildWestRevolverAimWalkToWalkClipPath =
            WildWestRevolverAnimationsPath + "/CCS_WW_Revolver_AimWalkToWalk.anim";

        public const string WildWestRevolverFireFanningClipPath =
            CombatRevolverAnimationsPath + "/CCS_WW_Revolver_Fire_Fanning.anim";

        public const string WildWestRevolverAimIdleClipPath =
            WildWestRevolverAimIdleFullDrawClipPath;

        public const string WildWestRevolverFireFanningLegacyRhClipPath =
            WildWestRevolverAnimationsPath + "/CCS_WW_Revolver_Fire_Fanning_RH.anim";

        public const string WildWestRevolverEditedAnimationsPath =
            WildWestRevolverAnimationsPath + "/Edited";

        public const string WildWestRevolverWalkAimedRhClipPath =
            WildWestRevolverAnimationsPath + "/CCS_WW_Revolver_WalkAimed_RH.anim";

        public const string WildWestRevolverWalkAimedClipPath =
            WildWestRevolverWalkAimedRhClipPath;

        public const string WildWestRevolverHipAimedWalkClipPath =
            WildWestRevolverAnimationsPath + "/CCS_WW_Revolver_HipAimedWalk_RH.anim";

        public const string WildWestRevolverDrawQuickdrawClipPath =
            WildWestRevolverAnimationsPath + "/CCS_WW_Revolver_Draw_Quickdraw_RH.anim";

        public const string WildWestRevolverHolsterClipPath =
            WildWestRevolverAnimationsPath + "/CCS_WW_Revolver_Holster_RH.anim";

        public const string WildWestRevolverReloadIdleClipPath = RevolverReloadIdleRhFutureClipPath;

        public const string RevolverAimIdleLegacyClipPath =
            CombatRevolverAnimationsPath + "/CCS_Revolver_AimIdle_UpperBody.anim";

        public const string RevolverFireLegacyClipPath =
            CombatRevolverAnimationsPath + "/CCS_Revolver_Fire_UpperBody.anim";

        public const string RevolverAimIdleClipPath = WildWestRevolverAimIdleClipPath;

        public const string RevolverFireClipPath = WildWestRevolverFireFanningClipPath;

        public const string RevolverIdlePistolLegacyClipPath =
            CombatRevolverAnimationsPath + "/CCS_Revolver_IdlePistol_UpperBody.anim";

        public const string RevolverReloadClipPath =
            CombatRevolverAnimationsPath + "/CCS_Revolver_Reload_UpperBody.anim";

        public const string WildWestRevolverAimIdleRhClipPath =
            WildWestRevolverAnimationsPath + "/CCS_WW_Revolver_AimIdle_RH.anim";

        public const string WildWestRevolverAimIdleFullDrawFitTestClipPath =
            WildWestRevolverEditedAnimationsPath + "/CCS_WW_Revolver_AimIdle_FullDraw_FitTest.anim";

        public const string WildWestRevolverRuntimeDefaultAimIdleClipPath = RevolverAimIdleFullDrawClipPath;

        public const string RevolverRuntimeDefaultAimIdleClipPath = RevolverAimIdleFullDrawClipPath;

        public const string AnimationFitStudioDefaultSourceClipPath = RevolverAimIdleFullDrawClipPath;

        public const string AnimationFitStudioControllerFullDrawClipPath = RevolverAimIdleFullDrawClipPath;

        public const string AnimationFitStudioDefaultFitTestClipPath = RevolverAimIdleFullDrawClipPath;

        public const string AnimationFitStudioWalkAimedFitTestClipPath =
            WildWestRevolverEditedAnimationsPath + "/CCS_WW_Revolver_WalkAimed_RH_FitTest.anim";

        public const string WildWestRevolverAimIdleRhFitTestClipPath =
            WildWestRevolverEditedAnimationsPath + "/CCS_WW_Revolver_AimIdle_RH_FitTest.anim";

        public const string RevolverAimPitchDownFitTestClipPath =
            WildWestRevolverEditedAnimationsPath + "/CCS_WW_Revolver_AimPitch_Down_FitTest.anim";

        public const string RevolverAimPitchCenterFitTestClipPath =
            WildWestRevolverEditedAnimationsPath + "/CCS_WW_Revolver_AimPitch_Center_FitTest.anim";

        public const string RevolverAimPitchUpFitTestClipPath =
            WildWestRevolverEditedAnimationsPath + "/CCS_WW_Revolver_AimPitch_Up_FitTest.anim";

        public const string WildWestRevolverAimIdleRhFirstPersonClipPath =
            WildWestRevolverEditedAnimationsPath + "/CCS_Revolver_AimIdle_RH_FirstPerson.anim";

        public const string AnimationFitStudioFitTestClipSuffix = "_FitTest";

        public const string AnimationFitStudioMenuPath =
            "CCS/Character Controller/Animations/Animation Fit Studio";

        public const string AnimationFitStudioEditorFolderPath =
            ModuleRootPath + "/Editor/AnimationFitStudio";

        public const string AnimationFitStudioWindowSourcePath =
            AnimationFitStudioEditorFolderPath + "/CCS_AnimationFitStudioWindow.cs";

        public const string AnimationFitStudioWindowLayoutSourcePath =
            AnimationFitStudioEditorFolderPath + "/CCS_AnimationFitStudioWindow.Layout.cs";

        public const string AnimationFitStudioPreviewUtilitySourcePath =
            AnimationFitStudioEditorFolderPath + "/CCS_AnimationFitStudioPreviewUtility.cs";

        public const string WildWestRevolverFireFanningRhClipPath =
            WildWestRevolverFireFanningLegacyRhClipPath;

        public const string AnimatorRevolverUpperBodyLayerName = "RevolverUpperBody";

        public const string AnimatorRevolverAimUpperBodyLayerNameObsolete = "Revolver Aim Upper Body";

        public const string AnimatorInteractionReservedLayerName = "Interaction";

        public const string AnimatorInteractionDefaultStateName = "NoInteraction";

        public const string AnimatorInteractPickUpStateName = "Interact_PickUp_RH";

        public const string AnimatorInteractWalkThroughDoorStateName = "Interact_WalkThroughDoor_RH";

        public const string AnimatorPickUpRightHandTriggerParameter = "PickUp_RH";

        public const string AnimatorWalkThroughDoorRightHandTriggerParameter = "WalkThroughDoor_RH";

        public const string InteractionPickUpRightHandClipPath =
            InteractionAnimationsPath + "/CCS_Interaction_PickUp_RH.anim";

        public const string InteractionWalkThroughDoorRightHandClipPath =
            InteractionAnimationsPath + "/CCS_Interaction_WalkThroughDoor_RH.anim";

        public const string AnimatorRevolverNoAimStateName = "NoAim";

        public const string AnimatorRevolverEmptyStateName = AnimatorRevolverNoAimStateName;

        public const string AnimatorRevolverAimToIdleReturnStateName = "Revolver_AimToIdle_Return";

        public const string AnimatorRevolverIdleToAimStateName = "Revolver_IdleToAim";

        public const string AnimatorRevolverAimIdleFullDrawStateName = "Revolver_AimIdle_FullDraw";

        public const string AnimatorRevolverAimToIdleStateName = "Revolver_AimToIdle";

        public const string AnimatorRevolverWalkToAimWalkStateName = "Revolver_WalkToAimWalk";

        public const string AnimatorRevolverAimWalkStateName = "Revolver_AimWalk";

        public const string AnimatorRevolverAimWalkToWalkStateName = "Revolver_AimWalkToWalk";

        public const string AnimatorRevolverAimIdleStateName = AnimatorRevolverAimPitchBlendStateName;

        public const string AnimatorRevolverAimPitchBlendStateName = "Revolver_AimPitch_Blend";

        public const string AnimatorRevolverAimPitchBlendTreeName = "Revolver_AimPitch_BlendTree";

        public const string AnimatorRevolverAimPitchParameter = "RevolverAimPitch";

        public const string AnimatorRevolverFireStateName = "Revolver_Fire";

        public const string AnimatorRevolverReloadStateName = "Revolver_Reload";

        public const string AnimatorRevolverAimHeldParameter = "RevolverAimHeld";

        public const string AnimatorRevolverFireTriggerParameter = "RevolverFireTrigger";

        public const string AnimatorRevolverReloadTriggerParameter = "RevolverReloadTrigger";

        public const string AnimatorRevolverIsReloadingParameter = "RevolverIsReloading";

        public const string AnimatorRevolverIsMovingParameter = "RevolverIsMoving";

        public const float RevolverAimWalkSpeedThreshold = 0.08f;

        public const string AnimatorRevolverRightHandPreviewLayerName = "RevolverRightHandPreview";

        public const string AnimatorRevolverWildWestEmptyStateName = "Revolver_WW_Empty";

        public const string AnimatorRevolverWildWestAimIdleStateName = "Revolver_WW_AimIdle_RH";

        public const string AnimatorRevolverWildWestFireStateName = "Revolver_WW_Fire_Fanning_RH";

        public const string AnimatorRevolverWildWestReloadStateName = "Revolver_WW_Reload_RH";

        public const string PlayerLocomotionAnimatorControllerPath =
            ModuleRootPath + "/Characters/Player/Animations/Controllers/AC_CCS_Player_Locomotion_StarterAssets.controller";

        public const string DefaultMovementProfilePath =
            ModuleRootPath + "/Profiles/Movement/CCS_CharacterMovementProfile_Default.asset";

        public const string DefaultCameraProfilePath =
            ModuleRootPath + "/Profiles/Camera/CCS_CharacterCameraProfile_ThirdPersonSurvival.asset";

        public const string ThirdPersonSurvivalCameraProfilePath = DefaultCameraProfilePath;

        public const string FirstPersonBodyAwareCameraProfilePath =
            ModuleRootPath + "/Profiles/Camera/CCS_CharacterCameraProfile_FirstPersonBodyAware.asset";

        public const string FirstPersonAimCameraProfilePath =
            ModuleRootPath + "/Profiles/Camera/CCS_CharacterCameraProfile_FirstPersonAim.asset";

        public const string DefaultCameraProfileSetPath =
            ModuleRootPath + "/Profiles/Camera/CCS_DefaultCharacterCameraProfileSet.asset";

        public const string AimCameraProfilePath =
            ModuleRootPath + "/Profiles/Camera/CCS_CharacterCameraProfile_AimOverShoulder.asset";

        public const string CameraRigPrefabPath = ModuleRootPath + "/Prefabs/Camera/PF_CCS_CharacterCameraRig.prefab";

        public const string ThirdPersonCinemachineCameraName = "CinemachineCamera_TP";

        public const string AimCinemachineCameraName = "CinemachineCamera_Aim";

        public const string FirstPersonBodyAwareCinemachineCameraName = "CinemachineCamera_FP_BodyAware";

        public const string FirstPersonAimCinemachineCameraName = "CinemachineCamera_FP_Aim";

        public const string CameraPitchTargetObjectName = "CameraPitchTarget";

        public const string CameraLookTargetObjectName = "CameraLookTarget";

        public const string CameraFollowAnchorObjectName = "CameraFollowAnchor";

        public const string FirstPersonCameraAnchorObjectName = "FirstPersonCameraAnchor";

        public const string FirstPersonAimCameraAnchorObjectName = "FirstPersonAimCameraAnchor";

        public const string FirstPersonBodyLookProbeObjectName = "FirstPersonBodyLookProbe";

        public const string PlayerLayerName = "Player";

        public const string PlayerTag = "Player";

        public const string InteractableLayerName = "Interactable";

        public const string LocalSelfHeadHiddenLayerName = "CCS_LocalSelfHeadHidden";

        public const string LocalFirstPersonBodyLayerName = "CCS_LocalFirstPersonBody";

        public const string LocalFirstPersonHeadVisibilityTypeName = "CCS_LocalFirstPersonHeadVisibility";

        public const string FirstPersonHeadlessBodyObjectName = "CCS_FirstPersonHeadlessBody";

        public const string FirstPersonHeadlessBodyMeshAssetName = "CCS_CC3_FirstPerson_HeadlessBody";

        public const string FirstPersonHeadlessBodyMeshFolderPath =
            ModuleRootPath + "/Content/Meshes/FirstPerson";

        public const string FirstPersonHeadlessBodyMeshAssetPath =
            FirstPersonHeadlessBodyMeshFolderPath + "/" + FirstPersonHeadlessBodyMeshAssetName + ".asset";

        public const string PlayerVisualPrefabPath =
            ModuleRootPath + "/Characters/Player/Prefabs/PF_CCS_Player_Visual.prefab";

        public const string Cc3BasePlusBodyFbxPath =
            "Assets/Reallusion/DataLink_Imports/CC3_Base_Plus/CC3_Base_Plus.fbx";

        public const string Cc3BasePlusPrefabPath =
            "Assets/Reallusion/DataLink_Imports/CC3_Base_Plus/Prefabs/CC3_Base_Plus.prefab";

        public const float CameraPitchTargetLocalHeight = 1.48f;

        public static readonly Vector3 CameraLookTargetLocalPosition = new Vector3(0f, 0.10f, 0.25f);

        public const float CameraPitchTargetMinimumLocalHeight = 1.40f;

        public const float CameraPitchTargetMaximumLocalHeight = 1.60f;

        public const float FirstPersonForwardEyeOffsetDefault = 0.22f;

        public const float FirstPersonForwardEyeOffsetMinimum = 0.18f;

        public const float FirstPersonForwardEyeOffsetMaximum = 0.26f;

        public const float FirstPersonVerticalEyeOffsetDefault = 0.05f;

        public const float FirstPersonVerticalEyeOffsetMinimum = 0.03f;

        public const float FirstPersonVerticalEyeOffsetMaximum = 0.08f;

        public const float FirstPersonFieldOfViewDefault = 70f;

        public const float FirstPersonFieldOfViewMinimum = 65f;

        public const float FirstPersonFieldOfViewMaximum = 75f;

        public const float FirstPersonAimFieldOfViewDefault = 58f;

        public const float FirstPersonAimFieldOfViewMinimum = 45f;

        public const float FirstPersonAimFieldOfViewMaximum = 75f;

        public const float FirstPersonAimForwardEyeOffsetDefault = 0.26f;

        public const float FirstPersonAimForwardEyeOffsetMinimum = 0.18f;

        public const float FirstPersonAimForwardEyeOffsetMaximum = 0.34f;

        public const float FirstPersonAimVerticalEyeOffsetDefault = 0.14f;

        public const float FirstPersonAimVerticalEyeOffsetMinimum = 0.08f;

        public const float FirstPersonAimVerticalEyeOffsetMaximum = 0.22f;

        public const float FirstPersonAimHorizontalEyeOffsetDefault = 0f;

        public const float FirstPersonAimHorizontalEyeOffsetMinimum = -0.02f;

        public const float FirstPersonAimHorizontalEyeOffsetMaximum = 0.04f;

        public const float FirstPersonNearClipDefault = 0.03f;

        public const float FirstPersonNearClipMinimum = 0.02f;

        public const float FirstPersonNearClipMaximum = 0.05f;

        public const float FirstPersonPitchMinimum = -53f;

        public const float FirstPersonPitchMinimumLegacy = -58f;

        public const float FirstPersonPitchMaximum = 75f;

        public const float FirstPersonPitchMinimumValidationFloor = -60f;

        public const float FirstPersonAimPitchMinimum = -50f;

        public const float FirstPersonAimPitchMinimumValidationFloor = -60f;

        public static readonly Vector3 FirstPersonBodyAwareHeadTrackedLocalOffsetDefault = new Vector3(0f, 0.06f, 0.10f);

        public static readonly Vector3 FirstPersonBodyAwareHeadTrackedLocalOffsetPrevious = new Vector3(0f, 0.04f, 0.18f);

        public static readonly Vector3 FirstPersonAimHeadTrackedLocalOffsetDefault = new Vector3(0f, 0.14f, 0.26f);

        public static readonly Vector3 FirstPersonAimFixedAnchorLocalOffsetDefault = new Vector3(0f, 0.30f, 0.32f);

        public const float FirstPersonAimFixedAnchorHorizontalMinimum = -0.04f;

        public const float FirstPersonAimFixedAnchorHorizontalMaximum = 0.06f;

        public const float FirstPersonAimFixedAnchorVerticalMinimum = 0.22f;

        public const float FirstPersonAimFixedAnchorVerticalMaximum = 0.38f;

        public const float FirstPersonAimFixedAnchorForwardMinimum = 0.28f;

        public const float FirstPersonAimFixedAnchorForwardMaximum = 0.46f;

        public static readonly Vector3 FirstPersonHeadTrackedLocalOffsetDefault = FirstPersonBodyAwareHeadTrackedLocalOffsetDefault;

        public const float FirstPersonHeadTrackingPositionLerpSpeedDefault = 30f;

        public const string FirstPersonBodyCameraAnchorTypeName = "CCS_FirstPersonBodyCameraAnchor";

        public const float FirstPersonAimBlendMaximumSeconds = 0.12f;

        public const float FirearmAimCameraBlendInSeconds = 0.15f;

        public const float FirearmAimCameraBlendOutSeconds = 0.25f;

        public const float FirstPersonCinemachineDamping = 0f;

        public const float ThirdPersonCameraDistanceTuned = 3.0f;

        public const float ThirdPersonCameraDistanceMinimum = 2.85f;

        public const float ThirdPersonCameraDistanceMaximum = 3.25f;

        public const float AimCameraDistanceTuned = 1.85f;

        public const float AimCameraDistanceMinimum = 1.65f;

        public const float AimCameraDistanceMaximum = 2.05f;

        public const float AimCameraDistanceLegacyLoose = 2.5f;

        public const float AimCameraShoulderOffsetXTuned = 0.45f;

        public const float AimCameraShoulderOffsetXMinimum = 0.40f;

        public const float AimCameraShoulderOffsetXMaximum = 0.55f;

        public const float AimCameraShoulderOffsetYMinimum = 0.10f;

        public const float AimCameraShoulderOffsetYMaximum = 0.25f;

        public const float AimCameraTrackingHeightTuned = 1.48f;

        public const float AimCameraTrackingHeightMinimum = 1.40f;

        public const float AimCameraTrackingHeightMaximum = 1.60f;

        public const float AimCameraFieldOfViewTuned = 58f;

        public const float AimCameraFieldOfViewMinimum = 56f;

        public const float AimCameraFieldOfViewMaximum = 60f;

        public const float ThirdPersonAimPitchDownLimitDegrees = -35f;

        public const float ThirdPersonAimPitchUpLimitDegrees = 55f;

        public const float ThirdPersonAimPitchSmoothingDefault = 14f;

        public const float ThirdPersonAimBodyYawFollowDegreesPerSecondDefault = 540f;

        public const float AimCameraBlendMinimumSeconds = 0.35f;

        public const float AimCameraBlendMaximumSeconds = 0.50f;

        public const float ThirdPersonVerticalArmLengthMinimum = 0.35f;

        public const float ThirdPersonVerticalArmLengthMaximum = 0.55f;

        public const float AimVerticalArmLengthMinimum = 0.18f;

        public const float AimVerticalArmLengthMaximum = 0.32f;

        public const int ThirdPersonCameraActivePriority = 20;

        public const int FirstPersonCameraActivePriority = 10;

        public const int AimCameraActivePriority = 25;

        public const int FirstPersonAimCameraActivePriority = 30;

        public const int FirstPersonBodyAwareCameraActivePriority = 30;

        public const int CinemachineCameraInactivePriority = 0;

        public const int LegacyFirstPersonAimCameraInactivePriority = -10;

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
