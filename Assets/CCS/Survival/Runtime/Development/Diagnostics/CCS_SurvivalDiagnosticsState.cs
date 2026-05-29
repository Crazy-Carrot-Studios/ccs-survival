// =============================================================================
// SCRIPT: CCS_SurvivalDiagnosticsState
// CATEGORY: Survival / Runtime / Development / Diagnostics
// PURPOSE: Lightweight lifecycle state values for survival development diagnostics reporting.
// PLACEMENT: Used by CCS_SurvivalDiagnosticsService and future module status reporters.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-28
// NOTES: No gameplay semantics. Event-driven status only.
// =============================================================================

namespace CCS.Survival.Development
{
    public enum CCS_SurvivalDiagnosticsState
    {
        Unknown = 0,
        Initializing = 1,
        Ready = 2,
        Warning = 3,
        Error = 4
    }
}
