using CCS.Survival.Editor.Development;
using UnityEditor;

// =============================================================================
// SCRIPT: CCS_SleepValidationMenu
// CATEGORY: Modules / Sleep / Editor / Validation
// PURPOSE: Menu entry for sleep validation through the central pipeline.
// PLACEMENT: Menu path CCS/Survival/Sleep/Validate Sleep.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: ValidateSleep is the Unity batchmode -executeMethod entry point.
// =============================================================================

namespace CCS.Modules.Sleep.Editor
{
    public static class CCS_SleepValidationMenu
    {
        private const string MenuPath = "CCS/Survival/Sleep/Validate Sleep";
        private const string LogPrefix = "CCS_SleepValidationMenu";

        #region Public Methods

        public static void ValidateSleep()
        {
            CCS_SurvivalValidationReport report = CCS_SurvivalValidationBatchUtility.RunAllValidators();
            int exitCode = CCS_SurvivalValidationBatchUtility.EvaluateReport(LogPrefix, report);
            CCS_SurvivalValidationBatchUtility.ShowResultDialog("Sleep Validation", report);
            CCS_SurvivalValidationBatchUtility.CompleteBatchRun(exitCode);
        }

        [MenuItem(MenuPath, priority = 177)]
        public static void ValidateSleepMenu()
        {
            ValidateSleep();
        }

        #endregion
    }
}
