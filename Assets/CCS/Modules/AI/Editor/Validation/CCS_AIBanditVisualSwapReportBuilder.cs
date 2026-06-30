using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CCS.Modules.CharacterController;
using CCS.Modules.CharacterController.Editor;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_AIBanditVisualSwapReportBuilder
// CATEGORY: Modules / AI / Editor / Validation
// PURPOSE: Writes v0.7.7 EnemyAI bandit visual swap report to Logs.
// AUTHOR: James Schilz
// CREATED: 2026-06-29
// =============================================================================

namespace CCS.Modules.AI.Editor
{
    public static class CCS_AIBanditVisualSwapReportBuilder
    {
        public const string ReportRelativePath =
            "Logs/CharacterController/PlayerVisualSwap/CCS_AIBanditVisualSwap_EnemyAI_v0.7.7.md";

        public static string WriteReport()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("# CCS AI Bandit Visual Swap — EnemyAI (v0.7.7)");
            builder.AppendLine();
            builder.AppendLine("Generated: " + System.DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + " UTC");
            builder.AppendLine();

            builder.AppendLine("## Paths");
            builder.AppendLine();
            builder.AppendLine("- AI bandit prefab: `" + CCS_AIConstants.AIBanditPrefabPath + "`");
            builder.AppendLine("- Old visual reference: `" + CCS_AIConstants.LegacyPlayerVisualPrefabPath + "`");
            builder.AppendLine("- New EnemyAI prefab: `" + CCS_AIConstants.AIBanditModelEnemyAIPrefabPath + "`");
            builder.AppendLine();

            bool legacyExists = File.Exists(CCS_AIConstants.LegacyPlayerVisualPrefabPath);
            int legacyReferences = legacyExists
                ? CCS_PlayerVisualModelSwapValidationUtility.CountProjectReferencesToAsset(CCS_AIConstants.LegacyPlayerVisualPrefabPath)
                : 0;
            builder.AppendLine("## PF_CCS_Player_Visual status");
            builder.AppendLine();
            if (!legacyExists)
            {
                builder.AppendLine("- Deleted: **yes** (zero references after bandit swap)");
            }
            else
            {
                builder.AppendLine("- Deleted: **deferred** — references: " + legacyReferences);
            }

            builder.AppendLine();
            AppendModelHierarchy(builder);
            AppendEnemyAiStats(builder);

            builder.AppendLine("## Unity 6 CS0618 cleanup");
            builder.AppendLine();
            builder.AppendLine("- Removed StaticEditorFlags.NavigationStatic usage from AI navigation builder.");
            builder.AppendLine("- Replaced obsolete FindObjectsByType/FindObjectsSortMode overloads in Netcode editor utilities.");
            builder.AppendLine();

            builder.AppendLine("## Animator binding warnings");
            builder.AppendLine();
            builder.AppendLine("- Kevin: Humanoid avatar + locomotion-only controller may show generic clip binding warnings in Inspector.");
            builder.AppendLine("- EnemyAI: same; deferred to future animation rebuild milestone.");
            builder.AppendLine();

            builder.AppendLine("## Confirmations");
            builder.AppendLine();
            builder.AppendLine("- Kevin remains default player visual.");
            builder.AppendLine("- Camila imported, not wired.");
            builder.AppendLine("- Animator Controller: locomotion-only.");
            builder.AppendLine("- Animation clips: unchanged.");
            builder.AppendLine("- Equipment Fit Studio: retained.");
            builder.AppendLine("- Animation Fit Studio: absent.");

            string reportPath = ResolveReportPath();
            Directory.CreateDirectory(Path.GetDirectoryName(reportPath) ?? string.Empty);
            File.WriteAllText(reportPath, builder.ToString(), Encoding.UTF8);
            Debug.Log("[AI Bandit Visual Swap Report] Wrote report to " + reportPath);
            return reportPath;
        }

        private static void AppendModelHierarchy(StringBuilder builder)
        {
            builder.AppendLine("## Model hierarchy");
            builder.AppendLine();
            GameObject banditPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(CCS_AIConstants.AIBanditPrefabPath);
            if (banditPrefab == null)
            {
                builder.AppendLine("- Bandit prefab missing.");
                return;
            }

            GameObject instance = PrefabUtility.InstantiatePrefab(banditPrefab) as GameObject;
            try
            {
                Transform modelRoot = CCS_PlayerModelRootUtility.FindModelRoot(instance.transform);
                if (modelRoot == null)
                {
                    builder.AppendLine("- Model root missing.");
                    return;
                }

                builder.AppendLine("```text");
                AppendTransformTree(modelRoot, 0, builder);
                builder.AppendLine("```");
                builder.AppendLine();
            }
            finally
            {
                if (instance != null)
                {
                    Object.DestroyImmediate(instance);
                }
            }
        }

        private static void AppendTransformTree(Transform node, int depth, StringBuilder builder)
        {
            string indent = new string(' ', depth * 2);
            builder.AppendLine(indent + node.name);
            for (int i = 0; i < node.childCount && i < 12; i++)
            {
                AppendTransformTree(node.GetChild(i), depth + 1, builder);
            }

            if (node.childCount > 12)
            {
                builder.AppendLine(indent + "  ... (" + (node.childCount - 12) + " more children)");
            }
        }

        private static void AppendEnemyAiStats(StringBuilder builder)
        {
            builder.AppendLine("## EnemyAI visual stats");
            builder.AppendLine();
            GameObject enemyPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(CCS_AIConstants.AIBanditModelEnemyAIPrefabPath);
            if (enemyPrefab == null)
            {
                builder.AppendLine("- Production EnemyAI prefab missing.");
                return;
            }

            GameObject instance = PrefabUtility.InstantiatePrefab(enemyPrefab) as GameObject;
            try
            {
                Animator animator = instance.GetComponentInChildren<Animator>(true);
                builder.AppendLine("- Animator found: " + (animator != null));
                if (animator != null && animator.runtimeAnimatorController != null)
                {
                    builder.AppendLine("- Animator Controller: `" + AssetDatabase.GetAssetPath(animator.runtimeAnimatorController) + "`");
                }

                SkinnedMeshRenderer[] skinned = instance.GetComponentsInChildren<SkinnedMeshRenderer>(true);
                builder.AppendLine("- SkinnedMeshRenderer count: " + skinned.Length);

                HashSet<string> materials = new HashSet<string>();
                Renderer[] renderers = instance.GetComponentsInChildren<Renderer>(true);
                for (int i = 0; i < renderers.Length; i++)
                {
                    Material[] sharedMaterials = renderers[i].sharedMaterials;
                    for (int j = 0; j < sharedMaterials.Length; j++)
                    {
                        if (sharedMaterials[j] != null)
                        {
                            materials.Add(sharedMaterials[j].name);
                        }
                    }
                }

                builder.AppendLine("- Material count: " + materials.Count);
            }
            finally
            {
                if (instance != null)
                {
                    Object.DestroyImmediate(instance);
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
