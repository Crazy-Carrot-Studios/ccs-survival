using UnityEngine;
using UnityEngine.SceneManagement;

// =============================================================================
// SCRIPT: CCS_EquipmentFitStudioCleanupUtility
// CATEGORY: Modules / CharacterController / Editor / EquipmentFitStudio
// PURPOSE: Removes editor-only preview and test attachment objects.
// PLACEMENT: Editor utility invoked from builders, validators, and Fit Studio.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Ensures no preview/test pollution remains after tool use or rebuild.
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
            return CleanupAllEditorTemporaryObjects();
        }

        public static bool CleanupAllEditorTemporaryObjects()
        {
            bool changed = false;
            for (int i = 0; i < CCS_EquipmentConstants.EditorTemporaryObjectNames.Length; i++)
            {
                changed |= DestroyObjectsByName(CCS_EquipmentConstants.EditorTemporaryObjectNames[i]);
            }

            string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/CCS" });
            for (int i = 0; i < prefabGuids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(prefabGuids[i]);
                changed |= CleanupEditorTemporaryObjectsInPrefab(path);
            }

            changed |= CleanupEditorTemporaryObjectsInOpenScenes();

            if (changed)
            {
                AssetDatabase.SaveAssets();
            }

            return changed;
        }

        public static bool CleanupPreviewObjectsInOpenScenes()
        {
            return CleanupEditorTemporaryObjectsInOpenScenes();
        }

        public static bool CleanupEditorTemporaryObjectsInOpenScenes()
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
                    GameObject root = roots[r];
                    if (root != null && IsEditorTemporaryObjectName(root.name))
                    {
                        Object.DestroyImmediate(root, true);
                        changed = true;
                        continue;
                    }

                    changed |= DestroyEditorTemporaryObjectsRecursive(root.transform);
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

            return DestroyEditorTemporaryObjectsRecursive(root.transform);
        }

        public static bool CleanupPreviewObjectsInPrefab(string prefabPath)
        {
            return CleanupEditorTemporaryObjectsInPrefab(prefabPath);
        }

        public static bool CleanupEditorTemporaryObjectsInPrefab(string prefabPath)
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

            bool changed = DestroyEditorTemporaryObjectsRecursive(prefabRoot.transform);
            if (changed)
            {
                PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath);
            }

            PrefabUtility.UnloadPrefabContents(prefabRoot);
            return changed;
        }

        public static bool IsEditorTemporaryObjectName(string objectName)
        {
            for (int i = 0; i < CCS_EquipmentConstants.EditorTemporaryObjectNames.Length; i++)
            {
                if (CCS_EquipmentConstants.EditorTemporaryObjectNames[i] == objectName)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool IsRuntimeTemporaryObjectName(string objectName)
        {
            for (int i = 0; i < CCS_EquipmentConstants.RuntimeTemporaryObjectNames.Length; i++)
            {
                if (CCS_EquipmentConstants.RuntimeTemporaryObjectNames[i] == objectName)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool IsTemporaryEquipmentObjectName(string objectName, bool includeRuntimeObjects)
        {
            if (IsEditorTemporaryObjectName(objectName))
            {
                return true;
            }

            return includeRuntimeObjects && IsRuntimeTemporaryObjectName(objectName);
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

        private static bool DestroyEditorTemporaryObjectsRecursive(Transform root)
        {
            bool changed = false;
            if (root == null)
            {
                return false;
            }

            for (int i = root.childCount - 1; i >= 0; i--)
            {
                Transform child = root.GetChild(i);
                if (IsTemporaryEquipmentObjectName(child.name, includeRuntimeObjects: true))
                {
                    Object.DestroyImmediate(child.gameObject, true);
                    changed = true;
                    continue;
                }

                changed |= DestroyEditorTemporaryObjectsRecursive(child);
            }

            return changed;
        }

        #endregion
    }
}
#endif
