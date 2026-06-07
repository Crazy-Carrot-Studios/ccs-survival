using CCS.Survival.Editor.Development;
using UnityEditor;

// =============================================================================
// SCRIPT: CCS_NpcValidationRegistration
// CATEGORY: Modules / NPCs / Editor / Validation
// PURPOSE: Registers NPC validators on editor load.
// PLACEMENT: Auto-registered with CCS_SurvivalValidationPipeline.
// AUTHOR: James Schilz
// CREATED: 2026-06-04
// NOTES: Milestone 4.1.0 NPC identity; 4.3.0 NPC service representatives; 4.5.0 NPC movement; 4.6.0 NPC schedule; 4.7.0 NPC activity; 4.8.0 NPC affiliation; 4.9.0 NPC dialogue stub; 5.0.0 NPC social presence.
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
            CCS_SurvivalValidationPipeline.RegisterValidator(
                new CCS_NpcMovementFoundationValidationValidator());
            CCS_SurvivalValidationPipeline.RegisterValidator(
                new CCS_NpcScheduleFoundationValidationValidator());
            CCS_SurvivalValidationPipeline.RegisterValidator(
                new CCS_NpcActivityFoundationValidationValidator());
            CCS_SurvivalValidationPipeline.RegisterValidator(
                new CCS_NpcAffiliationFoundationValidationValidator());
            CCS_SurvivalValidationPipeline.RegisterValidator(
                new CCS_NpcDialogueFoundationValidationValidator());
            CCS_SurvivalValidationPipeline.RegisterValidator(
                new CCS_NpcSocialFoundationValidationValidator());
        }
    }
}
