using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_PlayerAnimatorRuntimeDiagnostics
// CATEGORY: Modules / CharacterController / Runtime / Visuals
// PURPOSE: Dev-only Animator playback diagnostics for layer weights, states, and parameters.
// PLACEMENT: PF_CCS_CharacterController_TestPlayer_Networked / VisualRoot (optional, disabled by default).
// AUTHOR: James Schilz
// CREATED: 2026-06-25
// NOTES: v0.8.1b — console/Markdown only. No on-screen overlay.
// =============================================================================

namespace CCS.Modules.CharacterController
{
    [DefaultExecutionOrder(320)]
    public sealed class CCS_PlayerAnimatorRuntimeDiagnostics : MonoBehaviour
    {
        #region Variables

        private const string RuntimeReportRelativePath = "Logs/player-animator-runtime-report.md";

        [SerializeField] private Animator animator;
        [SerializeField] private bool enableDiagnostics;
        [SerializeField] private bool logToConsole;
        [SerializeField] private bool writeMarkdownReport;
        [SerializeField] private float logIntervalSeconds = 1f;

        private float nextLogTime;
        private bool loggedFallbackAnimator;

        #endregion

        #region Unity Callbacks

        private void LateUpdate()
        {
            if (!enableDiagnostics)
            {
                return;
            }

            if (!TryResolveAnimator(out Animator resolvedAnimator))
            {
                if (logToConsole && Time.unscaledTime >= nextLogTime)
                {
                    nextLogTime = Time.unscaledTime + Mathf.Max(0.25f, logIntervalSeconds);
                    Debug.Log("[Player Animator Diagnostics] No authoritative Animator found.", this);
                }

                return;
            }

            string report = BuildDiagnosticsReport(resolvedAnimator, BuildAnimatorPath(resolvedAnimator));

            if (logToConsole && Time.unscaledTime >= nextLogTime)
            {
                nextLogTime = Time.unscaledTime + Mathf.Max(0.25f, logIntervalSeconds);
                Debug.Log("[Player Animator Diagnostics]\n" + report, this);
            }

            if (writeMarkdownReport)
            {
                WriteMarkdownReport(report);
            }
        }

        #endregion

        #region Private Methods

        private void WriteMarkdownReport(string reportBody)
        {
            string projectRoot = Directory.GetParent(Application.dataPath).FullName;
            string reportPath = Path.Combine(projectRoot, RuntimeReportRelativePath);
            string directory = Path.GetDirectoryName(reportPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            StringBuilder builder = new StringBuilder(reportBody.Length + 128);
            builder.AppendLine("# Player Animator Runtime Report");
            builder.AppendLine();
            builder.AppendLine("```");
            builder.AppendLine(reportBody);
            builder.AppendLine("```");
            File.WriteAllText(reportPath, builder.ToString());
        }

        private bool TryResolveAnimator(out Animator resolvedAnimator)
        {
            if (animator != null && CCS_PlayerAnimatorResolver.IsAuthoritativeGameplayAnimator(animator))
            {
                resolvedAnimator = animator;
                return true;
            }

            CCS_PlayerRuntimeFacade facade = GetComponentInParent<CCS_PlayerRuntimeFacade>(true);
            if (facade != null
                && facade.Animator != null
                && CCS_PlayerAnimatorResolver.IsAuthoritativeGameplayAnimator(facade.Animator))
            {
                animator = facade.Animator;
                resolvedAnimator = animator;
                return true;
            }

            if (CCS_PlayerAnimatorResolver.TryResolveAuthoritativeAnimator(
                    transform,
                    out resolvedAnimator,
                    out bool usedFallback))
            {
                animator = resolvedAnimator;
                if (usedFallback && !loggedFallbackAnimator)
                {
                    loggedFallbackAnimator = true;
                    Debug.LogWarning(
                        "[Player Animator Diagnostics] Used fallback authoritative Animator resolution.",
                        this);
                }

                return true;
            }

            resolvedAnimator = null;
            return false;
        }

        private static string BuildAnimatorPath(Animator resolvedAnimator)
        {
            if (resolvedAnimator == null)
            {
                return "Missing";
            }

            Transform current = resolvedAnimator.transform;
            Stack<string> segments = new Stack<string>();
            while (current != null)
            {
                segments.Push(current.name);
                current = current.parent;
            }

            return string.Join("/", segments);
        }

        private static string BuildDiagnosticsReport(Animator resolvedAnimator, string animatorPath)
        {
            StringBuilder builder = new StringBuilder(4096);
            builder.Append("Animator Path: ");
            builder.Append(animatorPath);
            builder.Append("\nAvatar: ");
            builder.Append(resolvedAnimator.avatar != null ? resolvedAnimator.avatar.name : "Missing");
            builder.Append("\nController: ");
            RuntimeAnimatorController runtimeController = resolvedAnimator.runtimeAnimatorController;
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
            AppendFloat(builder, resolvedAnimator, CCS_PlayerAnimatorParameterIds.SpeedNormalizedName);
            AppendBool(builder, resolvedAnimator, CCS_PlayerAnimatorParameterIds.IsGroundedName);
            AppendBool(builder, resolvedAnimator, CCS_PlayerAnimatorParameterIds.IsSprintingName);
            AppendBool(builder, resolvedAnimator, CCS_PlayerAnimatorParameterIds.IsAimingMovementModeName);
            AppendFloat(builder, resolvedAnimator, CCS_PlayerAnimatorParameterIds.AimMoveXName);
            AppendFloat(builder, resolvedAnimator, CCS_PlayerAnimatorParameterIds.AimMoveYName);
            AppendBool(builder, resolvedAnimator, CCS_PlayerAnimatorParameterIds.RevolverAimHeldName);
            AppendBool(builder, resolvedAnimator, CCS_PlayerAnimatorParameterIds.RevolverIsReloadingName);
            AppendTrigger(builder, resolvedAnimator, CCS_PlayerAnimatorParameterIds.JumpTriggerName);
            AppendTrigger(builder, resolvedAnimator, CCS_PlayerAnimatorParameterIds.PickUpRightHandTriggerName);
            AppendTrigger(builder, resolvedAnimator, CCS_PlayerAnimatorParameterIds.WalkThroughDoorRightHandTriggerName);
            AppendTrigger(builder, resolvedAnimator, CCS_PlayerAnimatorParameterIds.RevolverFireTriggerName);
            AppendTrigger(builder, resolvedAnimator, CCS_PlayerAnimatorParameterIds.RevolverReloadTriggerName);

            return builder.ToString();
        }

        private static void AppendFloat(StringBuilder builder, Animator animator, string parameterName)
        {
            builder.Append("\n  ");
            builder.Append(parameterName);
            builder.Append(": ");
            builder.Append(animator.GetFloat(Animator.StringToHash(parameterName)).ToString("0.000"));
        }

        private static void AppendBool(StringBuilder builder, Animator animator, string parameterName)
        {
            builder.Append("\n  ");
            builder.Append(parameterName);
            builder.Append(": ");
            builder.Append(animator.GetBool(Animator.StringToHash(parameterName)));
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
