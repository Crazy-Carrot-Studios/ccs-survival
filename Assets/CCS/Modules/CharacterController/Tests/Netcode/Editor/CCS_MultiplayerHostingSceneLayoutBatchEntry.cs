using UnityEditor;

// =============================================================================
// SCRIPT: CCS_MultiplayerHostingSceneLayoutBatchEntry
// CATEGORY: Modules / CharacterController / Tests / Netcode / Editor
// PURPOSE: Batch-mode entry point for rebuilding the multiplayer hosting scene UI.
// PLACEMENT: Editor batch utility. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// =============================================================================

namespace CCS.Modules.CharacterController.Tests.Netcode.Editor
{
    public static class CCS_MultiplayerHostingSceneLayoutBatchEntry
    {
        public static void RebuildFromBatchMode()
        {
            bool success = CCS_MultiplayerHostingSceneLayoutEditor.BuildOrRebuildLayout();
            EditorApplication.Exit(success ? 0 : 1);
        }
    }
}
