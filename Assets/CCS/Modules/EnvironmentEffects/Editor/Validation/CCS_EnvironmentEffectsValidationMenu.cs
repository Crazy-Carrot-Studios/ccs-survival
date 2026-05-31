using CCS.Survival.Editor.Development;
using UnityEditor;

// =============================================================================
// SCRIPT: CCS_EnvironmentEffectsValidationMenu
// CATEGORY: Modules / EnvironmentEffects / Editor / Validation
// PURPOSE: Menu entry for environment effects validation through the central pipeline.
// PLACEMENT: Menu path CCS/Survival/Environment Effects/Validate Environment Effects.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-31
// NOTES: ValidateEnvironmentEffects is the Unity batchmode -executeMethod entry point.
// =============================================================================

namespace CCS.Modules.EnvironmentEffects.Editor
{
    public static class CCS_EnvironmentEffectsValidationMenu
    {
        private const string MenuPath = "CCS/Survival/Environment Effects/Validate Environment Effects";
        private const string LogPrefix = "CCS_EnvironmentEffectsValidationMenu";

        #region Public Methods

        [MenuItem(MenuPath, priority = 175)]
        public static void ValidateEnvironmentEffects()
        {
            CCS_SurvivalValidationReport report = CCS_SurvivalValidationBatchUtility.RunAllValidators();
            int exitCode = CCS_SurvivalValidationBatchUtility.EvaluateReport(LogPrefix, report);
            CCS_SurvivalValidationBatchUtility.ShowResultDialog("Environment Effects Validation", report);
            CCS_SurvivalValidationBatchUtility.CompleteBatchRun(exitCode);
        }

        #endregion
    }
}
