using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

// =============================================================================
// SCRIPT: CCS_SurvivalInputValidationUtility
// CATEGORY: Survival / Editor / Development / Validation
// PURPOSE: Scans project sources for banned legacy input and obsolete API usage.
// PLACEMENT: Called by foundation and playtesting validators.
// AUTHOR: James Schilz
// CREATED: 2026-06-02
// NOTES: Milestone 1.7.2 input audit enforcement.
// =============================================================================

namespace CCS.Survival.Editor.Development
{
    public static class CCS_SurvivalInputValidationUtility
    {
        private static readonly Regex LegacyInputGetPattern = new Regex(
            @"\bInput\.Get(?:Key|KeyDown|KeyUp|Axis|Button)\b",
            RegexOptions.Compiled);

        private static readonly Regex LegacyInputClassPattern = new Regex(
            @"\bUnityEngine\.Input\b",
            RegexOptions.Compiled);

        private static readonly Regex FindObjectsSortModePattern = new Regex(
            @"\bFindObjectsSortMode\b",
            RegexOptions.Compiled);

        private static bool ShouldSkipSourceScan(string normalizedPath)
        {
            return normalizedPath.EndsWith("CCS_SurvivalInputValidationUtility.cs")
                || normalizedPath.EndsWith("CCS_KeyboardInputUtility.cs");
        }

        public static List<string> CollectLegacyInputUsageFiles()
        {
            List<string> offenders = new List<string>();
            string[] files = Directory.GetFiles("Assets", "*.cs", SearchOption.AllDirectories);
            for (int index = 0; index < files.Length; index++)
            {
                string normalizedPath = files[index].Replace('\\', '/');
                if (ShouldSkipSourceScan(normalizedPath))
                {
                    continue;
                }

                string content = File.ReadAllText(normalizedPath);
                if (IsLegacyInputOffender(content))
                {
                    offenders.Add(normalizedPath);
                }
            }

            return offenders;
        }

        public static List<string> CollectFindObjectsSortModeUsageFiles()
        {
            List<string> offenders = new List<string>();
            string[] files = Directory.GetFiles("Assets", "*.cs", SearchOption.AllDirectories);
            for (int index = 0; index < files.Length; index++)
            {
                string normalizedPath = files[index].Replace('\\', '/');
                if (ShouldSkipSourceScan(normalizedPath))
                {
                    continue;
                }

                if (FindObjectsSortModePattern.IsMatch(File.ReadAllText(normalizedPath)))
                {
                    offenders.Add(normalizedPath);
                }
            }

            return offenders;
        }

        public static void ValidateNoLegacyInputUsage(CCS_SurvivalValidationReport report, string validatorContext)
        {
            List<string> offenders = CollectLegacyInputUsageFiles();
            report.AddIssue(
                offenders.Count == 0
                    ? CCS_SurvivalValidationIssueSeverity.Info
                    : CCS_SurvivalValidationIssueSeverity.Error,
                validatorContext,
                offenders.Count == 0
                    ? "No runtime legacy UnityEngine.Input usage detected under Assets/."
                    : $"Legacy UnityEngine.Input usage detected: {string.Join(", ", offenders)}. Route dev hotkeys through CCS_DevHotkeyUtility / CCS_KeyboardInputUtility.");
        }

        public static void ValidateNoFindObjectsSortModeUsage(CCS_SurvivalValidationReport report, string validatorContext)
        {
            List<string> offenders = CollectFindObjectsSortModeUsageFiles();
            report.AddIssue(
                offenders.Count == 0
                    ? CCS_SurvivalValidationIssueSeverity.Info
                    : CCS_SurvivalValidationIssueSeverity.Error,
                validatorContext,
                offenders.Count == 0
                    ? "No FindObjectsSortMode usage detected under Assets/."
                    : $"Obsolete FindObjectsSortMode usage detected: {string.Join(", ", offenders)}.");
        }

        public static void ValidateDevHotkeyRegistry(CCS_SurvivalValidationReport report, string validatorContext)
        {
            IReadOnlyList<CCS.Modules.CharacterController.CCS_DevHotkeyUtility.DevHotkeyBinding> bindings =
                CCS.Modules.CharacterController.CCS_DevHotkeyUtility.GetKnownBindings();

            Dictionary<string, List<CCS.Modules.CharacterController.CCS_DevHotkeyUtility.DevHotkeyBinding>> grouped =
                new Dictionary<string, List<CCS.Modules.CharacterController.CCS_DevHotkeyUtility.DevHotkeyBinding>>();

            for (int index = 0; index < bindings.Count; index++)
            {
                CCS.Modules.CharacterController.CCS_DevHotkeyUtility.DevHotkeyBinding binding = bindings[index];
                string signature = BuildHotkeySignature(binding);
                if (!grouped.TryGetValue(signature, out List<CCS.Modules.CharacterController.CCS_DevHotkeyUtility.DevHotkeyBinding> list))
                {
                    list = new List<CCS.Modules.CharacterController.CCS_DevHotkeyUtility.DevHotkeyBinding>();
                    grouped[signature] = list;
                }

                list.Add(binding);
            }

            bool hasUnexpectedConflict = false;
            foreach (KeyValuePair<string, List<CCS.Modules.CharacterController.CCS_DevHotkeyUtility.DevHotkeyBinding>> entry in grouped)
            {
                if (entry.Value.Count <= 1)
                {
                    continue;
                }

                bool allShared = true;
                for (int bindingIndex = 0; bindingIndex < entry.Value.Count; bindingIndex++)
                {
                    if (!entry.Value[bindingIndex].AllowShared)
                    {
                        allShared = false;
                        break;
                    }
                }

                if (allShared)
                {
                    continue;
                }

                hasUnexpectedConflict = true;
                List<string> owners = new List<string>(entry.Value.Count);
                for (int bindingIndex = 0; bindingIndex < entry.Value.Count; bindingIndex++)
                {
                    owners.Add(entry.Value[bindingIndex].OwnerId);
                }

                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    validatorContext,
                    $"Dev hotkey conflict on {entry.Key}: {string.Join(", ", owners)}.");
            }

            if (!hasUnexpectedConflict)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Info,
                    validatorContext,
                    $"Dev hotkey registry validated ({bindings.Count} bindings).");
            }
        }

        private static bool IsLegacyInputOffender(string content)
        {
            if (LegacyInputGetPattern.IsMatch(content))
            {
                return true;
            }

            return LegacyInputClassPattern.IsMatch(content)
                && content.Contains("UnityEngine.Input.");
        }

        private static string BuildHotkeySignature(
            CCS.Modules.CharacterController.CCS_DevHotkeyUtility.DevHotkeyBinding binding)
        {
            return $"{binding.KeyCode}|shift={binding.RequiresShift}|ctrl={binding.RequiresControl}|alt={binding.RequiresAlt}";
        }
    }
}
