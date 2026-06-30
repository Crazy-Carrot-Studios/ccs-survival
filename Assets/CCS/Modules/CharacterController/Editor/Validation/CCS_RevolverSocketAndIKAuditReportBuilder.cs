using System.Collections.Generic;
using System.IO;
using System.Text;
using CCS.Modules.CharacterController;
using CCS.Modules.CharacterController.Local;
using CCS.Modules.Weapons;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_RevolverSocketAndIKAuditReportBuilder
// CATEGORY: Modules / CharacterController / Editor / Validation
// PURPOSE: Audits player prefab socket and IK hierarchy for v0.7.10a hotfix.
// PLACEMENT: Editor report builder. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-30
// =============================================================================

namespace CCS.Modules.CharacterController.Editor
{
    public static class CCS_RevolverSocketAndIKAuditReportBuilder
    {
        private struct HierarchyAuditRow
        {
            public string ObjectName;
            public string Purpose;
            public string Path;
            public string WeaponAttach;
        }

        public static string WriteReport()
        {
            string reportPath = ResolveReportPath(CCS_CharacterControllerConstants.RevolverSocketAndIKAuditReportPath);
            string directory = Path.GetDirectoryName(reportPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(CCS_PlayerPrefabConstants.NetworkedPlayerPrefabPath);
            List<HierarchyAuditRow> rows = BuildRows(prefab);

            StringBuilder builder = new StringBuilder();
            builder.AppendLine("# CCS Revolver Socket and IK Audit (v0.7.10a)");
            builder.AppendLine();
            builder.AppendLine("Generated: " + System.DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + " UTC");
            builder.AppendLine();
            builder.AppendLine("Prefab: " + CCS_PlayerPrefabConstants.NetworkedPlayerPrefabPath);
            builder.AppendLine();
            builder.AppendLine("| Object | Purpose | Parent/Path | Should weapon attach here? |");
            builder.AppendLine("| ------ | ------- | ----------- | -------------------------- |");

            for (int i = 0; i < rows.Count; i++)
            {
                HierarchyAuditRow row = rows[i];
                builder.AppendLine(
                    "| "
                    + row.ObjectName
                    + " | "
                    + row.Purpose
                    + " | "
                    + row.Path
                    + " | "
                    + row.WeaponAttach
                    + " |");
            }

            builder.AppendLine();
            builder.AppendLine("## Definitions");
            builder.AppendLine("- **CCS_HandSocket_Right** = equipment attach socket. Diagnostics and equipped revolver visuals attach here.");
            builder.AppendLine("- **CCS_RightHandIKTarget** = IK target only. Do not parent weapon visuals here.");
            builder.AppendLine("- **CCS_LeftHandIKTarget / elbow hints** = IK only.");
            builder.AppendLine("- **CCS_WeaponAimTarget** = IK/aim target only.");
            builder.AppendLine("- **MuzzlePoint** = fire origin / feedback point, not hand attachment.");
            builder.AppendLine("- **WeaponRoot** = visual organization root / optional pool parent, not final hand socket.");

            File.WriteAllText(reportPath, builder.ToString(), Encoding.UTF8);
            return reportPath;
        }

        private static List<HierarchyAuditRow> BuildRows(GameObject prefab)
        {
            List<HierarchyAuditRow> rows = new List<HierarchyAuditRow>();
            if (prefab == null)
            {
                rows.Add(new HierarchyAuditRow
                {
                    ObjectName = "(missing prefab)",
                    Purpose = "Networked player prefab",
                    Path = CCS_PlayerPrefabConstants.NetworkedPlayerPrefabPath,
                    WeaponAttach = "N/A",
                });
                return rows;
            }

            AddRow(rows, prefab.transform, "Model", "Player visual and weapon hierarchy root", "No");
            AddRow(rows, prefab.transform, "WeaponRoot", "Visual organization root / optional pool parent", "No");
            AddRow(rows, prefab.transform, CCS_EquipmentConstants.WeaponIkTargetsObjectName, "IK target container", "No");
            AddRow(rows, prefab.transform, "CCS_RevolverArmReticleIKRoot", "Arm-to-reticle IK rig root", "No");
            AddRow(rows, prefab.transform, "PF_CCS_Player_Model_Kevin", "Kevin humanoid visual model", "No");
            AddRow(rows, prefab.transform, "CCS_HandSocket_Right", "Right-hand equipment attach socket", "Yes");
            AddRow(rows, prefab.transform, "CCS_HandSocket_Left", "Left-hand equipment attach socket", "No (left hand)");
            AddRow(rows, prefab.transform, "CCS_HolsterSocket_RightHip", "Right hip holster equipment socket", "Holster only");
            AddRow(rows, prefab.transform, "CCS_HolsterSocket_LeftHip", "Left hip holster equipment socket", "Holster only");
            AddRow(rows, prefab.transform, CCS_EquipmentConstants.RightHandIkTargetObjectName, "Right-hand IK target", "No");
            AddRow(rows, prefab.transform, CCS_EquipmentConstants.WeaponAimTargetObjectName, "Weapon aim IK target", "No");
            AddRow(rows, prefab.transform, "MuzzlePoint", "Fire origin / feedback point", "No");

            return rows;
        }

        private static void AddRow(
            List<HierarchyAuditRow> rows,
            Transform prefabRoot,
            string objectName,
            string purpose,
            string weaponAttach)
        {
            Transform target = FindChildByName(prefabRoot, objectName);
            rows.Add(new HierarchyAuditRow
            {
                ObjectName = objectName,
                Purpose = purpose,
                Path = target != null ? BuildTransformPath(target) : "(not found)",
                WeaponAttach = weaponAttach,
            });
        }

        private static Transform FindChildByName(Transform root, string objectName)
        {
            if (root == null)
            {
                return null;
            }

            if (root.name == objectName)
            {
                return root;
            }

            Transform[] transforms = root.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < transforms.Length; i++)
            {
                if (transforms[i] != null && transforms[i].name == objectName)
                {
                    return transforms[i];
                }
            }

            return null;
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
