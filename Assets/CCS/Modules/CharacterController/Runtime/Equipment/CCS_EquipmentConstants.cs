// =============================================================================
// SCRIPT: CCS_EquipmentConstants
// CATEGORY: Modules / CharacterController / Runtime / Equipment
// PURPOSE: Equipment socket IDs, item types, profile paths, and IK object names.
// PLACEMENT: Static constants. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: v0.6.8 adds Revolver M1879 fit profile pack paths and canonical IDs.
// =============================================================================

using UnityEngine;

namespace CCS.Modules.CharacterController
{
    public static class CCS_EquipmentConstants
    {
        public const string AnimationRiggingPackageName = "com.unity.animation.rigging";

        public const string EquipmentSocketsProfileRootPath =
            CCS_CharacterControllerConstants.ModuleRootPath + "/Profiles/EquipmentSockets";

        public const string DefaultEquipmentSocketProfilePath =
            EquipmentSocketsProfileRootPath + "/CCS_DefaultEquipmentSocketProfile.asset";

        public const string EquipmentSocketDefinitionsFolderPath =
            EquipmentSocketsProfileRootPath + "/Sockets";

        public const string VisualRootObjectName = "VisualRoot";

        public const string TestBoneSocketFallbacksObjectName = "CCS_TestBoneSocketFallbacks";

        public const string WeaponIkTargetsObjectName = "CCS_WeaponIKTargets";

        public const string WeaponIkRigObjectName = "Rig_WeaponIK";

        public const string RightHandIkTargetObjectName = "CCS_RightHandIKTarget";

        public const string RightPalmFitGuideObjectName = "RightPalmFitGuide";

        public const string RightTriggerFingerGuideObjectName = "RightTriggerFingerGuide";

        public const string RightBarrelForwardGuideObjectName = "RightBarrelForwardGuide";

        public const string RightElbowHintObjectName = "CCS_RightElbowHint";

        public const string LeftHandIkTargetObjectName = "CCS_LeftHandIKTarget";

        public const string LeftElbowHintObjectName = "CCS_LeftElbowHint";

        public const string WeaponAimTargetObjectName = "CCS_WeaponAimTarget";

        public const string HolsterSocketRightHipId = "CCS_HolsterSocket_RightHip";

        public const string HolsterSocketLeftHipId = "CCS_HolsterSocket_LeftHip";

        public const string HandSocketRightId = "CCS_HandSocket_Right";

        public const string HandSocketLeftId = "CCS_HandSocket_Left";

        public const string BackSocketLongGunAId = "CCS_BackSocket_LongGun_A";

        public const string BackSocketLongGunBId = "CCS_BackSocket_LongGun_B";

        public const string FallbackHipsAnchorName = "Fallback_Hips";

        public const string FallbackRightHandAnchorName = "Fallback_RightHand";

        public const string FallbackLeftHandAnchorName = "Fallback_LeftHand";

        public const string FallbackChestAnchorName = "Fallback_Chest";

        public const string FallbackSpineAnchorName = "Fallback_Spine";

        public static readonly string[] RequiredSocketIds =
        {
            HolsterSocketRightHipId,
            HolsterSocketLeftHipId,
            HandSocketRightId,
            HandSocketLeftId,
            BackSocketLongGunAId,
            BackSocketLongGunBId,
        };

        public static readonly Vector3 FallbackHipsLocalPosition = new Vector3(0f, 1.0f, 0f);

        public static readonly Vector3 FallbackRightHandLocalPosition = new Vector3(0.35f, 1.35f, 0.22f);

        public static readonly Vector3 FallbackLeftHandLocalPosition = new Vector3(-0.35f, 1.35f, 0.22f);

        public static readonly Vector3 FallbackChestLocalPosition = new Vector3(0f, 1.45f, 0.05f);

        public static readonly Vector3 FallbackSpineLocalPosition = new Vector3(0f, 1.35f, -0.05f);

        public const string EquipmentFittingProfileRootPath =
            CCS_CharacterControllerConstants.ModuleRootPath + "/Profiles/EquipmentFitting";

        public const string EquipmentFittingIkProfileFolderPath =
            EquipmentFittingProfileRootPath + "/IK";

        public const string EquipmentFittingHandPoseFolderPath =
            EquipmentFittingProfileRootPath + "/HandPoses";

        public const string EquipmentFitStudioSettingsPath =
            EquipmentFittingProfileRootPath + "/CCS_EquipmentFitStudioSettings.asset";

        public const string DefaultWeaponIkPoseProfilePath =
            EquipmentFittingIkProfileFolderPath + "/CCS_WeaponIKPoseProfile_DefaultRevolver.asset";

        public const string EditorPreviewItemObjectName = "CCS_EDITOR_PREVIEW_ITEM_DO_NOT_SAVE";

