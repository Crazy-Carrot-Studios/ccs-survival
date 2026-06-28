using CCS.Modules.CharacterController.Editor;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

// =============================================================================
// SCRIPT: CCS_AINavigationMasterTestBuilder
// CATEGORY: Modules / AI / Editor
// PURPOSE: Ensures Master Test NavMesh surface and static walkable geometry.
// PLACEMENT: Editor utility invoked from AI batch and Master Test setup.
// AUTHOR: James Schilz
// CREATED: 2026-06-25
// NOTES: Bakes NavMesh for AI bandit pathfinding around static obstacles.
// =============================================================================

namespace CCS.Modules.AI.Editor
{
    public static class CCS_AINavigationMasterTestBuilder
    {
        public static bool EnsureMasterTestNavigation()
        {
            Scene scene = EditorSceneManager.OpenScene(
                CCS_CharacterControllerMasterTestLayoutConstants.MasterTestScenePath,
                OpenSceneMode.Single);
            if (!scene.IsValid())
            {
                Debug.LogError("[AI Navigation] Could not open Master Test scene.");
                return false;
            }

            CCS_CharacterControllerMasterTestBuilder.SetupMasterTestScene();
            scene = EditorSceneManager.GetActiveScene();

            bool changed = false;
            changed |= CCS_AINavigationLinkBuilder.EnsureHumanoidNavMeshAgentSettings();
            changed |= MarkEnvironmentStatic(scene);
            changed |= EnsureNavigationRoot(scene);

            if (changed)
            {
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
            }

            return changed;
        }

        private static bool MarkEnvironmentStatic(Scene scene)
        {
            bool changed = false;
            GameObject environment = GameObject.Find(CCS_CharacterControllerMasterTestLayoutConstants.EnvironmentParentName);
            if (environment == null)
            {
                return false;
            }

            changed |= SetStaticRecursive(environment.transform);
            return changed;
        }

        private static bool SetStaticRecursive(Transform root)
        {
            bool changed = false;
            if (root == null)
            {
                return false;
            }

            StaticEditorFlags flags = GameObjectUtility.GetStaticEditorFlags(root.gameObject);
            StaticEditorFlags desiredFlags = flags | StaticEditorFlags.NavigationStatic | StaticEditorFlags.ContributeGI;
            if (flags != desiredFlags)
            {
                GameObjectUtility.SetStaticEditorFlags(root.gameObject, desiredFlags);
                changed = true;
            }

            for (int i = 0; i < root.childCount; i++)
            {
                changed |= SetStaticRecursive(root.GetChild(i));
            }

            return changed;
        }

        private static bool EnsureNavigationRoot(Scene scene)
        {
            bool changed = false;
            GameObject navigationRoot = GameObject.Find(CCS_AIConstants.NavigationRootObjectName);
            if (navigationRoot == null)
            {
                navigationRoot = new GameObject(CCS_AIConstants.NavigationRootObjectName);
                if (navigationRoot.scene != scene)
                {
                    SceneManager.MoveGameObjectToScene(navigationRoot, scene);
                }

                changed = true;
            }

            Transform surfaceTransform = navigationRoot.transform.Find(CCS_AIConstants.NavigationSurfaceObjectName);
            GameObject surfaceObject;
            if (surfaceTransform == null)
            {
                surfaceObject = new GameObject(CCS_AIConstants.NavigationSurfaceObjectName);
                surfaceObject.transform.SetParent(navigationRoot.transform, false);
                changed = true;
            }
            else
            {
                surfaceObject = surfaceTransform.gameObject;
            }

            NavMeshSurface surface = surfaceObject.GetComponent<NavMeshSurface>();
            if (surface == null)
            {
                surface = surfaceObject.AddComponent<NavMeshSurface>();
                changed = true;
            }

            if (surface.collectObjects != CollectObjects.All)
            {
                surface.collectObjects = CollectObjects.All;
                changed = true;
            }

            if (surface.useGeometry != NavMeshCollectGeometry.RenderMeshes)
            {
                surface.useGeometry = NavMeshCollectGeometry.RenderMeshes;
                changed = true;
            }

            if (surface.agentTypeID != 0)
            {
                surface.agentTypeID = 0;
                changed = true;
            }

            surface.BuildNavMesh();
            changed |= CCS_AINavigationLinkBuilder.EnsureMasterTestNavigationLinks(navigationRoot);
            changed |= CCS_AINavigationProbePointBuilder.EnsureMasterTestNavigationProbes(navigationRoot);
            return changed;
        }
    }
}
