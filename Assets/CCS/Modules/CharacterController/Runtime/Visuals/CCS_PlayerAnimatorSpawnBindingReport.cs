using System.Text;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_PlayerAnimatorSpawnBindingReport
// CATEGORY: Modules / CharacterController / Runtime / Visuals
// PURPOSE: One-shot console report confirming authoritative Animator binding at spawn.
// PLACEMENT: Player VisualRoot. Logs once on Start when enabled.
// AUTHOR: James Schilz
// CREATED: 2026-06-28
// NOTES: v0.8.1b — console only, no UI overlay.
// =============================================================================

namespace CCS.Modules.CharacterController
{
    [DefaultExecutionOrder(-400)]
    public sealed class CCS_PlayerAnimatorSpawnBindingReport : MonoBehaviour
    {
        #region Variables

        [SerializeField] private bool enableSpawnReport = true;

        private bool loggedReport;

        #endregion

        #region Unity Callbacks

        private void Start()
        {
            if (!enableSpawnReport || loggedReport)
            {
                return;
            }

            if (!CCS_PlayerAnimatorResolver.TryResolveAuthoritativeAnimator(
                    transform,
                    out Animator animator,
                    out _))
            {
                Debug.LogWarning("[CCS Player Animator Binding] No authoritative Animator found at spawn.", this);
                loggedReport = true;
                return;
            }

            loggedReport = true;
            Debug.Log(BuildReport(animator, transform.root), this);
        }

        #endregion

        #region Private Methods

        private static string BuildReport(Animator animator, Transform searchRoot)
        {
            StringBuilder builder = new StringBuilder(2048);
            builder.AppendLine("[CCS Player Animator Binding]");
            builder.Append("Animator: ");
            builder.AppendLine(BuildAnimatorPath(animator.transform));
            builder.Append("Avatar: ");
            builder.Append(animator.avatar != null ? animator.avatar.name : "Missing");
            builder.Append(", isHuman=");
            builder.Append((animator.avatar != null && animator.avatar.isHuman).ToString());
            builder.Append(", isValid=");
            builder.AppendLine((animator.avatar != null && animator.avatar.isValid).ToString());
            builder.Append("Controller: ");
            builder.AppendLine(animator.runtimeAnimatorController != null
                ? animator.runtimeAnimatorController.name
                : "Missing");
            builder.Append("ApplyRootMotion: ");
            builder.AppendLine(animator.applyRootMotion.ToString());
            builder.Append("CullingMode: ");
            builder.AppendLine(animator.cullingMode.ToString());
            builder.Append("UpdateMode: ");
            builder.AppendLine(animator.updateMode.ToString());

            AnimatorClipInfo[] baseClips = animator.GetCurrentAnimatorClipInfo(0);
            builder.Append("Base Clip: ");
            builder.AppendLine(baseClips.Length > 0 && baseClips[0].clip != null
                ? baseClips[0].clip.name
                : "None");
            builder.Append("SpeedNormalized: ");
            builder.AppendLine(animator.GetFloat(CCS_PlayerAnimatorParameterIds.SpeedNormalized).ToString("0.000"));

            SkinnedMeshRenderer[] renderers = animator.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            builder.Append("SkinnedMeshRenderers: ");
            builder.AppendLine(renderers.Length.ToString());
            builder.AppendLine("Authoritative Animator Users:");
            AppendUserMatch(builder, "CCS_PlayerLocomotionAnimator", FindAnimatorReference<CCS_PlayerLocomotionAnimator>(searchRoot), animator);
            AppendUserMatch(builder, "CCS_PlayerInteractionAnimator", FindAnimatorReference<CCS_PlayerInteractionAnimator>(searchRoot), animator);
            AppendUserMatch(builder, "CCS_RevolverUpperBodyAnimator", FindAnimatorReference<CCS_RevolverUpperBodyAnimator>(searchRoot), animator);
            AppendUserMatch(builder, "CCS_RevolverArmReticleIK", FindComponentAnimatorReference(searchRoot, "CCS_RevolverArmReticleIK"), animator);
            AppendUserMatch(builder, "CCS_RevolverBodyAimFollowController", FindComponentAnimatorReference(searchRoot, "CCS_RevolverBodyAimFollowController"), animator);
            return builder.ToString();
        }

        private static void AppendUserMatch(
            StringBuilder builder,
            string label,
            Animator referencedAnimator,
            Animator authoritativeAnimator)
        {
            builder.Append("* ");
            builder.Append(label);
            builder.Append(" -> same animator: ");
            builder.AppendLine(referencedAnimator == authoritativeAnimator ? "true" : "false");
        }

        private static Animator FindAnimatorReference<T>(Transform searchRoot) where T : Component
        {
            T component = searchRoot.GetComponentInChildren<T>(true);
            if (component == null)
            {
                return null;
            }

            return ReadAnimatorField(component, "animator")
                ?? ReadAnimatorField(component, "characterAnimator");
        }

        private static Animator FindComponentAnimatorReference(Transform searchRoot, string typeName)
        {
            MonoBehaviour[] behaviours = searchRoot.GetComponentsInChildren<MonoBehaviour>(true);
            for (int index = 0; index < behaviours.Length; index++)
            {
                MonoBehaviour behaviour = behaviours[index];
                if (behaviour == null || behaviour.GetType().Name != typeName)
                {
                    continue;
                }

                return ReadAnimatorField(behaviour, "animator");
            }

            return null;
        }

        private static Animator ReadAnimatorField(Component component, string fieldName)
        {
            System.Reflection.FieldInfo field = component.GetType().GetField(
                fieldName,
                System.Reflection.BindingFlags.Instance
                | System.Reflection.BindingFlags.NonPublic
                | System.Reflection.BindingFlags.Public);
            return field != null ? field.GetValue(component) as Animator : null;
        }

        private static string BuildAnimatorPath(Transform transform)
        {
            StringBuilder builder = new StringBuilder(256);
            BuildPathRecursive(transform, builder);
            return builder.ToString();
        }

        private static void BuildPathRecursive(Transform current, StringBuilder builder)
        {
            if (current.parent != null)
            {
                BuildPathRecursive(current.parent, builder);
                builder.Append('/');
            }

            builder.Append(current.name);
        }

        #endregion
    }
}
