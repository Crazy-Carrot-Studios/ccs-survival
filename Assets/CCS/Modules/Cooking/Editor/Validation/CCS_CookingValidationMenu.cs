using CCS.Survival.Editor.Development;
using UnityEditor;

// =============================================================================
// SCRIPT: CCS_CookingValidationMenu
// CATEGORY: Modules / Cooking / Editor / Validation
// PURPOSE: Menu entry for cooking validation through the central pipeline.
// PLACEMENT: Menu path CCS/Survival/Cooking/Validate Cooking.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: ValidateCooking is the Unity batchmode -executeMethod entry point.
// =============================================================================

namespace CCS.Modules.Cooking.Editor
{
    public static class CCS_CookingValidationMenu
    {
        private const string MenuPath = "CCS/Survival/Cooking/Validate Cooking";
        private const string LogPrefix = "CCS_CookingValidationMenu";

        #region Public Methods

        public static void ValidateCooking()
        {
            CCS_SurvivalValidationReport report = CCS_SurvivalValidationBatchUtility.RunAllValidators();
            int exitCode = CCS_SurvivalValidationBatchUtility.EvaluateReport(LogPrefix, report);
            CCS_SurvivalValidationBatchUtility.ShowResultDialog("Cooking Validation", report);
            CCS_SurvivalValidationBatchUtility.CompleteBatchRun(exitCode);
        }

        [MenuItem(MenuPath, priority = 176)]
        public static void ValidateCookingMenu()
        {
            ValidateCooking();
        }

        #endregion
    }
}
