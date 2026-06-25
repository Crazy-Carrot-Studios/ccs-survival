using UnityEditor;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

// =============================================================================
// SCRIPT: CCS_AnimationFitStudioPlayablePreviewSampler
// CATEGORY: Modules / CharacterController / Editor / AnimationFitStudio
// PURPOSE: Editor-only clip sampling via PlayableGraph with AnimationMode fallback.
// PLACEMENT: Used by Animation Fit Studio pose utility and clip audition.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Clears runtime Animator Controller during preview so idle does not override clips.
// =============================================================================

namespace CCS.Modules.CharacterController.Editor.AnimationFitStudio
{
    public static class CCS_AnimationFitStudioPlayablePreviewSampler
    {
        public const string PlayableGraphMethodLabel = "PlayableGraph";

        public const string AnimationModeMethodLabel = "AnimationMode";

        public static void PrepareAnimatorForPreviewSampling(
            Animator animator,
            CCS_AnimationFitStudioPreviewState previewState)
        {
            if (animator == null)
            {
                return;
            }

            animator.enabled = true;
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            animator.fireEvents = false;
            animator.keepAnimatorStateOnDisable = true;
            animator.speed = 0f;
            animator.Update(0f);

            if (previewState == null)
            {
                animator.runtimeAnimatorController = null;
                return;
            }

            if (!previewState.AnimatorControllerClearedForPreview
                && animator.runtimeAnimatorController != null)
            {
                previewState.StoredRuntimeAnimatorController = animator.runtimeAnimatorController;
                previewState.AnimatorControllerClearedForPreview = true;
            }

            animator.runtimeAnimatorController = null;
        }

        public static void RestoreAnimatorController(CCS_AnimationFitStudioPreviewState previewState)
        {
            if (previewState?.PreviewAnimator == null)
            {
                return;
            }

            if (previewState.AnimatorControllerClearedForPreview)
            {
                previewState.PreviewAnimator.runtimeAnimatorController =
                    previewState.StoredRuntimeAnimatorController;
                previewState.AnimatorControllerClearedForPreview = false;
            }
        }

        public static bool TrySampleClip(
            Animator animator,
            CCS_AnimationFitStudioPreviewState previewState,
            AnimationClip clip,
            float poseTime,
            out string methodUsed)
        {
            methodUsed = string.Empty;
            if (animator == null || clip == null)
            {
                return false;
            }

            PrepareAnimatorForPreviewSampling(animator, previewState);
            float clampedTime = Mathf.Clamp(poseTime, 0f, clip.length);

            if (TrySampleWithPlayableGraph(animator, clip, clampedTime))
            {
                methodUsed = PlayableGraphMethodLabel;
                return true;
            }

            if (TrySampleWithAnimationMode(animator, clip, clampedTime))
            {
                methodUsed = AnimationModeMethodLabel;
                return true;
            }

            return false;
        }

        private static bool TrySampleWithPlayableGraph(Animator animator, AnimationClip clip, float poseTime)
        {
            PlayableGraph graph = default;
            try
            {
                graph = PlayableGraph.Create("CCS_AnimationFitStudioPreview");
                graph.SetTimeUpdateMode(DirectorUpdateMode.Manual);

                AnimationClipPlayable clipPlayable = AnimationClipPlayable.Create(graph, clip);
                clipPlayable.SetApplyFootIK(false);
                clipPlayable.SetTime(poseTime);
                clipPlayable.SetTime(poseTime);

                AnimationPlayableOutput output = AnimationPlayableOutput.Create(
                    graph,
                    "CCS_AnimationFitStudioPreviewOutput",
                    animator);
                output.SetSourcePlayable(clipPlayable);

                graph.Evaluate(0f);
                animator.Update(0f);
                return true;
            }
            catch
            {
                return false;
            }
            finally
            {
                if (graph.IsValid())
                {
                    graph.Destroy();
                }
            }
        }

        private static bool TrySampleWithAnimationMode(Animator animator, AnimationClip clip, float poseTime)
        {
            try
            {
                if (!AnimationMode.InAnimationMode())
                {
                    AnimationMode.StartAnimationMode();
                }

                AnimationMode.BeginSampling();
                AnimationMode.SampleAnimationClip(animator.gameObject, clip, poseTime);
                AnimationMode.EndSampling();
                animator.Update(0f);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
