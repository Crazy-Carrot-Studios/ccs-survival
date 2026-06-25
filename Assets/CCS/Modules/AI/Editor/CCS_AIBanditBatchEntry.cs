using CCS.Project;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_AIBanditBatchEntry
// CATEGORY: Modules / AI / Editor
// PURPOSE: Batch-mode entry point for AI prefab setup, netcode registration, and validation.
// PLACEMENT: Editor batch utility. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-25
// NOTES: Invoked via Unity -batchmode -executeMethod ...RunFromBatchMode.
// =============================================================================

namespace CCS.Modules.AI.Editor
{
    public static class CCS_AIBanditBatchEntry
    {
        public static void RunFromBatchMode()
        {
            CCS_AIBanditPrefabBuilder.EnsureAIBanditPrefab();
            TryRebuildNetcodePrefabSetupViaReflection();
            CCS_AIBanditMasterTestBuilder.EnsureMasterTestBanditSpawner();

            CCS_SurvivalValidationResult result = CCS_AIBanditValidationUtility.ValidateMilestoneB13Foundation();
            if (!result.IsSuccess)
            {
                Debug.LogError("[AI Batch] Validation failed: " + result.Message);
                EditorApplication.Exit(1);
                return;
            }

            Debug.Log("[AI Batch] Validation passed: AI prefab, netcode registration, and scene spawner are configured.");
            EditorApplication.Exit(0);
        }

        private static void TryRebuildNetcodePrefabSetupViaReflection()
        {
            System.Type utilityType = System.Type.GetType(
                "CCS.Modules.CharacterController.Tests.Netcode.Editor.CCS_NetcodeNetworkPrefabSetupUtility, CCS.Modules.CharacterController.Tests.Netcode.Editor");
            if (utilityType == null)
            {
                return;
            }

            System.Reflection.MethodInfo method = utilityType.GetMethod(
                "RebuildNetworkPrefabSetup",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            if (method == null)
            {
                return;
            }

            method.Invoke(null, null);
        }
    }
}
