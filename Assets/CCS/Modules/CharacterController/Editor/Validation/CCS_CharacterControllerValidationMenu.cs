using CCS.Survival.Editor.Development;
using UnityEditor;

// =============================================================================
// SCRIPT: CCS_CharacterControllerValidationMenu
// CATEGORY: Modules / CharacterController / Editor / Validation
// PURPOSE: Menu entry for character controller validation through the central pipeline.
// PLACEMENT: Menu path CCS/Survival/Character Controller/Validate Character Controller.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: ValidateCharacterController is the Unity batchmode -executeMethod entry point.
// =============================================================================

namespace CCS.Modules.CharacterController.Editor
{
    public static class CCS_CharacterControllerValidationMenu
    {
        private const string MenuPath = "CCS/Survival/Character Controller/Validate Character Controller";
        private const string LogPrefix = "CCS_CharacterControllerValidationMenu";

        #region Public Methods

        [MenuItem(MenuPath, priority = 120)]
        public static void ValidateCharacterController()
        {
            CCS_SurvivalValidationReport report = CCS_SurvivalValidationBatchUtility.RunAllValidators();
            int exitCode = CCS_SurvivalValidationBatchUtility.EvaluateReport(LogPrefix, report);
            CCS_SurvivalValidationBatchUtility.ShowResultDialog("Character Controller Validation", report);
            CCS_SurvivalValidationBatchUtility.CompleteBatchRun(exitCode);
        }

        #endregion
    }
}
