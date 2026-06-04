using CCS.Survival.Editor.Development;
using UnityEditor;

// =============================================================================
// SCRIPT: CCS_ReputationValidationMenu
// CATEGORY: Modules / Reputation / Editor / Validation
// PURPOSE: Editor menu and batch entry for reputation foundation validation.
// PLACEMENT: Menu path CCS/Survival/Validation/Reputation Foundation.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 2.7.0 reputation and service trust foundation.
// =============================================================================

namespace CCS.Modules.Reputation.Editor
{
    public static class CCS_ReputationValidationMenu
    {
        private const string LogPrefix = "[CCS_ReputationValidationMenu]";
        private const string MenuPath = "CCS/Survival/Validation/Reputation Foundation";

        public static void ValidateReputationFoundation()
        {
            CCS_SurvivalValidationReport report = CCS_SurvivalValidationBatchUtility.RunAllValidators();
            int exitCode = CCS_SurvivalValidationBatchUtility.EvaluateReport(LogPrefix, report);
            CCS_SurvivalValidationBatchUtility.ShowResultDialog("Reputation Foundation Validation", report);
            CCS_SurvivalValidationBatchUtility.CompleteBatchRun(exitCode);
        }

        [MenuItem(MenuPath, priority = 520)]
        public static void ValidateReputationFoundationMenu()
        {
            ValidateReputationFoundation();
        }
    }
}
