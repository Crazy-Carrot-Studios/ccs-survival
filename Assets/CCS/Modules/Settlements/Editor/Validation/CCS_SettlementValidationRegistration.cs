using CCS.Survival.Editor.Development;
using UnityEditor;

// =============================================================================
// SCRIPT: CCS_SettlementValidationRegistration
// CATEGORY: Modules / Settlements / Editor / Validation
// PURPOSE: Registers settlement validator on the central survival validation pipeline.
// PLACEMENT: Auto-runs at editor load through InitializeOnLoad.
// AUTHOR: James Schilz
// CREATED: 2026-06-02
// =============================================================================

namespace CCS.Modules.Settlements.Editor
{
    [InitializeOnLoad]
    public static class CCS_SettlementValidationRegistration
    {
        static CCS_SettlementValidationRegistration()
        {
            CCS_SurvivalValidationPipeline.RegisterValidator(new CCS_FrontierSettlementValidationValidator());
        }
    }
}
