using CCS.Project;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_StrippedCharacterControllerBaselineBatchEntry
// CATEGORY: Modules / CharacterController / Editor / StripBaseline
// PURPOSE: Batch-mode entry for v0.7.13 stripped Character Controller baseline.
// PLACEMENT: Editor batch utility. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-25
// =============================================================================

namespace CCS.Modules.CharacterController.Editor
{
    public static class CCS_StrippedCharacterControllerBaselineBatchEntry
    {
        public static void RunFromBatchMode()
        {
            CCS_CharacterControllerMasterTestBuilder.SetupMasterTestScene();

            string removalAuditPath = CCS_StrippedBaselineRemovalAuditBuilder.WriteRemovalAuditReport();
            StrippedCharacterControllerBaselineStripResult stripResult =
                CCS_StrippedCharacterControllerBaselineStripBuilder.RunFullStrip();

            CCS_SurvivalValidationResult validationResult =
                CCS_StrippedCharacterControllerBaselineValidationUtility.ValidateStrippedCharacterControllerBaseline();
            if (!validationResult.IsSuccess)
            {
                Debug.LogError(
                    "[Stripped Character Controller Baseline Batch] Validation failed: "
                    + validationResult.Message);
                EditorApplication.Exit(1);
                return;
            }

            string reportPath = CCS_StrippedCharacterControllerBaselineReportBuilder.WriteReport(stripResult);
            Debug.Log(
                "[Stripped Character Controller Baseline Batch] Validation passed. Removal audit: "
                + removalAuditPath
                + " report: "
                + reportPath
                + ". "
                + validationResult.Message);
            EditorApplication.Exit(0);
        }
    }
}
