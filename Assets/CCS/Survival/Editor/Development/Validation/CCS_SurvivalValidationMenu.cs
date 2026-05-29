using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_SurvivalValidationMenu
// CATEGORY: Survival / Editor / Development / Validation
// PURPOSE: Unity menu entry for running survival development validation checks.
// PLACEMENT: Editor menu only. Menu path CCS/Survival/Validation/Run Survival Validation.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-28
// NOTES: Reports folder/config expectations for 0.3.6 foundation.
// =============================================================================

namespace CCS.Survival.Editor.Development
{
    public static class CCS_SurvivalValidationMenu
    {
        private const string MenuPath = "CCS/Survival/Validation/Run Survival Validation";

        #region Public Methods

        [MenuItem(MenuPath, priority = 100)]
        public static void RunSurvivalValidation()
        {
            CCS_SurvivalValidationReport report = CCS_SurvivalValidationPipeline.RunAll();
            string detailedLog = report.BuildDetailedLog();

            if (report.HasErrors())
            {
                Debug.LogError($"[CCS_SurvivalValidationMenu] {detailedLog}");
                EditorUtility.DisplayDialog(
                    "Survival Validation",
                    $"{report.BuildSummary()}\n\nSee Console for details.",
                    "OK");
                return;
            }

            Debug.Log($"[CCS_SurvivalValidationMenu] {detailedLog}");
            EditorUtility.DisplayDialog(
                "Survival Validation",
                $"{report.BuildSummary()}\n\nSee Console for details.",
                "OK");
        }

        #endregion
    }
}
