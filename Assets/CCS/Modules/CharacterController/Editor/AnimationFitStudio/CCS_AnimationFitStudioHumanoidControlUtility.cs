using System.Collections.Generic;
using System.Text;
using CCS.Modules.CharacterController;
using CCS.Project;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_AnimationFitStudioHumanoidControlUtility
// CATEGORY: Modules / CharacterController / Editor / AnimationFitStudio
// PURPOSE: Calibrated Humanoid muscle nudge, readout, preview apply, and save helpers.
// PLACEMENT: Used by Animation Fit Studio window, pose utility, and validation.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Shoulder uses reduced scale; wrist invert defaults verified on current rig.
// =============================================================================

namespace CCS.Modules.CharacterController.Editor.AnimationFitStudio
{
    public enum CCS_AnimationFitStudioHumanoidControlAxis
    {
        Pitch = 0,
        Yaw = 1,
        Roll = 2,
    }

    public sealed class CCS_AnimationFitStudioHumanoidNudgeResult
    {
        public bool Success { get; set; }

        public string MuscleName { get; set; } = string.Empty;

        public float ValueBefore { get; set; }

        public float ValueAfter { get; set; }

        public bool WasClamped { get; set; }

        public string Feedback { get; set; } = string.Empty;
    }

    public static class CCS_AnimationFitStudioHumanoidControlUtility
    {
        public const float ShoulderPartScale = 0.25f;

        public const float TestOffsetUpperArmDownUp = 0.10f;

        public const float TestOffsetUpperArmFrontBack = 0.08f;

        public const float TestOffsetHandDownUp = 0.03f;

        public const float TestOffsetHandInOut = 0.02f;

        private const float SaveCompareEpsilon = 0.0005f;

        public static float NudgeDegreesToMuscleDelta(float nudgeDegrees)
        {
            if (Mathf.Approximately(nudgeDegrees, 1f))
            {
                return 0.01f;
            }

            if (Mathf.Approximately(nudgeDegrees, 15f))
            {
                return 0.10f;
            }

            return 0.04f;
        }

        public static bool CaptureBaselineFromClip(
            AnimationClip clip,
            float poseTime,
            CCS_AnimationFitStudioHumanoidControlState state)
        {
            if (clip == null || state == null)
            {
                return false;
            }

            state.BaselineValues.Clear();
            state.CurrentValues.Clear();
            for (int i = 0; i < CCS_AnimationFitStudioHumanoidMuscleMapping.FullDrawSaveMuscleNames.Length; i++)
            {
                string muscleName = CCS_AnimationFitStudioHumanoidMuscleMapping.FullDrawSaveMuscleNames[i];
                if (!CCS_AnimationFitStudioHumanoidMuscleMapping.IsMuscleAvailableOnAvatar(muscleName))
                {
                    continue;
                }

                float value = ReadMuscleValueFromClip(clip, muscleName, poseTime);
                state.BaselineValues[muscleName] = value;
                state.CurrentValues[muscleName] = value;
            }

            state.IsInitialized = true;
            state.LastClampedMuscles.Clear();
            state.LastEditFeedback = string.Empty;
            return true;
        }

        public static void SyncCurrentFromAnimator(
            Animator animator,
            CCS_AnimationFitStudioHumanoidControlState state)
        {
            if (animator == null || state == null || !state.IsInitialized)
            {
                return;
            }

            HumanPose pose = CCS_AnimationFitStudioHumanoidMuscleWriteUtility.CaptureHumanPose(animator);
            for (int i = 0; i < CCS_AnimationFitStudioHumanoidMuscleMapping.FullDrawSaveMuscleNames.Length; i++)
            {
                string muscleName = CCS_AnimationFitStudioHumanoidMuscleMapping.FullDrawSaveMuscleNames[i];
                if (CCS_AnimationFitStudioHumanoidMuscleMapping.TryGetMuscleIndex(muscleName, out int muscleIndex))
                {
                    state.CurrentValues[muscleName] = Mathf.Clamp(pose.muscles[muscleIndex], -1f, 1f);
                }
            }
        }

