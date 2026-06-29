using System.Collections.Generic;
using System.IO;
using CCS.Project;
using UnityEditor;

// =============================================================================
// SCRIPT: CCS_CharacterControllerPhase2CValidationUtility
// CATEGORY: Modules / CharacterController / Editor / Validation
// PURPOSE: Validates player prefab audit foundation (v0.7.1e Phase 2C).
// PLACEMENT: Editor validation utility. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-25
// NOTES: Invoked from Master Test validator and player prefab audit batch paths.
// =============================================================================

namespace CCS.Modules.CharacterController.Editor
{
    public static class CCS_CharacterControllerPhase2CValidationUtility
    {
        public static CCS_SurvivalValidationResult ValidatePhase2CAuditFoundation()
        {
            List<string> failures = new List<string>();

            CCS_SurvivalValidationResult phase2B =
                CCS_CharacterControllerPhase2BValidationUtility.ValidatePhase2BFoundation();
            if (!phase2B.IsSuccess)
            {
                failures.Add(phase2B.Message);
            }

            CCS_SurvivalValidationResult auditFoundation =
                CCS_CharacterControllerPlayerPrefabAuditUtility.ValidatePhase2CAuditFoundation();
            if (!auditFoundation.IsSuccess)
            {
                failures.Add(auditFoundation.Message);
            }

            return failures.Count > 0
                ? CCS_SurvivalValidationResult.Fail(string.Join(" ", failures))
                : CCS_SurvivalValidationResult.Pass(
                    "Character Controller player prefab audit and Phase 2C foundation validated.");
        }
    }
}
