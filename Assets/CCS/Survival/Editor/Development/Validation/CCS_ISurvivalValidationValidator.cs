// =============================================================================
// SCRIPT: CCS_ISurvivalValidationValidator
// CATEGORY: Survival / Editor / Development / Validation
// PURPOSE: Contract for registrable survival editor validation checks.
// PLACEMENT: Implemented by foundation and future module validators.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-28
// NOTES: Validators append issues to a shared report via the central pipeline.
// =============================================================================

namespace CCS.Survival.Editor.Development
{
    public interface CCS_ISurvivalValidationValidator
    {
        string ValidatorId { get; }

        void Validate(CCS_SurvivalValidationReport report);
    }
}
