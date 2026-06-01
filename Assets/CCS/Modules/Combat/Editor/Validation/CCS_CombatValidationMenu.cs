using CCS.Survival.Editor.Development;
using UnityEditor;

// =============================================================================
// SCRIPT: CCS_CombatValidationMenu
// CATEGORY: Modules / Combat / Editor / Validation
// PURPOSE: Menu entry for combat validation through the central pipeline.
// PLACEMENT: Menu path CCS/Survival/Combat/Validate Combat.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: ValidateCombat is the Unity batchmode -executeMethod entry point.
// =============================================================================

namespace CCS.Modules.Combat.Editor
{
    public static class CCS_CombatValidationMenu
    {
        private const string MenuPath = "CCS/Survival/Combat/Validate Combat";
        private const string LogPrefix = "CCS_CombatValidationMenu";

        #region Public Methods

        public static void ValidateCombat()
        {
            CCS_SurvivalValidationReport report = CCS_SurvivalValidationBatchUtility.RunAllValidators();
            int exitCode = CCS_SurvivalValidationBatchUtility.EvaluateReport(LogPrefix, report);
            CCS_SurvivalValidationBatchUtility.ShowResultDialog("Combat Validation", report);
            CCS_SurvivalValidationBatchUtility.CompleteBatchRun(exitCode);
        }

        [MenuItem(MenuPath, priority = 178)]
        public static void ValidateCombatMenu()
        {
            ValidateCombat();
        }

        #endregion
    }
}
