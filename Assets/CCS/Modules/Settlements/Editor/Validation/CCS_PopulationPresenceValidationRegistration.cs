using CCS.Survival.Editor.Development;

// =============================================================================
// SCRIPT: CCS_PopulationPresenceValidationRegistration
// CATEGORY: Modules / Settlements / Editor / Validation
// PURPOSE: Registers population presence foundation validator on editor load.
// PLACEMENT: Auto-registered with CCS_SurvivalValidationPipeline.
// AUTHOR: James Schilz
// CREATED: 2026-06-04
// NOTES: Milestone 4.0.0 NPC population placeholder foundation.
// =============================================================================

namespace CCS.Modules.Settlements.Editor
{
    public static class CCS_PopulationPresenceValidationRegistration
    {
        static CCS_PopulationPresenceValidationRegistration()
        {
            CCS_SurvivalValidationPipeline.RegisterValidator(
                new CCS_PopulationPresenceFoundationValidationValidator());
        }
    }
}
