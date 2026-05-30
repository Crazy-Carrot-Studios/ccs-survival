using CCS.Survival.Editor.Development;
using UnityEditor;

// =============================================================================
// SCRIPT: CCS_InventoryValidationRegistration
// CATEGORY: Modules / Inventory / Editor / Validation
// PURPOSE: Registers inventory validator with the central validation pipeline.
// PLACEMENT: Editor assembly only. InitializeOnLoad registration.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Permanent validation infrastructure for 0.4.0+.
// =============================================================================

namespace CCS.Modules.Inventory.Editor
{
    [InitializeOnLoad]
    public static class CCS_InventoryValidationRegistration
    {
        #region Public Methods

        static CCS_InventoryValidationRegistration()
        {
            CCS_SurvivalValidationPipeline.RegisterValidator(
                new CCS_InventoryValidationValidator());
        }

        #endregion
    }
}
