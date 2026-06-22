using CCS.Project;
using UnityEditor;

// =============================================================================
// SCRIPT: CCS_EquipmentFitStudioBatchEntry
// CATEGORY: Modules / CharacterController / Editor / EquipmentFitStudio
// PURPOSE: Batch-mode validation entry for Equipment Fit Studio foundation.
// PLACEMENT: Editor batch utility invoked from Unity -batchmode.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Ensures preview objects are cleaned before validation.
// =============================================================================

namespace CCS.Modules.CharacterController.Editor.EquipmentFitStudio
{
    public static class CCS_EquipmentFitStudioBatchEntry
    {
        public static void RunFromBatchMode()
        {
            CCS_EquipmentFitStudioProfileBuilder.EnsureEquipmentFitStudioAssets();
            CCS_EquipmentFitStudioCleanupUtility.CleanupAllPreviewObjects();

            CCS_SurvivalValidationResult result =
                CCS_EquipmentFitStudioValidationUtility.ValidateEquipmentFitStudioFoundation();
            if (!result.IsSuccess)
            {
                UnityEngine.Debug.LogError("[Equipment Fit Studio Batch] Failed: " + result.Message);
                EditorApplication.Exit(1);
                return;
            }

            UnityEngine.Debug.Log("[Equipment Fit Studio Batch] Passed: " + result.Message);
            EditorApplication.Exit(0);
        }
    }
}
