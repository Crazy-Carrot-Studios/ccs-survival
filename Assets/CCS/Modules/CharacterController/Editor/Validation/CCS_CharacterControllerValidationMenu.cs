using CCS.Survival.Editor.Development;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_CharacterControllerValidationMenu
// CATEGORY: Modules / CharacterController / Editor / Validation
// PURPOSE: Menu entry for character controller validation through the central pipeline.
// PLACEMENT: Menu path CCS/Survival/Character Controller/Validate Character Controller.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Runs all registered validators, including survival core and bootstrap.
// =============================================================================

namespace CCS.Modules.CharacterController.Editor
{
    public static class CCS_CharacterControllerValidationMenu
    {
        private const string MenuPath = "CCS/Survival/Character Controller/Validate Character Controller";

        #region Public Methods

        [MenuItem(MenuPath, priority = 120)]
        public static void ValidateCharacterController()
        {
            CCS_SurvivalValidationReport report = CCS_SurvivalValidationPipeline.RunAll();
            string detailedLog = report.BuildDetailedLog();

            if (report.HasErrors())
            {
                Debug.LogError($"[CCS_CharacterControllerValidationMenu] {detailedLog}");
            }
            else
            {
                Debug.Log($"[CCS_CharacterControllerValidationMenu] {detailedLog}");
            }

            EditorUtility.DisplayDialog(
                "Character Controller Validation",
                $"{report.BuildSummary()}\n\nSee Console for details.",
                "OK");
        }

        #endregion
    }
}
