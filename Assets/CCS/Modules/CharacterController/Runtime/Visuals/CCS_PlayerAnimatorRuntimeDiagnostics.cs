using System.Text;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_PlayerAnimatorRuntimeDiagnostics
// CATEGORY: Modules / CharacterController / Runtime / Visuals
// PURPOSE: Dev-only Animator playback diagnostics for layer weights, states, and parameters.
// PLACEMENT: PF_CCS_CharacterController_TestPlayer_Networked / VisualRoot (optional, disabled by default).
// AUTHOR: James Schilz
// CREATED: 2026-06-25
// NOTES: v0.7.3 — enable in Master Test/hosting scenes to trace layer weight and parameter flow.
// =============================================================================

namespace CCS.Modules.CharacterController
{
    [DefaultExecutionOrder(320)]
    public sealed class CCS_PlayerAnimatorRuntimeDiagnostics : MonoBehaviour
    {
        #region Variables

        [SerializeField] private Animator animator;
        [SerializeField] private bool enableDiagnostics;
        [SerializeField] private bool logToConsole;
        [SerializeField] private bool showOnScreenOverlay = true;
        [SerializeField] private float logIntervalSeconds = 1f;

        private float nextLogTime;
        private string cachedOverlayText = string.Empty;

        #endregion

        #region Unity Callbacks

        private void LateUpdate()
        {
            if (!enableDiagnostics)
            {
                cachedOverlayText = string.Empty;
                return;
            }

            if (!TryResolveAnimator(out Animator resolvedAnimator))
            {
                cachedOverlayText = "Animator diagnostics: no playable Animator found.";
                return;
            }

            cachedOverlayText = BuildDiagnosticsReport(resolvedAnimator);

            if (logToConsole && Time.unscaledTime >= nextLogTime)
            {
                nextLogTime = Time.unscaledTime + Mathf.Max(0.25f, logIntervalSeconds);
                Debug.Log("[Player Animator Diagnostics]\n" + cachedOverlayText, this);
            }
        }

        private void OnGUI()
        {
            if (!enableDiagnostics || !showOnScreenOverlay || string.IsNullOrEmpty(cachedOverlayText))
            {
                return;
            }

            GUI.Label(new Rect(12f, 12f, 980f, 720f), cachedOverlayText);
        }

        #endregion

        #region Private Methods

        private bool TryResolveAnimator(out Animator resolvedAnimator)
        {
            if (animator != null && HasPlayableController(animator))
            {
                resolvedAnimator = animator;
                return true;
            }

            Animator[] animators = GetComponentsInChildren<Animator>(true);
            for (int i = 0; i < animators.Length; i++)
            {
                Animator candidate = animators[i];
                if (candidate != null && HasPlayableController(candidate))
                {
                    animator = candidate;
                    resolvedAnimator = candidate;
                    return true;
                }
            }

            resolvedAnimator = null;
            return false;
        }

        private static bool HasPlayableController(Animator candidate)
        {
            return candidate != null
                && candidate.isActiveAndEnabled
                && candidate.runtimeAnimatorController != null;
        }

        private static string BuildDiagnosticsReport(Animator resolvedAnimator)
        {
            StringBuilder builder = new StringBuilder(4096);
            RuntimeAnimatorController runtimeController = resolvedAnimator.runtimeAnimatorController;
            builder.Append("Controller: ");
            builder.Append(runtimeController != null ? runtimeController.name : "Missing");
            builder.Append("\nLayer Count: ");
            builder.Append(resolvedAnimator.layerCount);

            for (int layerIndex = 0; layerIndex < resolvedAnimator.layerCount; layerIndex++)
            {
                builder.Append("\n\n[Layer ");
                builder.Append(layerIndex);
                builder.Append("] ");
                builder.Append(resolvedAnimator.GetLayerName(layerIndex));
                builder.Append("\n  Weight: ");
                builder.Append(resolvedAnimator.GetLayerWeight(layerIndex).ToString("0.000"));

                AnimatorStateInfo currentState = resolvedAnimator.GetCurrentAnimatorStateInfo(layerIndex);
                builder.Append("\n  State Hash: ");
                builder.Append(currentState.shortNameHash);
                builder.Append("  NormalizedTime: ");
                builder.Append(currentState.normalizedTime.ToString("0.000"));

                if (resolvedAnimator.IsInTransition(layerIndex))
                {
                    AnimatorStateInfo nextState = resolvedAnimator.GetNextAnimatorStateInfo(layerIndex);
                    builder.Append("\n  Next State Hash: ");
                    builder.Append(nextState.shortNameHash);
                    builder.Append("  Next NormalizedTime: ");
                    builder.Append(nextState.normalizedTime.ToString("0.000"));
                }

                AnimatorClipInfo[] clipInfos = resolvedAnimator.GetCurrentAnimatorClipInfo(layerIndex);
                builder.Append("\n  Clip: ");
                builder.Append(clipInfos.Length > 0 && clipInfos[0].clip != null
                    ? clipInfos[0].clip.name
                    : "None");
            }

            builder.Append("\n\n[Parameters]");
            AppendBool(builder, resolvedAnimator, "SpeedNormalized", isFloat: true);
            AppendBool(builder, resolvedAnimator, "IsGrounded");
            AppendBool(builder, resolvedAnimator, "IsSprinting");
            AppendBool(builder, resolvedAnimator, CCS_CharacterControllerConstants.AnimatorIsAimingMovementModeParameter);
            AppendBool(builder, resolvedAnimator, CCS_CharacterControllerConstants.AnimatorAimMoveXParameter, isFloat: true);
            AppendBool(builder, resolvedAnimator, CCS_CharacterControllerConstants.AnimatorAimMoveYParameter, isFloat: true);
            AppendBool(builder, resolvedAnimator, CCS_CharacterControllerConstants.AnimatorRevolverAimHeldParameter);
            AppendBool(builder, resolvedAnimator, CCS_CharacterControllerConstants.AnimatorRevolverIsReloadingParameter);
            AppendTrigger(builder, resolvedAnimator, "JumpTrigger");
            AppendTrigger(builder, resolvedAnimator, CCS_CharacterControllerConstants.AnimatorPickUpRightHandTriggerParameter);
            AppendTrigger(builder, resolvedAnimator, CCS_CharacterControllerConstants.AnimatorWalkThroughDoorRightHandTriggerParameter);
            AppendTrigger(builder, resolvedAnimator, CCS_CharacterControllerConstants.AnimatorRevolverFireTriggerParameter);
            AppendTrigger(builder, resolvedAnimator, CCS_CharacterControllerConstants.AnimatorRevolverReloadTriggerParameter);

            return builder.ToString();
        }

        private static void AppendBool(StringBuilder builder, Animator animator, string parameterName, bool isFloat = false)
        {
            int hash = Animator.StringToHash(parameterName);
            builder.Append("\n  ");
            builder.Append(parameterName);
            builder.Append(": ");

            if (isFloat)
            {
                builder.Append(animator.GetFloat(hash).ToString("0.000"));
                return;
            }

            builder.Append(animator.GetBool(hash));
        }

        private static void AppendTrigger(StringBuilder builder, Animator animator, string parameterName)
        {
            builder.Append("\n  ");
            builder.Append(parameterName);
            builder.Append(" (trigger hash): ");
            builder.Append(Animator.StringToHash(parameterName));
        }

        #endregion
    }
}
