using System.Collections.Generic;
using System.IO;
using CCS.Modules.CharacterController;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_RevolverAimPitchClipBuilder
// CATEGORY: Modules / CharacterController / Editor / Validation
// PURPOSE: Generates Down/Center/Up FitTest aim pitch clips from center FitTest pose.
// PLACEMENT: Editor builder invoked from animation isolation and Master Test setup.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Starter poses only — refine in Animation Fit Studio after blend is working.
// =============================================================================

namespace CCS.Modules.CharacterController.Editor
{
    public static class CCS_RevolverAimPitchClipBuilder
    {
        private struct BonePitchRule
        {
            public string PathFragment;
            public float UpPitchDegrees;
            public float DownPitchDegrees;

            public BonePitchRule(string pathFragment, float upPitchDegrees, float downPitchDegrees)
            {
                PathFragment = pathFragment;
                UpPitchDegrees = upPitchDegrees;
                DownPitchDegrees = downPitchDegrees;
            }
        }

        private static readonly BonePitchRule[] BonePitchRules =
        {
            new BonePitchRule("spine_01", 6f, -6f),
            new BonePitchRule("spine_02", 10f, -10f),
            new BonePitchRule("spine_03", 14f, -14f),
            new BonePitchRule("clavicle_r", 8f, -8f),
            new BonePitchRule("upperarm_r", 12f, -12f),
            new BonePitchRule("lowerarm_r", 6f, -6f),
            new BonePitchRule("hand_r", 4f, -4f),
        };

        public static bool EnsureAimPitchFitTestClips()
        {
            string centerPath = CCS_CharacterControllerConstants.RevolverAimPitchCenterFitTestClipPath;
            string downPath = CCS_CharacterControllerConstants.RevolverAimPitchDownFitTestClipPath;
            string upPath = CCS_CharacterControllerConstants.RevolverAimPitchUpFitTestClipPath;
            string sourceCenterPath = CCS_CharacterControllerConstants.WildWestRevolverRuntimeDefaultAimIdleClipPath;

            if (!File.Exists(sourceCenterPath))
            {
                Debug.LogError(
                    "[Aim Pitch Clips] Missing center FitTest source at "
                    + sourceCenterPath);
                return false;
            }

            bool changed = false;
            EnsureFolder(Path.GetDirectoryName(centerPath));
            if (!File.Exists(centerPath))
            {
                File.Copy(sourceCenterPath, centerPath, overwrite: false);
                changed = true;
            }

            if (!File.Exists(downPath))
            {
                File.Copy(centerPath, downPath, overwrite: false);
                changed = true;
            }

            if (!File.Exists(upPath))
            {
                File.Copy(centerPath, upPath, overwrite: false);
                changed = true;
            }

            if (changed)
            {
                AssetDatabase.ImportAsset(centerPath);
                AssetDatabase.ImportAsset(downPath);
                AssetDatabase.ImportAsset(upPath);
            }

            AnimationClip centerClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(centerPath);
            AnimationClip downClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(downPath);
            AnimationClip upClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(upPath);
            if (centerClip == null || downClip == null || upClip == null)
            {
                Debug.LogError("[Aim Pitch Clips] Could not load generated aim pitch FitTest clips.");
                return false;
            }

            changed |= EnsureLoopingClip(centerClip);
            changed |= EnsureLoopingClip(downClip);
            changed |= EnsureLoopingClip(upClip);
            changed |= ApplyStarterPitchOffsets(downClip, pitchSign: -1f);
            changed |= ApplyStarterPitchOffsets(upClip, pitchSign: 1f);

            if (changed)
            {
                AssetDatabase.SaveAssets();
            }

            return true;
        }

