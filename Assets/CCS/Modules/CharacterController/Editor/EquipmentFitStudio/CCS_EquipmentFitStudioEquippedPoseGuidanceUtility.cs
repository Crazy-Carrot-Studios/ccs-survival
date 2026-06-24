using System.IO;
using CCS.Project;

// =============================================================================
// SCRIPT: CCS_EquipmentFitStudioEquippedPoseGuidanceUtility
// CATEGORY: Modules / CharacterController / Editor / EquipmentFitStudio
// PURPOSE: Copy and validation for equipped pose guidance and deferred IK notes.
// PLACEMENT: Editor utility used by Fit Studio UI and validation.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Documents two-handed source pose; finger/palm IK deferred for v0.6.8.
// =============================================================================

namespace CCS.Modules.CharacterController.Editor.EquipmentFitStudio
{
    public static class CCS_EquipmentFitStudioEquippedPoseGuidanceUtility
    {
        public const string TwoHandedSourcePoseWarning =
            "Current revolver aim preview uses a two-handed source pose. Final one-handed revolver animation mask is deferred.";

        public const string FingerIkDeferredNote =
            "Finger and palm IK are deferred. Weapon fit comes first. Hand/finger polish comes after the one-handed revolver mask is stable.";

        public const string IkOffNote =
            "IK is OFF. IK controls will not move the arm until Enable IK Preview is clicked.";

        public const string IkPreviewTemporaryWarning =
            "IK Preview is temporary. Saved/runtime IK weights remain 0 unless a future IK profile milestone enables them.";

        public const string OneHandRightHandGuidance =
            "For v0.6.8, tune the gun as close as possible in the current aim pose.\n\n"
            + "If the left hand looks involved, that is expected because the current source pose is two-handed.\n\n"
            + "Final one-handed revolver polish requires a left-arm-excluding avatar mask in the next animation pass.\n\n"
            + "Do not use finger/palm IK yet.";

        public const string TwoHandPreviewGuidance =
            "Two-hand fitting is intended for rifles, shotguns, bows, and future two-handed stances.\n\n"
            + "Left hand support IK and under-grip guides are deferred.";

        public const string PlayModeAimFitPurpose =
            "Tune the equipped right-hand weapon on the live runtime player while aim pose is forced active.";

        public static string GetEquippedPoseTypeLabel(CCS_EquipmentFitStudioEquippedPoseType poseType)
        {
            switch (poseType)
            {
                case CCS_EquipmentFitStudioEquippedPoseType.TwoHandWeaponPreview:
                    return "Two-Hand Weapon Preview (future/experimental)";
                default:
                    return "One-Hand Revolver";
            }
        }

        public static CCS_EquipmentFitStudioEquippedPoseType GetDefaultPoseTypeForWeapon(string weaponId)
        {
            if (weaponId == CCS_EquipmentConstants.RevolverM1879WeaponId)
            {
                return CCS_EquipmentFitStudioEquippedPoseType.OneHandRevolver;
            }

            return CCS_EquipmentFitStudioEquippedPoseType.TwoHandWeaponPreview;
        }

        public static bool ShouldShowTwoHandedSourceWarning(CCS_EquipmentFitStudioEquippedPoseType poseType)
        {
            return poseType == CCS_EquipmentFitStudioEquippedPoseType.OneHandRevolver;
        }

        public static CCS_SurvivalValidationResult ValidateEquippedPoseGuidanceFoundation()
        {
            string guidancePath =
                CCS_CharacterControllerConstants.ModuleRootPath
                + "/Editor/EquipmentFitStudio/CCS_EquipmentFitStudioEquippedPoseGuidanceUtility.cs";
            if (!File.Exists(guidancePath))
            {
                return CCS_SurvivalValidationResult.Fail("Missing CCS_EquipmentFitStudioEquippedPoseGuidanceUtility.");
            }

            string revampPath =
                CCS_CharacterControllerConstants.ModuleRootPath
                + "/Editor/EquipmentFitStudio/CCS_EquipmentFitStudioWindow.Revamp.cs";
            if (!File.Exists(revampPath))
            {
                return CCS_SurvivalValidationResult.Fail("Missing equipped pose guidance Fit Studio revamp partial.");
            }

            string revampSource = File.ReadAllText(revampPath);
            if (!revampSource.Contains("TwoHandedSourcePoseWarning")
                && !revampSource.Contains("two-handed source pose"))
            {
                return CCS_SurvivalValidationResult.Fail("Fit Studio must warn about two-handed revolver aim source pose.");
            }

            if (!revampSource.Contains("FingerIkDeferredNote")
                && !revampSource.Contains("one-hand animation mask is deferred")
                && !revampSource.Contains("Finger and palm IK are deferred"))
            {
                return CCS_SurvivalValidationResult.Fail("Fit Studio must defer finger/palm IK for v0.6.8.");
            }

            return CCS_SurvivalValidationResult.Pass("Equipped pose guidance foundation validated.");
        }
    }
}
