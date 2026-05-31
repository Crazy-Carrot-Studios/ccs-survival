using CCS.Survival.Editor.Development;
using UnityEditor;

// =============================================================================
// SCRIPT: CCS_BuildingValidationMenu
// CATEGORY: Modules / Building / Editor / Validation
// PURPOSE: Menu entry for building validation through the central pipeline.
// PLACEMENT: Menu path CCS/Survival/Building/Validate Building.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-31
// NOTES: ValidateBuilding is the Unity batchmode -executeMethod entry point.
// =============================================================================

namespace CCS.Modules.Building.Editor
{
    public static class CCS_BuildingValidationMenu
    {
        private const string MenuPath = "CCS/Survival/Building/Validate Building";
        private const string LogPrefix = "CCS_BuildingValidationMenu";

        #region Public Methods

        [MenuItem(MenuPath, priority = 180)]
        public static void ValidateBuilding()
        {
            CCS_SurvivalValidationReport report = CCS_SurvivalValidationBatchUtility.RunAllValidators();
            int exitCode = CCS_SurvivalValidationBatchUtility.EvaluateReport(LogPrefix, report);
            CCS_SurvivalValidationBatchUtility.ShowResultDialog("Building Validation", report);
            CCS_SurvivalValidationBatchUtility.CompleteBatchRun(exitCode);
        }

        #endregion
    }
}
