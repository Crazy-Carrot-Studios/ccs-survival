using CCS.Survival.Editor.Development;
using UnityEditor;

// =============================================================================
// SCRIPT: CCS_MultiSettlementValidationRegistration
// CATEGORY: Modules / Settlements / Editor / Validation
// PURPOSE: Registers multi-settlement validator on the central validation pipeline.
// PLACEMENT: Auto-runs at editor load through InitializeOnLoad.
// AUTHOR: James Schilz
// CREATED: 2026-06-04
// NOTES: Milestone 3.3.0 multi-settlement foundation.
// =============================================================================

namespace CCS.Modules.Settlements.Editor
{
    [InitializeOnLoad]
    public static class CCS_MultiSettlementValidationRegistration
    {
        static CCS_MultiSettlementValidationRegistration()
        {
            CCS_SurvivalValidationPipeline.RegisterValidator(new CCS_MultiSettlementFoundationValidationValidator());
        }
    }
}