        public static void ApplyCurrentMuscleValuesToAnimator(
            Animator animator,
            CCS_AnimationFitStudioHumanoidControlState state)
        {
            if (animator == null || state == null || !state.IsInitialized || animator.avatar == null)
            {
                return;
            }

            HumanPoseHandler poseHandler = new HumanPoseHandler(animator.avatar, animator.transform);
            HumanPose pose = new HumanPose();
            poseHandler.GetHumanPose(ref pose);

            foreach (KeyValuePair<string, float> entry in state.CurrentValues)
            {
                if (CCS_AnimationFitStudioHumanoidMuscleMapping.TryGetMuscleIndex(entry.Key, out int muscleIndex))
                {
                    pose.muscles[muscleIndex] = Mathf.Clamp(entry.Value, -1f, 1f);
                }
            }

            poseHandler.SetHumanPose(ref pose);
            animator.Update(0f);
        }

        public static bool TryNudgePart(
            CCS_AnimationFitStudioHumanoidControlState state,
            string partId,
            CCS_AnimationFitStudioHumanoidControlAxis axis,
            float sign,
            float nudgeDegrees,
            out CCS_AnimationFitStudioHumanoidNudgeResult result)
        {
            result = new CCS_AnimationFitStudioHumanoidNudgeResult();
            state.LastClampedMuscles.Clear();

            if (state == null || !state.IsInitialized)
            {
                result.Feedback = "Humanoid muscle state is not initialized.";
                return false;
            }

            if (!TryResolveMuscleForPartAxis(partId, axis, out string muscleName))
            {
                result.Feedback = "No Humanoid muscle mapping for this axis.";
                return false;
            }

            if (!CCS_AnimationFitStudioHumanoidMuscleMapping.IsMuscleAvailableOnAvatar(muscleName))
            {
                result.Feedback = "No Humanoid muscle mapping for this axis.";
                return false;
            }

            if (!state.CurrentValues.TryGetValue(muscleName, out float before))
            {
                before = ReadMuscleValueFromClip(null, muscleName, 0f);
                state.CurrentValues[muscleName] = before;
            }

            float delta = NudgeDegreesToMuscleDelta(nudgeDegrees)
                * GetPartScale(partId)
                * sign
                * GetInvertMultiplier(state, partId, axis);

            if (partId == "right_shoulder" && IsNearClampInDirection(before, delta))
            {
                result.WasClamped = true;
                result.MuscleName = muscleName;
                result.ValueBefore = before;
                result.ValueAfter = before;
                result.Feedback =
                    CCS_AnimationFitStudioEditPartCatalog.GetDisplayLabel(partId)
                    + " "
                    + GetAxisLabel(axis)
                    + (sign >= 0f ? " +" : " -")
                    + ":\n"
                    + muscleName
                    + " already near limit. Edit clamped.";
                state.LastClampedMuscles.Add(muscleName);
                state.LastEditFeedback = result.Feedback;
                return false;
            }

            float after = Mathf.Clamp(before + delta, -1f, 1f);
            if (Mathf.Approximately(before, after))
            {
                result.WasClamped = true;
                result.MuscleName = muscleName;
                result.ValueBefore = before;
                result.ValueAfter = after;
                result.Feedback = "Edit reached Humanoid muscle limit.";
                state.LastClampedMuscles.Add(muscleName);
                state.LastEditFeedback = result.Feedback;
                return false;
            }

            state.CurrentValues[muscleName] = after;
            result.Success = true;
            result.MuscleName = muscleName;
            result.ValueBefore = before;
            result.ValueAfter = after;
            result.Feedback = BuildEditFeedback(partId, axis, sign, muscleName, before, after);
            state.LastEditFeedback = result.Feedback;
            return true;
        }

        public static bool CanNudgePartAxis(string partId, CCS_AnimationFitStudioHumanoidControlAxis axis)
        {
            return TryResolveMuscleForPartAxis(partId, axis, out string muscleName)
                && CCS_AnimationFitStudioHumanoidMuscleMapping.IsMuscleAvailableOnAvatar(muscleName);
        }

        public static string GetPartAxisMappingLabel(string partId, CCS_AnimationFitStudioHumanoidControlAxis axis)
        {
            if (!TryResolveMuscleForPartAxis(partId, axis, out string muscleName))
            {
                return string.Empty;
            }

            return muscleName;
        }

