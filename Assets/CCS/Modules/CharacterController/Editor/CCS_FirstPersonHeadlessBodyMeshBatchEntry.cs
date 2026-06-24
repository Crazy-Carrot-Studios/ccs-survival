using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_FirstPersonHeadlessBodyMeshBatchEntry
// CATEGORY: Modules / CharacterController / Editor
// PURPOSE: Batch-mode entry for baking the CCS headless first-person body mesh asset.
// PLACEMENT: Editor batch utility. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-24
// NOTES: Invoked via Unity -batchmode -executeMethod ...RunFromBatchMode.
// =============================================================================

namespace CCS.Modules.CharacterController.Editor
{
    public static class CCS_FirstPersonHeadlessBodyMeshBatchEntry
    {
        public static void RunFromBatchMode()
        {
            bool success = CCS_FirstPersonHeadlessBodyMeshBuilder.EnsureHeadlessBodyMeshAsset();
            if (!success)
            {
                Debug.LogError("[Headless Body Mesh Batch] Failed to bake CCS_CC3_FirstPerson_HeadlessBody.asset.");
                EditorApplication.Exit(1);
                return;
            }

            Debug.Log("[Headless Body Mesh Batch] Completed successfully.");
            EditorApplication.Exit(0);
        }
    }
}
