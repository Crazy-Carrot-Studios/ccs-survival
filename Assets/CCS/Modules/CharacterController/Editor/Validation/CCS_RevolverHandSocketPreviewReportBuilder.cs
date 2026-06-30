using System.IO;
using System.Text;
using CCS.Modules.CharacterController;
using CCS.Modules.CharacterController.Local;
using CCS.Modules.Weapons;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_RevolverHandSocketPreviewReportBuilder
// CATEGORY: Modules / CharacterController / Editor / Validation
// PURPOSE: Writes v0.7.10 revolver hand socket preview report.
// PLACEMENT: Editor report builder. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-25
// =============================================================================

namespace CCS.Modules.CharacterController.Editor
{
    public static class CCS_RevolverHandSocketPreviewReportBuilder
    {
        public static string WriteReport()
        {
            string reportPath = ResolveReportPath(CCS_CharacterControllerConstants.RevolverHandSocketPreviewReportPath);
            string directory = Path.GetDirectoryName(reportPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            StringBuilder builder = new StringBuilder();
            builder.AppendLine("# CCS Revolver Hand Socket Preview (v0.7.10)");
            builder.AppendLine();
            builder.AppendLine("Generated: " + System.DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + " UTC");
            builder.AppendLine();
            builder.AppendLine("## Diagnostics manager");
            builder.AppendLine("- Object path: Validation scene / CCS_DiagnosticsManager");
            builder.AppendLine("- Bool: forceRevolverHandSocketPreview (Inspector: Force Revolver Hand Socket Preview)");
            builder.AppendLine("- Default: false");
            builder.AppendLine("- Interface: CCS_IRevolverHandSocketPreviewDebugSource");
            builder.AppendLine("- Property: ForceRevolverHandSocketPreview");
            builder.AppendLine();
            builder.AppendLine("## Right-hand socket path");
            builder.AppendLine("- Socket id: " + CCS_EquipmentConstants.HandSocketRightId);
            builder.AppendLine("- Registry: CCS_EquipmentSocketRegistry on player prefab");
            builder.AppendLine("- Fit profile: CCS_RevolverM1879_RightHandEquipped_Fit");
            builder.AppendLine();
            builder.AppendLine("## Visual attachment strategy");
            builder.AppendLine("- Driver: CCS_PlayerEquipmentVisualController.SetDiagnosticsRevolverHandSocketPreviewActive");
            builder.AppendLine("- Uses shared ShouldShowDiagnosticsEquippedVisualPreview with aim setup pose");
            builder.AppendLine("- ShowEquippedVisual on right-hand socket via equipped fit profile");
            builder.AppendLine("- Visual-only revolver prefab; gameplay components stripped");
            builder.AppendLine();
            builder.AppendLine("## Duplicate preview prevention");
            builder.AppendLine("- Aim setup pose and hand socket preview share one equipped visual instance");
            builder.AppendLine("- When both toggles are true, aim animator follows setup pose only");
            builder.AppendLine("- Equipment visual controller shows at most one equipped revolver visual");
            builder.AppendLine();
            builder.AppendLine("## Relation to Force Revolver Aim Setup Pose");
            builder.AppendLine("- Setup pose: right-hand visual + draw/hold/holster animation");
            builder.AppendLine("- Hand socket preview: right-hand visual only, no forced aim animation");
            builder.AppendLine("- Setup pose wins animation presentation when both are enabled");
            builder.AppendLine();
            builder.AppendLine("## Confirmations");
            builder.AppendLine("- Gameplay ownership, ammo, damage, fire, and pickup remain unchanged.");
            builder.AppendLine("- No new Animator layers or animation states.");
            builder.AppendLine("- CapsuleVisual and VisualGlasses remain removed from production player prefab.");
            builder.AppendLine("- Equipment Fit Studio retained; Animation Fit Studio absent.");
            builder.AppendLine();
            builder.AppendLine("## Known limitations");
            builder.AppendLine("- Validation scene only.");
            builder.AppendLine("- Socket offset may require manual tuning.");
            builder.AppendLine("- Future animation/IK refinement may be needed.");

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(CCS_PlayerPrefabConstants.NetworkedPlayerPrefabPath);
            if (prefab != null)
            {
                builder.AppendLine();
                builder.AppendLine("## Prefab");
                builder.AppendLine("- Root MonoBehaviour count: " + prefab.GetComponents<MonoBehaviour>().Length);
            }

            File.WriteAllText(reportPath, builder.ToString(), Encoding.UTF8);
            return reportPath;
        }

        private static string ResolveReportPath(string relativePath)
        {
            string projectRoot = Directory.GetParent(Application.dataPath).FullName;
            return Path.Combine(projectRoot, relativePath.Replace('/', Path.DirectorySeparatorChar));
        }
    }
}
