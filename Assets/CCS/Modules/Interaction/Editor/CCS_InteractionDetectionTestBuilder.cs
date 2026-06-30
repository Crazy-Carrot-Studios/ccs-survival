using CCS.Modules.Interaction;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// =============================================================================
// SCRIPT: CCS_InteractionDetectionTestBuilder
// CATEGORY: Modules / Interaction / Editor
// PURPOSE: Builds Master Test building door interactable and player wiring.
// PLACEMENT: Editor utility invoked from CCS/Interaction/Build Master Test Interactions.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Bakes building door and runtime bootstrap for solo play. Pickup uses revolver world pickup.
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
            changed |= RemoveLegacyDetectionCubeArtifacts();
            changed |= CCS_InteractionBuildingDoorBuilder.RemoveLegacyWalkThroughDoorSlab();
            changed |= CCS_InteractionBuildingDoorBuilder.EnsureSceneBuildingDoors(interactableLayer);
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

        private static bool RemoveLegacyDetectionCubeArtifacts()
        {
            bool changed = false;
            changed |= DestroyAllSceneObjectsByName("CCS_TestDetectionCube");
            changed |= DestroyAllSceneObjectsByName("CCS_TestDetectionCubeSceneBootstrap");
            return changed;
        }

        private static bool DestroyAllSceneObjectsByName(string objectName)
        {
            bool changed = false;
            GameObject[] roots = SceneManager.GetActiveScene().GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                Transform[] transforms = roots[i].GetComponentsInChildren<Transform>(true);
                for (int j = transforms.Length - 1; j >= 0; j--)
                {
                    Transform candidate = transforms[j];
                    if (candidate != null && candidate.name == objectName)
                    {
                        Object.DestroyImmediate(candidate.gameObject);
                        changed = true;
                    }
                }
            }

            return changed;
        }

        #endregion
    }
}
