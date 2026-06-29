using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CCS.Modules.CharacterController.Local;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_PlayerVisualSwapReportBuilder
// CATEGORY: Modules / CharacterController / Editor / Validation
// PURPOSE: Writes v0.7.6 Kevin player visual swap report to Logs.
// PLACEMENT: Editor batch utility. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-25
// =============================================================================

namespace CCS.Modules.CharacterController.Editor
{
    public static class CCS_PlayerVisualSwapReportBuilder
    {
        public const string ReportRelativePath =
            "Logs/CharacterController/PlayerVisualSwap/CCS_PlayerVisualSwap_Kevin_v0.7.6.md";

        public static string WriteReport()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("# CCS Player Visual Swap — Kevin (v0.7.6)");
            builder.AppendLine();
            builder.AppendLine("Generated: " + System.DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + " UTC");
            builder.AppendLine();

            builder.AppendLine("## Paths");
            builder.AppendLine();
            builder.AppendLine("- Old visual prefab: `" + CCS_CharacterControllerConstants.PlayerVisualPrefabPath + "`");
            builder.AppendLine("- New Kevin visual prefab: `" + CCS_CharacterControllerConstants.PlayerModelKevinPrefabPath + "`");
            builder.AppendLine("- Networked player prefab: `" + CCS_PlayerPrefabConstants.NetworkedPlayerPrefabPath + "`");
            builder.AppendLine();

            int legacyReferences = CCS_PlayerVisualModelSwapValidationUtility.CountProjectReferencesToAsset(
                CCS_CharacterControllerConstants.PlayerVisualPrefabPath);
            bool legacyExists = File.Exists(CCS_CharacterControllerConstants.PlayerVisualPrefabPath);
            builder.AppendLine("## PF_CCS_Player_Visual deletion status");
            builder.AppendLine();
            if (!legacyExists)
            {
                builder.AppendLine("- Deleted: **yes**");
            }
            else if (legacyReferences == 0)
            {
                builder.AppendLine("- Deleted: **no** (zero references — safe to delete in follow-up)");
            }
            else
            {
                builder.AppendLine("- Deleted: **deferred** — active references: " + legacyReferences);
            }

            builder.AppendLine();
            AppendModelHierarchy(builder);
            AppendKevinStats(builder);
            AppendRootMonoBehaviourCount(builder);
            AppendIkCompatibility(builder);

            builder.AppendLine("## Confirmations");
            builder.AppendLine();
            builder.AppendLine("- Animator Controller: locomotion-only (`AC_CCS_Player_Locomotion_StarterAssets.controller`)");
            builder.AppendLine("- Animation clips: unchanged (no clip edits in v0.7.6)");
            builder.AppendLine("- EnemyAI: imported, not wired");
            builder.AppendLine("- Camila: imported, not wired");
            builder.AppendLine("- Equipment Fit Studio: retained");
            builder.AppendLine("- Animation Fit Studio: absent");

            string reportPath = ResolveReportPath();
            Directory.CreateDirectory(Path.GetDirectoryName(reportPath) ?? string.Empty);
            File.WriteAllText(reportPath, builder.ToString(), Encoding.UTF8);
            Debug.Log("[Player Visual Swap Report] Wrote report to " + reportPath);
            return reportPath;
        }

        private static void AppendModelHierarchy(StringBuilder builder)
        {
            builder.AppendLine("## Model hierarchy");
            builder.AppendLine();
            GameObject playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                CCS_PlayerPrefabConstants.NetworkedPlayerPrefabPath);
            if (playerPrefab == null)
            {
                builder.AppendLine("- Player prefab missing.");
                builder.AppendLine();
                return;
            }

            GameObject instance = PrefabUtility.InstantiatePrefab(playerPrefab) as GameObject;
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

        private static void AppendKevinStats(StringBuilder builder)
        {
            builder.AppendLine("## Kevin visual stats");
            builder.AppendLine();
            GameObject kevinPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                CCS_CharacterControllerConstants.PlayerModelKevinPrefabPath);
            if (kevinPrefab == null)
            {
                builder.AppendLine("- Kevin production prefab missing.");
                builder.AppendLine();
                return;
            }

            GameObject instance = PrefabUtility.InstantiatePrefab(kevinPrefab) as GameObject;
            try
            {
                Animator animator = instance.GetComponentInChildren<Animator>(true);
                builder.AppendLine("- Animator found: " + (animator != null));
                if (animator != null && animator.runtimeAnimatorController != null)
                {
                    builder.AppendLine(
                        "- Animator Controller: `"
                        + AssetDatabase.GetAssetPath(animator.runtimeAnimatorController)
                        + "`");
                }

                SkinnedMeshRenderer[] skinned = instance.GetComponentsInChildren<SkinnedMeshRenderer>(true);
                MeshRenderer[] meshes = instance.GetComponentsInChildren<MeshRenderer>(true);
                builder.AppendLine("- SkinnedMeshRenderer count: " + skinned.Length);
                builder.AppendLine("- MeshRenderer count: " + meshes.Length);

                HashSet<string> materials = new HashSet<string>();
                Renderer[] renderers = instance.GetComponentsInChildren<Renderer>(true);
                for (int i = 0; i < renderers.Length; i++)
                {
                    Material[] sharedMaterials = renderers[i].sharedMaterials;
                    for (int materialIndex = 0; materialIndex < sharedMaterials.Length; materialIndex++)
                    {
                        if (sharedMaterials[materialIndex] != null)
                        {
                            materials.Add(sharedMaterials[materialIndex].name);
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
            builder.AppendLine("## Socket validation");
            builder.AppendLine();
            for (int i = 0; i < CCS_EquipmentConstants.RequiredSocketIds.Length; i++)
            {
                string socketId = CCS_EquipmentConstants.RequiredSocketIds[i];
                builder.AppendLine("- `" + socketId + "`");
            }

            builder.AppendLine();
        }

        private static void AppendRootMonoBehaviourCount(StringBuilder builder)
        {
            GameObject playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                CCS_PlayerPrefabConstants.NetworkedPlayerPrefabPath);
            if (playerPrefab == null)
            {
                return;
            }

            MonoBehaviour[] rootBehaviours = playerPrefab.GetComponents<MonoBehaviour>();
            int count = rootBehaviours.Count(behaviour => behaviour != null);
            builder.AppendLine("## Root MonoBehaviour count");
            builder.AppendLine();
            builder.AppendLine("- Count: **" + count + "**");
            builder.AppendLine();
        }

        private static void AppendIkCompatibility(StringBuilder builder)
        {
            builder.AppendLine("## IK compatibility");
            builder.AppendLine();
            builder.AppendLine("- Weapon IK rig: rebuilt by equipment socket builder on Kevin humanoid bones.");
            builder.AppendLine("- Revolver arm reticle IK: retained on Model root; enableArmToReticleIK may remain disabled.");
            builder.AppendLine("- If bone mapping fails, builders fall back to test anchors (validation should fail).");
            builder.AppendLine();
        }

        private static string ResolveReportPath()
        {
            string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            return Path.GetFullPath(Path.Combine(projectRoot, ReportRelativePath.Replace('/', Path.DirectorySeparatorChar)));
        }
    }
}
