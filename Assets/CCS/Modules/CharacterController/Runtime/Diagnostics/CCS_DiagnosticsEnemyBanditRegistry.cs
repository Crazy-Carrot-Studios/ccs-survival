// =============================================================================
// SCRIPT: CCS_DiagnosticsEnemyBanditRegistry
// CATEGORY: Modules / CharacterController / Runtime / Diagnostics
// PURPOSE: Validation-scene registry for optional enemy bandit spawn diagnostics.
// PLACEMENT: Runtime static registry. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-25
// NOTES: Diagnostics manager syncs EnableEnemy on validation scenes only.
// =============================================================================

namespace CCS.Modules.CharacterController.Diagnostics
{
    public static class CCS_DiagnosticsEnemyBanditRegistry
    {
        public static bool EnableEnemy { get; set; }
    }
}
