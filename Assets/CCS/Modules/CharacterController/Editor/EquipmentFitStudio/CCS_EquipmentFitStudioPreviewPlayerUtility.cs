using System.IO;
using CCS.Modules.CharacterController;
using CCS.Modules.CharacterController.Local;
using CCS.Project;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// =============================================================================
// SCRIPT: CCS_EquipmentFitStudioPreviewPlayerUtility
// CATEGORY: Modules / CharacterController / Editor / EquipmentFitStudio
// PURPOSE: Editor-only preview player/mannequin for Fit Studio tuning in Edit Mode.
// PLACEMENT: Editor utility used by Equipment Fit Studio window.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Instantiates test player prefab temporarily; never saved to scene/prefab.
// =============================================================================

namespace CCS.Modules.CharacterController.Editor.EquipmentFitStudio
{
    public static class CCS_EquipmentFitStudioPreviewPlayerUtility
    {
        public static readonly Vector3 DefaultPreviewPlayerPosition = new Vector3(0f, 1f, 0f);

        public static GameObject FindExistingPreviewPlayer()
        {
            GameObject[] roots = SceneManager.GetActiveScene().GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                GameObject root = roots[i];
                if (root != null && root.name == CCS_EquipmentConstants.EditorFitPreviewPlayerObjectName)
                {
                    return root;
                }
            }

            GameObject[] allObjects = Object.FindObjectsByType<GameObject>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);
            for (int i = 0; i < allObjects.Length; i++)
            {
                GameObject candidate = allObjects[i];
                if (candidate != null && candidate.name == CCS_EquipmentConstants.EditorFitPreviewPlayerObjectName)
                {
                    return candidate;
                }
            }

