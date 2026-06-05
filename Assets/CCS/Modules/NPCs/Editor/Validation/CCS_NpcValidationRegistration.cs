using CCS.Survival.Editor.Development;

// =============================================================================
// SCRIPT: CCS_NpcValidationRegistration
// CATEGORY: Modules / NPCs / Editor / Validation
// PURPOSE: Registers NPC identity foundation validator on editor load.
// PLACEMENT: Auto-registered with CCS_SurvivalValidationPipeline.
// AUTHOR: James Schilz
// CREATED: 2026-06-04
// NOTES: Milestone 4.1.0 NPC identity and role foundation.
// =============================================================================

namespace CCS.Modules.NPCs.Editor
{
    public static class CCS_NpcValidationRegistration
    {
        static CCS_NpcValidationRegistration()
        {
            CCS_SurvivalValidationPipeline.RegisterValidator(
                new CCS_NpcIdentityFoundationValidationValidator());
        }
    }
}
