using UnityEngine;

// =============================================================================
// SCRIPT: CCS_PlayerAnimatorResolver
// CATEGORY: Modules / CharacterController / Runtime / Animation
// PURPOSE: Resolves the single authoritative humanoid gameplay Animator on player prefabs.
// PLACEMENT: Runtime utility. Used by facade and animation-driving components.
// AUTHOR: James Schilz
// CREATED: 2026-06-28
// NOTES: v0.8.1 — ignores TestVisuals branch and prefers Presentation/VisualRoot model Animator.
// =============================================================================

namespace CCS.Modules.CharacterController
{
    public static class CCS_PlayerAnimatorResolver
    {
        public static bool TryResolveAuthoritativeAnimator(
            Transform searchRoot,
            out Animator animator,
            out bool usedFallback)
        {
            animator = null;
            usedFallback = false;

            if (searchRoot == null)
            {
                return false;
            }

            CCS_PlayerRuntimeFacade facade = searchRoot.GetComponentInParent<CCS_PlayerRuntimeFacade>(true);
            if (facade != null && facade.Animator != null && IsAuthoritativeGameplayAnimator(facade.Animator))
            {
                animator = facade.Animator;
                return true;
            }

            Transform presentation = FindChildRecursive(searchRoot, CCS_PlayerPrefabConstants.PresentationObjectName)
                ?? searchRoot;
            Transform visualRoot = FindChildRecursive(presentation, "VisualRoot");
            if (visualRoot != null)
            {
                Animator visualRootAnimator = FindBestAnimatorUnder(visualRoot);
                if (visualRootAnimator != null)
                {
                    animator = visualRootAnimator;
                    usedFallback = facade == null || facade.Animator != visualRootAnimator;
                    return true;
                }
            }

            Animator[] animators = searchRoot.GetComponentsInChildren<Animator>(true);
            Animator bestCandidate = null;
            int bestScore = int.MinValue;
            for (int animatorIndex = 0; animatorIndex < animators.Length; animatorIndex++)
            {
                Animator candidate = animators[animatorIndex];
                if (!IsAuthoritativeGameplayAnimator(candidate))
                {
                    continue;
                }

                int score = ScoreAnimatorCandidate(candidate);
                if (score > bestScore)
                {
                    bestScore = score;
                    bestCandidate = candidate;
                }
            }

            if (bestCandidate == null)
            {
                return false;
            }

            animator = bestCandidate;
            usedFallback = true;
            return true;
        }

        public static bool IsAuthoritativeGameplayAnimator(Animator candidate)
        {
            if (candidate == null
                || !candidate.isActiveAndEnabled
                || candidate.runtimeAnimatorController == null)
            {
                return false;
            }

            if (IsUnderTestVisuals(candidate.transform))
            {
                return false;
            }

            if (candidate.avatar == null || !candidate.avatar.isHuman)
            {
                return false;
            }

            return true;
        }

        public static bool IsUnderTestVisuals(Transform transform)
        {
            Transform current = transform;
            while (current != null)
            {
                if (current.name == CCS_PlayerPrefabConstants.TestVisualsObjectName)
                {
                    return true;
                }

                if (IsTestVisualObjectName(current.name))
                {
                    return true;
                }

                current = current.parent;
            }

            return false;
        }

        public static bool IsTestVisualObjectName(string objectName)
        {
            if (string.IsNullOrEmpty(objectName)
                || objectName == CCS_PlayerPrefabConstants.TestVisualsObjectName)
            {
                return false;
            }

            return objectName.Contains("Glasses")
                || objectName.Contains("CapsuleVisual")
                || objectName.Contains("TestCapsule")
                || objectName.Contains("DebugVisual")
                || objectName.Contains("CapsuleProxy")
                || objectName.Contains("CapsulePlayer")
                || objectName.Contains("TestVisual");
        }

        private static Animator FindBestAnimatorUnder(Transform root)
        {
            Animator[] animators = root.GetComponentsInChildren<Animator>(true);
            Animator bestCandidate = null;
            int bestScore = int.MinValue;
            for (int animatorIndex = 0; animatorIndex < animators.Length; animatorIndex++)
            {
                Animator candidate = animators[animatorIndex];
                if (!IsAuthoritativeGameplayAnimator(candidate))
                {
                    continue;
                }

                int score = ScoreAnimatorCandidate(candidate);
                if (score > bestScore)
                {
                    bestScore = score;
                    bestCandidate = candidate;
                }
            }

            return bestCandidate;
        }

        private static int ScoreAnimatorCandidate(Animator candidate)
        {
            int score = 0;
            if (candidate.avatar != null && candidate.avatar.isHuman)
            {
                score += 100;
            }

            Transform transform = candidate.transform;
            if (transform.name == CCS_PlayerPrefabConstants.PlayerVisualPrefabInstanceName
                || transform.name == CCS_PlayerPrefabConstants.RealCharacterModelObjectName)
            {
                score += 50;
            }

            if (transform.parent != null && transform.parent.name == "VisualRoot")
            {
                score += 25;
            }

            Transform presentation = FindParentNamed(transform, CCS_PlayerPrefabConstants.PresentationObjectName);
            if (presentation != null)
            {
                score += 10;
            }

            return score;
        }

        private static Transform FindChildRecursive(Transform parent, string childName)
        {
            if (parent == null)
            {
                return null;
            }

            if (parent.name == childName)
            {
                return parent;
            }

            for (int childIndex = 0; childIndex < parent.childCount; childIndex++)
            {
                Transform match = FindChildRecursive(parent.GetChild(childIndex), childName);
                if (match != null)
                {
                    return match;
                }
            }

            return null;
        }

        private static Transform FindParentNamed(Transform transform, string parentName)
        {
            Transform current = transform.parent;
            while (current != null)
            {
                if (current.name == parentName)
                {
                    return current;
                }

                current = current.parent;
            }

            return null;
        }
    }
}
