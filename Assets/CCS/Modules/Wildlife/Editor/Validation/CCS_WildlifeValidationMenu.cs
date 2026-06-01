using CCS.Survival.Editor.Development;
using UnityEditor;

// =============================================================================
// SCRIPT: CCS_WildlifeValidationMenu
// CATEGORY: Modules / Wildlife / Editor / Validation
// PURPOSE: Menu entry for wildlife validation through the central pipeline.
// PLACEMENT: Menu path CCS/Survival/Wildlife/Validate Wildlife.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: ValidateWildlife is the Unity batchmode -executeMethod entry point.
// =============================================================================

namespace CCS.Modules.Wildlife.Editor
{
    public static class CCS_WildlifeValidationMenu
    {
        private const string MenuPath = "CCS/Survival/Wildlife/Validate Wildlife";
        private const string LogPrefix = "CCS_WildlifeValidationMenu";

        #region Public Methods

        public static void ValidateWildlife()
        {
            CCS_SurvivalValidationReport report = CCS_SurvivalValidationBatchUtility.RunAllValidators();
            int exitCode = CCS_SurvivalValidationBatchUtility.EvaluateReport(LogPrefix, report);
            CCS_SurvivalValidationBatchUtility.ShowResultDialog("Wildlife Validation", report);
            CCS_SurvivalValidationBatchUtility.CompleteBatchRun(exitCode);
        }

        [MenuItem(MenuPath, priority = 175)]
        public static void ValidateWildlifeMenu()
        {
            ValidateWildlife();
        }

        #endregion
    }
}
