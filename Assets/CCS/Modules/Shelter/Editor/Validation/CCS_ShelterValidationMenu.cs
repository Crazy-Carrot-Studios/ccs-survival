using CCS.Survival.Editor.Development;
using UnityEditor;

// =============================================================================
// SCRIPT: CCS_ShelterValidationMenu
// CATEGORY: Modules / Shelter / Editor / Validation
// PURPOSE: Menu entry for shelter validation through the central pipeline.
// PLACEMENT: Menu path CCS/Survival/Shelter/Validate Shelter.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-31
// NOTES: ValidateShelter is the Unity batchmode -executeMethod entry point.
// =============================================================================

namespace CCS.Modules.Shelter.Editor
{
    public static class CCS_ShelterValidationMenu
    {
        private const string MenuPath = "CCS/Survival/Shelter/Validate Shelter";
        private const string LogPrefix = "CCS_ShelterValidationMenu";

        #region Public Methods

        [MenuItem(MenuPath, priority = 175)]
        public static void ValidateShelter()
        {
            CCS_SurvivalValidationReport report = CCS_SurvivalValidationBatchUtility.RunAllValidators();
            int exitCode = CCS_SurvivalValidationBatchUtility.EvaluateReport(LogPrefix, report);
            CCS_SurvivalValidationBatchUtility.ShowResultDialog("Shelter Validation", report);
            CCS_SurvivalValidationBatchUtility.CompleteBatchRun(exitCode);
        }

        #endregion
    }
}
