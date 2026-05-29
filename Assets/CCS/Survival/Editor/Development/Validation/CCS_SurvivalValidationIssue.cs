// =============================================================================
// SCRIPT: CCS_SurvivalValidationIssue
// CATEGORY: Survival / Editor / Development / Validation
// PURPOSE: Single validation issue entry for survival editor validation reports.
// PLACEMENT: Created by CCS_SurvivalValidationUtility checks.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-28
// NOTES: Context should identify folder, asset, or rule name.
// =============================================================================

namespace CCS.Survival.Editor.Development
{
    public readonly struct CCS_SurvivalValidationIssue
    {
        #region Public Methods

        public CCS_SurvivalValidationIssue(
            CCS_SurvivalValidationIssueSeverity severity,
            string context,
            string message)
        {
            Severity = severity;
            Context = string.IsNullOrWhiteSpace(context) ? "General" : context.Trim();
            Message = string.IsNullOrWhiteSpace(message) ? "Validation issue with no detail." : message.Trim();
        }

        #endregion

        #region Properties

        public CCS_SurvivalValidationIssueSeverity Severity { get; }

        public string Context { get; }

        public string Message { get; }

        #endregion
    }
}
