using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_PlayerModelKevinPrefabBuilder
// CATEGORY: Modules / CharacterController / Editor / Builders
// PURPOSE: Creates production PF_CCS_Player_Model_Kevin wrapper prefab from Kevin import.
// PLACEMENT: Editor builder. Invoked before networked player visual swap.
// AUTHOR: James Schilz
// CREATED: 2026-06-25
// NOTES: Does not modify Reallusion source import assets.
// =============================================================================

namespace CCS.Modules.CharacterController.Editor
{
    public static class CCS_PlayerModelKevinPrefabBuilder
    {
        private const string ProductionPrefabObjectName = "PF_CCS_Player_Model_Kevin";

        private const float WrapperLocalYOffset = -0.079f;

        public static bool EnsurePlayerModelKevinPrefab()
        {
            GameObject kevinImportPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                CCS_CharacterControllerConstants.KevinImportPrefabPath);
            RuntimeAnimatorController locomotionController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(
                CCS_CharacterControllerConstants.PlayerLocomotionAnimatorControllerPath);
            Avatar kevinAvatar = LoadKevinHumanoidAvatar();

            if (kevinImportPrefab == null)
            {
                Debug.LogError(
                    "[Kevin Model Prefab Builder] Missing Kevin import prefab at "
                    + CCS_CharacterControllerConstants.KevinImportPrefabPath);
                return false;
            }

            if (locomotionController == null)
            {
                Debug.LogError("[Kevin Model Prefab Builder] Missing locomotion Animator Controller.");
                return false;
            }

            if (kevinAvatar == null)
            {
                Debug.LogError("[Kevin Model Prefab Builder] Missing Kevin humanoid Avatar.");
                return false;
            }

            string productionPath = CCS_CharacterControllerConstants.PlayerModelKevinPrefabPath;
            string directory = Path.GetDirectoryName(productionPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            GameObject existingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(productionPath);
            if (existingPrefab != null)
            {
                return EnsureExistingProductionKevinPrefab(productionPath, locomotionController, kevinAvatar);
            }

            return CreateProductionKevinPrefab(productionPath, kevinImportPrefab, locomotionController, kevinAvatar);
        }

        private static Avatar LoadKevinHumanoidAvatar()
        {
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(CCS_CharacterControllerConstants.KevinFbxPath);
            for (int i = 0; i < assets.Length; i++)
            {
                if (assets[i] is Avatar avatar && avatar.isValid && avatar.isHuman)
                {
                    return avatar;
                }
            }

            return null;
        }

        private static bool CreateProductionKevinPrefab(
            string productionPath,
            GameObject kevinImportPrefab,
            RuntimeAnimatorController locomotionController,
            Avatar kevinAvatar)
        {
            GameObject wrapper = new GameObject(ProductionPrefabObjectName);
            wrapper.transform.localPosition = new Vector3(0f, WrapperLocalYOffset, 0f);

            GameObject kevinInstance = PrefabUtility.InstantiatePrefab(kevinImportPrefab, wrapper.transform) as GameObject;
            if (kevinInstance == null)
            {
                Object.DestroyImmediate(wrapper);
                Debug.LogError("[Kevin Model Prefab Builder] Failed to instantiate Kevin import prefab.");
                return false;
            }

            kevinInstance.transform.localPosition = Vector3.zero;
            kevinInstance.transform.localRotation = Quaternion.identity;
            kevinInstance.transform.localScale = Vector3.one;

            ConfigureKevinAnimator(kevinInstance, locomotionController, kevinAvatar);
            CCS_PlayerVisualKevinSwapBuilder.RemoveReallusionPresentationScripts(kevinInstance.transform);

            GameObject savedPrefab = PrefabUtility.SaveAsPrefabAsset(wrapper, productionPath);
            Object.DestroyImmediate(wrapper);

            if (savedPrefab == null)
            {
                Debug.LogError("[Kevin Model Prefab Builder] Failed to save production Kevin prefab.");
                return false;
            }

            Debug.Log("[Kevin Model Prefab Builder] Created production Kevin prefab at " + productionPath);
            return true;
        }

        private static bool EnsureExistingProductionKevinPrefab(
            string productionPath,
            RuntimeAnimatorController locomotionController,
            Avatar kevinAvatar)
        {
            GameObject contents = PrefabUtility.LoadPrefabContents(productionPath);
            if (contents == null)
            {
                return false;
            }

            bool changed = false;
            if (!string.Equals(contents.name, ProductionPrefabObjectName))
            {
                contents.name = ProductionPrefabObjectName;
                changed = true;
            }

            if (!Mathf.Approximately(contents.transform.localPosition.y, WrapperLocalYOffset))
            {
                Vector3 localPosition = contents.transform.localPosition;
                localPosition.y = WrapperLocalYOffset;
                contents.transform.localPosition = localPosition;
                changed = true;
            }

            Animator animator = contents.GetComponentInChildren<Animator>(true);
            if (animator != null)
            {
                changed |= ConfigureKevinAnimator(animator.gameObject, locomotionController, kevinAvatar);
            }

            changed |= CCS_PlayerVisualKevinSwapBuilder.RemoveReallusionPresentationScripts(contents.transform);

            if (changed)
            {
                PrefabUtility.SaveAsPrefabAsset(contents, productionPath);
            }

            PrefabUtility.UnloadPrefabContents(contents);
            return changed;
        }

        private static bool ConfigureKevinAnimator(
            GameObject kevinModelObject,
            RuntimeAnimatorController locomotionController,
            Avatar kevinAvatar)
        {
            Animator animator = kevinModelObject.GetComponent<Animator>();
            if (animator == null)
            {
                animator = kevinModelObject.AddComponent<Animator>();
            }

            bool changed = false;
            if (animator.avatar != kevinAvatar)
            {
                animator.avatar = kevinAvatar;
                changed = true;
            }

            if (animator.runtimeAnimatorController != locomotionController)
            {
                animator.runtimeAnimatorController = locomotionController;
                changed = true;
            }

            if (animator.applyRootMotion)
            {
                animator.applyRootMotion = false;
                changed = true;
            }

            return changed;
        }
    }
}
