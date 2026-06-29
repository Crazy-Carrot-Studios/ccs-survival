using System.Collections.Generic;
using System.IO;
using System.Text;
using CCS.Modules.CharacterController.Local;
using CCS.Modules.CharacterController.Netcode;
using CCS.Project;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// =============================================================================
// SCRIPT: CCS_MissingScriptScanUtility
// CATEGORY: Modules / CharacterController / Editor / Validation
// PURPOSE: Scans and repairs missing MonoBehaviour script slots on production assets.
// PLACEMENT: Editor validation utility. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-25
// NOTES: Uses GameObjectUtility.GetMonoBehavioursWithMissingScriptCount for detection.
// =============================================================================

namespace CCS.Modules.CharacterController.Editor
{
    public struct MissingScriptReportEntry
    {
        public string AssetPath;
        public string HierarchyPath;
        public string PrefabSourcePath;
        public int MissingCount;
    }

    public static class CCS_MissingScriptScanUtility
    {
        private const string PrototypingEnvironmentPrefabRoot =
            "Assets/CCS/Modules/CharacterController/Prototyping/Prefabs/Environment";

        public static IReadOnlyList<string> ProductionAssetPaths => ProductionAssetPathList;

        private static readonly string[] ProductionAssetPathList =
        {
            CCS_NetcodeConstants.MasterTestScenePath,
            CCS_NetcodeConstants.MultiplayerHostingScenePath,
            CCS_PlayerPrefabConstants.NetworkedPlayerPrefabPath,
            CCS_NetcodeConstants.NetworkManagerPrefabPath,
            CCS_CharacterControllerMasterTestLayoutConstants.NpcPrefabPath,
        };

        public static CCS_SurvivalValidationResult ValidateProductionAssetsHaveNoMissingScripts()
        {
            List<MissingScriptReportEntry> entries = ScanProductionAssets();
            if (entries.Count == 0)
            {
                return CCS_SurvivalValidationResult.Pass(
                    "Production scenes and prefabs contain no missing script slots.");
            }

            return CCS_SurvivalValidationResult.Fail(FormatReport(entries));
        }

        public static List<MissingScriptReportEntry> ScanProductionAssets()
        {
            List<MissingScriptReportEntry> entries = new List<MissingScriptReportEntry>();
            for (int i = 0; i < ProductionAssetPathList.Length; i++)
            {
                ScanAssetAtPath(ProductionAssetPathList[i], entries);
            }

            ScanPrototypingEnvironmentPrefabs(entries);
            return entries;
        }

        public static List<MissingScriptReportEntry> ScanOpenSceneHierarchy(Scene scene)
        {
            List<MissingScriptReportEntry> entries = new List<MissingScriptReportEntry>();
            if (!scene.IsValid() || !scene.isLoaded)
            {
                return entries;
            }

            string scenePath = string.IsNullOrEmpty(scene.path) ? "<unsaved-scene>" : scene.path;
            GameObject[] roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                ScanHierarchyRecursive(roots[i], scenePath, entries);
            }

            return entries;
        }

        public static bool RepairGameObjectHierarchy(
            GameObject root,
            string assetPath,
            out List<MissingScriptReportEntry> removedEntries)
        {
            removedEntries = new List<MissingScriptReportEntry>();
            if (root == null)
            {
                return false;
            }

            return RepairHierarchyRecursive(root, assetPath, removedEntries);
        }

        public static int RepairProductionAssets(out List<MissingScriptReportEntry> removedEntries)
        {
            removedEntries = new List<MissingScriptReportEntry>();
            int removedCount = 0;

            for (int i = 0; i < ProductionAssetPathList.Length; i++)
            {
                removedCount += RepairAssetAtPath(ProductionAssetPathList[i], removedEntries);
            }

            removedCount += RepairPrototypingEnvironmentPrefabs(removedEntries);
            return removedCount;
        }

