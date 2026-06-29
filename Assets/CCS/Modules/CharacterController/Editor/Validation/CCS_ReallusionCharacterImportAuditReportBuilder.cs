using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CCS.Project;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_ReallusionCharacterImportAuditReportBuilder
// CATEGORY: Modules / CharacterController / Editor / Validation
// PURPOSE: Writes v0.7.6 Reallusion character import audit report to Logs.
// PLACEMENT: Editor batch utility. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-25
// =============================================================================

namespace CCS.Modules.CharacterController.Editor
{
    public static class CCS_ReallusionCharacterImportAuditReportBuilder
    {
        public const string ReportRelativePath =
            "Logs/CharacterController/PlayerVisualSwap/CCS_ReallusionCharacterImportAudit_v0.7.6.md";

        public static string WriteReport()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("# CCS Reallusion Character Import Audit (v0.7.6)");
            builder.AppendLine();
            builder.AppendLine("Generated: " + System.DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + " UTC");
            builder.AppendLine();

            AppendCharacterSection(builder, "Kevin", CCS_CharacterControllerConstants.KevinImportPrefabPath, CCS_CharacterControllerConstants.KevinFbxPath);
            AppendImportRootSection(builder, "EnemyAI (not wired)", CCS_CharacterControllerConstants.EnemyAiImportRootPath);
            AppendImportRootSection(builder, "Camila (not wired)", CCS_CharacterControllerConstants.CamilaImportRootPath);

            string reportPath = ResolveReportPath();
            Directory.CreateDirectory(Path.GetDirectoryName(reportPath) ?? string.Empty);
            File.WriteAllText(reportPath, builder.ToString(), Encoding.UTF8);
            Debug.Log("[Reallusion Import Audit] Wrote report to " + reportPath);
            return reportPath;
        }

        private static void AppendCharacterSection(
            StringBuilder builder,
            string label,
            string prefabPath,
            string fbxPath)
        {
            builder.AppendLine("## " + label);
            builder.AppendLine();
            builder.AppendLine("- Import prefab: `" + prefabPath + "` — exists: " + File.Exists(prefabPath));
            builder.AppendLine("- FBX/model: `" + fbxPath + "` — exists: " + File.Exists(fbxPath));

            if (File.Exists(fbxPath))
            {
                ModelImporter importer = AssetImporter.GetAtPath(fbxPath) as ModelImporter;
                if (importer != null)
                {
                    builder.AppendLine("- FBX animationType: " + importer.animationType);
                    builder.AppendLine("- FBX avatarSetup: " + importer.avatarSetup);
                }

                Avatar avatar = AssetDatabase.LoadAllAssetsAtPath(fbxPath).OfType<Avatar>().FirstOrDefault();
                builder.AppendLine("- Humanoid avatar valid: " + (avatar != null && avatar.isValid && avatar.isHuman));
            }

            string importFolder = Path.GetDirectoryName(fbxPath)?.Replace('\\', '/');
            if (!string.IsNullOrEmpty(importFolder) && Directory.Exists(importFolder))
            {
                string[] prefabs = Directory.GetFiles(importFolder, "*.prefab", SearchOption.AllDirectories);
                string[] materials = Directory.GetFiles(importFolder, "*.mat", SearchOption.AllDirectories);
                string[] textures = Directory.GetFiles(importFolder, "*.png", SearchOption.AllDirectories)
                    .Concat(Directory.GetFiles(importFolder, "*.jpg", SearchOption.AllDirectories))
                    .Concat(Directory.GetFiles(importFolder, "*.tga", SearchOption.AllDirectories))
                    .ToArray();

                builder.AppendLine("- Prefab assets: " + prefabs.Length);
                for (int i = 0; i < prefabs.Length && i < 8; i++)
                {
                    builder.AppendLine("  - `" + prefabs[i].Replace('\\', '/') + "`");
                }

                builder.AppendLine("- Material count: " + materials.Length);
                builder.AppendLine("- Texture files: " + textures.Length);
            }

            GameObject prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefabAsset != null)
            {
                GameObject instance = PrefabUtility.InstantiatePrefab(prefabAsset) as GameObject;
                try
                {
                    Animator animator = instance != null ? instance.GetComponentInChildren<Animator>(true) : null;
                    builder.AppendLine("- Animator on import prefab: " + (animator != null));
                    builder.AppendLine("- RigBuilder on import prefab: " + (instance != null && instance.GetComponentInChildren<UnityEngine.Animations.Rigging.RigBuilder>(true) != null));

                    if (instance != null)
                    {
                        SkinnedMeshRenderer[] skinned = instance.GetComponentsInChildren<SkinnedMeshRenderer>(true);
                        MeshRenderer[] staticMeshes = instance.GetComponentsInChildren<MeshRenderer>(true);
                        builder.AppendLine("- SkinnedMeshRenderer count: " + skinned.Length);
                        builder.AppendLine("- MeshRenderer count: " + staticMeshes.Length);

                        MonoBehaviour[] behaviours = instance.GetComponentsInChildren<MonoBehaviour>(true);
                        int reallusionScripts = behaviours.Count(behaviour =>
                            behaviour != null && behaviour.GetType().Namespace != null
                            && behaviour.GetType().Namespace.StartsWith("Reallusion"));
                        builder.AppendLine("- Reallusion scripts attached: " + reallusionScripts);
                    }

                    RuntimeAnimatorController locomotionController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(
                        CCS_CharacterControllerConstants.PlayerLocomotionAnimatorControllerPath);
                    builder.AppendLine("- Locomotion-only controller available: " + (locomotionController != null));
                    builder.AppendLine(
                        "- Kevin can use locomotion-only controller: "
                        + (avatarIsHumanoid(fbxPath) && locomotionController != null));
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

        private static bool avatarIsHumanoid(string fbxPath)
        {
            Avatar avatar = AssetDatabase.LoadAllAssetsAtPath(fbxPath).OfType<Avatar>().FirstOrDefault();
            return avatar != null && avatar.isValid && avatar.isHuman;
        }

        private static void AppendImportRootSection(StringBuilder builder, string label, string rootPath)
        {
            builder.AppendLine("## " + label);
            builder.AppendLine();
            builder.AppendLine("- Import root: `" + rootPath + "` — exists: " + Directory.Exists(rootPath));
            if (!Directory.Exists(rootPath))
            {
                builder.AppendLine();
                return;
            }

            string[] prefabs = Directory.GetFiles(rootPath, "*.prefab", SearchOption.AllDirectories);
            string[] fbxFiles = Directory.GetFiles(rootPath, "*.fbx", SearchOption.AllDirectories);
            string[] materials = Directory.GetFiles(rootPath, "*.mat", SearchOption.AllDirectories);
            builder.AppendLine("- Prefab assets: " + prefabs.Length);
            builder.AppendLine("- FBX assets: " + fbxFiles.Length);
            builder.AppendLine("- Material count: " + materials.Length);
            builder.AppendLine("- Wired in production: **no** (v0.7.6)");
            builder.AppendLine();
        }

        private static string ResolveReportPath()
        {
            string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            return Path.GetFullPath(Path.Combine(projectRoot, ReportRelativePath.Replace('/', Path.DirectorySeparatorChar)));
        }
    }
}
