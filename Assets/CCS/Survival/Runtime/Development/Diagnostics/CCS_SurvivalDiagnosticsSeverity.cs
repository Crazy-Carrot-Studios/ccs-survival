// =============================================================================
// SCRIPT: CCS_SurvivalDiagnosticsSeverity
// CATEGORY: Survival / Runtime / Development / Diagnostics
// PURPOSE: Severity classification for survival development diagnostic messages.
// PLACEMENT: Used by CCS_SurvivalDiagnosticsMessage and module status reporters.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-28
// NOTES: Info / Warning / Error align with validation severity vocabulary.
// =============================================================================

namespace CCS.Survival.Development
{
    public enum CCS_SurvivalDiagnosticsSeverity
    {
        Info = 0,
        Warning = 1,
        Error = 2
    }
}
