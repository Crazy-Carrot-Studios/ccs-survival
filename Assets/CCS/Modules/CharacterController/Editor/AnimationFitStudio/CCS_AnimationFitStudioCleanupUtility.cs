using System.Collections.Generic;
using CCS.Modules.CharacterController.Editor.EquipmentFitStudio;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

// =============================================================================
// SCRIPT: CCS_AnimationFitStudioCleanupUtility
// CATEGORY: Modules / CharacterController / Editor / AnimationFitStudio
// PURPOSE: Removes Animation Fit Studio temporary preview objects from open scenes.
// PLACEMENT: Editor utility invoked on window close and before preview reload.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Never removes Equipment Fit Studio preview objects unless names overlap.
// =============================================================================

namespace CCS.Modules.CharacterController.Editor.AnimationFitStudio
{
    public static class CCS_AnimationFitStudioCleanupUtility
    {
        private static readonly string[] PreviewObjectNames =
        {
            CCS_AnimationFitStudioConstants.PreviewPlayerObjectName,
            CCS_AnimationFitStudioConstants.WeaponAttachmentRootObjectName,
            CCS_AnimationFitStudioConstants.PreviewWeaponObjectName,
            CCS_AnimationFitStudioConstants.PreviewCameraObjectName,
        };

        public static void CleanupAllPreviewArtifacts(CCS_EquipmentFitStudioPreviewCamera previewCamera)
        {
            CleanupPreviewObjectsInOpenScenes();
            previewCamera?.DestroyCamera();
        }

        public static void CleanupPreviewObjectsInOpenScenes()
        {
            for (int sceneIndex = 0; sceneIndex < SceneManager.sceneCount; sceneIndex++)
            {
                Scene scene = SceneManager.GetSceneAt(sceneIndex);
                if (!scene.isLoaded)
                {
                    continue;
                }

                GameObject[] roots = scene.GetRootGameObjects();
                for (int i = 0; i < roots.Length; i++)
                {
                    CleanupHierarchy(roots[i]);
                }
            }

            GameObject[] allObjects = Object.FindObjectsByType<GameObject>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);
            for (int i = 0; i < allObjects.Length; i++)
            {
                GameObject candidate = allObjects[i];
                if (candidate != null && IsAnimationFitPreviewObjectName(candidate.name))
                {
                    Object.DestroyImmediate(candidate);
                }
            }
        }

        private static void CleanupHierarchy(GameObject root)
        {
            if (root == null)
            {
                return;
            }

            List<Transform> matches = new List<Transform>();
            CollectNamedTransforms(root.transform, matches);
            for (int i = matches.Count - 1; i >= 0; i--)
            {
                Transform match = matches[i];
                if (match != null)
                {
                    Object.DestroyImmediate(match.gameObject);
                }
            }
        }

        private static void CollectNamedTransforms(Transform root, List<Transform> matches)
        {
            if (root == null)
            {
                return;
            }

            if (IsAnimationFitPreviewObjectName(root.name))
            {
                matches.Add(root);
            }

            for (int i = 0; i < root.childCount; i++)
            {
                CollectNamedTransforms(root.GetChild(i), matches);
            }
        }

        private static bool IsAnimationFitPreviewObjectName(string objectName)
        {
            for (int i = 0; i < PreviewObjectNames.Length; i++)
            {
                if (objectName == PreviewObjectNames[i])
                {
                    return true;
                }
            }

            return false;
        }
    }
}