        public const string EditorPreviewCameraObjectName = "CCS_EquipmentFitPreviewCamera";

        public const string EditorTestHolsterFitObjectName = "CCS_EDITOR_TEST_HOLSTER_FIT_DO_NOT_SAVE";

        public const string EditorTestEquippedFitObjectName = "CCS_EDITOR_TEST_EQUIPPED_FIT_DO_NOT_SAVE";

        public const string EditorFitPreviewPlayerObjectName = "CCS_EDITOR_FIT_PREVIEW_PLAYER_DO_NOT_SAVE";

        public const string RuntimeHolsterAttachmentRootObjectName = "CCS_RUNTIME_Revolver_HolsterAttachmentRoot";

        public const string RuntimeHolsteredVisualObjectName = "CCS_RUNTIME_Revolver_HolsteredVisual";

        public const string RuntimeEquippedAttachmentRootObjectName = "CCS_RUNTIME_Revolver_EquippedAttachmentRoot";

        public const string RuntimeEquippedAimConvergenceRootObjectName = "CCS_RUNTIME_Revolver_AimConvergenceRoot";

        public const string RuntimeEquippedVisualObjectName = "CCS_RUNTIME_Revolver_EquippedVisual";

        public static readonly string[] EditorTemporaryObjectNames =
        {
            EditorPreviewItemObjectName,
            EditorPreviewCameraObjectName,
            EditorTestHolsterFitObjectName,
            EditorTestEquippedFitObjectName,
            EditorFitPreviewPlayerObjectName,
            "EDITOR_TEST_FIT_LABEL_DO_NOT_SAVE",
        };

        public static readonly string[] RuntimeTemporaryObjectNames =
        {
            RuntimeHolsterAttachmentRootObjectName,
            RuntimeHolsteredVisualObjectName,
            RuntimeEquippedAttachmentRootObjectName,
            RuntimeEquippedAimConvergenceRootObjectName,
            RuntimeEquippedVisualObjectName,
        };

        public const string EquipmentFitStudioVersionLabel = "v0.6.8";

        public const string EquipmentFitStudioMenuPath =
            "CCS/Character Controller/Equipment/Equipment Fit Studio";

        public const string DefaultPreviewWeaponId = RevolverM1879WeaponId;

        public const string DefaultCharacterRigId = TestPlayerCc3BasePlusRigId;

        public const string RevolverM1879WeaponId = "ccs.weapon.revolver.m1879";

        public const string TestPlayerCc3BasePlusRigId = "ccs.character.testplayer.cc3_base_plus";

        public const string RevolverM1879AimPoseId = "revolver.aim.basic";

        public const string RevolverM1879RightHandGripPoseId = "revolver.right_hand.trigger_ready";

        public const string RevolverM1879FitProfileFolderPath =
            EquipmentFittingProfileRootPath + "/RevolverM1879";

        public const string RevolverM1879RightHipHolsterFitPath =
            RevolverM1879FitProfileFolderPath + "/CCS_RevolverM1879_RightHipHolster_Fit.asset";

        public const string RevolverM1879RightHandEquippedFitPath =
            RevolverM1879FitProfileFolderPath + "/CCS_RevolverM1879_RightHandEquipped_Fit.asset";

        public const string RevolverM1879AimIkPosePath =
            RevolverM1879FitProfileFolderPath + "/CCS_RevolverM1879_AimIKPose.asset";

        public const string RevolverM1879RightHandGripPosePath =
            RevolverM1879FitProfileFolderPath + "/CCS_RevolverM1879_RightHandGripPose.asset";

        public const string RevolverM1879FitTuningNotesPath =
            RevolverM1879FitProfileFolderPath + "/README.md";

        public const string EquipmentFitStudioAxisTestProfileFolderPath =
            EquipmentFittingProfileRootPath + "/Test";

        public const string EquipmentFitStudioAxisTestProfilePath =
            EquipmentFitStudioAxisTestProfileFolderPath + "/CCS_EquipmentFitStudio_AxisTest_DO_NOT_SHIP.asset";
    }

    public static class CCS_EquipmentItemTypes
    {
        public const string WeaponRevolver = "weapon.revolver";

        public const string WeaponPistol = "weapon.pistol";

        public const string WeaponRifle = "weapon.rifle";

        public const string WeaponShotgun = "weapon.shotgun";

        public const string WeaponBow = "weapon.bow";

        public const string ToolKnife = "tool.knife";

        public const string ToolHand = "tool.hand";

        public const string ToolLantern = "tool.lantern";

        public const string ToolOffhand = "tool.offhand";
    }

    public enum CCS_EquipmentSocketParentMode
    {
        RealHumanoidBone = 0,
        TestFallbackAnchor = 1,
    }
}
