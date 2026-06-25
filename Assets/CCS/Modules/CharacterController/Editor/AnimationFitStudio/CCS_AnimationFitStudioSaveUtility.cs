using CCS.Modules.CharacterController;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_AnimationFitStudioSaveUtility
// CATEGORY: Modules / CharacterController / Editor / AnimationFitStudio
// PURPOSE: In-place controller FullDraw clip overwrite for Animation Fit Studio.
// PLACEMENT: Editor utility invoked from pose save workflow.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Never deletes/recreates controller clip assets. Preserves GUID via in-place curve writes.
// =============================================================================

namespace CCS.Modules.CharacterController.Editor.AnimationFitStudio
{
    public enum CCS_AnimationFitStudioSaveMode
    {
        OverwriteControllerClip = 0,
    }

    public sealed class CCS_AnimationFitStudioSaveResult
    {
        public bool Success { get; set; }

        public string SavedAssetPath { get; set; } = string.Empty;

        public bool GuidPreserved { get; set; }

        public bool AnimatorControllerModified { get; set; }

        public bool ControllerAlreadyReferencesClip { get; set; }

        public int ArmBonesWritten { get; set; }

        public int FingerBonesWritten { get; set; }

        public int FingerBonesFound { get; set; }

        public int EditedFingerSegmentsDetected { get; set; }

        public int FingerCurvesWritten { get; set; }

        public int HumanoidMuscleCurvesWritten { get; set; }

        public int TransformCurvesWritten { get; set; }

        public CCS_AnimationFitStudioClipCurveMode ClipCurveMode { get; set; }

        public float RightArmDownUpBefore { get; set; }

        public float RightArmDownUpAfter { get; set; }

        public float RightArmFrontBackBefore { get; set; }

        public float RightArmFrontBackAfter { get; set; }

        public float RightHandDownUpBefore { get; set; }

        public float RightHandDownUpAfter { get; set; }

        public float RightHandInOutBefore { get; set; }

        public float RightHandInOutAfter { get; set; }

        public string ClampedMuscleNames { get; set; } = string.Empty;

        public string CurveHashAfter { get; set; } = string.Empty;

        public bool CurveHashChanged { get; set; }

        public bool ControllerStillReferencesSavedClip { get; set; }

        public bool PoseEditsDetected { get; set; }

        public string CurveHashBefore { get; set; } = string.Empty;

        public string ErrorMessage { get; set; } = string.Empty;
    }

    public static class CCS_AnimationFitStudioSaveUtility
    {
        public static bool TryLoadExistingControllerClip(
            string controllerClipAssetPath,
            out AnimationClip controllerClip,
            out string errorMessage)
        {
            controllerClip = null;
            errorMessage = string.Empty;
            controllerClipAssetPath = NormalizeAssetPath(controllerClipAssetPath);
            if (string.IsNullOrEmpty(controllerClipAssetPath))
            {
                errorMessage = "Controller clip path is missing.";
                return false;
            }

            controllerClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(controllerClipAssetPath);
            if (controllerClip == null)
            {
                errorMessage = "Missing controller clip at " + controllerClipAssetPath + ".";
                return false;
            }

            return true;
        }

        public static bool TryResolveSaveTargetClipPath(
            string controllerClipAssetPath,
            out string targetAssetPath,
            out string errorMessage)
        {
            targetAssetPath = string.Empty;
            errorMessage = string.Empty;
            controllerClipAssetPath = NormalizeAssetPath(controllerClipAssetPath);
            if (string.IsNullOrEmpty(controllerClipAssetPath))
            {
                errorMessage = "Controller clip path is missing.";
                return false;
            }

            targetAssetPath = controllerClipAssetPath;
            return true;
        }

