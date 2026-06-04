using CCS.Survival.Editor.Development;
using UnityEditor;

// =============================================================================
// SCRIPT: CCS_BusinessPresenceValidationRegistration
// CATEGORY: Modules / Settlements / Editor / Validation
// PURPOSE: Registers business presence foundation validator on editor load.
// PLACEMENT: Auto-loaded by Unity Editor.
// AUTHOR: James Schilz
// CREATED: 2026-06-04
// NOTES: Milestone 3.8.0 — visible business presence foundation.
// =============================================================================

namespace CCS.Modules.Settlements.Editor
{
    [InitializeOnLoad]
    public static class CCS_BusinessPresenceValidationRegistration
    {
        static CCS_BusinessPresenceValidationRegistration()
        {
            CCS_SurvivalValidationPipeline.RegisterValidator(new CCS_BusinessPresenceFoundationValidationValidator());
        }
    }
}