        private static string BuildEditFeedback(
            string partId,
            CCS_AnimationFitStudioHumanoidControlAxis axis,
            float sign,
            string muscleName,
            float before,
            float after)
        {
            string partLabel = CCS_AnimationFitStudioEditPartCatalog.GetDisplayLabel(partId);
            string axisLabel = axis switch
            {
                CCS_AnimationFitStudioHumanoidControlAxis.Pitch => "Pitch",
                CCS_AnimationFitStudioHumanoidControlAxis.Yaw => "Yaw",
                _ => "Roll",
            };
            string direction = sign >= 0f ? "+" : "-";
            return partLabel
                + " "
                + axisLabel
                + " "
                + direction
                + ":\n"
                + muscleName
                + " "
                + before.ToString("F3")
                + " → "
                + after.ToString("F3");
        }

        public static bool ApplyTestAimOffset(CCS_AnimationFitStudioHumanoidControlState state)
        {
            if (state == null || !state.IsInitialized)
            {
                return false;
            }

            bool changed = false;
            changed |= TryApplyFixedDelta(state, "Right Arm Down-Up", TestOffsetUpperArmDownUp);
            changed |= TryApplyFixedDelta(state, "Right Arm Front-Back", TestOffsetUpperArmFrontBack);
            changed |= TryApplyFixedDelta(state, "Right Hand Down-Up", TestOffsetHandDownUp);
            changed |= TryApplyFixedDelta(state, "Right Hand In-Out", TestOffsetHandInOut);

            if (changed)
            {
                state.LastEditFeedback = "Applied test aim offset to upper arm and wrist muscles.";
            }

            return changed;
        }

        public static int WriteCurrentValuesToClip(
            AnimationClip clip,
            CCS_AnimationFitStudioHumanoidControlState state,
            bool includeFingerMusclesFromPose,
            HumanPose fingerPose)
        {
            if (clip == null || state == null || !state.IsInitialized)
            {
                return 0;
            }

            float endTime = Mathf.Max(clip.length, 0.01f);
            int written = 0;
            foreach (KeyValuePair<string, float> entry in state.CurrentValues)
            {
                WriteMuscleConstantCurve(clip, entry.Key, entry.Value, endTime);
                written++;
            }

            if (includeFingerMusclesFromPose)
            {
                for (int i = 0; i < CCS_AnimationFitStudioHumanoidMuscleMapping.RightHandFingerMuscleNames.Length; i++)
                {
                    string muscleName = CCS_AnimationFitStudioHumanoidMuscleMapping.RightHandFingerMuscleNames[i];
                    if (CCS_AnimationFitStudioHumanoidMuscleMapping.TryGetMuscleIndex(muscleName, out int muscleIndex))
                    {
                        float value = Mathf.Clamp(fingerPose.muscles[muscleIndex], -1f, 1f);
                        WriteMuscleConstantCurve(clip, muscleName, value, endTime);
                        written++;
                    }
                }
            }

            EditorUtility.SetDirty(clip);
            return written;
        }

        public static bool ValidateReloadedClipMatchesState(
            AnimationClip clip,
            float poseTime,
            CCS_AnimationFitStudioHumanoidControlState state,
            out string errorMessage)
        {
            errorMessage = string.Empty;
            if (clip == null || state == null || !state.IsInitialized)
            {
                errorMessage = "Humanoid state or clip missing for reload validation.";
                return false;
            }

            List<string> mismatches = new List<string>();
            for (int i = 0; i < CCS_AnimationFitStudioHumanoidMuscleMapping.FullDrawSaveMuscleNames.Length; i++)
            {
                string muscleName = CCS_AnimationFitStudioHumanoidMuscleMapping.FullDrawSaveMuscleNames[i];
                if (!state.CurrentValues.TryGetValue(muscleName, out float expected))
                {
                    continue;
                }

                float reloaded = ReadMuscleValueFromClip(clip, muscleName, poseTime);
                if (Mathf.Abs(expected - reloaded) > SaveCompareEpsilon)
                {
                    mismatches.Add(muscleName + " expected " + expected.ToString("F4") + " got " + reloaded.ToString("F4"));
                }
            }

            if (mismatches.Count > 0)
            {
                errorMessage = "Reloaded clip values do not match UI Humanoid muscle values: "
                    + string.Join("; ", mismatches);
                return false;
            }

            return true;
        }

