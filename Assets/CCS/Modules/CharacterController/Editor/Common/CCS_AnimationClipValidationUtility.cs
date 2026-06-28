using System.Text;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_AnimationClipValidationUtility
// CATEGORY: Modules / CharacterController / Editor / Common
// PURPOSE: Shared animation clip curve checks for production validation.
// PLACEMENT: Editor validation utilities. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-28
// NOTES: Replaces Animation Fit Studio curve helpers removed in v0.7.1c.
// =============================================================================

namespace CCS.Modules.CharacterController.Editor
{
    public static class CCS_AnimationClipValidationUtility
    {
        public static bool ClipUsesHumanoidMuscleCurves(AnimationClip clip)
        {
            if (clip == null)
            {
                return false;
            }

            EditorCurveBinding[] bindings = AnimationUtility.GetCurveBindings(clip);
            for (int i = 0; i < bindings.Length; i++)
            {
                EditorCurveBinding binding = bindings[i];
                if (binding.type == typeof(Animator) && string.IsNullOrEmpty(binding.path))
                {
                    return true;
                }
            }

            return false;
        }

        public static string ComputeCurveHash(AnimationClip clip)
        {
            if (clip == null)
            {
                return "missing-clip";
            }

            EditorCurveBinding[] bindings = AnimationUtility.GetCurveBindings(clip);
            EditorCurveBinding[] objectBindings = AnimationUtility.GetObjectReferenceCurveBindings(clip);
            StringBuilder builder = new StringBuilder(4096);
            builder.Append(clip.name).Append('|').Append(bindings.Length).Append('|').Append(objectBindings.Length).Append('|');

            for (int i = 0; i < bindings.Length; i++)
            {
                EditorCurveBinding binding = bindings[i];
                builder.Append(binding.path).Append(':').Append(binding.propertyName).Append('=');
                AnimationCurve curve = AnimationUtility.GetEditorCurve(clip, binding);
                if (curve == null || curve.length == 0)
                {
                    builder.Append("empty;");
                    continue;
                }

                builder.Append(curve.length).Append('@');
                for (int keyIndex = 0; keyIndex < curve.length; keyIndex++)
                {
                    Keyframe keyframe = curve[keyIndex];
                    builder.Append(keyframe.time.ToString("F4"))
                        .Append(',')
                        .Append(keyframe.value.ToString("F4"))
                        .Append(';');
                }
            }

            for (int i = 0; i < objectBindings.Length; i++)
            {
                EditorCurveBinding binding = objectBindings[i];
                builder.Append('O')
                    .Append(binding.path)
                    .Append(':')
                    .Append(binding.propertyName)
                    .Append(';');
            }

            return builder.ToString().GetHashCode().ToString("X8");
        }
    }
}
