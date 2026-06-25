using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_AnimationFitStudioClipCurveModeUtility
// CATEGORY: Modules / CharacterController / Editor / AnimationFitStudio
// PURPOSE: Detects Humanoid muscle vs Transform curve clips for save routing.
// PLACEMENT: Editor utility used by Animation Fit Studio save and UI.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Humanoid bindings use empty path, Animator type, and HumanTrait muscle names.
// =============================================================================

namespace CCS.Modules.CharacterController.Editor.AnimationFitStudio
{
    public static class CCS_AnimationFitStudioClipCurveModeUtility
    {
        public const string HumanoidMuscleCurvesLabel = "Humanoid Muscle Curves";
        public const string TransformCurvesLabel = "Transform Curves";

        public static CCS_AnimationFitStudioClipCurveMode DetectClipCurveMode(AnimationClip clip)
        {
            if (clip == null)
            {
                return CCS_AnimationFitStudioClipCurveMode.TransformCurves;
            }

            EditorCurveBinding[] bindings = AnimationUtility.GetCurveBindings(clip);
            for (int i = 0; i < bindings.Length; i++)
            {
                if (IsHumanoidMuscleBinding(bindings[i]))
                {
                    return CCS_AnimationFitStudioClipCurveMode.HumanoidMuscleCurves;
                }
            }

            return CCS_AnimationFitStudioClipCurveMode.TransformCurves;
        }

        public static bool IsHumanoidMuscleBinding(EditorCurveBinding binding)
        {
            if (binding.type != typeof(Animator))
            {
                return false;
            }

            if (!string.IsNullOrEmpty(binding.path))
            {
                return false;
            }

            return CCS_AnimationFitStudioHumanoidMuscleMapping.TryGetMuscleIndex(
                binding.propertyName,
                out _);
        }

        public static string GetDisplayLabel(CCS_AnimationFitStudioClipCurveMode mode)
        {
            return mode == CCS_AnimationFitStudioClipCurveMode.HumanoidMuscleCurves
                ? HumanoidMuscleCurvesLabel
                : TransformCurvesLabel;
        }

        public static int CountHumanoidMuscleBindings(AnimationClip clip)
        {
            if (clip == null)
            {
                return 0;
            }

            int count = 0;
            EditorCurveBinding[] bindings = AnimationUtility.GetCurveBindings(clip);
            for (int i = 0; i < bindings.Length; i++)
            {
                if (IsHumanoidMuscleBinding(bindings[i]))
                {
                    count++;
                }
            }

            return count;
        }
    }
}