        public static string BuildReadoutText(CCS_AnimationFitStudioHumanoidControlState state)
        {
            if (state == null || !state.IsInitialized)
            {
                return "Humanoid muscle readout unavailable until pose is loaded.";
            }

            StringBuilder builder = new StringBuilder(1024);
            builder.AppendLine("Humanoid Muscle Values");
            for (int i = 0; i < CCS_AnimationFitStudioHumanoidMuscleMapping.FullDrawSaveMuscleNames.Length; i++)
            {
                string muscleName = CCS_AnimationFitStudioHumanoidMuscleMapping.FullDrawSaveMuscleNames[i];
                float current = state.GetCurrentValue(muscleName);
                float baseline = state.BaselineValues.TryGetValue(muscleName, out float baseValue) ? baseValue : 0f;
                float saved = state.GetLastSavedValue(muscleName);
                float delta = current - baseline;
                builder.Append(muscleName)
                    .Append("\n  Current: ").Append(current.ToString("F4"))
                    .Append("\n  Last saved: ")
                    .Append(float.IsNaN(saved) ? "(none)" : saved.ToString("F4"))
                    .Append("\n  Delta from loaded pose: ")
                    .Append(delta.ToString("F4"));

                if (state.IsNearClamp(muscleName))
                {
                    builder.Append("\n  Warning: muscle is near Humanoid clamp limit. Further edits may not visibly move this part.");
                }

                builder.AppendLine();
            }

            if (!string.IsNullOrEmpty(state.LastEditFeedback))
            {
                builder.AppendLine("Last edit: " + state.LastEditFeedback);
            }

            if (state.LastClampedMuscles.Count > 0)
            {
                builder.AppendLine("Clamped values: " + string.Join(", ", state.LastClampedMuscles));
            }

            return builder.ToString().TrimEnd();
        }

        public static float ReadMuscleValueFromClip(AnimationClip clip, string muscleName, float poseTime)
        {
            if (clip == null || string.IsNullOrEmpty(muscleName))
            {
                return 0f;
            }

            EditorCurveBinding binding = EditorCurveBinding.FloatCurve(string.Empty, typeof(Animator), muscleName);
            AnimationCurve curve = AnimationUtility.GetEditorCurve(clip, binding);
            if (curve == null || curve.length == 0)
            {
                return 0f;
            }

            return curve.Evaluate(Mathf.Clamp(poseTime, 0f, clip.length));
        }

        private static bool TryApplyFixedDelta(
            CCS_AnimationFitStudioHumanoidControlState state,
            string muscleName,
            float delta)
        {
            if (!state.CurrentValues.TryGetValue(muscleName, out float before))
            {
                return false;
            }

            if (muscleName.StartsWith("Right Shoulder") && state.IsNearClamp(muscleName))
            {
                return false;
            }

            float after = Mathf.Clamp(before + delta, -1f, 1f);
            if (Mathf.Approximately(before, after))
            {
                state.LastClampedMuscles.Add(muscleName);
                return false;
            }

            state.CurrentValues[muscleName] = after;
            return true;
        }

        private static void WriteMuscleConstantCurve(
            AnimationClip clip,
            string muscleName,
            float value,
            float endTime)
        {
            EditorCurveBinding binding = EditorCurveBinding.FloatCurve(string.Empty, typeof(Animator), muscleName);
            AnimationCurve curve = new AnimationCurve(
                new Keyframe(0f, value),
                new Keyframe(endTime, value));
            AnimationUtility.SetEditorCurve(clip, binding, curve);
        }

        public static bool TryResolveMuscleForPartAxis(
            string partId,
            CCS_AnimationFitStudioHumanoidControlAxis axis,
            out string muscleName)
        {
            return TryResolveMuscleForPartAxisInternal(partId, axis, out muscleName);
        }

        public static void ResetPartMusclesToBaseline(
            CCS_AnimationFitStudioHumanoidControlState state,
            string partId)
        {
            if (state == null || !state.IsInitialized)
            {
                return;
            }

            ResetPartAxisToBaseline(state, partId, CCS_AnimationFitStudioHumanoidControlAxis.Pitch);
            ResetPartAxisToBaseline(state, partId, CCS_AnimationFitStudioHumanoidControlAxis.Yaw);
            ResetPartAxisToBaseline(state, partId, CCS_AnimationFitStudioHumanoidControlAxis.Roll);
        }

        private static void ResetPartAxisToBaseline(
            CCS_AnimationFitStudioHumanoidControlState state,
            string partId,
            CCS_AnimationFitStudioHumanoidControlAxis axis)
        {
            if (!TryResolveMuscleForPartAxisInternal(partId, axis, out string muscleName))
            {
                return;
            }

            if (state.BaselineValues.TryGetValue(muscleName, out float baseline))
            {
                state.CurrentValues[muscleName] = baseline;
            }
        }

