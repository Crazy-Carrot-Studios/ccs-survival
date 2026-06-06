using CCS.Survival.Editor.Development;
using UnityEditor;

// =============================================================================
// SCRIPT: CCS_NpcValidationRegistration
// CATEGORY: Modules / NPCs / Editor / Validation
// PURPOSE: Registers NPC validators on editor load.
// PLACEMENT: Auto-registered with CCS_SurvivalValidationPipeline.
// AUTHOR: James Schilz
// CREATED: 2026-06-04
// NOTES: Milestone 4.1.0 NPC identity; 4.3.0 NPC service representatives foundation.
// =============================================================================

namespace CCS.Modules.NPCs.Editor
{
    [InitializeOnLoad]
    public static class CCS_NpcValidationRegistration
    {
        static CCS_NpcValidationRegistration()
        {
            CCS_SurvivalValidationPipeline.RegisterValidator(
                new CCS_NpcIdentityFoundationValidationValidator());
            CCS_SurvivalValidationPipeline.RegisterValidator(
                new CCS_NpcServiceRepresentativeFoundationValidationValidator());
        }
    }
}
