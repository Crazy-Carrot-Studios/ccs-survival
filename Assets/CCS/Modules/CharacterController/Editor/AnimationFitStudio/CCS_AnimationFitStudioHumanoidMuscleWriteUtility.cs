using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_AnimationFitStudioHumanoidMuscleWriteUtility
// CATEGORY: Modules / CharacterController / Editor / AnimationFitStudio
// PURPOSE: Writes Humanoid muscle curves from preview pose into AnimationClip assets.
// PLACEMENT: Editor utility invoked by Animation Fit Studio save and nudge workflows.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: FullDraw hold clips use constant muscle keys at time 0 and clip.length.
// =============================================================================

namespace CCS.Modules.CharacterController.Editor.AnimationFitStudio
{
    public sealed class CCS_AnimationFitStudioHumanoidMuscleWriteResult
    {
        public int HumanoidMuscleCurvesWritten { get; set; }

        public int TransformCurvesWritten { get; set; }
    }

    public static class CCS_AnimationFitStudioHumanoidMuscleWriteUtility
    {
        public static bool ApplyMuscleDeltaToClip(
            AnimationClip clip,
            string muscleName,
            float delta,
            out int keysUpdated)
        {
            keysUpdated = 0;
            if (clip == null || string.IsNullOrEmpty(muscleName))
            {
                return false;
            }

            if (!CCS_AnimationFitStudioHumanoidMuscleMapping.TryGetMuscleIndex(muscleName, out _))
            {
                return false;
            }

            float endTime = Mathf.Max(clip.length, 0.01f);
            EditorCurveBinding binding = EditorCurveBinding.FloatCurve(string.Empty, typeof(Animator), muscleName);
            AnimationCurve curve = AnimationUtility.GetEditorCurve(clip, binding);
            if (curve == null || curve.length == 0)
            {
                float clamped = Mathf.Clamp(delta, -1f, 1f);
                curve = new AnimationCurve(
                    new Keyframe(0f, clamped),
                    new Keyframe(endTime, clamped));
                keysUpdated = 2;
            }
            else
            {
                curve = new AnimationCurve(curve.keys);
                for (int i = 0; i < curve.length; i++)
                {
                    Keyframe keyframe = curve[i];
                    keyframe.value = Mathf.Clamp(keyframe.value + delta, -1f, 1f);
                    curve.MoveKey(i, keyframe);
                    keysUpdated++;
                }
            }

            AnimationUtility.SetEditorCurve(clip, binding, curve);
            return true;
        }

        public static CCS_AnimationFitStudioHumanoidMuscleWriteResult WriteHumanPoseToClip(
            AnimationClip clip,
            HumanPose pose,
            bool includeFingerMuscles)
        {
            CCS_AnimationFitStudioHumanoidMuscleWriteResult result =
                new CCS_AnimationFitStudioHumanoidMuscleWriteResult();
            if (clip == null)
            {
                return result;
            }

            float endTime = Mathf.Max(clip.length, 0.01f);
            System.Collections.Generic.IReadOnlyList<string> muscleNames =
                CCS_AnimationFitStudioHumanoidMuscleMapping.GetSaveMuscleNames(
                    includeFingerMuscles,
                    includeExistingClipBindings: true,
                    clip);

            for (int i = 0; i < muscleNames.Count; i++)
            {
                string muscleName = muscleNames[i];
                if (!CCS_AnimationFitStudioHumanoidMuscleMapping.TryGetMuscleIndex(muscleName, out int muscleIndex))
                {
                    continue;
                }

                float value = Mathf.Clamp(pose.muscles[muscleIndex], -1f, 1f);
                EditorCurveBinding binding = EditorCurveBinding.FloatCurve(string.Empty, typeof(Animator), muscleName);
                AnimationCurve curve = new AnimationCurve(
                    new Keyframe(0f, value),
                    new Keyframe(endTime, value));
                AnimationUtility.SetEditorCurve(clip, binding, curve);
                result.HumanoidMuscleCurvesWritten++;
            }

            return result;
        }

        public static HumanPose CaptureHumanPose(Animator animator)
        {
            HumanPose pose = new HumanPose();
            if (animator == null || animator.avatar == null || !animator.isHuman)
            {
                return pose;
            }

            HumanPoseHandler poseHandler = new HumanPoseHandler(animator.avatar, animator.transform);
            poseHandler.GetHumanPose(ref pose);
            return pose;
        }
    }
}
