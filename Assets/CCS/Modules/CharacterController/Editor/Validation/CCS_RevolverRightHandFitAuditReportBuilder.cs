using System.Collections.Generic;
using System.IO;
using System.Text;
using CCS.Modules.CharacterController;
using CCS.Modules.CharacterController.Local;
using CCS.Modules.Weapons;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_RevolverRightHandFitAuditReportBuilder
// CATEGORY: Modules / CharacterController / Editor / Validation
// PURPOSE: Audits right-hand revolver fit profile and preview hierarchy for v0.7.10b.
// PLACEMENT: Editor report builder. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-30
// =============================================================================

namespace CCS.Modules.CharacterController.Editor
{
    public static class CCS_RevolverRightHandFitAuditReportBuilder
    {
        public static string WriteReport()
        {
            string reportPath = ResolveReportPath(CCS_CharacterControllerConstants.RevolverRightHandFitAuditReportPath);
            string directory = Path.GetDirectoryName(reportPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            CCS_WeaponAttachmentFitProfile profile = AssetDatabase.LoadAssetAtPath<CCS_WeaponAttachmentFitProfile>(
                CCS_EquipmentConstants.RevolverM1879RightHandEquippedFitPath);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(CCS_PlayerPrefabConstants.NetworkedPlayerPrefabPath);
            string handSocketPath = ResolveHandSocketPath(prefab);

            StringBuilder builder = new StringBuilder();
            builder.AppendLine("# CCS Revolver Right-Hand Fit Audit (v0.7.10b)");
            builder.AppendLine();
            builder.AppendLine("Generated: " + System.DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + " UTC");
            builder.AppendLine();
            builder.AppendLine("## Fit profile");
            builder.AppendLine("- Asset path: " + CCS_EquipmentConstants.RevolverM1879RightHandEquippedFitPath);
            if (profile != null)
            {
                builder.AppendLine("- Profile local position: " + FormatVector3(profile.SocketLocalPosition));
                builder.AppendLine("- Profile local Euler: " + FormatVector3(profile.SocketLocalEulerAngles));
                builder.AppendLine("- Profile local scale: " + FormatVector3(profile.SocketLocalScale));
            }
            else
            {
                builder.AppendLine("- Profile: missing");
            }

            builder.AppendLine();
            builder.AppendLine("## Preview hierarchy");
            builder.AppendLine("- Socket path: " + handSocketPath);
            builder.AppendLine(
                "- Offset parent name: "
                + CCS_EquipmentConstants.RightHandRevolverAttachmentOffsetObjectName);
            builder.AppendLine("- Visual child wrapper: CCS_DiagnosticsEquippedVisual (diagnostics) / "
                + CCS_EquipmentConstants.RuntimeEquippedVisualObjectName + " (gameplay)");
            builder.AppendLine("- Visual prefab: PF_CCS_RevolverM1879_VisualOnly");
            builder.AppendLine("- Expected visual child local transform: identity under offset parent");
            builder.AppendLine();
            builder.AppendLine("## Source of truth");
            builder.AppendLine("- ScriptableObject fit profile stores socket-local offset values.");
            builder.AppendLine("- Applicator computes offset parent locals from profile + socket definition baseline.");
            builder.AppendLine("- Runtime scripts do not hardcode final offset values.");
            builder.AppendLine("- Offset is applied on "
                + CCS_EquipmentConstants.RightHandRevolverAttachmentOffsetObjectName
                + ", not on nested RevolverVisual.");

            File.WriteAllText(reportPath, builder.ToString(), Encoding.UTF8);
            return reportPath;
        }

        private static string ResolveHandSocketPath(GameObject prefab)
        {
            if (prefab == null)
            {
                return "(prefab missing)";
            }

            Transform[] transforms = prefab.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < transforms.Length; i++)
            {
                Transform candidate = transforms[i];
                if (candidate != null && candidate.name == "CCS_HandSocket_Right")
                {
                    return BuildTransformPath(candidate);
                }
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

        private static string FormatVector3(Vector3 value)
        {
            return "(" + value.x.ToString("0.######") + ", " + value.y.ToString("0.######") + ", " + value.z.ToString("0.######") + ")";
        }

        private static string ResolveReportPath(string relativePath)
        {
            string projectRoot = Directory.GetParent(Application.dataPath).FullName;
            return Path.Combine(projectRoot, relativePath.Replace('/', Path.DirectorySeparatorChar));
        }
    }
}
