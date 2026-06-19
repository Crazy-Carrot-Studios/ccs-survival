using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// =============================================================================
// SCRIPT: CCS_InteractionMasterTestBuilder
// CATEGORY: Modules / Interaction / Editor
// PURPOSE: Places the test toggle interactable in the Master Test scene near spawn.
// PLACEMENT: Editor utility invoked from master test setup and Interaction validation.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Keeps one reachable interactable near TP_Spawn_Host without cluttering the scene.
// =============================================================================

namespace CCS.Modules.Interaction.Editor
{
    public static class CCS_InteractionMasterTestBuilder
    {
        private const string EnvironmentParentName = "Environment";

        #region Public Methods

        public static bool EnsureMasterTestInteractable()
        {
            Scene scene = EditorSceneManager.OpenScene(
                CCS_InteractionConstants.MasterTestScenePath,
                OpenSceneMode.Single);
            if (!scene.IsValid())
            {
                Debug.LogError(
                    "[Interaction Master Test Builder] Could not open "
                    + CCS_InteractionConstants.MasterTestScenePath);
                return false;
            }

            CCS_InteractionAssetBuilder.EnsureInteractionAssets();

            Transform environment = FindChildByName(scene.GetRootGameObjects(), EnvironmentParentName);
            if (environment == null)
            {
                Debug.LogError(
                    "[Interaction Master Test Builder] Missing environment parent: "
                    + EnvironmentParentName);
                return false;
            }

            bool changed = EnsureInteractableInstance(environment);
            if (changed)
            {
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
            }

            return changed;
        }

        #endregion

        #region Private Methods

        private static bool EnsureInteractableInstance(Transform environmentParent)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                CCS_InteractionConstants.TestToggleInteractablePrefabPath);
            if (prefab == null)
            {
                Debug.LogError(
                    "[Interaction Master Test Builder] Missing prefab: "
                    + CCS_InteractionConstants.TestToggleInteractablePrefabPath);
                return false;
            }

            List<Transform> matches = FindAllByName(
                CCS_InteractionConstants.TestToggleInteractableInstanceName,
                environmentParent);
            Transform existing = matches.Count > 0 ? matches[0] : null;
            if (matches.Count > 1)
            {
                for (int i = 1; i < matches.Count; i++)
                {
                    Object.DestroyImmediate(matches[i].gameObject);
                }
            }

            bool changed = false;
            if (existing == null)
            {
                GameObject instance = PrefabUtility.InstantiatePrefab(prefab, environmentParent) as GameObject;
                if (instance == null)
                {
                    return false;
                }

                instance.name = CCS_InteractionConstants.TestToggleInteractableInstanceName;
                instance.transform.SetPositionAndRotation(
                    CCS_InteractionConstants.TestToggleInteractablePosition,
                    Quaternion.identity);
                return true;
            }

            if (existing.parent != environmentParent)
            {
                existing.SetParent(environmentParent, true);
                changed = true;
            }

            if (existing.position != CCS_InteractionConstants.TestToggleInteractablePosition)
            {
                existing.position = CCS_InteractionConstants.TestToggleInteractablePosition;
                changed = true;
            }

            if (existing.rotation != Quaternion.identity)
            {
                existing.rotation = Quaternion.identity;
                changed = true;
            }

            return changed;
        }

        private static Transform FindChildByName(GameObject[] roots, string childName)
        {
            for (int i = 0; i < roots.Length; i++)
            {
                Transform[] children = roots[i].GetComponentsInChildren<Transform>(true);
                for (int j = 0; j < children.Length; j++)
                {
                    if (children[j].name == childName)
                    {
                        return children[j];
                    }
                }
            }

            return null;
        }

        private static List<Transform> FindAllByName(string objectName, Transform parent)
        {
            List<Transform> matches = new List<Transform>();
            if (parent == null)
            {
                return matches;
            }

            Transform[] children = parent.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < children.Length; i++)
            {
                if (children[i].name == objectName)
                {
                    matches.Add(children[i]);
                }
            }

            return matches;
        }

        #endregion
    }
}
