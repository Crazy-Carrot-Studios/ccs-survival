// =============================================================================
// SCRIPT: CCS_SurvivalValidationIssueSeverity
// CATEGORY: Survival / Editor / Development / Validation
// PURPOSE: Severity levels for editor-side survival validation reports.
// PLACEMENT: Used by CCS_SurvivalValidationIssue and CCS_SurvivalValidationReport.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-28
// NOTES: Editor-only validation taxonomy. Not used for runtime gameplay rules.
// =============================================================================

namespace CCS.Survival.Editor.Development
{
    public enum CCS_SurvivalValidationIssueSeverity
    {
        Info = 0,
        Warning = 1,
        Error = 2
    }
}
