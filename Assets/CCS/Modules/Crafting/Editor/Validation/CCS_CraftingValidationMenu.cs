using CCS.Survival.Editor.Development;
using UnityEditor;

// =============================================================================
// SCRIPT: CCS_CraftingValidationMenu
// CATEGORY: Modules / Crafting / Editor / Validation
// PURPOSE: Menu entry for crafting validation through the central pipeline.
// PLACEMENT: Menu path CCS/Survival/Crafting/Validate Crafting.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: ValidateCrafting is the Unity batchmode -executeMethod entry point.
// =============================================================================

namespace CCS.Modules.Crafting.Editor
{
    public static class CCS_CraftingValidationMenu
    {
        private const string MenuPath = "CCS/Survival/Crafting/Validate Crafting";
        private const string LogPrefix = "CCS_CraftingValidationMenu";

        #region Public Methods

        [MenuItem(MenuPath, priority = 160)]
        public static void ValidateCrafting()
        {
            CCS_SurvivalValidationReport report = CCS_SurvivalValidationBatchUtility.RunAllValidators();
            int exitCode = CCS_SurvivalValidationBatchUtility.EvaluateReport(LogPrefix, report);
            CCS_SurvivalValidationBatchUtility.ShowResultDialog("Crafting Validation", report);
            CCS_SurvivalValidationBatchUtility.CompleteBatchRun(exitCode);
        }

        #endregion
    }
}
