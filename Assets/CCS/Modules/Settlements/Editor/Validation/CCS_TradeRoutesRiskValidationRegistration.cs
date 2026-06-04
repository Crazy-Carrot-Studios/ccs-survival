using CCS.Survival.Editor.Development;
using UnityEditor;

// =============================================================================
// SCRIPT: CCS_TradeRoutesRiskValidationRegistration
// CATEGORY: Modules / Settlements / Editor / Validation
// PURPOSE: Registers route risk freight validator on the central validation pipeline.
// PLACEMENT: Auto-runs at editor load through InitializeOnLoad.
// AUTHOR: James Schilz
// CREATED: 2026-06-04
// NOTES: Milestone 3.5.0 route risk and freight bonus foundation.
// =============================================================================

namespace CCS.Modules.Settlements.Editor
{
    [InitializeOnLoad]
    public static class CCS_TradeRoutesRiskValidationRegistration
    {
        static CCS_TradeRoutesRiskValidationRegistration()
        {
            CCS_SurvivalValidationPipeline.RegisterValidator(new CCS_TradeRoutesRiskFoundationValidationValidator());
        }
    }
}
