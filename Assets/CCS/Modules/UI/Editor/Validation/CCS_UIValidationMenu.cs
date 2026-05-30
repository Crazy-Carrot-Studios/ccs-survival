using CCS.Survival.Editor.Development;
using UnityEditor;

// =============================================================================
// SCRIPT: CCS_UIValidationMenu
// CATEGORY: Modules / UI / Editor / Validation
// PURPOSE: Menu entry for UI validation through the central pipeline.
// PLACEMENT: Menu path CCS/Survival/UI/Validate UI.
// AUTHOR: James Schilz
// CREATED: 2026-05-30
// NOTES: ValidateUI is the Unity batchmode -executeMethod entry point.
// =============================================================================

namespace CCS.Modules.UI.Editor
{
    public static class CCS_UIValidationMenu
    {
        private const string MenuPath = "CCS/Survival/UI/Validate UI";
        private const string LogPrefix = "CCS_UIValidationMenu";

        #region Public Methods

        [MenuItem(MenuPath, priority = 120)]
        public static void ValidateUI()
        {
            CCS_SurvivalValidationReport report = CCS_SurvivalValidationBatchUtility.RunAllValidators();
            int exitCode = CCS_SurvivalValidationBatchUtility.EvaluateReport(LogPrefix, report);
            CCS_SurvivalValidationBatchUtility.ShowResultDialog("UI Validation", report);
            CCS_SurvivalValidationBatchUtility.CompleteBatchRun(exitCode);
        }

        #endregion
    }
}
