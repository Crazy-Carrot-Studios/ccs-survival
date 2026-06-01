using CCS.Survival.Editor.Development;
using UnityEditor;

// =============================================================================
// SCRIPT: CCS_WildlifeAiValidationMenu
// CATEGORY: Modules / Wildlife / Editor / Validation
// PURPOSE: Menu entry for wildlife AI validation through the central pipeline.
// PLACEMENT: Menu path CCS/Survival/Wildlife/Validate Wildlife AI.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: ValidateWildlifeAi is the Unity batchmode -executeMethod entry point.
// =============================================================================

namespace CCS.Modules.Wildlife.Editor
{
    public static class CCS_WildlifeAiValidationMenu
    {
        private const string MenuPath = "CCS/Survival/Wildlife/Validate Wildlife AI";
        private const string LogPrefix = "CCS_WildlifeAiValidationMenu";

        #region Public Methods

        public static void ValidateWildlifeAi()
        {
            CCS_SurvivalValidationReport report = CCS_SurvivalValidationBatchUtility.RunAllValidators();
            int exitCode = CCS_SurvivalValidationBatchUtility.EvaluateReport(LogPrefix, report);
            CCS_SurvivalValidationBatchUtility.ShowResultDialog("Wildlife AI Validation", report);
            CCS_SurvivalValidationBatchUtility.CompleteBatchRun(exitCode);
        }

        [MenuItem(MenuPath, priority = 175)]
        public static void ValidateWildlifeAiMenu()
        {
            ValidateWildlifeAi();
        }

        #endregion
    }
}
