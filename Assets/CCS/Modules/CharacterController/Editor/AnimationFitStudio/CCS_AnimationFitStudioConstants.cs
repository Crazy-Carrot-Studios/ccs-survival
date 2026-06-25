// =============================================================================
// SCRIPT: CCS_AnimationFitStudioConstants
// CATEGORY: Modules / CharacterController / Editor / AnimationFitStudio
// PURPOSE: Editor-only preview object names and layout labels for Animation Fit Studio.
// PLACEMENT: Shared constants for Animation Fit Studio editor utilities.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: All preview objects use DO_NOT_SAVE names and must never persist to scene/prefab.
// =============================================================================

namespace CCS.Modules.CharacterController.Editor.AnimationFitStudio
{
    public static class CCS_AnimationFitStudioConstants
    {
        public const string VersionLabel = "v0.6.16";

        public const string PreviewPlayerObjectName = "CCS_EDITOR_ANIMATION_FIT_PREVIEW_PLAYER_DO_NOT_SAVE";

        public const string WeaponAttachmentRootObjectName =
            "CCS_EDITOR_ANIMATION_FIT_WEAPON_ATTACHMENT_ROOT_DO_NOT_SAVE";

        public const string PreviewWeaponObjectName = "CCS_EDITOR_ANIMATION_FIT_PREVIEW_WEAPON_DO_NOT_SAVE";

        public const string PreviewCameraObjectName = "CCS_AnimationFitPreviewCamera";

        public const string DefaultWeaponId = "ccs.weapon.revolver.m1879";

        public const string DefaultEquippedFitProfileName = "CCS_RevolverM1879_RightHandEquipped_Fit";

        public const string DefaultSourceClipName = "CCS_WW_Revolver_AimIdle_FullDraw";

        public const string DefaultAimedMovementClipName = "CCS_WW_Revolver_WalkAimed_RH";

        public const string DefaultControllerFullDrawClipFileName = "CCS_WW_Revolver_AimIdle_FullDraw.anim";

        public const string DefaultFitTestClipFileName = DefaultControllerFullDrawClipFileName;
    }
}
