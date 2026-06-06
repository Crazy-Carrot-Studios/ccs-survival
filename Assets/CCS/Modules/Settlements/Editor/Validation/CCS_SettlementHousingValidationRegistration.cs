using CCS.Survival.Editor.Development;
using UnityEditor;

// =============================================================================
// SCRIPT: CCS_SettlementHousingValidationRegistration
// CATEGORY: Modules / Settlements / Editor / Validation
// PURPOSE: Registers settlement housing foundation validator on editor load.
// PLACEMENT: Auto-registered with CCS_SurvivalValidationPipeline.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 4.4.0 settlement housing foundation.
// =============================================================================

namespace CCS.Modules.Settlements.Editor
{
    [InitializeOnLoad]
    public static class CCS_SettlementHousingValidationRegistration
    {
        static CCS_SettlementHousingValidationRegistration()
        {
            CCS_SurvivalValidationPipeline.RegisterValidator(
                new CCS_SettlementHousingFoundationValidationValidator());
        }
    }
}
