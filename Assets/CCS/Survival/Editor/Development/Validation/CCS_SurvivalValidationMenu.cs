using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_SurvivalValidationMenu
// CATEGORY: Survival / Editor / Development / Validation
// PURPOSE: Unity menu entry for running survival development validation checks.
// PLACEMENT: Editor menu only. Menu path CCS/Survival/Validation/Run Survival Validation.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-28
// NOTES: RunSurvivalValidation is the Unity batchmode -executeMethod entry point.
// =============================================================================

namespace CCS.Survival.Editor.Development
{
    public static class CCS_SurvivalValidationMenu
    {
        private const string MenuPath = "CCS/Survival/Validation/Run Survival Validation";
        private const string LogPrefix = "CCS_SurvivalValidationMenu";

        #region Public Methods

        [MenuItem(MenuPath, priority = 100)]
        public static void RunSurvivalValidation()
        {
            CCS_SurvivalValidationReport report = CCS_SurvivalValidationBatchUtility.RunAllValidators();
            int exitCode = CCS_SurvivalValidationBatchUtility.EvaluateReport(LogPrefix, report);
            CCS_SurvivalValidationBatchUtility.ShowResultDialog("Survival Validation", report);
            CCS_SurvivalValidationBatchUtility.CompleteBatchRun(exitCode);
        }

        #endregion
    }
}
