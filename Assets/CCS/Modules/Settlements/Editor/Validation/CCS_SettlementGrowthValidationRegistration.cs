using CCS.Survival.Editor.Development;
using UnityEditor;

// =============================================================================
// SCRIPT: CCS_SettlementGrowthValidationRegistration
// CATEGORY: Modules / Settlements / Editor / Validation
// PURPOSE: Registers settlement growth validator on the central validation pipeline.
// PLACEMENT: Auto-runs at editor load through InitializeOnLoad.
// AUTHOR: James Schilz
// CREATED: 2026-06-04
// NOTES: Milestone 3.2.0 settlement growth foundation.
// =============================================================================

namespace CCS.Modules.Settlements.Editor
{
    [InitializeOnLoad]
    public static class CCS_SettlementGrowthValidationRegistration
    {
        static CCS_SettlementGrowthValidationRegistration()
        {
            CCS_SurvivalValidationPipeline.RegisterValidator(new CCS_SettlementGrowthFoundationValidationValidator());
        }
    }
}
