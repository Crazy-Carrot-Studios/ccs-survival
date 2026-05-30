using CCS.Survival.Editor.Development;
using UnityEditor;

// =============================================================================
// SCRIPT: CCS_EquipmentValidationRegistration
// CATEGORY: Modules / Equipment / Editor / Validation
// PURPOSE: Registers equipment validator with the central validation pipeline.
// PLACEMENT: Editor assembly only. InitializeOnLoad registration.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Permanent validation infrastructure for 0.4.1+.
// =============================================================================

namespace CCS.Modules.Equipment.Editor
{
    [InitializeOnLoad]
    public static class CCS_EquipmentValidationRegistration
    {
        #region Public Methods

        static CCS_EquipmentValidationRegistration()
        {
            CCS_SurvivalValidationPipeline.RegisterValidator(
                new CCS_EquipmentValidationValidator());
        }

        #endregion
    }
}
