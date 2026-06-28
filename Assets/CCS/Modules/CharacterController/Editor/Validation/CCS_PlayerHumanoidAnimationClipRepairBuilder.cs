using System.Collections.Generic;
using System.IO;
using CCS.Project;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_PlayerHumanoidAnimationClipRepairBuilder
// CATEGORY: Modules / CharacterController / Editor / Validation
// PURPOSE: Repairs hybrid Generic+Humanoid player clips and refreshes player prefab binding.
// PLACEMENT: Editor builder invoked by v0.8.1b humanoid binding batch entry.
// AUTHOR: James Schilz
// CREATED: 2026-06-28
// NOTES: Strips generic transform curves, then re-saves player prefabs to clear binding warnings.
// =============================================================================

namespace CCS.Modules.CharacterController.Editor
{
    public static class CCS_PlayerHumanoidAnimationClipRepairBuilder
    {
        private const string ClipReportPath = "Logs/player-humanoid-clip-validation-report.md";

        public static bool EnsurePlayerHumanoidAnimationClipRepair(out List<string> repairSummaries)
        {
            repairSummaries = new List<string>();
            bool changed = false;

            if (CCS_PlayerHumanoidAnimationClipRepairUtility.RepairRequiredControllerClips(out List<string> clipRepairs))
            {
                repairSummaries.AddRange(clipRepairs);
                changed = true;
            }

            changed |= RefreshPlayerPrefabsAfterClipRepair(repairSummaries);
            WriteClipValidationReport();
            return changed;
        }

        private static bool RefreshPlayerPrefabsAfterClipRepair(List<string> repairSummaries)
        {
            bool changed = false;
            string[] prefabPaths =
            {
                CCS_PlayerPrefabConstants.ProductionPlayerPrefabPath,
                CCS_PlayerPrefabConstants.TestHarnessPlayerPrefabPath,
                CCS_PlayerPrefabConstants.LegacyMasterTestPlayerPrefabPath,
            };

            for (int prefabIndex = 0; prefabIndex < prefabPaths.Length; prefabIndex++)
            {
                string prefabPath = prefabPaths[prefabIndex];
                if (!File.Exists(prefabPath))
                {
                    continue;
                }

                GameObject prefabRoot = PrefabUtility.LoadPrefabContents(prefabPath);
                if (prefabRoot == null)
                {
                    continue;
                }

                if (CCS_PlayerVisualAndAnimatorBindingBuilder.TryResolveAuthoritativeAnimator(
                        prefabRoot,
                        out Animator animator)
                    && animator != null)
                {
                    EditorUtility.SetDirty(animator);
                }

                PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath);
                PrefabUtility.UnloadPrefabContents(prefabRoot);
                repairSummaries.Add("Refreshed prefab after clip repair: " + prefabPath);
                changed = true;
            }

            if (changed)
            {
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            return changed;
        }

        private static void WriteClipValidationReport()
        {
            List<CCS_PlayerHumanoidClipValidationReport> reports =
                CCS_PlayerHumanoidAnimationClipValidationUtility.BuildRequiredClipReports();
            string markdown = CCS_PlayerHumanoidAnimationClipValidationUtility.BuildMarkdownClipReport(reports);
            string fullPath = Path.Combine(Directory.GetParent(Application.dataPath).FullName, ClipReportPath);
            string directory = Path.GetDirectoryName(fullPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(fullPath, markdown);
            Debug.Log("[Player Humanoid Clip Repair] Wrote clip validation report: " + fullPath);
        }
    }
}
