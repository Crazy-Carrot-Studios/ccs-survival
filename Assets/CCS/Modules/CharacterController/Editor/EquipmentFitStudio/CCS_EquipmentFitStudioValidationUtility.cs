using System.Collections.Generic;
using System.IO;
using CCS.Modules.CharacterController;
using CCS.Modules.Weapons;
using CCS.Project;
using UnityEditor;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.SceneManagement;

// =============================================================================
// SCRIPT: CCS_EquipmentFitStudioValidationUtility
// CATEGORY: Modules / CharacterController / Editor / EquipmentFitStudio
// PURPOSE: Validates Fit Studio assets, cleanup, and existing equipment foundation.
// PLACEMENT: Editor validator invoked from batch and Save/Validate tab.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Fails if preview objects remain in scenes or prefabs.
// =============================================================================

namespace CCS.Modules.CharacterController.Editor.EquipmentFitStudio
{
    public static class CCS_EquipmentFitStudioValidationUtility
    {
        #region Public Methods

        public static CCS_SurvivalValidationResult ValidateEquipmentFitStudioFoundation()
        {
            List<string> failures = new List<string>();

            AppendIfMissing(
                failures,
                File.Exists(CCS_EquipmentConstants.EquipmentFitStudioSettingsPath),
                "Missing CCS_EquipmentFitStudioSettings.asset.");
            AppendIfMissing(
                failures,
                Directory.Exists(CCS_EquipmentConstants.EquipmentFittingProfileRootPath),
                "Missing Profiles/EquipmentFitting folder.");
            AppendIfMissing(
                failures,
                Directory.Exists(CCS_EquipmentConstants.EquipmentFittingIkProfileFolderPath),
                "Missing Profiles/EquipmentFitting/IK folder.");
            AppendIfMissing(
                failures,
                Directory.Exists(CCS_EquipmentConstants.EquipmentFittingHandPoseFolderPath),
                "Missing Profiles/EquipmentFitting/HandPoses folder.");

            CCS_EquipmentFitStudioSettings settings = AssetDatabase.LoadAssetAtPath<CCS_EquipmentFitStudioSettings>(
                CCS_EquipmentConstants.EquipmentFitStudioSettingsPath);
            if (settings != null)
            {
                AppendIfMissing(
                    failures,
                    settings.DefaultSocketProfile != null,
                    "Equipment Fit Studio settings must assign defaultSocketProfile.");
                AppendIfMissing(
                    failures,
                    settings.NudgePositionSmall > 0f && settings.NudgePositionLarge > 0f,
                    "Equipment Fit Studio nudge position values must be greater than zero.");
                AppendIfMissing(
                    failures,
                    settings.NudgeRotationSmall > 0f && settings.NudgeRotationLarge > 0f,
                    "Equipment Fit Studio nudge rotation values must be greater than zero.");
                AppendIfMissing(
                    failures,
                    settings.PreviewCameraNearClip > 0f && settings.PreviewCameraFarClip > settings.PreviewCameraNearClip,
                    "Equipment Fit Studio preview camera clip planes must be valid.");
            }

            ValidatePreviewObjectCleanup(failures);
            ValidateWorldPickupPreviewSource(failures);
            AppendResult(failures, CCS_EquipmentSocketValidationUtility.ValidateAnimationRiggingPackageInstalled());
            AppendResult(failures, CCS_EquipmentSocketValidationUtility.ValidateDefaultEquipmentSocketProfile());

            GameObject playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                CCS.Modules.CharacterController.Tests.CCS_TestPlayerPrefabConstants.NetworkedPlayerPrefabPath);
            if (playerPrefab != null)
            {
                AppendResult(
                    failures,
                    CCS_EquipmentSocketValidationUtility.ValidatePlayerEquipmentSocketFoundation(playerPrefab));
                AppendResult(
                    failures,
                    CCS_EquipmentSocketValidationUtility.ValidatePlayerWeaponIkFoundation(playerPrefab));
                ValidatePlayerIkWeightsDefault(failures, playerPrefab);
            }

            return failures.Count > 0
                ? CCS_SurvivalValidationResult.Fail(string.Join(" ", failures))
                : CCS_SurvivalValidationResult.Pass("Equipment Fit Studio foundation validated.");
        }

