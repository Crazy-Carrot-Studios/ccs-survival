using CCS.Modules.Attributes.Editor;
using CCS.Modules.CharacterController.Editor;
using CCS.Modules.Interaction.Editor;

using CCS.Project;

using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_ProjectAuditMenus
// CATEGORY: Project / Editor
// PURPOSE: Registers project-level audit menu under CCS/Project.
// PLACEMENT: Editor-only static registration. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Report-first audit; does not regenerate scenes or repair assets.
// =============================================================================

namespace CCS.Project.Editor
{
    public static class CCS_ProjectAuditMenus
    {
        private const string MenuRoot = "CCS/Project/";

        [MenuItem(MenuRoot + "Run Project Audit")]
        public static void RunProjectAuditMenu()
        {
            LogResult(CCS_ProjectAuditValidator.RunProjectAudit(includeModuleValidators: true));
        }

        public static void RunFromBatchMode()
        {
            CCS_SurvivalValidationResult result = CCS_ProjectAuditValidator.RunProjectAudit(includeModuleValidators: true);
            if (result.IsSuccess)
            {
                Debug.Log("[Project Audit] Passed: " + result.Message);
                EditorApplication.Exit(0);
                return;
            }

            Debug.LogError("[Project Audit] Failed: " + result.Message);
            EditorApplication.Exit(1);
        }

        private static void LogResult(CCS_SurvivalValidationResult result)
        {
            if (result.IsSuccess)
            {
                Debug.Log("[Project Audit] Passed: " + result.Message);
            }
            else
            {
                Debug.LogError("[Project Audit] Failed: " + result.Message);
            }
        }
    }
}
