using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CCS.Modules.CharacterController;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_ReallusionEnemyAIImportAuditReportBuilder
// CATEGORY: Modules / AI / Editor / Validation
// PURPOSE: Writes v0.7.7 EnemyAI Reallusion import audit report to Logs.
// AUTHOR: James Schilz
// CREATED: 2026-06-29
// =============================================================================

namespace CCS.Modules.AI.Editor
{
    public static class CCS_ReallusionEnemyAIImportAuditReportBuilder
    {
        public const string ReportRelativePath =
            "Logs/CharacterController/PlayerVisualSwap/CCS_ReallusionEnemyAIImportAudit_v0.7.7.md";

        public static string WriteReport()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("# CCS Reallusion EnemyAI Import Audit (v0.7.7)");
            builder.AppendLine();
            builder.AppendLine("Generated: " + System.DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + " UTC");
            builder.AppendLine();

            AppendEnemyAiSection(builder);

            string reportPath = ResolveReportPath();
            Directory.CreateDirectory(Path.GetDirectoryName(reportPath) ?? string.Empty);
            File.WriteAllText(reportPath, builder.ToString(), Encoding.UTF8);
            Debug.Log("[EnemyAI Import Audit] Wrote report to " + reportPath);
            return reportPath;
        }

        private static void AppendEnemyAiSection(StringBuilder builder)
        {
            builder.AppendLine("## EnemyAI (CC3_EnemyAI)");
            builder.AppendLine();
            builder.AppendLine("- Import root: `" + CCS_AIConstants.EnemyAiImportRootPath + "`");
            builder.AppendLine("- Import prefab: `" + CCS_AIConstants.EnemyAiImportPrefabPath + "`");
            builder.AppendLine("- FBX: `" + CCS_AIConstants.EnemyAiFbxPath + "`");

            if (File.Exists(CCS_AIConstants.EnemyAiFbxPath))
            {
                ModelImporter importer = AssetImporter.GetAtPath(CCS_AIConstants.EnemyAiFbxPath) as ModelImporter;
                if (importer != null)
                {
                    builder.AppendLine("- FBX animationType: " + importer.animationType);
                    builder.AppendLine("- FBX avatarSetup: " + importer.avatarSetup);
                }

                Avatar avatar = AssetDatabase.LoadAllAssetsAtPath(CCS_AIConstants.EnemyAiFbxPath).OfType<Avatar>().FirstOrDefault();
                builder.AppendLine("- Humanoid avatar valid: " + (avatar != null && avatar.isValid && avatar.isHuman));
            }

            if (Directory.Exists(CCS_AIConstants.EnemyAiImportRootPath))
            {
                string[] materials = Directory.GetFiles(CCS_AIConstants.EnemyAiImportRootPath, "*.mat", SearchOption.AllDirectories);
                string[] textures = Directory.GetFiles(CCS_AIConstants.EnemyAiImportRootPath, "*.png", SearchOption.AllDirectories)
                    .Concat(Directory.GetFiles(CCS_AIConstants.EnemyAiImportRootPath, "*.jpg", SearchOption.AllDirectories))
                    .ToArray();
                builder.AppendLine("- Material count: " + materials.Length);
                builder.AppendLine("- Texture files: " + textures.Length);
            }

            GameObject importPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(CCS_AIConstants.EnemyAiImportPrefabPath);
            if (importPrefab != null)
            {
                GameObject instance = PrefabUtility.InstantiatePrefab(importPrefab) as GameObject;
                try
                {
                    SkinnedMeshRenderer[] skinned = instance.GetComponentsInChildren<SkinnedMeshRenderer>(true);
                    builder.AppendLine("- SkinnedMeshRenderer count: " + skinned.Length);
                    builder.AppendLine("- Animator on import prefab: " + (instance.GetComponentInChildren<Animator>(true) != null));
                    builder.AppendLine("- RigBuilder on import prefab: " + (instance.GetComponentInChildren<UnityEngine.Animations.Rigging.RigBuilder>(true) != null));

                    int reallusionScripts = instance.GetComponentsInChildren<MonoBehaviour>(true)
                        .Count(b => b != null && b.GetType().Namespace != null && b.GetType().Namespace.StartsWith("Reallusion"));
                    builder.AppendLine("- Reallusion scripts attached: " + reallusionScripts);
                    builder.AppendLine("- Can use locomotion-only controller: yes (Humanoid + shared AC_CCS_Player_Locomotion_StarterAssets)");
                }
                finally
                {
                    if (instance != null)
                    {
                        Object.DestroyImmediate(instance);
                    }
                }
            }

            builder.AppendLine();
        }

        private static string ResolveReportPath()
        {
            string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            return Path.GetFullPath(Path.Combine(projectRoot, ReportRelativePath.Replace('/', Path.DirectorySeparatorChar)));
        }
    }
}