        public static bool SceneContainsPreviewObjects()
        {
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
                    if (ContainsPreviewObjectRecursive(roots[r].transform))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        #endregion

        #region Private Methods

        private static void ValidatePreviewObjectCleanup(List<string> failures)
        {
            AppendIfMissing(
                failures,
                !SceneContainsPreviewObjects(),
                "Open scenes must not contain editor preview objects.");
            AppendIfMissing(
                failures,
                !PrefabContainsPreviewObjects(
                    CCS.Modules.CharacterController.Tests.CCS_TestPlayerPrefabConstants.NetworkedPlayerPrefabPath),
                "Networked test player prefab must not contain editor preview objects.");
            AppendIfMissing(
                failures,
                !PrefabContainsPreviewObjects(CCS_WeaponsConstants.RevolverM1879WorldPickupPrefabPath),
                "World pickup prefab must not contain editor preview objects.");
        }

        private static void ValidateWorldPickupPreviewSource(List<string> failures)
        {
            GameObject worldPickup = AssetDatabase.LoadAssetAtPath<GameObject>(
                CCS_WeaponsConstants.RevolverM1879WorldPickupPrefabPath);
            if (worldPickup == null)
            {
                failures.Add("Missing world pickup prefab for preview source validation.");
                return;
            }

            Transform modelRoot = worldPickup.transform.Find(CCS_WeaponsConstants.RevolverModelRootObjectName);
            Transform revolverVisual = modelRoot != null
                ? modelRoot.Find(CCS_WeaponsConstants.RevolverMaterializedVisualChildName)
                : null;
            AppendIfMissing(failures, modelRoot != null, "World pickup must contain ModelRoot.");
            AppendIfMissing(failures, revolverVisual != null, "World pickup must contain ModelRoot/RevolverVisual.");
            AppendIfMissing(
                failures,
                worldPickup.transform.Find("RevolverMesh") == null,
                "World pickup must not contain top-level RevolverMesh.");
        }

        private static void ValidatePlayerIkWeightsDefault(List<string> failures, GameObject playerPrefab)
        {
            Transform visualRoot = FindDeepChild(playerPrefab.transform, CCS_EquipmentConstants.VisualRootObjectName);
            if (visualRoot == null)
            {
                return;
            }

            Animator animator = visualRoot.GetComponentInChildren<Animator>(true);
            if (animator == null)
            {
                return;
            }

            Rig rig = FindDeepChild(animator.transform, CCS_EquipmentConstants.WeaponIkRigObjectName)
                ?.GetComponent<Rig>();
            if (rig != null && rig.weight != 0f)
            {
                failures.Add("Player Rig_WeaponIK weight must default to 0.");
            }

            TwoBoneIKConstraint[] constraints = animator.GetComponentsInChildren<TwoBoneIKConstraint>(true);
            for (int i = 0; i < constraints.Length; i++)
            {
                if (constraints[i] != null && constraints[i].weight != 0f)
                {
                    failures.Add("Player IK constraint weights must default to 0.");
                    break;
                }
            }
        }

        private static bool PrefabContainsPreviewObjects(string prefabPath)
        {
            if (!File.Exists(prefabPath))
            {
                return false;
            }

            string text = File.ReadAllText(prefabPath);
            return text.Contains("m_Name: " + CCS_EquipmentConstants.EditorPreviewItemObjectName)
                || text.Contains("m_Name: " + CCS_EquipmentConstants.EditorPreviewCameraObjectName);
        }

        private static bool ContainsPreviewObjectRecursive(Transform root)
        {
            if (root == null)
            {
                return false;
            }

            if (root.name == CCS_EquipmentConstants.EditorPreviewItemObjectName
                || root.name == CCS_EquipmentConstants.EditorPreviewCameraObjectName)
            {
                return true;
            }

            for (int i = 0; i < root.childCount; i++)
            {
                if (ContainsPreviewObjectRecursive(root.GetChild(i)))
                {
                    return true;
                }
            }

            return false;
        }

        private static Transform FindDeepChild(Transform root, string childName)
        {
            if (root == null)
            {
                return null;
            }

            if (root.name == childName)
            {
                return root;
            }

            for (int i = 0; i < root.childCount; i++)
            {
                Transform match = FindDeepChild(root.GetChild(i), childName);
                if (match != null)
                {
                    return match;
                }
            }

            return null;
        }

        private static void AppendResult(List<string> failures, CCS_SurvivalValidationResult result)
        {
            if (!result.IsSuccess)
            {
                failures.Add(result.Message);
            }
        }

        private static void AppendIfMissing(List<string> failures, bool condition, string message)
        {
            if (!condition)
            {
                failures.Add(message);
            }
        }

        #endregion
    }
}
