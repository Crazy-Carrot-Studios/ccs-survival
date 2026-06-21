using System.Collections.Generic;
using System.IO;
using CCS.Project;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_CharacterControllerAnimationValidationUtility
// CATEGORY: Modules / CharacterController / Editor / Validation
// PURPOSE: Validates player Animator Controller uses only CCS-owned animation clips.
// PLACEMENT: Called from master test validator and animation isolation menu.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: v0.5.6 — rejects vendor-pack clip paths on production player Animator Controller.
// =============================================================================

namespace CCS.Modules.CharacterController.Editor
{
    public static class CCS_CharacterControllerAnimationValidationUtility
    {
        #region Public Methods

        public static CCS_SurvivalValidationResult ValidatePlayerAnimatorControllerAnimationIsolation()
        {
            List<string> failures = new List<string>();

            AppendIfMissing(
                failures,
                File.Exists(CCS_CharacterControllerConstants.PlayerLocomotionAnimatorControllerPath),
                "Missing player Animator Controller at "
                + CCS_CharacterControllerConstants.PlayerLocomotionAnimatorControllerPath
                + ".");

            AppendIfMissing(
                failures,
                Directory.Exists(CCS_CharacterControllerConstants.ContentAnimationsRootPath),
                "Missing Content/Animations folder at "
                + CCS_CharacterControllerConstants.ContentAnimationsRootPath
                + ".");

            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(
                CCS_CharacterControllerConstants.PlayerLocomotionAnimatorControllerPath);
            if (controller == null)
            {
                return CCS_SurvivalValidationResult.Fail(string.Join(" ", failures));
            }

            HashSet<Motion> visitedMotions = new HashSet<Motion>();
            List<AnimationClip> clips = new List<AnimationClip>();
            for (int layerIndex = 0; layerIndex < controller.layers.Length; layerIndex++)
            {
                CollectAnimationClips(controller.layers[layerIndex].stateMachine, visitedMotions, clips);
            }

            AppendIfMissing(
                failures,
                clips.Count > 0,
                "Player Animator Controller resolved zero animation clips.");

            string allowedRoot = NormalizeAssetPath(CCS_CharacterControllerConstants.ContentAnimationsRootPath);
            for (int i = 0; i < clips.Count; i++)
            {
                AnimationClip clip = clips[i];
                if (clip == null)
                {
                    continue;
                }

                string clipPath = AssetDatabase.GetAssetPath(clip);
                if (string.IsNullOrEmpty(clipPath))
                {
                    failures.Add("Player Animator Controller references unresolved animation clip.");
                    continue;
                }

                string normalizedClipPath = NormalizeAssetPath(clipPath);
                if (!normalizedClipPath.StartsWith(allowedRoot))
                {
                    failures.Add(
                        "Player Animator Controller references non-CCS animation clip: "
                        + normalizedClipPath);
                }
            }

            return failures.Count > 0
                ? CCS_SurvivalValidationResult.Fail(string.Join(" ", failures))
                : CCS_SurvivalValidationResult.Pass(
                    "Player Animator Controller uses CCS-owned animation clips only.");
        }

        #endregion

        #region Private Methods

        private static void CollectAnimationClips(
            AnimatorStateMachine stateMachine,
            HashSet<Motion> visitedMotions,
            List<AnimationClip> clips)
        {
            ChildAnimatorState[] states = stateMachine.states;
            for (int i = 0; i < states.Length; i++)
            {
                CollectMotionClips(states[i].state != null ? states[i].state.motion : null, visitedMotions, clips);
            }

            ChildAnimatorStateMachine[] childStateMachines = stateMachine.stateMachines;
            for (int i = 0; i < childStateMachines.Length; i++)
            {
                if (childStateMachines[i].stateMachine != null)
                {
                    CollectAnimationClips(childStateMachines[i].stateMachine, visitedMotions, clips);
                }
            }
        }

        private static void CollectMotionClips(Motion motion, HashSet<Motion> visitedMotions, List<AnimationClip> clips)
        {
            if (motion == null || !visitedMotions.Add(motion))
            {
                return;
            }

            if (motion is AnimationClip clip)
            {
                clips.Add(clip);
                return;
            }

            if (motion is BlendTree blendTree)
            {
                ChildMotion[] children = blendTree.children;
                for (int i = 0; i < children.Length; i++)
                {
                    CollectMotionClips(children[i].motion, visitedMotions, clips);
                }
            }
        }

        private static string NormalizeAssetPath(string assetPath)
        {
            return assetPath.Replace('\\', '/');
        }

        private static void AppendIfMissing(List<string> failures, bool condition, string message)
        {
            if (!condition)
            {
                failures.Add(message);
            }
        }

        #endregion
    }
}
