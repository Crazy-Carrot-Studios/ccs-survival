// =============================================================================
// SCRIPT: CCS_EquipmentConstants
// CATEGORY: Modules / CharacterController / Runtime / Equipment
// PURPOSE: Equipment socket IDs, item types, profile paths, and IK object names.
// PLACEMENT: Static constants. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: v0.6.7 adds Equipment Fit Studio paths and preview object names.
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

        public const string EquipmentFitStudioMenuPath =
            "CCS/Character Controller/Equipment/Equipment Fit Studio";

        public const string DefaultPreviewWeaponId = "weapon.revolver";

        public const string DefaultCharacterRigId = "ccs.testplayer.humanoid";
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
