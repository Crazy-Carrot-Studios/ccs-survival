// =============================================================================
// SCRIPT: CCS_AimPresentationDiagnosticsRegistry
// CATEGORY: Modules / CharacterController / Runtime / Data
// PURPOSE: Lightweight read-only gate for aim/reticle transition console logging.
// PLACEMENT: Synced from CCS_CharacterControllerDiagnosticsManager when aim diagnostics enabled.
// AUTHOR: James Schilz
// CREATED: 2026-06-30
// NOTES: Avoids Weapons -> Diagnostics assembly dependency.
// =============================================================================

namespace CCS.Modules.CharacterController
{
    public static class CCS_AimPresentationDiagnosticsRegistry
    {
        public static bool EnableReticleTransitionLogging { get; set; }

        public static bool EnableAimTargetDebugRays { get; set; }
    }
}