            return null;
        }

        public static bool IsPreviewPlayer(GameObject playerRoot)
        {
            return playerRoot != null
                && playerRoot.name == CCS_EquipmentConstants.EditorFitPreviewPlayerObjectName;
        }

        public static bool IsValidEditFitPlayerTarget(GameObject playerRoot)
        {
            if (playerRoot == null)
            {
                return false;
            }

            return playerRoot.GetComponent<CCS_EquipmentSocketRegistry>() != null
                && HasRequiredSockets(playerRoot);
        }

        public static bool CreateOrRefreshPreviewPlayer(out GameObject previewPlayer, out string errorMessage)
        {
            previewPlayer = null;
            errorMessage = string.Empty;

            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                errorMessage = "Create preview player in Edit Mode only. Exit Play Mode first.";
                return false;
            }

            Scene activeScene = SceneManager.GetActiveScene();
            if (!activeScene.IsValid() || !activeScene.isLoaded)
            {
                errorMessage = "Open a scene before creating the editor preview player.";
                return false;
            }

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                CCS_PlayerPrefabConstants.NetworkedPlayerPrefabPath);
            if (prefab == null)
            {
                errorMessage = "Missing test player prefab at "
                    + CCS_PlayerPrefabConstants.NetworkedPlayerPrefabPath;
                return false;
            }

            ClearPreviewPlayer();
            CCS_EquipmentFitStudioCleanupUtility.CleanupEditorTemporaryObjectsInOpenScenes();

            GameObject instance = PrefabUtility.InstantiatePrefab(prefab, activeScene) as GameObject;
            if (instance == null)
            {
                errorMessage = "Could not instantiate editor preview player from test player prefab.";
                return false;
            }

            instance.name = CCS_EquipmentConstants.EditorFitPreviewPlayerObjectName;
            instance.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
            instance.transform.SetPositionAndRotation(DefaultPreviewPlayerPosition, Quaternion.identity);
            PreparePreviewPlayerForEditMode(instance);

            if (!IsValidEditFitPlayerTarget(instance))
            {
                Object.DestroyImmediate(instance);
                errorMessage = "Preview player is missing required socket registry or hip/hand sockets.";
                return false;
            }

            Selection.activeGameObject = instance;
            EditorSceneManager.MarkSceneDirty(activeScene);
            previewPlayer = instance;
            return true;
        }

        public static bool TryUseSelectedScenePlayer(out GameObject playerRoot, out string errorMessage)
        {
            playerRoot = null;
            errorMessage = string.Empty;

            if (EditorApplication.isPlaying)
            {
                errorMessage = "Use Find Runtime Player during Play Mode Runtime Test.";
                return false;
            }

            GameObject selected = Selection.activeGameObject;
            if (selected == null)
            {
                errorMessage = "Select a scene object with CCS equipment sockets first.";
                return false;
            }

            CCS_EquipmentSocketRegistry registry = selected.GetComponentInParent<CCS_EquipmentSocketRegistry>();
            if (registry == null)
            {
                errorMessage = "Selected object is not part of a player with CCS_EquipmentSocketRegistry.";
                return false;
            }

            if (!IsValidEditFitPlayerTarget(registry.gameObject))
            {
                errorMessage = "Selected player is missing required hip/hand sockets.";
                return false;
            }

            playerRoot = registry.gameObject;
            return true;
        }

        public static bool TryFindRuntimePlayer(out GameObject playerRoot, out string errorMessage)
        {
            playerRoot = null;
            errorMessage = string.Empty;

            if (!EditorApplication.isPlaying)
            {
                errorMessage = "Find Runtime Player is available in Play Mode only.";
                return false;
            }

            CCS_EquipmentSocketRegistry[] registries = Object.FindObjectsByType<CCS_EquipmentSocketRegistry>(
                FindObjectsInactive.Exclude,
                FindObjectsSortMode.None);
            for (int i = 0; i < registries.Length; i++)
            {
                CCS_EquipmentSocketRegistry registry = registries[i];
                if (registry == null || IsPreviewPlayer(registry.gameObject))
                {
                    continue;
                }

                if (HasRequiredSockets(registry.gameObject))
                {
                    playerRoot = registry.gameObject;
                    return true;
                }
            }

            errorMessage = "No live spawned player with equipment sockets found in Play Mode.";
            return false;
        }

        public static void ClearPreviewPlayer()
        {
            GameObject existing = FindExistingPreviewPlayer();
            if (existing == null)
            {
                return;
            }

            CCS_EquipmentFitStudioTestAttachmentUtility.ClearTestAttachments(existing);
            Object.DestroyImmediate(existing);
        }

        public static CCS_SurvivalValidationResult ValidatePreviewPlayerFoundation()
        {
            string utilityPath =
                CCS_CharacterControllerConstants.ModuleRootPath
                + "/Editor/EquipmentFitStudio/CCS_EquipmentFitStudioPreviewPlayerUtility.cs";
            if (!File.Exists(utilityPath))
            {
                return CCS_SurvivalValidationResult.Fail("Missing CCS_EquipmentFitStudioPreviewPlayerUtility.");
            }

            if (!File.Exists(CCS_PlayerPrefabConstants.NetworkedPlayerPrefabPath))
            {
                return CCS_SurvivalValidationResult.Fail(
                    "Missing networked test player prefab for editor preview player.");
            }

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                CCS_PlayerPrefabConstants.NetworkedPlayerPrefabPath);
            if (prefab == null)
            {
                return CCS_SurvivalValidationResult.Fail("Could not load networked test player prefab.");
            }

            if (prefab.GetComponent<CCS_EquipmentSocketRegistry>() == null)
            {
                return CCS_SurvivalValidationResult.Fail("Test player prefab must contain CCS_EquipmentSocketRegistry.");
            }

            if (!HasRequiredSockets(prefab))
            {
                return CCS_SurvivalValidationResult.Fail(
                    "Test player prefab must contain Right Hip and Right Hand sockets.");
            }

            return CCS_SurvivalValidationResult.Pass("Editor preview player foundation validated.");
        }

        private static bool HasRequiredSockets(GameObject playerRoot)
        {
            CCS_EquipmentSocketRegistry registry = playerRoot.GetComponent<CCS_EquipmentSocketRegistry>();
            if (registry == null)
            {
                return false;
            }

            return registry.TryGetSocket(CCS_EquipmentConstants.HolsterSocketRightHipId, out _)
                && registry.TryGetSocket(CCS_EquipmentConstants.HandSocketRightId, out _);
        }

        private static void PreparePreviewPlayerForEditMode(GameObject instance)
        {
            MonoBehaviour[] behaviours = instance.GetComponentsInChildren<MonoBehaviour>(true);
            for (int i = 0; i < behaviours.Length; i++)
            {
                MonoBehaviour behaviour = behaviours[i];
                if (behaviour == null)
                {
                    continue;
                }

                string typeName = behaviour.GetType().FullName ?? behaviour.GetType().Name;
                if (typeName.Contains("Unity.Netcode")
                    || typeName.Contains("CCS.Modules.Weapons.CCS_RevolverController")
                    || typeName.Contains("CCS.Modules.Weapons.CCS_PlayerWeaponLoadout")
                    || typeName.Contains("CCS.Modules.Weapons.CCS_PlayerEquipmentVisualController"))
                {
                    behaviour.enabled = false;
                }
            }
        }
    }
}
