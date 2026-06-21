using CCS.Modules.Interaction;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// =============================================================================
// SCRIPT: CCS_InteractionMasterTestBuilder
// CATEGORY: Modules / Interaction / Editor
// PURPOSE: Ensures Master Test pickup interaction wiring and runtime spawner setup.
// PLACEMENT: Editor utility invoked from master test setup and Interaction validation.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Removes legacy toggle-cube scene objects and spawn controllers.
// =============================================================================

namespace CCS.Modules.Interaction.Editor
{
    public static class CCS_InteractionMasterTestBuilder
    {
        #region Public Methods

        public static bool BuildMasterTestPickupInteraction()
        {
            CCS_InteractionAssetBuilder.EnsureInteractionAssets();
            CCS_InteractionTestPlayerPrefabBuilder.EnsureTestPlayerInteractionWiring();
            CCS_InteractionPromptHudPrefabBuilder.EnsureTestPlayerInteractionPromptHud();
            return EnsureMasterTestPickupInteraction();
        }

        public static bool EnsureMasterTestPickupInteraction()
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

            bool changed = RemoveLegacyInteractableObjects();
            changed |= RemoveLegacySpawnController();
            changed |= EnsurePickupItemSpawner();
            if (changed)
            {
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
            }

            return changed;
        }

        #endregion

        #region Private Methods

        private static bool RemoveLegacyInteractableObjects()
        {
            bool changed = false;
            GameObject[] roots = SceneManager.GetActiveScene().GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                Transform[] children = roots[i].GetComponentsInChildren<Transform>(true);
                for (int j = children.Length - 1; j >= 0; j--)
                {
                    Transform child = children[j];
                    if (child == null)
                    {
                        continue;
                    }

                    if (child.name == "PF_CCS_TestInteractable_ToggleCube"
                        || child.name == CCS_InteractionConstants.TestPickupInteractableInstanceName)
                    {
                        Object.DestroyImmediate(child.gameObject);
                        changed = true;
                    }
                }
            }

            return changed;
        }

        private static bool RemoveLegacySpawnController()
        {
            bool changed = false;
            GameObject[] roots = SceneManager.GetActiveScene().GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                Transform[] children = roots[i].GetComponentsInChildren<Transform>(true);
                for (int j = children.Length - 1; j >= 0; j--)
                {
                    Transform child = children[j];
                    if (child != null && child.name == "CCS_MasterTestInteractableSpawnController")
                    {
                        Object.DestroyImmediate(child.gameObject);
                        changed = true;
                    }
                }
            }

            return changed;
        }

        private static bool EnsurePickupItemSpawner()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                CCS_InteractionConstants.TestPickupInteractablePrefabPath);
            if (prefab == null)
            {
                Debug.LogError(
                    "[Interaction Master Test Builder] Missing prefab: "
                    + CCS_InteractionConstants.TestPickupInteractablePrefabPath);
                return false;
            }

            Transform spawnOrigin = FindSpawnOrigin();
            if (spawnOrigin == null)
            {
                Debug.LogError(
                    "[Interaction Master Test Builder] Missing spawn origin: "
                    + CCS_InteractionConstants.MasterTestSpawnOriginObjectPath);
                return false;
            }

            bool changed = false;
            CCS_TestPickupItemSpawner spawner = Object.FindAnyObjectByType<CCS_TestPickupItemSpawner>();
            if (spawner == null)
            {
                GameObject spawnerObject = new GameObject(CCS_InteractionConstants.PickupItemSpawnerObjectName);
                spawner = spawnerObject.AddComponent<CCS_TestPickupItemSpawner>();
                changed = true;
            }

            SerializedObject serializedSpawner = new SerializedObject(spawner);
            changed |= SetObjectReference(serializedSpawner, "pickupItemPrefab", prefab);
            changed |= SetObjectReference(serializedSpawner, "spawnOrigin", spawnOrigin);
            changed |= SetFloat(
                serializedSpawner,
                "spawnForwardDistance",
                CCS_InteractionConstants.TestPickupSpawnForwardDistance);

            if (changed)
            {
                serializedSpawner.ApplyModifiedPropertiesWithoutUndo();
            }

            return changed;
        }

        private static Transform FindSpawnOrigin()
        {
            GameObject testPoints = GameObject.Find("TestPoints");
            if (testPoints == null)
            {
                return null;
            }

            Transform spawnHost = testPoints.transform.Find("TP_Spawn_Host");
            return spawnHost;
        }

        private static bool SetObjectReference(SerializedObject serializedObject, string propertyName, Object value)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property == null || property.objectReferenceValue == value)
            {
                return false;
            }

            property.objectReferenceValue = value;
            return true;
        }

        private static bool SetFloat(SerializedObject serializedObject, string propertyName, float value)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property == null || Mathf.Approximately(property.floatValue, value))
            {
                return false;
            }

            property.floatValue = value;
            return true;
        }

        #endregion
    }
}
