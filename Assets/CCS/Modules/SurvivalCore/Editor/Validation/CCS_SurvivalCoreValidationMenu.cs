using CCS.Survival.Editor.Development;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_SurvivalCoreValidationMenu
// CATEGORY: Modules / SurvivalCore / Editor / Validation
// PURPOSE: Unity menu entry for survival core validation through the central pipeline.
// PLACEMENT: Menu path CCS/Survival/Survival Core/Validate Survival Core.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-28
// NOTES: Runs all registered validators; survival core rules live in CCS_SurvivalCoreValidationValidator.
// =============================================================================

namespace CCS.Modules.SurvivalCore.Editor
{
    public static class CCS_SurvivalCoreValidationMenu
    {
        private const string MenuPath = "CCS/Survival/Survival Core/Validate Survival Core";

        #region Public Methods

        [MenuItem(MenuPath, priority = 110)]
        public static void ValidateSurvivalCore()
        {
            CCS_SurvivalValidationReport report = CCS_SurvivalValidationPipeline.RunAll();
            string detailedLog = report.BuildDetailedLog();

            if (report.HasErrors())
            {
                Debug.LogError($"[CCS_SurvivalCoreValidationMenu] {detailedLog}");
            }
            else
            {
                Debug.Log($"[CCS_SurvivalCoreValidationMenu] {detailedLog}");
            }

            EditorUtility.DisplayDialog(
                "Survival Core Validation",
                $"{report.BuildSummary()}\n\nSee Console for details.",
                "OK");
        }

        #endregion
    }
}
