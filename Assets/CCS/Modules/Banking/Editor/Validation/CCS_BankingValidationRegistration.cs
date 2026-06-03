using CCS.Survival.Editor.Development;

// =============================================================================
// SCRIPT: CCS_BankingValidationRegistration
// CATEGORY: Modules / Banking / Editor / Validation
// PURPOSE: Registers banking foundation validator on editor load.
// PLACEMENT: Auto-loaded by Unity editor assembly.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 2.4.0 banking and land office foundation.
// =============================================================================

namespace CCS.Modules.Banking.Editor
{
    public static class CCS_BankingValidationRegistration
    {
        static CCS_BankingValidationRegistration()
        {
            CCS_SurvivalValidationPipeline.RegisterValidator(new CCS_BankingFoundationValidationValidator());
        }
    }
}
