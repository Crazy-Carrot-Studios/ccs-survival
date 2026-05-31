using CCS.Survival.Editor.Development;
using UnityEditor;

// =============================================================================
// SCRIPT: CCS_TimeOfDayValidationMenu
// CATEGORY: Modules / TimeOfDay / Editor / Validation
// PURPOSE: Menu entry for time-of-day validation through the central pipeline.
// PLACEMENT: Menu path CCS/Survival/Time Of Day/Validate Time Of Day.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-31
// NOTES: ValidateTimeOfDay is the Unity batchmode -executeMethod entry point.
// =============================================================================

namespace CCS.Modules.TimeOfDay.Editor
{
    public static class CCS_TimeOfDayValidationMenu
    {
        private const string MenuPath = "CCS/Survival/Time Of Day/Validate Time Of Day";
        private const string LogPrefix = "CCS_TimeOfDayValidationMenu";

        #region Public Methods

        [MenuItem(MenuPath, priority = 165)]
        public static void ValidateTimeOfDay()
        {
            CCS_SurvivalValidationReport report = CCS_SurvivalValidationBatchUtility.RunAllValidators();
            int exitCode = CCS_SurvivalValidationBatchUtility.EvaluateReport(LogPrefix, report);
            CCS_SurvivalValidationBatchUtility.ShowResultDialog("Time Of Day Validation", report);
            CCS_SurvivalValidationBatchUtility.CompleteBatchRun(exitCode);
        }

        #endregion
    }
}