        private static bool TryResolveMuscleForPartAxisInternal(
            string partId,
            CCS_AnimationFitStudioHumanoidControlAxis axis,
            out string muscleName)
        {
            muscleName = string.Empty;
            switch (partId)
            {
                case "spine":
                    muscleName = axis switch
                    {
                        CCS_AnimationFitStudioHumanoidControlAxis.Pitch => "Spine Front-Back",
                        CCS_AnimationFitStudioHumanoidControlAxis.Yaw => "Spine Left-Right",
                        CCS_AnimationFitStudioHumanoidControlAxis.Roll => "Spine Twist Left-Right",
                        _ => string.Empty,
                    };
                    break;
                case "chest_aim_lean":
                    muscleName = axis switch
                    {
                        CCS_AnimationFitStudioHumanoidControlAxis.Pitch => "Chest Front-Back",
                        CCS_AnimationFitStudioHumanoidControlAxis.Yaw => "Chest Left-Right",
                        CCS_AnimationFitStudioHumanoidControlAxis.Roll => "Chest Twist Left-Right",
                        _ => string.Empty,
                    };
                    break;
                case "upper_chest_aim_lean":
                    muscleName = axis switch
                    {
                        CCS_AnimationFitStudioHumanoidControlAxis.Pitch => "UpperChest Front-Back",
                        CCS_AnimationFitStudioHumanoidControlAxis.Yaw => "UpperChest Left-Right",
                        CCS_AnimationFitStudioHumanoidControlAxis.Roll => "UpperChest Twist Left-Right",
                        _ => string.Empty,
                    };
                    break;
                case "right_shoulder":
                    muscleName = axis switch
                    {
                        CCS_AnimationFitStudioHumanoidControlAxis.Pitch => "Right Shoulder Down-Up",
                        CCS_AnimationFitStudioHumanoidControlAxis.Yaw => "Right Shoulder Front-Back",
                        _ => string.Empty,
                    };
                    break;
                case "right_upper_arm":
                    muscleName = axis switch
                    {
                        CCS_AnimationFitStudioHumanoidControlAxis.Pitch => "Right Arm Down-Up",
                        CCS_AnimationFitStudioHumanoidControlAxis.Yaw => "Right Arm Front-Back",
                        CCS_AnimationFitStudioHumanoidControlAxis.Roll => "Right Arm Twist In-Out",
                        _ => string.Empty,
                    };
                    break;
                case "right_forearm":
                    muscleName = axis == CCS_AnimationFitStudioHumanoidControlAxis.Roll
                        ? "Right Forearm Twist In-Out"
                        : string.Empty;
                    break;
                case "right_hand":
                    muscleName = axis switch
                    {
                        CCS_AnimationFitStudioHumanoidControlAxis.Pitch => "Right Hand Down-Up",
                        CCS_AnimationFitStudioHumanoidControlAxis.Yaw => "Right Hand In-Out",
                        _ => string.Empty,
                    };
                    break;
            }

            return !string.IsNullOrEmpty(muscleName);
        }

        private static float GetPartScale(string partId)
        {
            return partId == "right_shoulder" ? ShoulderPartScale : 1f;
        }

        private static float GetInvertMultiplier(
            CCS_AnimationFitStudioHumanoidControlState state,
            string partId,
            CCS_AnimationFitStudioHumanoidControlAxis axis)
        {
            bool invert = partId switch
            {
                "right_shoulder" => (axis == CCS_AnimationFitStudioHumanoidControlAxis.Pitch && state.InvertShoulderPitch)
                    || (axis == CCS_AnimationFitStudioHumanoidControlAxis.Yaw && state.InvertShoulderYaw),
                "right_upper_arm" => (axis == CCS_AnimationFitStudioHumanoidControlAxis.Pitch && state.InvertUpperArmPitch)
                    || (axis == CCS_AnimationFitStudioHumanoidControlAxis.Yaw && state.InvertUpperArmYaw),
                "right_hand" => (axis == CCS_AnimationFitStudioHumanoidControlAxis.Pitch && state.InvertWristPitch)
                    || (axis == CCS_AnimationFitStudioHumanoidControlAxis.Yaw && state.InvertWristYaw),
                _ => false,
            };

            return invert ? -1f : 1f;
        }

