using CCS.Project;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_AttributesHudSetupBatchEntry
// CATEGORY: Modules / Attributes / Editor
// PURPOSE: Batch-mode entry point for attribute bar HUD prefab wiring and validation.
// PLACEMENT: Editor batch utility. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// =============================================================================

namespace CCS.Modules.Attributes.Editor
{
    public static class CCS_AttributesHudSetupBatchEntry
    {
        public static void RunFromBatchMode()
        {
            CCS_AttributesAssetBuilder.EnsureAttributesAssets();
            CCS_AttributesTestPlayerPrefabBuilder.EnsureTestPlayerAttributes();
            CCS_SurvivalValidationResult result = CCS_AttributesModuleValidator.ValidateAttributesModule();
            if (!result.IsSuccess)
            {
                Debug.LogError("[Attributes Batch] Failed: " + result.Message);
                EditorApplication.Exit(1);
                return;
            }

            Debug.Log("[Attributes Batch] Passed: " + result.Message);
            EditorApplication.Exit(0);
        }
    }
}
