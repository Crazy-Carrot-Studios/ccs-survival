using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_InteractionMasterTestInteractionsBatchEntry
// CATEGORY: Modules / Interaction / Editor
// PURPOSE: Batch-mode entry point for Master Test pickup cube and building door wiring.
// PLACEMENT: Editor batch utility. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Invoked via Unity -batchmode -executeMethod ...RunFromBatchMode.
// =============================================================================

namespace CCS.Modules.Interaction.Editor
{
    public static class CCS_InteractionMasterTestInteractionsBatchEntry
    {
        public static void RunFromBatchMode()
        {
            bool changed = CCS_InteractionDetectionTestBuilder.BuildMasterTestInteractions();
            Debug.Log(
                changed
                    ? "[Interaction Batch] Master Test interactions updated."
                    : "[Interaction Batch] Master Test interactions already up to date.");
            AssetDatabase.SaveAssets();
            EditorApplication.Exit(0);
        }
    }
}
