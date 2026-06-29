using System.Collections.Generic;
using CCS.Project;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_MissingScriptScanBatchEntry
// CATEGORY: Modules / CharacterController / Editor / Validation
// PURPOSE: Batch-mode scan and repair for missing script slots on production assets.
// PLACEMENT: Editor batch utility. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-25
// NOTES: Invoked via Unity -batchmode -executeMethod ...RunFromBatchMode.
// =============================================================================

namespace CCS.Modules.CharacterController.Editor
{
    public static class CCS_MissingScriptScanBatchEntry
    {
        public static void RunFromBatchMode()
        {
            List<MissingScriptReportEntry> beforeEntries = CCS_MissingScriptScanUtility.ScanProductionAssets();
            if (beforeEntries.Count > 0)
            {
                Debug.LogWarning(
                    "[Missing Script Scan] Before repair: "
                    + CCS_MissingScriptScanUtility.FormatReport(beforeEntries));
            }

            int removed = CCS_MissingScriptScanUtility.RepairProductionAssets(out List<MissingScriptReportEntry> removedEntries);
            if (removed > 0)
            {
                Debug.Log(
                    "[Missing Script Scan] Removed "
                    + removed
                    + " missing script slot(s): "
                    + CCS_MissingScriptScanUtility.FormatReport(removedEntries));
                AssetDatabase.SaveAssets();
            }

            CCS_SurvivalValidationResult validationResult =
                CCS_MissingScriptScanUtility.ValidateProductionAssetsHaveNoMissingScripts();
            if (!validationResult.IsSuccess)
            {
                Debug.LogError("[Missing Script Scan] Validation failed: " + validationResult.Message);
                EditorApplication.Exit(1);
                return;
            }

            Debug.Log("[Missing Script Scan] " + validationResult.Message);
            EditorApplication.Exit(0);
        }
    }
}
