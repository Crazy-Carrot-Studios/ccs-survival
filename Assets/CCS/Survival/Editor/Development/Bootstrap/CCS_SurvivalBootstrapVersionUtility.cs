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
        private const string ProjectSettingsPath = "ProjectSettings/ProjectSettings.asset";

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

        private static int CompareVersions(string left, string right)
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
