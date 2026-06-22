using UnityEngine;
using UnityEngine.SceneManagement;

// =============================================================================
// SCRIPT: CCS_EquipmentFitStudioCleanupUtility
// CATEGORY: Modules / CharacterController / Editor / EquipmentFitStudio
// PURPOSE: Removes editor-only preview objects from open scenes and prefabs.
// PLACEMENT: Editor utility invoked from builders, validators, and Fit Studio.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Ensures no preview pollution remains after tool use or rebuild.
// =============================================================================

#if UNITY_EDITOR
using CCS.Modules.CharacterController;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace CCS.Modules.CharacterController.Editor.EquipmentFitStudio
{
    public static class CCS_EquipmentFitStudioCleanupUtility
    {
        #region Public Methods

        public static bool CleanupAllPreviewObjects()
        {
            bool changed = false;
            changed |= DestroyObjectsByName(CCS_EquipmentConstants.EditorPreviewItemObjectName);
            changed |= DestroyObjectsByName(CCS_EquipmentConstants.EditorPreviewCameraObjectName);

            string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/CCS" });
            for (int i = 0; i < prefabGuids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(prefabGuids[i]);
                changed |= CleanupPreviewObjectsInPrefab(path);
            }

            if (changed)
            {
                AssetDatabase.SaveAssets();
            }

            return changed;
        }

        public static bool CleanupPreviewObjectsInOpenScenes()
        {
            bool changed = false;
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (!scene.isLoaded)
                {
                    continue;
                }

                GameObject[] roots = scene.GetRootGameObjects();
                for (int r = 0; r < roots.Length; r++)
                {
                    changed |= DestroyPreviewObjectsRecursive(roots[r].transform);
                }
            }

            return changed;
        }

        public static bool CleanupPreviewObjectsOnInstance(GameObject root)
        {
            if (root == null)
            {
                return false;
            }

            return DestroyPreviewObjectsRecursive(root.transform);
        }

        public static bool CleanupPreviewObjectsInPrefab(string prefabPath)
        {
            if (string.IsNullOrEmpty(prefabPath) || !prefabPath.EndsWith(".prefab"))
            {
                return false;
            }

            GameObject prefabRoot = PrefabUtility.LoadPrefabContents(prefabPath);
            if (prefabRoot == null)
            {
                return false;
            }

            bool changed = DestroyPreviewObjectsRecursive(prefabRoot.transform);
            if (changed)
            {
                PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath);
            }

            PrefabUtility.UnloadPrefabContents(prefabRoot);
            return changed;
        }

        #endregion

        #region Private Methods

        private static bool DestroyObjectsByName(string objectName)
        {
            bool changed = false;
            GameObject[] allObjects = Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (int i = 0; i < allObjects.Length; i++)
            {
                GameObject candidate = allObjects[i];
                if (candidate != null && candidate.name == objectName)
                {
                    Object.DestroyImmediate(candidate);
                    changed = true;
                }
            }

            return changed;
        }

        private static bool DestroyPreviewObjectsRecursive(Transform root)
        {
            bool changed = false;
            if (root == null)
            {
                return false;
            }

            for (int i = root.childCount - 1; i >= 0; i--)
            {
                Transform child = root.GetChild(i);
                if (child.name == CCS_EquipmentConstants.EditorPreviewItemObjectName
                    || child.name == CCS_EquipmentConstants.EditorPreviewCameraObjectName)
                {
                    Object.DestroyImmediate(child.gameObject, true);
                    changed = true;
                    continue;
                }

                changed |= DestroyPreviewObjectsRecursive(child);
            }

            return changed;
        }

        #endregion
    }
}
#endif
