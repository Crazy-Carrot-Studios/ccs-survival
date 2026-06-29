using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CCS.Modules.CharacterController;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_AnimatorControllerInventoryReporter
// CATEGORY: Modules / CharacterController / Editor / Validation
// PURPOSE: Inventories Animator Controller layers, states, parameters, and script writes.
// PLACEMENT: Editor validation utility. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-25
// NOTES: v0.7.3 Phase 3B before/after animator reset reports.
// =============================================================================

namespace CCS.Modules.CharacterController.Editor
{
    public static class CCS_AnimatorControllerInventoryReporter
    {
        private static readonly Regex AnimatorWriteRegex = new Regex(
            @"Animator\.(SetBool|SetFloat|SetTrigger|ResetTrigger|Play|CrossFade|CrossFadeInFixedTime)\s*\(",
            RegexOptions.Compiled);

        public static string GenerateBeforeReportPath =>
            CCS_CharacterControllerConstants.Phase3BAnimatorResetBeforeReportPath;

        public static string GenerateAfterReportPath =>
            CCS_CharacterControllerConstants.Phase3BAnimatorResetAfterReportPath;

        public static string WriteBeforeReport()
        {
            return WriteReport(GenerateBeforeReportPath, "Before v0.7.3 Locomotion-Only Animator Reset");
        }

        public static string WriteAfterReport(
            IReadOnlyList<string> removedLayers,
            IReadOnlyList<string> removedStates,
            IReadOnlyList<string> removedParameters,
            IReadOnlyList<string> changedScripts,
            int playerRootMonoBehaviourCount)
        {
            StringBuilder appendix = new StringBuilder();
            appendix.AppendLine("## Removed Layers");
            AppendBulletList(appendix, removedLayers);
            appendix.AppendLine("## Removed States");
            AppendBulletList(appendix, removedStates);
            appendix.AppendLine("## Removed Parameters");
            AppendBulletList(appendix, removedParameters);
            appendix.AppendLine("## Scripts/Components Changed or Removed");
            AppendBulletList(appendix, changedScripts);
            appendix.AppendLine("## Player Prefab Root MonoBehaviour Count");
            appendix.AppendLine(playerRootMonoBehaviourCount.ToString());
            appendix.AppendLine("## Unused Clips On Disk");
            appendix.AppendLine(
                "Non-locomotion clips remain on disk for later review. See controller clip references above.");

            return WriteReport(
                GenerateAfterReportPath,
                "After v0.7.3 Locomotion-Only Animator Reset",
                appendix.ToString());
        }