        private static string GetAxisLabel(CCS_AnimationFitStudioHumanoidControlAxis axis)
        {
            return axis switch
            {
                CCS_AnimationFitStudioHumanoidControlAxis.Pitch => "Pitch",
                CCS_AnimationFitStudioHumanoidControlAxis.Yaw => "Yaw",
                _ => "Roll",
            };
        }

        private static bool IsNearClampInDirection(float currentValue, float delta)
        {
            if (Mathf.Abs(currentValue) < CCS_AnimationFitStudioHumanoidControlState.ClampWarningThreshold)
            {
                return false;
            }

            return (currentValue > 0f && delta > 0f) || (currentValue < 0f && delta < 0f);
        }

        public static CCS_SurvivalValidationResult ValidateHumanoidControlCalibration()
        {
            List<string> failures = new List<string>();
            AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(
                CCS_CharacterControllerConstants.RevolverAimIdleFullDrawClipPath);
            if (clip == null)
            {
                return CCS_SurvivalValidationResult.Fail(
                    "Missing controller FullDraw clip for Humanoid control calibration validation.");
            }

            if (CCS_AnimationFitStudioClipCurveModeUtility.DetectClipCurveMode(clip)
                != CCS_AnimationFitStudioClipCurveMode.HumanoidMuscleCurves)
            {
                failures.Add("FullDraw clip must use Humanoid Muscle Curves mode.");
            }

            if (!Mathf.Approximately(NudgeDegreesToMuscleDelta(1f), 0.01f)
                || !Mathf.Approximately(NudgeDegreesToMuscleDelta(5f), 0.04f)
                || !Mathf.Approximately(NudgeDegreesToMuscleDelta(15f), 0.10f))
            {
                failures.Add("Humanoid nudge step mapping must use 1°=0.01, 5°=0.04, 15°=0.10 muscle deltas.");
            }

            CCS_AnimationFitStudioHumanoidControlState state = new CCS_AnimationFitStudioHumanoidControlState();
            if (!CaptureBaselineFromClip(clip, 0f, state))
            {
                failures.Add("Failed to capture Humanoid muscle baseline from FullDraw clip.");
            }
            else
            {
                ValidateShoulderClampBehavior(state, failures);
                ValidateUpperArmNudgeMapping(state, failures);
                ValidateWristNudgeMapping(state, failures);
                ValidateTestAimOffset(state, failures);
                ValidateInMemorySaveRoundTrip(clip, state, failures);
            }

            return failures.Count > 0
                ? CCS_SurvivalValidationResult.Fail(string.Join(" ", failures))
                : CCS_SurvivalValidationResult.Pass(
                    "Humanoid control calibration validated. Upper arm and wrist buttons modify correct muscles.");
        }

        private static void ValidateShoulderClampBehavior(
            CCS_AnimationFitStudioHumanoidControlState state,
            List<string> failures)
        {
            if (!state.IsNearClamp("Right Shoulder Down-Up"))
            {
                failures.Add(
                    "Right Shoulder Down-Up should be near Humanoid clamp on controller FullDraw clip.");
            }

            float before = state.GetCurrentValue("Right Shoulder Down-Up");
            bool shoulderNudgeSucceeded = TryNudgePart(
                state,
                "right_shoulder",
                CCS_AnimationFitStudioHumanoidControlAxis.Pitch,
                -1f,
                5f,
                out _);
            if (shoulderNudgeSucceeded)
            {
                failures.Add(
                    "Shoulder Pitch- must not push Right Shoulder Down-Up further into clamp when already near -1.");
            }

            if (!Mathf.Approximately(state.GetCurrentValue("Right Shoulder Down-Up"), before))
            {
                failures.Add("Shoulder clamp skip must leave Right Shoulder Down-Up unchanged.");
            }

            state.ResetCurrentToBaseline();
        }

        private static void ValidateUpperArmNudgeMapping(
            CCS_AnimationFitStudioHumanoidControlState state,
            List<string> failures)
        {
            float downUpBefore = state.GetCurrentValue("Right Arm Down-Up");
            if (!TryNudgePart(
                    state,
                    "right_upper_arm",
                    CCS_AnimationFitStudioHumanoidControlAxis.Pitch,
                    1f,
                    5f,
                    out CCS_AnimationFitStudioHumanoidNudgeResult pitchResult)
                || !pitchResult.Success
                || pitchResult.ValueAfter <= downUpBefore)
            {
                failures.Add("Upper Arm Pitch+ must increase Right Arm Down-Up.");
            }

            float frontBackBefore = state.GetCurrentValue("Right Arm Front-Back");
            if (!TryNudgePart(
                    state,
                    "right_upper_arm",
                    CCS_AnimationFitStudioHumanoidControlAxis.Yaw,
                    1f,
                    5f,
                    out CCS_AnimationFitStudioHumanoidNudgeResult yawResult)
                || !yawResult.Success
                || Mathf.Approximately(yawResult.ValueAfter, frontBackBefore))
            {
                failures.Add("Upper Arm Yaw+ must change Right Arm Front-Back.");
            }

            state.ResetCurrentToBaseline();
        }

