using System.Collections.Generic;
using System.Text;

// =============================================================================
// SCRIPT: CCS_SurvivalValidationReport
// CATEGORY: Survival / Editor / Development / Validation
// PURPOSE: Aggregated editor validation report for survival project structure checks.
// PLACEMENT: Returned by CCS_SurvivalValidationUtility.RunDevelopmentValidation().
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-28
// NOTES: Future modules may append issues through utility extension points.
// =============================================================================

namespace CCS.Survival.Editor.Development
{
    public sealed class CCS_SurvivalValidationReport
    {
        #region Variables

        private readonly List<CCS_SurvivalValidationIssue> issues = new List<CCS_SurvivalValidationIssue>(16);

        #endregion

        #region Public Methods

        public void AddIssue(CCS_SurvivalValidationIssue issue)
        {
            issues.Add(issue);
        }

        public void AddIssue(CCS_SurvivalValidationIssueSeverity severity, string context, string message)
        {
            issues.Add(new CCS_SurvivalValidationIssue(severity, context, message));
        }

        public IReadOnlyList<CCS_SurvivalValidationIssue> GetIssues()
        {
            return issues;
        }

        public bool HasErrors()
        {
            for (int index = 0; index < issues.Count; index++)
            {
                if (issues[index].Severity == CCS_SurvivalValidationIssueSeverity.Error)
                {
                    return true;
                }
            }

            return false;
        }

        public bool HasWarnings()
        {
            for (int index = 0; index < issues.Count; index++)
            {
                if (issues[index].Severity == CCS_SurvivalValidationIssueSeverity.Warning)
                {
                    return true;
                }
            }

            return false;
        }

        public string BuildSummary()
        {
            int infoCount = 0;
            int warningCount = 0;
            int errorCount = 0;

            for (int index = 0; index < issues.Count; index++)
            {
                switch (issues[index].Severity)
                {
                    case CCS_SurvivalValidationIssueSeverity.Info:
                        infoCount++;
                        break;
                    case CCS_SurvivalValidationIssueSeverity.Warning:
                        warningCount++;
                        break;
                    case CCS_SurvivalValidationIssueSeverity.Error:
                        errorCount++;
                        break;
                }
            }

            return $"Survival validation report: {infoCount} info, {warningCount} warnings, {errorCount} errors.";
        }

        public string BuildDetailedLog()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine(BuildSummary());

            for (int index = 0; index < issues.Count; index++)
            {
                CCS_SurvivalValidationIssue issue = issues[index];
                builder.Append('[');
                builder.Append(issue.Severity);
                builder.Append("] ");
                builder.Append(issue.Context);
                builder.Append(" — ");
                builder.AppendLine(issue.Message);
            }

            return builder.ToString();
        }

        #endregion
    }
}
