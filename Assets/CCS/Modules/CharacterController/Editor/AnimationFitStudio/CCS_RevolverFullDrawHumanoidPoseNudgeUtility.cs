using CCS.Modules.CharacterController;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_RevolverFullDrawHumanoidPoseNudgeUtility
// CATEGORY: Modules / CharacterController / Editor / AnimationFitStudio
// PURPOSE: Applies configurable Humanoid muscle deltas to controller FullDraw clip.
// PLACEMENT: Editor menu/batch utility for immediate arm aiming offset verification.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Flip delta signs at top if visual direction is inverted during manual review.
// =============================================================================

namespace CCS.Modules.CharacterController.Editor.AnimationFitStudio
{
    public sealed class CCS_RevolverFullDrawHumanoidNudgeResult
    {
        public bool Success { get; set; }

        public string ClipPath { get; set; } = string.Empty;

        public string CurveHashBefore { get; set; } = string.Empty;

        public string CurveHashAfter { get; set; } = string.Empty;

        public bool CurveHashChanged { get; set; }

        public bool ControllerStillReferencesSavedClip { get; set; }

        public bool GuidPreserved { get; set; }

        public string ErrorMessage { get; set; } = string.Empty;
    }

    public static class CCS_RevolverFullDrawHumanoidPoseNudgeUtility
    {
        public const float RightArmDownUpDelta = 0.12f;
        public const float RightArmFrontBackDelta = 0.08f;
        public const float RightShoulderDownUpDelta = 0.04f;
        public const float RightShoulderFrontBackDelta = 0.04f;
        public const float RightHandDownUpDelta = 0.03f;
        public const float RightHandInOutDelta = 0.02f;

        private static readonly (string MuscleName, float Delta)[] DefaultMuscleDeltas =
        {
            ("Right Arm Down-Up", RightArmDownUpDelta),
            ("Right Arm Front-Back", RightArmFrontBackDelta),
            ("Right Shoulder Down-Up", RightShoulderDownUpDelta),
            ("Right Shoulder Front-Back", RightShoulderFrontBackDelta),
            ("Right Hand Down-Up", RightHandDownUpDelta),
            ("Right Hand In-Out", RightHandInOutDelta),
        };

        public static bool ApplyDefaultRightArmAimingNudge(out CCS_RevolverFullDrawHumanoidNudgeResult result)
        {
            result = new CCS_RevolverFullDrawHumanoidNudgeResult();
            if (!CCS_AnimationFitStudioRuntimeControllerClipUtility.TryResolveControllerFullDrawSaveTarget(
                    out string controllerClipPath,
                    out AnimationClip controllerClip,
                    out string resolveError))
            {
                result.ErrorMessage = resolveError;
                return false;
            }

            if (!string.Equals(
                    controllerClipPath,
                    CCS_CharacterControllerConstants.RevolverAimIdleFullDrawClipPath,
                    System.StringComparison.OrdinalIgnoreCase))
            {
                result.ErrorMessage =
                    "Controller FullDraw motion path "
                    + controllerClipPath
                    + " does not match expected "
                    + CCS_CharacterControllerConstants.RevolverAimIdleFullDrawClipPath
                    + ".";
                return false;
            }

            if (CCS_AnimationFitStudioClipCurveModeUtility.DetectClipCurveMode(controllerClip)
                != CCS_AnimationFitStudioClipCurveMode.HumanoidMuscleCurves)
            {
                result.ErrorMessage = "FullDraw clip is not Humanoid muscle mode.";
                return false;
            }

            string guidBefore = AssetDatabase.AssetPathToGUID(controllerClipPath);
            result.ClipPath = controllerClipPath;
            result.CurveHashBefore = CCS_AnimationFitStudioCurveHashUtility.ComputeCurveHash(controllerClip);

            int musclesUpdated = 0;
            for (int i = 0; i < DefaultMuscleDeltas.Length; i++)
            {
                (string muscleName, float delta) = DefaultMuscleDeltas[i];
                if (CCS_AnimationFitStudioHumanoidMuscleWriteUtility.ApplyMuscleDeltaToClip(
                        controllerClip,
                        muscleName,
                        delta,
                        out _))
                {
                    musclesUpdated++;
                }
            }

            if (musclesUpdated == 0)
            {
                result.ErrorMessage = "No Humanoid muscle curves were updated.";
                return false;
            }

            EditorUtility.SetDirty(controllerClip);
            CCS_AnimationFitStudioSaveUtility.FinalizeSavedClipImport(controllerClipPath);

            AnimationClip reloadedClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(controllerClipPath);
            result.CurveHashAfter = CCS_AnimationFitStudioCurveHashUtility.ComputeCurveHash(reloadedClip);
            result.CurveHashChanged = result.CurveHashBefore != result.CurveHashAfter;
            result.GuidPreserved = guidBefore == AssetDatabase.AssetPathToGUID(controllerClipPath);
            result.ControllerStillReferencesSavedClip =
                CCS_AnimationFitStudioRuntimeControllerClipUtility.ControllerReferencesClip(controllerClipPath);

            if (!result.CurveHashChanged)
            {
                result.ErrorMessage = "Curve hash unchanged after FullDraw humanoid nudge.";
                return false;
            }

            result.Success = true;
            LogNudgeResult(result);
            return true;
        }

        public static void LogNudgeResult(CCS_RevolverFullDrawHumanoidNudgeResult result)
        {
            Debug.Log(
                "[Revolver FullDraw Nudge] Applied humanoid muscle nudge:\n"
                + "Path: "
                + result.ClipPath
                + "\nRight Arm Down-Up delta: +"
                + RightArmDownUpDelta.ToString("0.##")
                + "\nRight Arm Front-Back delta: +"
                + RightArmFrontBackDelta.ToString("0.##")
                + "\nRight Shoulder Down-Up delta: +"
                + RightShoulderDownUpDelta.ToString("0.##")
                + "\nRight Shoulder Front-Back delta: +"
                + RightShoulderFrontBackDelta.ToString("0.##")
                + "\nRight Hand Down-Up delta: +"
                + RightHandDownUpDelta.ToString("0.##")
                + "\nRight Hand In-Out delta: +"
                + RightHandInOutDelta.ToString("0.##")
                + "\nCurve hash changed: "
                + (result.CurveHashChanged ? "true" : "false")
                + "\nController still references saved clip: "
                + (result.ControllerStillReferencesSavedClip ? "true" : "false")
                + "\nGUID preserved: "
                + (result.GuidPreserved ? "true" : "false"));
        }

        [MenuItem("CCS/Character Controller/Animations/Apply Default Revolver FullDraw Nudge")]
        public static void ApplyDefaultRightArmAimingNudgeMenu()
        {
            if (!ApplyDefaultRightArmAimingNudge(out CCS_RevolverFullDrawHumanoidNudgeResult result))
            {
                Debug.LogError("[Revolver FullDraw Nudge] Failed: " + result.ErrorMessage);
            }
        }
    }
}
