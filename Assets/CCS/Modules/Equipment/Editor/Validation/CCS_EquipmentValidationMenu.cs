using CCS.Survival.Editor.Development;
using UnityEditor;

// =============================================================================
// SCRIPT: CCS_EquipmentValidationMenu
// CATEGORY: Modules / Equipment / Editor / Validation
// PURPOSE: Menu entry for equipment validation through the central pipeline.
// PLACEMENT: Menu path CCS/Survival/Equipment/Validate Equipment.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: ValidateEquipment is the Unity batchmode -executeMethod entry point.
// =============================================================================

namespace CCS.Modules.Equipment.Editor
{
    public static class CCS_EquipmentValidationMenu
    {
        private const string MenuPath = "CCS/Survival/Equipment/Validate Equipment";
        private const string LogPrefix = "CCS_EquipmentValidationMenu";

        #region Public Methods

        [MenuItem(MenuPath, priority = 150)]
        public static void ValidateEquipment()
        {
            CCS_SurvivalValidationReport report = CCS_SurvivalValidationBatchUtility.RunAllValidators();
            int exitCode = CCS_SurvivalValidationBatchUtility.EvaluateReport(LogPrefix, report);
            CCS_SurvivalValidationBatchUtility.ShowResultDialog("Equipment Validation", report);
            CCS_SurvivalValidationBatchUtility.CompleteBatchRun(exitCode);
        }

        #endregion
    }
}
