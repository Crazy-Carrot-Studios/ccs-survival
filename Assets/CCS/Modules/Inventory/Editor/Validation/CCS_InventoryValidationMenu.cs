using CCS.Survival.Editor.Development;
using UnityEditor;

// =============================================================================
// SCRIPT: CCS_InventoryValidationMenu
// CATEGORY: Modules / Inventory / Editor / Validation
// PURPOSE: Menu entry for inventory validation through the central pipeline.
// PLACEMENT: Menu path CCS/Survival/Inventory/Validate Inventory.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: ValidateInventory is the Unity batchmode -executeMethod entry point.
// =============================================================================

namespace CCS.Modules.Inventory.Editor
{
    public static class CCS_InventoryValidationMenu
    {
        private const string MenuPath = "CCS/Survival/Inventory/Validate Inventory";
        private const string LogPrefix = "CCS_InventoryValidationMenu";

        #region Public Methods

        [MenuItem(MenuPath, priority = 140)]
        public static void ValidateInventory()
        {
            CCS_SurvivalValidationReport report = CCS_SurvivalValidationBatchUtility.RunAllValidators();
            int exitCode = CCS_SurvivalValidationBatchUtility.EvaluateReport(LogPrefix, report);
            CCS_SurvivalValidationBatchUtility.ShowResultDialog("Inventory Validation", report);
            CCS_SurvivalValidationBatchUtility.CompleteBatchRun(exitCode);
        }

        #endregion
    }
}
