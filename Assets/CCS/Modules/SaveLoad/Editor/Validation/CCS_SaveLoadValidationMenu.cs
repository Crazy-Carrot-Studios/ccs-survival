using CCS.Survival.Editor.Development;
using UnityEditor;

// =============================================================================
// SCRIPT: CCS_SaveLoadValidationMenu
// CATEGORY: Modules / SaveLoad / Editor / Validation
// PURPOSE: Menu entry for save/load validation through the central pipeline.
// PLACEMENT: Menu path CCS/Survival/Save Load/Validate Save Load.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: ValidateSaveLoad is the Unity batchmode -executeMethod entry point.
// =============================================================================

namespace CCS.Modules.SaveLoad.Editor
{
    public static class CCS_SaveLoadValidationMenu
    {
        private const string MenuPath = "CCS/Survival/Save Load/Validate Save Load";
        private const string LogPrefix = "CCS_SaveLoadValidationMenu";

        #region Public Methods

        [MenuItem(MenuPath, priority = 170)]
        public static void ValidateSaveLoad()
        {
            CCS_SurvivalValidationReport report = CCS_SurvivalValidationBatchUtility.RunAllValidators();
            int exitCode = CCS_SurvivalValidationBatchUtility.EvaluateReport(LogPrefix, report);
            CCS_SurvivalValidationBatchUtility.ShowResultDialog("Save Load Validation", report);
            CCS_SurvivalValidationBatchUtility.CompleteBatchRun(exitCode);
        }

        #endregion
    }
}