        private static void EnsureFolder(string folderPath)
        {
            if (string.IsNullOrEmpty(folderPath) || AssetDatabase.IsValidFolder(folderPath))
            {
                return;
            }

            string parent = Path.GetDirectoryName(folderPath)?.Replace('\\', '/');
            string leaf = Path.GetFileName(folderPath);
            if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent))
            {
                EnsureFolder(parent);
            }

            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                AssetDatabase.CreateFolder(parent, leaf);
            }
        }

        private static bool EnsureLoopingClip(AnimationClip clip)
        {
            AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
            if (settings.loopTime)
            {
                return false;
            }

            settings.loopTime = true;
            AnimationUtility.SetAnimationClipSettings(clip, settings);
            EditorUtility.SetDirty(clip);
            return true;
        }

        private static bool ApplyStarterPitchOffsets(AnimationClip clip, float pitchSign)
        {
            bool changed = false;
            Dictionary<string, EditorCurveBinding[]> bindingsByPath = CollectRotationBindingsByPath(clip);
            foreach (KeyValuePair<string, EditorCurveBinding[]> pair in bindingsByPath)
            {
                float pitchDegrees = ResolvePitchDegreesForPath(pair.Key, pitchSign);
                if (Mathf.Approximately(pitchDegrees, 0f))
                {
                    continue;
                }

                if (ApplyPitchToPath(clip, pair.Value, pitchDegrees))
                {
                    changed = true;
                }
            }

            if (changed)
            {
                EditorUtility.SetDirty(clip);
            }

            return changed;
        }

        private static float ResolvePitchDegreesForPath(string path, float pitchSign)
        {
            for (int i = 0; i < BonePitchRules.Length; i++)
            {
                BonePitchRule rule = BonePitchRules[i];
                if (path.IndexOf(rule.PathFragment, System.StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return pitchSign < 0f ? rule.DownPitchDegrees : rule.UpPitchDegrees;
                }
            }

            return 0f;
        }

        private static Dictionary<string, EditorCurveBinding[]> CollectRotationBindingsByPath(AnimationClip clip)
        {
            EditorCurveBinding[] bindings = AnimationUtility.GetCurveBindings(clip);
            Dictionary<string, List<EditorCurveBinding>> grouped = new Dictionary<string, List<EditorCurveBinding>>();
            for (int i = 0; i < bindings.Length; i++)
            {
                EditorCurveBinding binding = bindings[i];
                if (!binding.propertyName.StartsWith("m_LocalRotation."))
                {
                    continue;
                }

                if (!grouped.TryGetValue(binding.path, out List<EditorCurveBinding> list))
                {
                    list = new List<EditorCurveBinding>();
                    grouped[binding.path] = list;
                }

                list.Add(binding);
            }

            Dictionary<string, EditorCurveBinding[]> result = new Dictionary<string, EditorCurveBinding[]>();
            foreach (KeyValuePair<string, List<EditorCurveBinding>> pair in grouped)
            {
                result[pair.Key] = pair.Value.ToArray();
            }

            return result;
        }

        private static bool ApplyPitchToPath(AnimationClip clip, EditorCurveBinding[] bindings, float pitchDegrees)
        {
            float[] sampleTimes = { 0f, 0.01f };
            bool changed = false;
            for (int timeIndex = 0; timeIndex < sampleTimes.Length; timeIndex++)
            {
                float time = sampleTimes[timeIndex];
                if (!TrySampleLocalRotation(clip, bindings, time, out Quaternion rotation))
                {
                    continue;
                }

                Quaternion pitched = Quaternion.AngleAxis(pitchDegrees, Vector3.right) * rotation;
                if (WriteLocalRotation(clip, bindings, time, pitched))
                {
                    changed = true;
                }
            }

            return changed;
        }

        private static bool TrySampleLocalRotation(
            AnimationClip clip,
            EditorCurveBinding[] bindings,
            float time,
            out Quaternion rotation)
        {
            rotation = Quaternion.identity;
            float x = 0f;
            float y = 0f;
            float z = 0f;
            float w = 1f;
            bool hasAny = false;
            for (int i = 0; i < bindings.Length; i++)
            {
                EditorCurveBinding binding = bindings[i];
                AnimationCurve curve = AnimationUtility.GetEditorCurve(clip, binding);
                if (curve == null || curve.length == 0)
                {
                    continue;
                }

                float value = curve.Evaluate(time);
                switch (binding.propertyName)
                {
                    case "m_LocalRotation.x":
                        x = value;
                        hasAny = true;
                        break;
                    case "m_LocalRotation.y":
                        y = value;
                        hasAny = true;
                        break;
                    case "m_LocalRotation.z":
                        z = value;
                        hasAny = true;
                        break;
                    case "m_LocalRotation.w":
                        w = value;
                        hasAny = true;
                        break;
                }
            }

            if (!hasAny)
            {
                return false;
            }

            rotation = new Quaternion(x, y, z, w);
            return true;
        }

        private static bool WriteLocalRotation(
            AnimationClip clip,
            EditorCurveBinding[] bindings,
            float time,
            Quaternion rotation)
        {
            bool changed = false;
            for (int i = 0; i < bindings.Length; i++)
            {
                EditorCurveBinding binding = bindings[i];
                float value;
                switch (binding.propertyName)
                {
                    case "m_LocalRotation.x":
                        value = rotation.x;
                        break;
                    case "m_LocalRotation.y":
                        value = rotation.y;
                        break;
                    case "m_LocalRotation.z":
                        value = rotation.z;
                        break;
                    case "m_LocalRotation.w":
                        value = rotation.w;
                        break;
                    default:
                        continue;
                }

                AnimationCurve curve = AnimationUtility.GetEditorCurve(clip, binding);
                if (curve == null)
                {
                    curve = new AnimationCurve();
                }

                if (!SetOrAddKey(curve, time, value))
                {
                    continue;
                }

                AnimationUtility.SetEditorCurve(clip, binding, curve);
                changed = true;
            }

            return changed;
        }

        private static bool SetOrAddKey(AnimationCurve curve, float time, float value)
        {
            bool changed = false;
            int keyIndex = curve.keys.Length > 0 ? 0 : -1;
            for (int i = 0; i < curve.keys.Length; i++)
            {
                if (Mathf.Approximately(curve.keys[i].time, time))
                {
                    keyIndex = i;
                    break;
                }
            }

            if (keyIndex >= 0)
            {
                Keyframe key = curve.keys[keyIndex];
                if (!Mathf.Approximately(key.value, value))
                {
                    key.value = value;
                    curve.MoveKey(keyIndex, key);
                    changed = true;
                }
            }
            else
            {
                curve.AddKey(time, value);
                changed = true;
            }

            return changed;
        }
    }
}
