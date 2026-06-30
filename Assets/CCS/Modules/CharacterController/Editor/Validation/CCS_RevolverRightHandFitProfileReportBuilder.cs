using System.Collections.Generic;
using System.IO;
using System.Text;
using CCS.Modules.CharacterController;
using CCS.Modules.CharacterController.Local;
using CCS.Modules.Weapons;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_RevolverRightHandFitProfileReportBuilder
// CATEGORY: Modules / CharacterController / Editor / Validation
// PURPOSE: Writes v0.7.10c right-hand revolver fit profile report.
// PLACEMENT: Editor report builder. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-30
// =============================================================================

namespace CCS.Modules.CharacterController.Editor
{
    public static class CCS_RevolverRightHandFitProfileReportBuilder
    {
        public static string WriteReport()
        {
            string reportPath = ResolveReportPath(CCS_CharacterControllerConstants.RevolverRightHandFitProfileReportPath);
            string directory = Path.GetDirectoryName(reportPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            CCS_WeaponAttachmentFitProfile profile = AssetDatabase.LoadAssetAtPath<CCS_WeaponAttachmentFitProfile>(
                CCS_EquipmentConstants.RevolverM1879RightHandEquippedFitPath);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(CCS_PlayerPrefabConstants.NetworkedPlayerPrefabPath);
            string handSocketPath = ResolveNamedTransformPath(prefab, "CCS_HandSocket_Right");

            StringBuilder builder = new StringBuilder();
            builder.AppendLine("# CCS Revolver Right-Hand Fit Profile (v0.7.10c)");
            builder.AppendLine();
            builder.AppendLine("Generated: " + System.DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + " UTC");
            builder.AppendLine();
            builder.AppendLine("## Previous v0.7.10b fit values");
            builder.AppendLine("- Local position: (0.0909511, 0.125923, 0.07095162)");
            builder.AppendLine("- Local Euler: (-49.556, 32.577, 49.983)");
            builder.AppendLine("- Local scale: (1, 1, 1)");
            builder.AppendLine();
            builder.AppendLine("## New v0.7.10c fit values");
            builder.AppendLine("- Local position: (0.099, 0.176, 0.014)");
            builder.AppendLine("- Local Euler: (-48.275, 103.374, 64.828)");
            builder.AppendLine("- Local scale: (1, 1, 1)");
            builder.AppendLine();
            builder.AppendLine("## Right-hand socket path");
            builder.AppendLine(handSocketPath);
            builder.AppendLine();
            builder.AppendLine("## Fit profile");
            builder.AppendLine("- Path: " + CCS_EquipmentConstants.RevolverM1879RightHandEquippedFitPath);
            if (profile != null)
            {
                builder.AppendLine("- Current local position: " + FormatVector3(profile.SocketLocalPosition));
                builder.AppendLine("- Current local Euler: " + FormatVector3(profile.SocketLocalEulerAngles));
                builder.AppendLine("- Current local scale: " + FormatVector3(profile.SocketLocalScale));
                builder.AppendLine("- Source: manual scene alignment on CCS_RightHandRevolverAttachmentOffset");
            }
            builder.AppendLine();
            builder.AppendLine("## Offset parent");
            builder.AppendLine("- Name: " + CCS_EquipmentConstants.RightHandRevolverAttachmentOffsetObjectName);
            builder.AppendLine("- Path: " + handSocketPath + "/" + CCS_EquipmentConstants.RightHandRevolverAttachmentOffsetObjectName);
            builder.AppendLine("- Visual child: CCS_DiagnosticsEquippedVisual or " + CCS_EquipmentConstants.RuntimeEquippedVisualObjectName);
            builder.AppendLine("- Child transform policy: identity local under offset parent");
            builder.AppendLine();
            builder.AppendLine("## Equipment Fit Studio");
            builder.AppendLine("- Loads CCS_RevolverM1879_RightHandEquipped_Fit");
            builder.AppendLine("- Previews on CCS_HandSocket_Right");
            builder.AppendLine("- Captures/saves offset parent values back to profile");
            builder.AppendLine("- Editor menus: Capture / Apply / Reset Right-Hand Preview");
            builder.AppendLine();
            builder.AppendLine("## Confirmations");
            builder.AppendLine("- Gameplay ownership, ammo, damage, fire, pickup remain unchanged.");
            builder.AppendLine("- Equipment Fit Studio retained; Animation Fit Studio absent.");

            if (prefab != null)
            {
                builder.AppendLine();
                builder.AppendLine("## Prefab");
                builder.AppendLine("- Root MonoBehaviour count: " + prefab.GetComponents<MonoBehaviour>().Length);
            }

            File.WriteAllText(reportPath, builder.ToString(), Encoding.UTF8);
            return reportPath;
        }

        private static string ResolveNamedTransformPath(GameObject prefab, string objectName)
        {
            if (prefab == null)
            {
                return "(prefab missing)";
            }

            Transform[] transforms = prefab.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < transforms.Length; i++)
            {
                if (transforms[i] != null && transforms[i].name == objectName)
                {
                    string path = transforms[i].name;
                    Transform current = transforms[i].parent;
                    while (current != null)
                    {
                        path = current.name + "/" + path;
                        current = current.parent;
                    }

                    return path;
                }
            }

            return "(" + objectName + " not found)";
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
