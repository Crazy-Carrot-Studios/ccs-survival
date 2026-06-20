using UnityEditor;

// =============================================================================
// SCRIPT: CCS_MultiplayerHostingSetupBatchEntry
// CATEGORY: Modules / CharacterController / Tests / Netcode / Editor
// PURPOSE: Batch-mode entry point for full multiplayer hosting scene setup.
// PLACEMENT: Editor batch utility. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// =============================================================================

namespace CCS.Modules.CharacterController.Tests.Netcode.Editor
{
    public static class CCS_MultiplayerHostingSetupBatchEntry
    {
        public static void RebuildFromBatchMode()
        {
            CCS_CharacterControllerTestHarnessMenus.RunFromBatchMode();
        }
    }
}
