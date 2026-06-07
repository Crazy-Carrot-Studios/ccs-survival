using CCS.Survival.Editor.Development;
using UnityEditor;

// =============================================================================
// SCRIPT: CCS_SettlementEventsValidationRegistration
// CATEGORY: Modules / Settlements / Editor / Validation
// PURPOSE: Registers settlement events validator on editor load.
// PLACEMENT: Auto-registered with CCS_SurvivalValidationPipeline.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 5.1.0 dynamic settlement events foundation.
// =============================================================================

namespace CCS.Modules.Settlements.Editor
{
    [InitializeOnLoad]
    public static class CCS_SettlementEventsValidationRegistration
    {
        static CCS_SettlementEventsValidationRegistration()
        {
            CCS_SurvivalValidationPipeline.RegisterValidator(
                new CCS_SettlementEventsFoundationValidationValidator());
        }
    }
}
