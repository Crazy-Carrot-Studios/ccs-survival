using CCS.Survival.Editor.Development;
using UnityEditor;

// =============================================================================
// SCRIPT: CCS_PrimitiveToolWeaponValidationRegistration
// CATEGORY: Survival / Editor / Development / Validation
// PURPOSE: Registers primitive tool/weapon validator with the central validation pipeline.
// PLACEMENT: Editor assembly only. InitializeOnLoad registration.
// AUTHOR: James Schilz
// CREATED: 2026-05-31
// NOTES: Permanent validation infrastructure for 0.9.2+.
// =============================================================================

namespace CCS.Survival.Editor.Development
{
    [InitializeOnLoad]
    public static class CCS_PrimitiveToolWeaponValidationRegistration
    {
        #region Public Methods

        static CCS_PrimitiveToolWeaponValidationRegistration()
        {
            CCS_SurvivalValidationPipeline.RegisterValidator(
                new CCS_PrimitiveToolWeaponValidationValidator());
        }

        #endregion
    }
}
