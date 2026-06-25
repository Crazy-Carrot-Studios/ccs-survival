using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_AnimationFitStudioClipDiagnostics
// CATEGORY: Modules / CharacterController / Editor / AnimationFitStudio
// PURPOSE: Reports Animation Fit Studio clip compatibility and curve binding diagnostics.
// PLACEMENT: Built by pose utility and shown in Animation Fit Studio right panel.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Read-only diagnostics for humanoid clip sampling compatibility.
// =============================================================================

namespace CCS.Modules.CharacterController.Editor.AnimationFitStudio
{
    public sealed class CCS_AnimationFitStudioClipDiagnostics
    {
        public string ClipName { get; set; } = string.Empty;

        public float ClipLength { get; set; }

        public float FrameRate { get; set; }

        public bool Legacy { get; set; }

        public bool HumanMotion { get; set; }

        public int CurveBindingCount { get; set; }

        public bool HasRightArmCurves { get; set; }

        public bool HasFingerCurves { get; set; }

        public int SampleChangedBoneCount { get; set; }

        public float AimPoseScore { get; set; }

        public float RightHandForwardDistance { get; set; }

        public float RightHandHeightDelta { get; set; }

        public float RightArmExtension { get; set; }

        public int ChangedAimBones { get; set; }

        public string AimPoseResult { get; set; } = "(none)";

        public string SampleMethod { get; set; } = "(none)";

        public string MatchedRightArmPaths { get; set; } = "(none)";

        public CCS_AnimationFitStudioClipCurveMode ClipCurveMode { get; set; }

        public string ClipCurveModeLabel { get; set; } = string.Empty;

        public int HumanoidMuscleBindingCount { get; set; }

        public static CCS_AnimationFitStudioClipDiagnostics Build(
            AnimationClip clip,
            int sampleChangedBoneCount = 0,
            CCS_AnimationFitStudioAimPoseScore aimScore = null,
            string sampleMethod = null)
        {
            CCS_AnimationFitStudioClipDiagnostics diagnostics = new CCS_AnimationFitStudioClipDiagnostics
            {
                SampleChangedBoneCount = sampleChangedBoneCount,
            };

            if (clip == null)
            {
                return diagnostics;
            }

            diagnostics.ClipName = clip.name;
            diagnostics.ClipLength = clip.length;
            diagnostics.FrameRate = clip.frameRate;
            diagnostics.Legacy = clip.legacy;
            diagnostics.HumanMotion = clip.humanMotion;
            diagnostics.ClipCurveMode = CCS_AnimationFitStudioClipCurveModeUtility.DetectClipCurveMode(clip);
            diagnostics.ClipCurveModeLabel =
                CCS_AnimationFitStudioClipCurveModeUtility.GetDisplayLabel(diagnostics.ClipCurveMode);
            diagnostics.HumanoidMuscleBindingCount =
                CCS_AnimationFitStudioClipCurveModeUtility.CountHumanoidMuscleBindings(clip);

            EditorCurveBinding[] floatBindings = AnimationUtility.GetCurveBindings(clip);
            EditorCurveBinding[] objectBindings = AnimationUtility.GetObjectReferenceCurveBindings(clip);
            diagnostics.CurveBindingCount = floatBindings.Length + objectBindings.Length;

            HashSet<string> matchedPaths = new HashSet<string>();
            string[] rightArmTokens =
            {
                "RightArm",
                "RightForeArm",
                "RightHand",
                "RightShoulder",
                "mixamorig:RightArm",
                "CC_Base_R_Upperarm",
                "RightUpperArm",
                "RightLowerArm",
            };

            string[] fingerTokens =
            {
                "RightHand",
                "RightHandIndex",
                "RightHandMiddle",
                "RightHandRing",
                "RightHandPinky",
                "RightHandThumb",
                "Right Index",
            };

            for (int i = 0; i < floatBindings.Length; i++)
            {
                string path = floatBindings[i].path ?? string.Empty;
                string attribute = floatBindings[i].propertyName ?? string.Empty;
                string combined = path + " " + attribute;

                for (int t = 0; t < rightArmTokens.Length; t++)
                {
                    if (combined.Contains(rightArmTokens[t]))
                    {
                        diagnostics.HasRightArmCurves = true;
                        matchedPaths.Add(string.IsNullOrEmpty(path) ? attribute : path);
                        break;
                    }
                }

                for (int t = 0; t < fingerTokens.Length; t++)
                {
                    if (combined.Contains(fingerTokens[t]))
                    {
                        diagnostics.HasFingerCurves = true;
                        break;
                    }
                }
            }

            if (matchedPaths.Count > 0)
            {
                diagnostics.MatchedRightArmPaths = string.Join(", ", matchedPaths);
            }

            if (aimScore != null)
            {
                diagnostics.AimPoseScore = aimScore.Score;
                diagnostics.RightHandForwardDistance = aimScore.RightHandForwardDistance;
                diagnostics.RightHandHeightDelta = aimScore.RightHandHeightDelta;
                diagnostics.RightArmExtension = aimScore.RightArmExtension;
                diagnostics.ChangedAimBones = aimScore.ChangedAimBones;
                diagnostics.AimPoseResult = aimScore.ResultKind.ToString();
            }

            if (!string.IsNullOrEmpty(sampleMethod))
            {
                diagnostics.SampleMethod = sampleMethod;
            }

            return diagnostics;
        }

        public string ToDisplayText()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("Clip: " + (string.IsNullOrEmpty(ClipName) ? "(none)" : ClipName));
            builder.AppendLine("Length: " + ClipLength.ToString("0.###") + "s");
            builder.AppendLine("Frame rate: " + FrameRate.ToString("0.##"));
            builder.AppendLine("Legacy: " + (Legacy ? "true" : "false"));
            builder.AppendLine("Human motion: " + (HumanMotion ? "true" : "false"));
            builder.AppendLine("Clip curve mode: " + ClipCurveModeLabel);
            builder.AppendLine("Humanoid muscle bindings: " + HumanoidMuscleBindingCount);
            builder.AppendLine("Curve bindings: " + CurveBindingCount);
            builder.AppendLine("Right arm curves: " + (HasRightArmCurves ? "yes" : "no"));
            builder.AppendLine("Finger curves: " + (HasFingerCurves ? "yes" : "no"));
            builder.AppendLine("Matched right-arm paths: " + MatchedRightArmPaths);
            builder.AppendLine("Sample changed bones: " + SampleChangedBoneCount);
            builder.AppendLine("Aim Pose Score: " + AimPoseScore.ToString("0.#") + " / 100");
            builder.AppendLine("Right Hand Forward Distance: " + RightHandForwardDistance.ToString("0.###"));
            builder.AppendLine("Right Hand Height Delta: " + RightHandHeightDelta.ToString("0.###"));
            builder.AppendLine("Right Arm Extension: " + RightArmExtension.ToString("0.###"));
            builder.AppendLine("Changed Aim Bones: " + ChangedAimBones);
            builder.AppendLine("Result: " + AimPoseResult);
            builder.AppendLine("Sample method: " + SampleMethod);
            return builder.ToString().TrimEnd();
        }
    }
}
