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
// PURPOSE: Writes v0.7.10a revolver hand socket preview hotfix report.
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

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(CCS_PlayerPrefabConstants.NetworkedPlayerPrefabPath);
            string rightHandSocketPath = ResolveRightHandSocketPath(prefab);

            StringBuilder builder = new StringBuilder();
            builder.AppendLine("# CCS Revolver Hand Socket Preview (v0.7.10a)");
            builder.AppendLine();
            builder.AppendLine("Generated: " + System.DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + " UTC");
            builder.AppendLine();
            builder.AppendLine("## v0.7.10 smoke failure");
            builder.AppendLine("- Diagnostics manager resolved the first `CCS_RevolverController` in the scene.");
            builder.AppendLine("- Validation scene also spawns an AI bandit with `CCS_RevolverController` but without `CCS_PlayerEquipmentVisualController`.");
            builder.AppendLine("- Hand socket preview calls never reached the player equipment visual controller.");
            builder.AppendLine("- Preview also reused gameplay equipped visual state without a dedicated diagnostics instance.");
            builder.AppendLine();
            builder.AppendLine("## Hotfix");
            builder.AppendLine("- Diagnostics manager now resolves `CCS_PlayerEquipmentVisualController` directly on the player.");
            builder.AppendLine("- Diagnostics preview uses a dedicated visual instance parented to `CCS_HandSocket_Right`.");
            builder.AppendLine("- Toggle-change logging reports socket path, visual source, fit profile, and final local transform.");
            builder.AppendLine();
            builder.AppendLine("## Diagnostics manager");
            builder.AppendLine("- Object path: Validation scene / CCS_DiagnosticsManager");
            builder.AppendLine("- Bool: forceRevolverHandSocketPreview (Inspector: Force Revolver Hand Socket Preview)");
            builder.AppendLine("- Default: false");
            builder.AppendLine("- Interface: CCS_IRevolverHandSocketPreviewDebugSource");
            builder.AppendLine("- Property: ForceRevolverHandSocketPreview");
            builder.AppendLine();
            builder.AppendLine("## Fixed preview attachment parent");
            builder.AppendLine("- Socket id: " + CCS_EquipmentConstants.HandSocketRightId);
            builder.AppendLine("- Final parent path: " + rightHandSocketPath + "/CCS_DiagnosticsEquippedAttachmentRoot");
            builder.AppendLine("- Visual child: CCS_DiagnosticsEquippedVisual");
            builder.AppendLine();
            builder.AppendLine("## Visual source / fit profile");
            builder.AppendLine("- Visual prefab: PF_CCS_RevolverM1879_VisualOnly");
            builder.AppendLine("- Fit profile: CCS_RevolverM1879_RightHandEquipped_Fit");
            builder.AppendLine("- Gameplay components stripped on diagnostics instance only");
            builder.AppendLine();
            builder.AppendLine("## Socket vs IK target");
            builder.AppendLine("- **Attach weapon here:** CCS_HandSocket_Right (equipment socket anchor)");
            builder.AppendLine("- **Do not attach weapon:** CCS_RightHandIKTarget, CCS_WeaponAimTarget, MuzzlePoint, CCS_WeaponIKTargets");
            builder.AppendLine("- **WeaponRoot:** organization root only");
            builder.AppendLine("- See also: " + CCS_CharacterControllerConstants.RevolverSocketAndIKAuditReportPath);
            builder.AppendLine();
            builder.AppendLine("## Duplicate preview prevention");
            builder.AppendLine("- Aim setup pose and hand socket preview share one diagnostics equipped visual instance.");
            builder.AppendLine("- Gameplay equipped visual is suppressed while diagnostics preview is active.");
            builder.AppendLine("- Real gameplay equipped visual is restored when preview turns off and the player owns the revolver.");
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

            if (prefab != null)
            {
                builder.AppendLine();
                builder.AppendLine("## Prefab");
                builder.AppendLine("- Root MonoBehaviour count: " + prefab.GetComponents<MonoBehaviour>().Length);
            }

            File.WriteAllText(reportPath, builder.ToString(), Encoding.UTF8);
            return reportPath;
        }

        private static string ResolveRightHandSocketPath(GameObject prefab)
        {
            if (prefab == null)
            {
                return "(prefab missing)";
            }

            Transform[] transforms = prefab.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < transforms.Length; i++)
            {
                Transform candidate = transforms[i];
                if (candidate == null || candidate.name != "CCS_HandSocket_Right")
                {
                    continue;
                }

                return BuildTransformPath(candidate);
            }

            return "(CCS_HandSocket_Right not found)";
        }

        private static string BuildTransformPath(Transform transform)
        {
            string path = transform.name;
            Transform current = transform.parent;
            while (current != null)
            {
                path = current.name + "/" + path;
                current = current.parent;
            }

            return path;
        }

        private static string ResolveReportPath(string relativePath)
        {
            string projectRoot = Directory.GetParent(Application.dataPath).FullName;
            return Path.Combine(projectRoot, relativePath.Replace('/', Path.DirectorySeparatorChar));
        }
    }
}
