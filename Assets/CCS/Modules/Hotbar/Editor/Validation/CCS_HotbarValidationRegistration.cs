using CCS.Survival.Editor.Development;
using UnityEditor;

// =============================================================================
// SCRIPT: CCS_HotbarValidationRegistration
// CATEGORY: Modules / Hotbar / Editor / Validation
// PURPOSE: Registers hotbar module validator on the survival validation pipeline.
// PLACEMENT: Editor load via InitializeOnLoadMethod.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// =============================================================================

namespace CCS.Modules.Hotbar.Editor
{
    public static class CCS_HotbarValidationRegistration
    {
        [InitializeOnLoadMethod]
        private static void RegisterValidator()
        {
            CCS_SurvivalValidationPipeline.RegisterValidator(new CCS_HotbarValidationValidator());
        }
    }
}
