using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_SurvivalValidationBatchUtility
// CATEGORY: Survival / Editor / Development / Validation
// PURPOSE: Shared batchmode-safe validation runner with strict error/warning policy.
// PLACEMENT: Called by validation menus and Unity -executeMethod batch entries.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: In batchmode, calls EditorApplication.Exit. Warnings fail unless explicitly allowed.
// =============================================================================

namespace CCS.Survival.Editor.Development
{
    public static class CCS_SurvivalValidationBatchUtility
    {
        #region Public Methods

        public static CCS_SurvivalValidationReport RunAllValidators()
        {
            return CCS_SurvivalValidationPipeline.RunAll();
        }

        public static int EvaluateReport(
            string logPrefix,
            CCS_SurvivalValidationReport report,
            bool failOnWarnings = true)
        {
            string detailedLog = report.BuildDetailedLog();

            if (report.HasErrors())
            {
                Debug.LogError($"[{logPrefix}] {detailedLog}");
                return 1;
            }

            if (failOnWarnings && report.HasWarnings())
            {
                Debug.LogError(
                    $"[{logPrefix}] Validation completed with warnings (batch policy: fail).{System.Environment.NewLine}{detailedLog}");
                return 1;
            }

            Debug.Log($"[{logPrefix}] {detailedLog}");
            return 0;
        }

        public static int RunPipelineValidation(string logPrefix, bool failOnWarnings = true)
        {
            CCS_SurvivalValidationReport report = RunAllValidators();
            return EvaluateReport(logPrefix, report, failOnWarnings);
        }

        public static void CompleteBatchRun(int exitCode)
        {
            if (Application.isBatchMode)
            {
                EditorApplication.Exit(exitCode);
            }
        }

        public static void ShowResultDialog(string title, CCS_SurvivalValidationReport report)
        {
            if (Application.isBatchMode)
            {
                return;
            }

            EditorUtility.DisplayDialog(
                title,
                $"{report.BuildSummary()}\n\nSee Console for details.",
                "OK");
        }

        #endregion
    }
}
