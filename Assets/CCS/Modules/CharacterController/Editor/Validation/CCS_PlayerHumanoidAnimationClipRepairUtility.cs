using System.Collections.Generic;
using System.IO;
using CCS.Modules.CharacterController.Editor.AnimationFitStudio;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_PlayerHumanoidAnimationClipRepairUtility
// CATEGORY: Modules / CharacterController / Editor / Validation
// PURPOSE: Strips generic transform curves from humanoid player animation clips.
// PLACEMENT: Editor utility used by v0.8.1b humanoid binding repair builder.
// AUTHOR: James Schilz
// CREATED: 2026-06-28
// NOTES: Hybrid Humanoid+Transform clips cause avatar binding warnings and stiff playback.
// =============================================================================

namespace CCS.Modules.CharacterController.Editor
{
    public static class CCS_PlayerHumanoidAnimationClipRepairUtility
    {
        public static bool IsGenericTransformBinding(EditorCurveBinding binding)
        {
            if (CCS_AnimationFitStudioClipCurveModeUtility.IsHumanoidMuscleBinding(binding))
            {
                return false;
            }

            if (!string.IsNullOrEmpty(binding.path))
            {
                return true;
            }

            return binding.type == typeof(Transform);
        }

        public static bool StripGenericTransformCurves(AnimationClip clip, out int removedBindingCount)
        {
            removedBindingCount = 0;
            if (clip == null)
            {
                return false;
            }

            bool changed = false;
            EditorCurveBinding[] curveBindings = AnimationUtility.GetCurveBindings(clip);
            for (int bindingIndex = 0; bindingIndex < curveBindings.Length; bindingIndex++)
            {
                EditorCurveBinding binding = curveBindings[bindingIndex];
                if (!IsGenericTransformBinding(binding))
                {
                    continue;
                }

                AnimationUtility.SetEditorCurve(clip, binding, null);
                removedBindingCount++;
                changed = true;
            }

            EditorCurveBinding[] objectReferenceBindings = AnimationUtility.GetObjectReferenceCurveBindings(clip);
            for (int bindingIndex = 0; bindingIndex < objectReferenceBindings.Length; bindingIndex++)
            {
                EditorCurveBinding binding = objectReferenceBindings[bindingIndex];
                if (string.IsNullOrEmpty(binding.path))
                {
                    continue;
                }

                AnimationUtility.SetObjectReferenceCurve(clip, binding, null);
                removedBindingCount++;
                changed = true;
            }

            if (changed)
            {
                EditorUtility.SetDirty(clip);
            }

            return changed;
        }

        public static bool RepairClipAtPath(string clipAssetPath, out int removedBindingCount)
        {
            removedBindingCount = 0;
            if (string.IsNullOrEmpty(clipAssetPath) || !File.Exists(clipAssetPath))
            {
                return false;
            }

            AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipAssetPath);
            if (clip == null)
            {
                return false;
            }

            bool changed = StripGenericTransformCurves(clip, out removedBindingCount);
            if (changed)
            {
                CCS_AnimationFitStudioSaveUtility.FinalizeSavedClipImport(clipAssetPath);
            }

            return changed;
        }

        public static bool RepairRequiredControllerClips(out List<string> repairSummaries)
        {
            repairSummaries = new List<string>();
            bool anyChanged = false;
            string[] clipPaths = CCS_PlayerHumanoidAnimationClipValidationUtility.RequiredControllerClipPaths;
            for (int clipIndex = 0; clipIndex < clipPaths.Length; clipIndex++)
            {
                string clipPath = clipPaths[clipIndex];
                if (!RepairClipAtPath(clipPath, out int removedBindings))
                {
                    continue;
                }

                anyChanged = true;
                repairSummaries.Add(clipPath + " removed " + removedBindings + " generic transform binding(s).");
            }

            if (anyChanged)
            {
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            return anyChanged;
        }
    }
}