        public static void LogOverwriteResult(CCS_AnimationFitStudioSaveResult result)
        {
            if (result.PoseEditsDetected && !result.CurveHashChanged)
            {
                Debug.LogError(
                    "[Animation Fit Studio] Save failed: visible pose edits were not written to the controller clip.");
            }
            else if (!result.PoseEditsDetected)
            {
                Debug.Log(
                    "[Animation Fit Studio] No pose changes detected; curve hash unchanged.\n"
                    + "Path: "
                    + result.SavedAssetPath);
                return;
            }

            Debug.Log(
                "[Animation Fit Studio] Overwrote controller FullDraw clip in place:\n"
                + "Path: "
                + result.SavedAssetPath
                + "\nClip Curve Mode: "
                + CCS_AnimationFitStudioClipCurveModeUtility.GetDisplayLabel(result.ClipCurveMode)
                + (result.ClipCurveMode == CCS_AnimationFitStudioClipCurveMode.HumanoidMuscleCurves
                    ? "\n[Animation Fit Studio] Humanoid FullDraw saved:"
                        + "\nRight Arm Down-Up before/after: "
                        + result.RightArmDownUpBefore.ToString("F4")
                        + " -> "
                        + result.RightArmDownUpAfter.ToString("F4")
                        + "\nRight Arm Front-Back before/after: "
                        + result.RightArmFrontBackBefore.ToString("F4")
                        + " -> "
                        + result.RightArmFrontBackAfter.ToString("F4")
                        + "\nRight Hand Down-Up before/after: "
                        + result.RightHandDownUpBefore.ToString("F4")
                        + " -> "
                        + result.RightHandDownUpAfter.ToString("F4")
                        + "\nRight Hand In-Out before/after: "
                        + result.RightHandInOutBefore.ToString("F4")
                        + " -> "
                        + result.RightHandInOutAfter.ToString("F4")
                        + "\nClamped values: "
                        + (string.IsNullOrEmpty(result.ClampedMuscleNames) ? "(none)" : result.ClampedMuscleNames)
                    : string.Empty)
                + "\nCurve hash before: "
                + result.CurveHashBefore
                + "\nCurve hash after: "
                + result.CurveHashAfter
                + "\nCurve hash changed: "
                + (result.CurveHashChanged ? "true" : "false")
                + "\nController still references saved clip: "
                + (result.ControllerStillReferencesSavedClip ? "true" : "false")
                + "\nGUID preserved: "
                + (result.GuidPreserved ? "true" : "false")
                + "\nAnimator Controller modified: "
                + (result.AnimatorControllerModified ? "true" : "false")
                + (result.ClipCurveMode == CCS_AnimationFitStudioClipCurveMode.HumanoidMuscleCurves
                    ? "\nHumanoid muscle curves written: " + result.HumanoidMuscleCurvesWritten
                        + "\nTransform curves written: " + result.TransformCurvesWritten
                    : "\nTransform curves written: " + result.TransformCurvesWritten
                        + "\nHumanoid muscle curves written: " + result.HumanoidMuscleCurvesWritten)
                + "\nEdited finger segments detected: "
                + result.EditedFingerSegmentsDetected
                + "\nFinger bones found: "
                + result.FingerBonesFound
                + "\nFinger curves written: "
                + result.FingerCurvesWritten);

            if (result.EditedFingerSegmentsDetected > 0 && result.FingerCurvesWritten == 0)
            {
                Debug.LogError(
                    "[Animation Fit Studio] Save failed: finger edits were detected but no finger curves were written.");
            }

            if (result.ClipCurveMode == CCS_AnimationFitStudioClipCurveMode.HumanoidMuscleCurves
                && result.HumanoidMuscleCurvesWritten <= 0)
            {
                Debug.LogError(
                    "[Animation Fit Studio] Save failed: Humanoid FullDraw clip wrote zero muscle curves.");
            }
        }

        public static void FinalizeSavedClipImport(string savedAssetPath)
        {
            savedAssetPath = NormalizeAssetPath(savedAssetPath);
            if (string.IsNullOrEmpty(savedAssetPath))
            {
                return;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(savedAssetPath, ImportAssetOptions.ForceUpdate);
            AssetDatabase.Refresh();
        }

        private static string NormalizeAssetPath(string assetPath)
        {
            return string.IsNullOrEmpty(assetPath) ? string.Empty : assetPath.Replace('\\', '/');
        }
    }
}
