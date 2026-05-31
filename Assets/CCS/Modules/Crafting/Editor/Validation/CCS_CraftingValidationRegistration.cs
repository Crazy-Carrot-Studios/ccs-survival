using CCS.Survival.Editor.Development;
using UnityEditor;

// =============================================================================
// SCRIPT: CCS_CraftingValidationRegistration
// CATEGORY: Modules / Crafting / Editor / Validation
// PURPOSE: Registers crafting validator with the central validation pipeline.
// PLACEMENT: Editor assembly only. InitializeOnLoad registration.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Permanent validation infrastructure for 0.5.0+.
// =============================================================================

namespace CCS.Modules.Crafting.Editor
{
    [InitializeOnLoad]
    public static class CCS_CraftingValidationRegistration
    {
        #region Public Methods

        static CCS_CraftingValidationRegistration()
        {
            CCS_SurvivalValidationPipeline.RegisterValidator(
                new CCS_CraftingValidationValidator());
        }

        #endregion
    }
}