        public static int RepairOpenScene(Scene scene, out List<MissingScriptReportEntry> removedEntries)
        {
            removedEntries = new List<MissingScriptReportEntry>();
            if (!scene.IsValid() || !scene.isLoaded)
            {
                return 0;
            }

            bool changed = false;
            GameObject[] roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                changed |= RepairHierarchyRecursive(roots[i], scene.path, removedEntries);
            }

            if (changed)
            {
                EditorSceneManager.MarkSceneDirty(scene);
            }

            int totalRemoved = 0;
            for (int i = 0; i < removedEntries.Count; i++)
            {
                totalRemoved += removedEntries[i].MissingCount;
            }

            return totalRemoved;
        }

        public static string FormatReport(IReadOnlyList<MissingScriptReportEntry> entries)
        {
            if (entries == null || entries.Count == 0)
            {
                return "No missing script slots found.";
            }

            StringBuilder builder = new StringBuilder();
            int totalMissing = 0;
            for (int i = 0; i < entries.Count; i++)
            {
                MissingScriptReportEntry entry = entries[i];
                totalMissing += entry.MissingCount;
                builder.Append("Missing scripts (")
                    .Append(entry.MissingCount)
                    .Append(") at ")
                    .Append(entry.AssetPath)
                    .Append(" :: ")
                    .Append(entry.HierarchyPath);
                if (!string.IsNullOrEmpty(entry.PrefabSourcePath))
                {
                    builder.Append(" [prefab: ").Append(entry.PrefabSourcePath).Append(']');
                }

                builder.Append(". ");
            }

            builder.Append("Total missing script slots: ").Append(totalMissing).Append('.');
            return builder.ToString();
        }

        private static void ScanAssetAtPath(string assetPath, List<MissingScriptReportEntry> entries)
        {
            if (string.IsNullOrEmpty(assetPath) || !File.Exists(assetPath))
            {
                return;
            }

            if (assetPath.EndsWith(".unity"))
            {
                ScanSceneAsset(assetPath, entries);
                return;
            }

            if (assetPath.EndsWith(".prefab"))
            {
                ScanPrefabAsset(assetPath, entries);
            }
        }

        private static int RepairAssetAtPath(string assetPath, List<MissingScriptReportEntry> removedEntries)
        {
            if (string.IsNullOrEmpty(assetPath) || !File.Exists(assetPath))
            {
                return 0;
            }

            if (assetPath.EndsWith(".unity"))
            {
                return RepairSceneAsset(assetPath, removedEntries);
            }

            if (assetPath.EndsWith(".prefab"))
            {
                return RepairPrefabAsset(assetPath, removedEntries);
            }

            return 0;
        }

        private static void ScanSceneAsset(string scenePath, List<MissingScriptReportEntry> entries)
        {
            Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            if (!scene.IsValid())
            {
                return;
            }

            GameObject[] roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                ScanHierarchyRecursive(roots[i], scenePath, entries);
            }
        }

        private static int RepairSceneAsset(string scenePath, List<MissingScriptReportEntry> removedEntries)
        {
            Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            if (!scene.IsValid())
            {
                return 0;
            }

            int removed = RepairOpenScene(scene, out List<MissingScriptReportEntry> sceneRemoved);
            if (sceneRemoved.Count > 0)
            {
                removedEntries.AddRange(sceneRemoved);
                EditorSceneManager.SaveScene(scene);
            }

            return removed;
        }

        private static void ScanPrefabAsset(string prefabPath, List<MissingScriptReportEntry> entries)
        {
            GameObject prefabRoot = PrefabUtility.LoadPrefabContents(prefabPath);
            if (prefabRoot == null)
            {
                return;
            }

            try
            {
                ScanHierarchyRecursive(prefabRoot, prefabPath, entries);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(prefabRoot);
            }
        }

        private static int RepairPrefabAsset(string prefabPath, List<MissingScriptReportEntry> removedEntries)
        {
            GameObject prefabRoot = PrefabUtility.LoadPrefabContents(prefabPath);
            if (prefabRoot == null)
            {
                return 0;
            }

            bool changed = false;
            try
            {
                changed = RepairHierarchyRecursive(prefabRoot, prefabPath, removedEntries);
                if (changed)
                {
                    PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath);
                }
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(prefabRoot);
            }

            int removed = 0;
            for (int i = removedEntries.Count - 1; i >= 0; i--)
            {
                if (removedEntries[i].AssetPath == prefabPath)
                {
                    removed += removedEntries[i].MissingCount;
                }
            }

            return removed;
        }

        private static void ScanPrototypingEnvironmentPrefabs(List<MissingScriptReportEntry> entries)
        {
            if (!Directory.Exists(PrototypingEnvironmentPrefabRoot))
            {
                return;
            }

            string[] prefabPaths = Directory.GetFiles(PrototypingEnvironmentPrefabRoot, "*.prefab", SearchOption.AllDirectories);
            for (int i = 0; i < prefabPaths.Length; i++)
            {
                ScanPrefabAsset(prefabPaths[i].Replace('\\', '/'), entries);
            }
        }

        private static int RepairPrototypingEnvironmentPrefabs(List<MissingScriptReportEntry> removedEntries)
        {
            if (!Directory.Exists(PrototypingEnvironmentPrefabRoot))
            {
                return 0;
            }

            int removed = 0;
            string[] prefabPaths = Directory.GetFiles(PrototypingEnvironmentPrefabRoot, "*.prefab", SearchOption.AllDirectories);
            for (int i = 0; i < prefabPaths.Length; i++)
            {
                removed += RepairPrefabAsset(prefabPaths[i].Replace('\\', '/'), removedEntries);
            }

            return removed;
        }

        private static void ScanHierarchyRecursive(
            GameObject gameObject,
            string assetPath,
            List<MissingScriptReportEntry> entries)
        {
            if (gameObject == null)
            {
                return;
            }

            int missingCount = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(gameObject);
            if (missingCount > 0)
            {
                entries.Add(new MissingScriptReportEntry
                {
                    AssetPath = assetPath,
                    HierarchyPath = BuildHierarchyPath(gameObject.transform),
                    PrefabSourcePath = GetPrefabSourcePath(gameObject),
                    MissingCount = missingCount,
                });
            }

            Transform transform = gameObject.transform;
            for (int i = 0; i < transform.childCount; i++)
            {
                ScanHierarchyRecursive(transform.GetChild(i).gameObject, assetPath, entries);
            }
        }

        private static bool RepairHierarchyRecursive(
            GameObject gameObject,
            string assetPath,
            List<MissingScriptReportEntry> removedEntries)
        {
            if (gameObject == null)
            {
                return false;
            }

            bool changed = false;
            int missingCount = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(gameObject);
            if (missingCount > 0)
            {
                removedEntries.Add(new MissingScriptReportEntry
                {
                    AssetPath = assetPath,
                    HierarchyPath = BuildHierarchyPath(gameObject.transform),
                    PrefabSourcePath = GetPrefabSourcePath(gameObject),
                    MissingCount = missingCount,
                });
                GameObjectUtility.RemoveMonoBehavioursWithMissingScript(gameObject);
                changed = true;
            }

            Transform transform = gameObject.transform;
            for (int i = 0; i < transform.childCount; i++)
            {
                changed |= RepairHierarchyRecursive(transform.GetChild(i).gameObject, assetPath, removedEntries);
            }

            return changed;
        }

        private static string BuildHierarchyPath(Transform transform)
        {
            if (transform == null)
            {
                return string.Empty;
            }

            string path = transform.name;
            Transform current = transform.parent;
            while (current != null)
            {
                path = current.name + "/" + path;
                current = current.parent;
            }

            return path;
        }

        private static string GetPrefabSourcePath(GameObject gameObject)
        {
            if (gameObject == null)
            {
                return string.Empty;
            }

            Object source = PrefabUtility.GetCorrespondingObjectFromSource(gameObject);
            if (source == null)
            {
                return string.Empty;
            }

            return AssetDatabase.GetAssetPath(source);
        }
    }
}
