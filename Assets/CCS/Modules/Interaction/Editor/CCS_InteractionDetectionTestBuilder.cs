using CCS.Modules.Interaction;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// =============================================================================
// SCRIPT: CCS_InteractionDetectionTestBuilder
// CATEGORY: Modules / Interaction / Editor
// PURPOSE: Builds Master Test pickup cube, building door interactable, and player wiring.
// PLACEMENT: Editor utility invoked from CCS/Interaction/Build Master Test Interactions.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Bakes pickup cube, building door, and runtime bootstrap for solo play.
// =============================================================================

namespace CCS.Modules.Interaction.Editor
{
    public static class CCS_InteractionDetectionTestBuilder
    {
        #region Public Methods

        public static bool BuildMasterTestInteractions()
        {
            bool changed = EnsureMasterTestInteractionPrerequisites();

            Scene scene = EditorSceneManager.OpenScene(
                CCS_InteractionConstants.MasterTestScenePath,
                OpenSceneMode.Single);
            if (!scene.IsValid())
            {
                Debug.LogError(
                    "[Interaction Builder] Could not open "
                    + CCS_InteractionConstants.MasterTestScenePath);
                return changed;
            }

            changed |= ApplyMasterTestInteractionsToActiveScene();

            if (changed)
            {
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
            }

            return changed;
        }

        public static bool EnsureMasterTestInteractionPrerequisites()
        {
            bool changed = false;
            changed |= CCS_InteractionLayerUtility.EnsureInteractableLayer();
            changed |= CCS_InteractionAssetBuilder.EnsureScannerProfileAsset();
            changed |= CCS_InteractionTestPlayerPrefabBuilder.EnsureTestPlayerDetectionWiring();
            changed |= CCS_InteractionBuildingDoorBuilder.EnsureTestDoorSinglePrefab();
            return changed;
        }

        public static bool ApplyMasterTestInteractionsToActiveScene()
        {
            int interactableLayer = LayerMask.NameToLayer(CCS_InteractionConstants.InteractableLayerName);

            bool changed = RemovePickupItemSpawners();
            changed |= EnsureDetectionCube(out bool cubeChanged);
            changed |= cubeChanged;
            changed |= CCS_InteractionBuildingDoorBuilder.RemoveLegacyWalkThroughDoorSlab();
            changed |= CCS_InteractionBuildingDoorBuilder.EnsureSceneBuildingDoors(interactableLayer);
            changed |= EnsureDetectionBootstrap(out bool bootstrapChanged);
            changed |= bootstrapChanged;
            return changed;
        }

        public static bool BuildMasterTestDetectionCube()
        {
            return BuildMasterTestInteractions();
        }

        public static bool EnsureMasterTestDetectionCube()
        {
            return BuildMasterTestInteractions();
        }

        #endregion

        #region Private Methods

        private static bool RemovePickupItemSpawners()
        {
            bool changed = false;
            CCS_TestPickupItemSpawner[] spawners = Object.FindObjectsByType<CCS_TestPickupItemSpawner>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);
            for (int i = spawners.Length - 1; i >= 0; i--)
            {
                CCS_TestPickupItemSpawner spawner = spawners[i];
                if (spawner != null)
                {
                    Object.DestroyImmediate(spawner.gameObject);
                    changed = true;
                }
            }

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

