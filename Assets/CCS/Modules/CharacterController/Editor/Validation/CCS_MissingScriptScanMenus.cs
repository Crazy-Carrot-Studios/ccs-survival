using CCS.Project;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_MissingScriptScanMenus
// CATEGORY: Modules / CharacterController / Editor / Validation
// PURPOSE: Editor menu entries for missing script scan and repair on production assets.
// PLACEMENT: Editor menu utility. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-25
// =============================================================================

namespace CCS.Modules.CharacterController.Editor
{
    public static class CCS_MissingScriptScanMenus
    {
        private const string MenuRoot = "CCS/Character Controller/Validation/Missing Scripts/";

        [MenuItem(MenuRoot + "Scan Production Assets")]
        public static void ScanProductionAssets()
        {
            System.Collections.Generic.List<MissingScriptReportEntry> entries =
                CCS_MissingScriptScanUtility.ScanProductionAssets();
            if (entries.Count == 0)
            {
                Debug.Log("[Missing Script Scan] No missing script slots found on production assets.");
                return;
            }

            Debug.LogWarning("[Missing Script Scan] " + CCS_MissingScriptScanUtility.FormatReport(entries));
        }

        [MenuItem(MenuRoot + "Repair Production Assets")]
        public static void RepairProductionAssets()
        {
            int removed = CCS_MissingScriptScanUtility.RepairProductionAssets(
                out System.Collections.Generic.List<MissingScriptReportEntry> removedEntries);
            if (removed > 0)
            {
                AssetDatabase.SaveAssets();
                Debug.Log(
                    "[Missing Script Scan] Removed "
                    + removed
                    + " missing script slot(s): "
                    + CCS_MissingScriptScanUtility.FormatReport(removedEntries));
            }
            else
            {
                Debug.Log("[Missing Script Scan] No missing script slots found to repair.");
            }

            CCS_SurvivalValidationResult validationResult =
                CCS_MissingScriptScanUtility.ValidateProductionAssetsHaveNoMissingScripts();
            if (validationResult.IsSuccess)
            {
                Debug.Log("[Missing Script Scan] " + validationResult.Message);
                return;
            }

            Debug.LogError("[Missing Script Scan] Validation failed after repair: " + validationResult.Message);
        }
    }
}
