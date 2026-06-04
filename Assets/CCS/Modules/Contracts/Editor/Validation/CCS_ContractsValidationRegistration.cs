using CCS.Survival.Editor.Development;
using UnityEditor;

// =============================================================================
// SCRIPT: CCS_ContractsValidationRegistration
// CATEGORY: Modules / Contracts / Editor / Validation
// PURPOSE: Registers contracts foundation validator on editor load.
// PLACEMENT: Auto-invoked via InitializeOnLoad in editor assembly.
// AUTHOR: James Schilz
// CREATED: 2026-06-04
// NOTES: Milestone 3.0.0 frontier contracts foundation.
// =============================================================================

namespace CCS.Modules.Contracts.Editor
{
    [InitializeOnLoad]
    public static class CCS_ContractsValidationRegistration
    {
        static CCS_ContractsValidationRegistration()
        {
            CCS_SurvivalValidationPipeline.RegisterValidator(new CCS_ContractsFoundationValidationValidator());
        }
    }
}
