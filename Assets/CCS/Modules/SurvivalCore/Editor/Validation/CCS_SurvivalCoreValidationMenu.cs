using CCS.Survival.Editor.Development;
using UnityEditor;

// =============================================================================
// SCRIPT: CCS_SurvivalCoreValidationMenu
// CATEGORY: Modules / SurvivalCore / Editor / Validation
// PURPOSE: Unity menu entry for survival core validation through the central pipeline.
// PLACEMENT: Menu path CCS/Survival/Survival Core/Validate Survival Core.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-28
// NOTES: ValidateSurvivalCore is the Unity batchmode -executeMethod entry point.
// =============================================================================

namespace CCS.Modules.SurvivalCore.Editor
{
    public static class CCS_SurvivalCoreValidationMenu
    {
        private const string MenuPath = "CCS/Survival/Survival Core/Validate Survival Core";
        private const string LogPrefix = "CCS_SurvivalCoreValidationMenu";

        #region Public Methods

        [MenuItem(MenuPath, priority = 110)]
        public static void ValidateSurvivalCore()
        {
            CCS_SurvivalValidationReport report = CCS_SurvivalValidationBatchUtility.RunAllValidators();
            int exitCode = CCS_SurvivalValidationBatchUtility.EvaluateReport(LogPrefix, report);
            CCS_SurvivalValidationBatchUtility.ShowResultDialog("Survival Core Validation", report);
            CCS_SurvivalValidationBatchUtility.CompleteBatchRun(exitCode);
        }

        #endregion
    }
}
