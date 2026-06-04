using CCS.Survival.Editor.Development;
using UnityEditor;

// =============================================================================
// SCRIPT: CCS_TradeRoutesFreightValidationRegistration
// CATEGORY: Modules / Settlements / Editor / Validation
// PURPOSE: Registers trade route freight validator on the central validation pipeline.
// PLACEMENT: Auto-runs at editor load through InitializeOnLoad.
// AUTHOR: James Schilz
// CREATED: 2026-06-04
// NOTES: Milestone 3.4.0 trade routes and freight contracts.
// =============================================================================

namespace CCS.Modules.Settlements.Editor
{
    [InitializeOnLoad]
    public static class CCS_TradeRoutesFreightValidationRegistration
    {
        static CCS_TradeRoutesFreightValidationRegistration()
        {
            CCS_SurvivalValidationPipeline.RegisterValidator(new CCS_TradeRoutesFreightFoundationValidationValidator());
        }
    }
}
