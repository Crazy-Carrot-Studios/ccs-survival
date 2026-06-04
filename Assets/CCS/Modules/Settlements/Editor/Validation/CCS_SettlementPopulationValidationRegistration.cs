using CCS.Survival.Editor.Development;
using UnityEditor;

// =============================================================================
// SCRIPT: CCS_SettlementPopulationValidationRegistration
// CATEGORY: Modules / Settlements / Editor / Validation
// PURPOSE: Registers population foundation validator on the central validation pipeline.
// PLACEMENT: Auto-runs at editor load through InitializeOnLoad.
// AUTHOR: James Schilz
// CREATED: 2026-06-04
// NOTES: Milestone 3.6.0 population foundation.
// =============================================================================

namespace CCS.Modules.Settlements.Editor
{
    [InitializeOnLoad]
    public static class CCS_SettlementPopulationValidationRegistration
    {
        static CCS_SettlementPopulationValidationRegistration()
        {
            CCS_SurvivalValidationPipeline.RegisterValidator(new CCS_SettlementPopulationFoundationValidationValidator());
        }
    }
}