                    if (child.name == CCS_InteractionConstants.TestPickupInteractableInstanceName)
                    {
                        Object.DestroyImmediate(child.gameObject);
                        changed = true;
                    }
                }
            }

            return changed;
        }

        private static bool EnsureDetectionCube(out bool changed)
        {
            changed = false;
            Transform spawnOrigin = FindSpawnOriginInScene();
            if (spawnOrigin == null)
            {
                Debug.LogError(
                    "[Interaction Detection Builder] Missing spawn origin: "
                    + CCS_InteractionConstants.MasterTestSpawnOriginObjectPath);
                return false;
            }

            int interactableLayer = LayerMask.NameToLayer(CCS_InteractionConstants.InteractableLayerName);
            if (interactableLayer < 0)
            {
                Debug.LogError("[Interaction Detection Builder] Interactable layer was not found.");
                return false;
            }

            GameObject cubeObject = FindSceneDetectionCube();
            if (cubeObject == null)
            {
                cubeObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                Undo.RegisterCreatedObjectUndo(cubeObject, "Create Master Test Detection Cube");
                changed = true;
            }

            if (!cubeObject.scene.IsValid())
            {
                SceneManager.MoveGameObjectToScene(cubeObject, SceneManager.GetActiveScene());
                changed = true;
            }

            if (cubeObject.transform.parent != null)
            {
                cubeObject.transform.SetParent(null, true);
                changed = true;
            }

            Vector3 targetPosition = CCS_TestDetectionCubeUtility.GetDetectionCubeWorldPosition(spawnOrigin);
            if (cubeObject.transform.position != targetPosition)
            {
                cubeObject.transform.position = targetPosition;
                changed = true;
            }

            if (cubeObject.transform.rotation != spawnOrigin.rotation)
            {
                cubeObject.transform.rotation = spawnOrigin.rotation;
                changed = true;
            }

            if (cubeObject.transform.localScale != Vector3.one)
            {
                cubeObject.transform.localScale = Vector3.one;
                changed = true;
            }

            if (cubeObject.name != CCS_InteractionConstants.TestDetectionCubeObjectName)
            {
                cubeObject.name = CCS_InteractionConstants.TestDetectionCubeObjectName;
                changed = true;
            }

            if (cubeObject.layer != interactableLayer)
            {
                cubeObject.layer = interactableLayer;
                changed = true;
            }

            if (!cubeObject.CompareTag(CCS_InteractionConstants.InteractableTagName))
            {
                cubeObject.tag = CCS_InteractionConstants.InteractableTagName;
                changed = true;
            }

            BoxCollider boxCollider = cubeObject.GetComponent<BoxCollider>();
            if (boxCollider == null)
            {
                boxCollider = cubeObject.AddComponent<BoxCollider>();
                changed = true;
            }

            if (!boxCollider.enabled)
            {
                boxCollider.enabled = true;
                changed = true;
            }

            if (boxCollider.isTrigger)
            {
                boxCollider.isTrigger = false;
                changed = true;
            }

            CCS_InteractableLabelTarget labelTarget = cubeObject.GetComponent<CCS_InteractableLabelTarget>();
            if (labelTarget == null)
            {
                labelTarget = cubeObject.AddComponent<CCS_InteractableLabelTarget>();
                changed = true;
            }

            labelTarget.ConfigureForKind(
                CCS_InteractionKind.Pickup,
                CCS_InteractionConstants.TestDetectionCubeDisplayName);

            if (cubeObject.GetComponent<CCS_InteractableExecutor>() == null)
            {
                cubeObject.AddComponent<CCS_InteractableExecutor>();
                changed = true;
            }

            RemoveLegacyInteractableComponents(cubeObject);

            MeshRenderer renderer = cubeObject.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                Material material = renderer.sharedMaterial;
                if (material == null || material.name.Contains("Default"))
                {
                    Shader shader = Shader.Find("Universal Render Pipeline/Lit");
                    if (shader != null)
                    {
                        renderer.sharedMaterial = new Material(shader)
                        {
                            color = new Color(0.2f, 0.55f, 0.95f, 1f)
                        };
                        changed = true;
                    }
                }
            }

            if (!cubeObject.activeSelf)
            {
                cubeObject.SetActive(true);
                changed = true;
            }

            if (changed)
            {
                EditorUtility.SetDirty(cubeObject);
            }

            Debug.Log(
                $"[Interaction Test] Detection cube ready: pos={cubeObject.transform.position}, "
                + $"layer={CCS_InteractionConstants.InteractableLayerName}, tag={cubeObject.tag}",
                cubeObject);

            return true;
        }

        private static void RemoveLegacyInteractableComponents(GameObject targetObject)
        {
            MonoBehaviour[] behaviours = targetObject.GetComponents<MonoBehaviour>();
            for (int i = 0; i < behaviours.Length; i++)
            {
                MonoBehaviour behaviour = behaviours[i];
                if (behaviour == null)
                {
                    continue;
                }

                if (behaviour.GetType().Name == "CCS_TestDetectionCubePickup")
                {
                    Object.DestroyImmediate(behaviour);
                }
            }
        }

        private static bool EnsureDetectionBootstrap(out bool changed)
        {
            changed = false;
            GameObject bootstrapObject = FindSceneObjectByName(
                CCS_InteractionConstants.TestDetectionCubeBootstrapObjectName);

            if (FindSceneDetectionCube() != null)
            {
                CCS_TestDetectionCubeSceneBootstrap[] bootstraps =
                    Object.FindObjectsByType<CCS_TestDetectionCubeSceneBootstrap>(
                        FindObjectsInactive.Include,
                        FindObjectsSortMode.None);
                for (int i = bootstraps.Length - 1; i >= 0; i--)
                {
                    CCS_TestDetectionCubeSceneBootstrap bootstrap = bootstraps[i];
                    if (bootstrap != null)
                    {
                        Object.DestroyImmediate(bootstrap.gameObject);
                        changed = true;
                    }
                }

                return changed;
            }

            if (bootstrapObject != null)
            {
                if (!bootstrapObject.activeSelf)
                {
                    bootstrapObject.SetActive(true);
                    changed = true;
                }

                return changed;
            }

            bootstrapObject = new GameObject(CCS_InteractionConstants.TestDetectionCubeBootstrapObjectName);
            Undo.RegisterCreatedObjectUndo(bootstrapObject, "Create Detection Cube Bootstrap");
            bootstrapObject.AddComponent<CCS_TestDetectionCubeSceneBootstrap>();
            if (!bootstrapObject.scene.IsValid())
            {
                SceneManager.MoveGameObjectToScene(bootstrapObject, SceneManager.GetActiveScene());
            }

            changed = true;
            EditorUtility.SetDirty(bootstrapObject);
            return changed;
        }

        private static Transform FindSpawnOriginInScene()
        {
            GameObject[] roots = SceneManager.GetActiveScene().GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                Transform[] transforms = roots[i].GetComponentsInChildren<Transform>(true);
                for (int j = 0; j < transforms.Length; j++)
                {
                    Transform candidate = transforms[j];
                    if (candidate != null && candidate.name == "TP_Spawn_Host")
                    {
                        return candidate;
                    }
                }
            }

            return null;
        }

        private static GameObject FindSceneDetectionCube()
        {
            return FindSceneObjectByName(CCS_InteractionConstants.TestDetectionCubeObjectName);
        }

        private static GameObject FindSceneObjectByName(string objectName)
        {
            GameObject[] roots = SceneManager.GetActiveScene().GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                Transform[] transforms = roots[i].GetComponentsInChildren<Transform>(true);
                for (int j = 0; j < transforms.Length; j++)
                {
                    Transform candidate = transforms[j];
                    if (candidate != null && candidate.name == objectName)
                    {
                        return candidate.gameObject;
                    }
                }
            }

            return null;
        }

        private static bool SetString(SerializedObject serializedObject, string propertyName, string value)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property == null || property.stringValue == value)
            {
                return false;
            }

            property.stringValue = value;
            return true;
        }

        #endregion
    }
}
