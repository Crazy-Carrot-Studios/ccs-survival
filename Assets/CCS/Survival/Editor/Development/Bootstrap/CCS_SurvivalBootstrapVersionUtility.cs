using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

// =============================================================================
// SCRIPT: CCS_SurvivalBootstrapVersionUtility
// CATEGORY: Survival / Editor / Development / Bootstrap
// PURPOSE: Bumps ProjectSettings bundleVersion forward without downgrading milestones.
// PLACEMENT: Used by milestone bootstrap ExecuteBatch methods.
// AUTHOR: James Schilz
// CREATED: 2026-06-02
// =============================================================================

namespace CCS.Survival.Editor.Development
{
    public static class CCS_SurvivalBootstrapVersionUtility
    {
        public const string CurrentMilestoneVersion = "2.5.1";

        private const string ProjectSettingsPath = "ProjectSettings/ProjectSettings.asset";
        private static readonly Regex HardcodedBundleVersionReplacementPattern = new Regex(
            @"""bundleVersion:\s*[\d]+\.[\d]+\.[\d]+""",
            RegexOptions.Compiled);

        public static void EnsureBundleVersionAtLeast(string minimumVersion)
        {
            if (!File.Exists(ProjectSettingsPath) || string.IsNullOrWhiteSpace(minimumVersion))
            {
                return;
            }

            string text = File.ReadAllText(ProjectSettingsPath);
            Match match = Regex.Match(text, @"bundleVersion:\s*([\d\.]+)");
            string currentVersion = match.Success ? match.Groups[1].Value : "0.0.0";
            string resolvedVersion = CompareVersions(currentVersion, minimumVersion) >= 0
                ? currentVersion
                : minimumVersion;

            text = Regex.Replace(text, @"bundleVersion:\s*[\d\.]+", $"bundleVersion: {resolvedVersion}");
            File.WriteAllText(ProjectSettingsPath, text);
        }

        public static bool TryReadProjectBundleVersion(out string version)
        {
            version = string.Empty;
            if (!File.Exists(ProjectSettingsPath))
            {
                return false;
            }

            Match match = Regex.Match(File.ReadAllText(ProjectSettingsPath), @"bundleVersion:\s*([\d\.]+)");
            if (!match.Success)
            {
                return false;
            }

            version = match.Groups[1].Value;
            return true;
        }

        public static bool IsProjectBundleVersionAtLeast(string minimumVersion)
        {
            if (!TryReadProjectBundleVersion(out string currentVersion))
            {
                return false;
            }

            return CompareVersions(currentVersion, minimumVersion) >= 0;
        }

        public static void AddBundleVersionValidationIssue(
            CCS_SurvivalValidationReport report,
            string validatorContext,
            string minimumVersion = CurrentMilestoneVersion,
            string remediationHint = null)
        {
            if (IsProjectBundleVersionAtLeast(minimumVersion))
            {
                TryReadProjectBundleVersion(out string currentVersion);
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Info,
                    validatorContext,
                    $"bundleVersion {currentVersion} meets minimum {minimumVersion}.");
                return;
            }

            report.AddIssue(
                CCS_SurvivalValidationIssueSeverity.Error,
                validatorContext,
                $"Expected bundleVersion at least {minimumVersion}. {remediationHint ?? "Review ProjectSettings/ProjectSettings.asset."}");
        }

        public static void ValidateNoHardcodedBootstrapVersionWrites(
            CCS_SurvivalValidationReport report,
            string validatorContext)
        {
            List<string> offenders = CollectHardcodedBootstrapVersionWriteFiles();
            bool ok = offenders.Count == 0;
            report.AddIssue(
                ok ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                validatorContext,
                ok
                    ? "Bootstrap setup scripts use EnsureBundleVersionAtLeast for bundleVersion writes."
                    : $"Stale bootstrap bundleVersion writes remain: {string.Join(", ", offenders)}. Use CCS_SurvivalBootstrapVersionUtility.");
        }

        public static List<string> CollectHardcodedBootstrapVersionWriteFiles()
        {
            List<string> offenders = new List<string>();
            if (!Directory.Exists("Assets"))
            {
                return offenders;
            }

            string[] files = Directory.GetFiles("Assets", "*BootstrapSetup.cs", SearchOption.AllDirectories);
            for (int index = 0; index < files.Length; index++)
            {
                string normalizedPath = files[index].Replace('\\', '/');
                string content = File.ReadAllText(normalizedPath);
                if (!content.Contains("bundleVersion:"))
                {
                    continue;
                }

                if (HardcodedBundleVersionReplacementPattern.IsMatch(content))
                {
                    offenders.Add(normalizedPath);
                }
            }

            return offenders;
        }

        public static int CompareVersions(string left, string right)
        {
            int[] leftParts = ParseVersionParts(left);
            int[] rightParts = ParseVersionParts(right);
            int maxLength = leftParts.Length > rightParts.Length ? leftParts.Length : rightParts.Length;

            for (int index = 0; index < maxLength; index++)
            {
                int leftValue = index < leftParts.Length ? leftParts[index] : 0;
                int rightValue = index < rightParts.Length ? rightParts[index] : 0;
                if (leftValue != rightValue)
                {
                    return leftValue.CompareTo(rightValue);
                }
            }

            return 0;
        }

        private static int[] ParseVersionParts(string version)
        {
            if (string.IsNullOrWhiteSpace(version))
            {
                return new[] { 0 };
            }

            string[] split = version.Split('.');
            int[] parts = new int[split.Length];
            for (int index = 0; index < split.Length; index++)
            {
                if (!int.TryParse(split[index], out parts[index]))
                {
                    parts[index] = 0;
                }
            }

            return parts;
        }
    }
}
