using CCS.Survival.Editor.Development;
using UnityEditor;

// =============================================================================
// SCRIPT: CCS_WorldResourceValidationMenu
// CATEGORY: Modules / WorldResources / Editor / Validation
// PURPOSE: Menu entry for world resource validation through the central pipeline.
// PLACEMENT: Menu path CCS/Survival/World Resources/Validate World Resources.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: ValidateWorldResources is the Unity batchmode -executeMethod entry point.
// =============================================================================

namespace CCS.Modules.WorldResources.Editor
{
    public static class CCS_WorldResourceValidationMenu
    {
        private const string MenuPath = "CCS/Survival/World Resources/Validate World Resources";
        private const string LogPrefix = "CCS_WorldResourceValidationMenu";

        #region Public Methods

        [MenuItem(MenuPath, priority = 170)]
        public static void ValidateWorldResources()
        {
            CCS_SurvivalValidationReport report = CCS_SurvivalValidationBatchUtility.RunAllValidators();
            int exitCode = CCS_SurvivalValidationBatchUtility.EvaluateReport(LogPrefix, report);
            CCS_SurvivalValidationBatchUtility.ShowResultDialog("World Resources Validation", report);
            CCS_SurvivalValidationBatchUtility.CompleteBatchRun(exitCode);
        }

        #endregion
    }
}
