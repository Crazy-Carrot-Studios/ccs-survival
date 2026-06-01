using CCS.Survival.Editor.Development;
using UnityEditor;

// =============================================================================
// SCRIPT: CCS_GatheringValidationMenu
// CATEGORY: Modules / Gathering / Editor / Validation
// PURPOSE: Menu entry for gathering validation through the central pipeline.
// PLACEMENT: Menu path CCS/Survival/Gathering/Validate Gathering.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-06-01
// NOTES: ValidateGathering is the Unity batchmode -executeMethod entry point.
// =============================================================================

namespace CCS.Modules.Gathering.Editor
{
    public static class CCS_GatheringValidationMenu
    {
        private const string MenuPath = "CCS/Survival/Gathering/Validate Gathering";
        private const string LogPrefix = "CCS_GatheringValidationMenu";

        #region Public Methods

        public static void ValidateGathering()
        {
            CCS_SurvivalValidationReport report = CCS_SurvivalValidationBatchUtility.RunAllValidators();
            int exitCode = CCS_SurvivalValidationBatchUtility.EvaluateReport(LogPrefix, report);
            CCS_SurvivalValidationBatchUtility.ShowResultDialog("Gathering Validation", report);
            CCS_SurvivalValidationBatchUtility.CompleteBatchRun(exitCode);
        }

        [MenuItem(MenuPath, priority = 179)]
        public static void ValidateGatheringMenu()
        {
            ValidateGathering();
        }

        #endregion
    }
}
