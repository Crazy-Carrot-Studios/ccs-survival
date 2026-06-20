using CCS.Modules.CharacterController.Tests.Netcode;
using CCS.Modules.Interaction;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// =============================================================================
// SCRIPT: CCS_InteractionMasterTestBuilder
// CATEGORY: Modules / Interaction / Editor
// PURPOSE: Ensures Master Test uses runtime interactable spawning instead of scene NetworkObjects.
// PLACEMENT: Editor utility invoked from master test setup and Interaction validation.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Removes scene-placed toggle cubes to avoid in-scene NetworkObject hash drift.
// =============================================================================

namespace CCS.Modules.Interaction.Editor
{
    public static class CCS_InteractionMasterTestBuilder
    {
        private const string InteractableSpawnControllerObjectName = "CCS_MasterTestInteractableSpawnController";

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

            bool changed = RemoveScenePlacedInteractableInstances();
            changed |= EnsureInteractableSpawnController();
            if (changed)
            {
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
            }

            return changed;
        }

        #endregion

        #region Private Methods

        private static bool RemoveScenePlacedInteractableInstances()
        {
            bool changed = false;
            GameObject[] roots = SceneManager.GetActiveScene().GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                Transform[] children = roots[i].GetComponentsInChildren<Transform>(true);
                for (int j = children.Length - 1; j >= 0; j--)
                {
                    Transform child = children[j];
                    if (child == null || child.name != CCS_InteractionConstants.TestToggleInteractableInstanceName)
                    {
                        continue;
                    }

                    Object.DestroyImmediate(child.gameObject);
                    changed = true;
                }
            }

            return changed;
        }

        private static bool EnsureInteractableSpawnController()
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

            bool changed = false;
            CCS_MasterTestInteractableSpawnController controller =
                Object.FindFirstObjectByType<CCS_MasterTestInteractableSpawnController>();
            if (controller == null)
            {
                GameObject controllerObject = new GameObject(InteractableSpawnControllerObjectName);
                controller = controllerObject.AddComponent<CCS_MasterTestInteractableSpawnController>();
                changed = true;
            }

            SerializedObject serializedController = new SerializedObject(controller);
            SerializedProperty prefabProperty = serializedController.FindProperty("toggleInteractablePrefab");
            if (prefabProperty != null && prefabProperty.objectReferenceValue != prefab)
            {
                prefabProperty.objectReferenceValue = prefab;
                serializedController.ApplyModifiedPropertiesWithoutUndo();
                changed = true;
            }

            return changed;
        }

        #endregion
    }
}