        private static string WriteReport(string reportPath, string title, string appendix = null)
        {
            AnimatorController controller = LoadController();
            StringBuilder builder = new StringBuilder();
            builder.Append("# ").Append(title).AppendLine();
            builder.AppendLine();
            builder.Append("Generated: ").Append(System.DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"))
                .AppendLine(" UTC");
            builder.Append("Controller: ")
                .Append(CCS_CharacterControllerConstants.PlayerLocomotionAnimatorControllerPath)
                .AppendLine();
            builder.AppendLine();

            if (controller == null)
            {
                builder.AppendLine("Animator Controller asset could not be loaded.");
            }
            else
            {
                AppendControllerInventory(builder, controller);
            }

            builder.AppendLine("## Runtime Animator Parameter Writers");
            AppendScriptWriterInventory(builder);

            if (!string.IsNullOrEmpty(appendix))
            {
                builder.AppendLine();
                builder.Append(appendix);
            }

            string absolutePath = Path.GetFullPath(reportPath);
            string directory = Path.GetDirectoryName(absolutePath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(absolutePath, builder.ToString());
            return absolutePath;
        }

        private static void AppendControllerInventory(StringBuilder builder, AnimatorController controller)
        {
            builder.AppendLine("## Layers");
            for (int layerIndex = 0; layerIndex < controller.layers.Length; layerIndex++)
            {
                AnimatorControllerLayer layer = controller.layers[layerIndex];
                builder.Append("- ").Append(layer.name);
                if (layer.avatarMask != null)
                {
                    builder.Append(" (mask: ").Append(AssetDatabase.GetAssetPath(layer.avatarMask)).Append(')');
                }

                builder.AppendLine();
            }

            builder.AppendLine("## Parameters");
            AnimatorControllerParameter[] parameters = controller.parameters;
            for (int i = 0; i < parameters.Length; i++)
            {
                builder.Append("- ").Append(parameters[i].name).Append(" (").Append(parameters[i].type)
                    .AppendLine(")");
            }

            HashSet<string> referencedClips = new HashSet<string>();
            builder.AppendLine("## States and Sub-State Machines");
            for (int layerIndex = 0; layerIndex < controller.layers.Length; layerIndex++)
            {
                AnimatorControllerLayer layer = controller.layers[layerIndex];
                builder.Append("### Layer: ").Append(layer.name).AppendLine();
                AppendStateMachineInventory(builder, layer.stateMachine, referencedClips, 0);
            }

            builder.AppendLine("## Referenced Clips");
            foreach (string clipPath in referencedClips.OrderBy(path => path))
            {
                builder.Append("- ").AppendLine(clipPath);
            }

            builder.AppendLine("## Parameters Not Written By Active CharacterController Runtime");
            HashSet<string> writtenParameters = CollectWrittenParameterNames();
            for (int i = 0; i < parameters.Length; i++)
            {
                string parameterName = parameters[i].name;
                if (!writtenParameters.Contains(parameterName))
                {
                    builder.Append("- ").AppendLine(parameterName);
                }
            }
        }

        private static void AppendStateMachineInventory(
            StringBuilder builder,
            AnimatorStateMachine stateMachine,
            HashSet<string> referencedClips,
            int depth)
        {
            if (stateMachine == null)
            {
                return;
            }

            string indent = new string(' ', depth * 2);
            ChildAnimatorState[] states = stateMachine.states;
            for (int i = 0; i < states.Length; i++)
            {
                AnimatorState state = states[i].state;
                if (state == null)
                {
                    continue;
                }

                builder.Append(indent).Append("- State: ").Append(state.name);
                AnimationClip clip = state.motion as AnimationClip;
                if (clip != null)
                {
                    string clipPath = AssetDatabase.GetAssetPath(clip);
                    referencedClips.Add(string.IsNullOrEmpty(clipPath) ? clip.name : clipPath);
                    builder.Append(" [clip: ").Append(clip.name).Append(']');
                }

                builder.AppendLine();
                AppendTransitionSummary(builder, state.transitions, indent + "  ");
            }

            ChildAnimatorStateMachine[] childMachines = stateMachine.stateMachines;
            for (int i = 0; i < childMachines.Length; i++)
            {
                AnimatorStateMachine childMachine = childMachines[i].stateMachine;
                if (childMachine == null)
                {
                    continue;
                }

                builder.Append(indent).Append("- Sub-State Machine: ").Append(childMachine.name).AppendLine();
                AppendStateMachineInventory(builder, childMachine, referencedClips, depth + 1);
            }

            AnimatorStateTransition[] anyStateTransitions = stateMachine.anyStateTransitions;
            for (int i = 0; i < anyStateTransitions.Length; i++)
            {
                AppendTransitionLine(builder, anyStateTransitions[i], indent + "- AnyState -> ");
            }
        }

        private static void AppendTransitionSummary(
            StringBuilder builder,
            AnimatorStateTransition[] transitions,
            string indent)
        {
            for (int i = 0; i < transitions.Length; i++)
            {
                AppendTransitionLine(builder, transitions[i], indent + "- Transition -> ");
            }
        }

        private static void AppendTransitionLine(
            StringBuilder builder,
            AnimatorStateTransition transition,
            string prefix)
        {
            if (transition == null)
            {
                return;
            }

            builder.Append(prefix);
            if (transition.destinationState != null)
            {
                builder.Append(transition.destinationState.name);
            }
            else if (transition.destinationStateMachine != null)
            {
                builder.Append(transition.destinationStateMachine.name);
            }
            else
            {
                builder.Append("(exit)");
            }

            AnimatorCondition[] conditions = transition.conditions;
            if (conditions != null && conditions.Length > 0)
            {
                builder.Append(" [");
                for (int i = 0; i < conditions.Length; i++)
                {
                    if (i > 0)
                    {
                        builder.Append(", ");
                    }

                    builder.Append(conditions[i].parameter);
                }

                builder.Append(']');
            }

            builder.AppendLine();
        }

        private static void AppendScriptWriterInventory(StringBuilder builder)
        {
            string runtimeRoot = CCS_CharacterControllerConstants.ModuleRootPath + "/Runtime";
            string[] scriptPaths = Directory.GetFiles(runtimeRoot, "*.cs", SearchOption.AllDirectories);
            for (int i = 0; i < scriptPaths.Length; i++)
            {
                string scriptPath = scriptPaths[i].Replace('\\', '/');
                string source = File.ReadAllText(scriptPath);
                if (!AnimatorWriteRegex.IsMatch(source))
                {
                    continue;
                }

                builder.Append("### ").Append(Path.GetFileName(scriptPath)).AppendLine();
                builder.Append("- Path: ").Append(scriptPath).AppendLine();
                string[] lines = source.Split('\n');
                for (int lineIndex = 0; lineIndex < lines.Length; lineIndex++)
                {
                    if (!AnimatorWriteRegex.IsMatch(lines[lineIndex]))
                    {
                        continue;
                    }

                    builder.Append("  - L").Append(lineIndex + 1).Append(": ").Append(lines[lineIndex].Trim())
                        .AppendLine();
                }
            }
        }

        private static HashSet<string> CollectWrittenParameterNames()
        {
            HashSet<string> names = new HashSet<string>();
            string runtimeRoot = CCS_CharacterControllerConstants.ModuleRootPath + "/Runtime";
            string[] scriptPaths = Directory.GetFiles(runtimeRoot, "*.cs", SearchOption.AllDirectories);
            for (int i = 0; i < scriptPaths.Length; i++)
            {
                string source = File.ReadAllText(scriptPaths[i]);
                if (!source.Contains("Animator."))
                {
                    continue;
                }

                MatchCollection matches = Regex.Matches(
                    source,
                    @"StringToHash\(\s*""([^""]+)""\s*\)|StringToHash\(\s*CCS_CharacterControllerConstants\.([A-Za-z0-9_]+)",
                    RegexOptions.Compiled);
                for (int matchIndex = 0; matchIndex < matches.Count; matchIndex++)
                {
                    Match match = matches[matchIndex];
                    if (match.Groups[1].Success)
                    {
                        names.Add(match.Groups[1].Value);
                    }
                }
            }

            names.Add("SpeedNormalized");
            names.Add("IsGrounded");
            names.Add("IsSprinting");
            names.Add("JumpTrigger");
            return names;
        }

        private static void AppendBulletList(StringBuilder builder, IReadOnlyList<string> items)
        {
            if (items == null || items.Count == 0)
            {
                builder.AppendLine("- (none)");
                return;
            }

            for (int i = 0; i < items.Count; i++)
            {
                builder.Append("- ").Append(items[i]).AppendLine();
            }
        }

        private static AnimatorController LoadController()
        {
            return AssetDatabase.LoadAssetAtPath<AnimatorController>(
                CCS_CharacterControllerConstants.PlayerLocomotionAnimatorControllerPath);
        }
    }
}