        private static void ValidateWristNudgeMapping(
            CCS_AnimationFitStudioHumanoidControlState state,
            List<string> failures)
        {
            state.InvertWristPitch = true;
            float handDownUpBefore = state.GetCurrentValue("Right Hand Down-Up");
            if (!TryNudgePart(
                    state,
                    "right_hand",
                    CCS_AnimationFitStudioHumanoidControlAxis.Pitch,
                    1f,
                    5f,
                    out CCS_AnimationFitStudioHumanoidNudgeResult wristPitchResult)
                || !wristPitchResult.Success
                || Mathf.Approximately(wristPitchResult.ValueAfter, handDownUpBefore))
            {
                failures.Add("Wrist Pitch+ must change Right Hand Down-Up with default invert settings.");
            }

            float handInOutBefore = state.GetCurrentValue("Right Hand In-Out");
            if (!TryNudgePart(
                    state,
                    "right_hand",
                    CCS_AnimationFitStudioHumanoidControlAxis.Yaw,
                    1f,
                    5f,
                    out CCS_AnimationFitStudioHumanoidNudgeResult wristYawResult)
                || !wristYawResult.Success
                || Mathf.Approximately(wristYawResult.ValueAfter, handInOutBefore))
            {
                failures.Add("Wrist Yaw+ must change Right Hand In-Out.");
            }

            state.ResetCurrentToBaseline();
        }

        private static void ValidateTestAimOffset(
            CCS_AnimationFitStudioHumanoidControlState state,
            List<string> failures)
        {
            float shoulderBefore = state.GetCurrentValue("Right Shoulder Down-Up");
            float armDownUpBefore = state.GetCurrentValue("Right Arm Down-Up");
            float armFrontBackBefore = state.GetCurrentValue("Right Arm Front-Back");
            float handDownUpBefore = state.GetCurrentValue("Right Hand Down-Up");
            float handInOutBefore = state.GetCurrentValue("Right Hand In-Out");

            if (!ApplyTestAimOffset(state))
            {
                failures.Add("Apply Test Aim Offset must change upper arm and wrist muscles.");
                return;
            }

            if (!Mathf.Approximately(state.GetCurrentValue("Right Shoulder Down-Up"), shoulderBefore))
            {
                failures.Add("Apply Test Aim Offset must not push shoulder when shoulder is near clamp.");
            }

            if (state.GetCurrentValue("Right Arm Down-Up") <= armDownUpBefore
                || state.GetCurrentValue("Right Arm Front-Back") <= armFrontBackBefore
                || state.GetCurrentValue("Right Hand Down-Up") <= handDownUpBefore
                || state.GetCurrentValue("Right Hand In-Out") <= handInOutBefore)
            {
                failures.Add("Apply Test Aim Offset must increase upper arm and wrist muscle values.");
            }

            state.ResetCurrentToBaseline();
        }

        private static void ValidateInMemorySaveRoundTrip(
            AnimationClip sourceClip,
            CCS_AnimationFitStudioHumanoidControlState state,
            List<string> failures)
        {
            if (!TryNudgePart(
                    state,
                    "right_upper_arm",
                    CCS_AnimationFitStudioHumanoidControlAxis.Pitch,
                    1f,
                    5f,
                    out _))
            {
                failures.Add("Save round-trip validation requires a successful upper arm edit.");
                return;
            }

            AnimationClip tempClip = Object.Instantiate(sourceClip);
            tempClip.name = sourceClip.name + "_HumanoidCalibrationValidation";
            WriteCurrentValuesToClip(tempClip, state, false, default);

            if (!ValidateReloadedClipMatchesState(tempClip, 0f, state, out string reloadError))
            {
                failures.Add(reloadError);
            }

            Object.DestroyImmediate(tempClip);
        }
    }
}
