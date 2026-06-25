using System.Text;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_AnimationFitStudioCurveHashUtility
// CATEGORY: Modules / CharacterController / Editor / AnimationFitStudio
// PURPOSE: Computes stable curve signatures for controller clip save proof.
// PLACEMENT: Editor utility used by Animation Fit Studio save workflow and validation.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Hashes all curve bindings and sampled key values for before/after save comparison.
// =============================================================================

namespace CCS.Modules.CharacterController.Editor.AnimationFitStudio
{
    public static class CCS_AnimationFitStudioCurveHashUtility
    {
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
