using CCS.Project;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_InteractionValidationBatchEntry
// CATEGORY: Modules / Interaction / Editor
// PURPOSE: Batch-mode compile and validation entry for Interaction module releases.
// PLACEMENT: Editor batch utility. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Invoked via Unity -batchmode -executeMethod ...RunFromBatchMode.
// =============================================================================

namespace CCS.Modules.Interaction.Editor
{
    public static class CCS_InteractionValidationBatchEntry
    {
        public static void RunFromBatchMode()
        {
            CCS_InteractionAssetBuilder.EnsureInteractionAssets();
            CCS_InteractionTestPlayerPrefabBuilder.EnsureTestPlayerInteractionWiring();
            CCS_InteractionDetectionTestBuilder.BuildMasterTestInteractions();

            CCS_SurvivalValidationResult result = CCS_InteractionModuleValidator.ValidateInteractionModule();
            if (result.IsSuccess)
            {
                Debug.Log("[Interaction Batch] Validation passed: " + result.Message);
                EditorApplication.Exit(0);
                return;
            }

            Debug.LogError("[Interaction Batch] Validation failed: " + result.Message);
            EditorApplication.Exit(1);
        }
    }
}
