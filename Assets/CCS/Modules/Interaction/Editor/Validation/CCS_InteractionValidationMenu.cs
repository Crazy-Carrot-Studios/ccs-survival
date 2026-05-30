using CCS.Survival.Editor.Development;
using UnityEditor;

// =============================================================================
// SCRIPT: CCS_InteractionValidationMenu
// CATEGORY: Modules / Interaction / Editor / Validation
// PURPOSE: Menu entry for interaction validation through the central pipeline.
// PLACEMENT: Menu path CCS/Survival/Interaction/Validate Interaction.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: ValidateInteraction is the Unity batchmode -executeMethod entry point.
// =============================================================================

namespace CCS.Modules.Interaction.Editor
{
    public static class CCS_InteractionValidationMenu
    {
        private const string MenuPath = "CCS/Survival/Interaction/Validate Interaction";
        private const string LogPrefix = "CCS_InteractionValidationMenu";

        #region Public Methods

        [MenuItem(MenuPath, priority = 130)]
        public static void ValidateInteraction()
        {
            CCS_SurvivalValidationReport report = CCS_SurvivalValidationBatchUtility.RunAllValidators();
            int exitCode = CCS_SurvivalValidationBatchUtility.EvaluateReport(LogPrefix, report);
            CCS_SurvivalValidationBatchUtility.ShowResultDialog("Interaction Validation", report);
            CCS_SurvivalValidationBatchUtility.CompleteBatchRun(exitCode);
        }

        #endregion
    }
}
