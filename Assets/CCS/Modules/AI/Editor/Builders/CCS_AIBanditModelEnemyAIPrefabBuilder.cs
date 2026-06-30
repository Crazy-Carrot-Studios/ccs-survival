using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_AIBanditModelEnemyAIPrefabBuilder
// CATEGORY: Modules / AI / Editor / Builders
// PURPOSE: Creates production PF_CCS_AI_Bandit_Model_EnemyAI wrapper from EnemyAI import.
// PLACEMENT: Editor builder invoked before bandit visual swap.
// AUTHOR: James Schilz
// CREATED: 2026-06-29
// NOTES: Does not modify Reallusion source import assets.
// =============================================================================

namespace CCS.Modules.AI.Editor
{
    public static class CCS_AIBanditModelEnemyAIPrefabBuilder
    {
        private const string ProductionPrefabObjectName = "PF_CCS_AI_Bandit_Model_EnemyAI";

        private const float WrapperLocalYOffset = -0.079f;

        public static bool EnsureBanditModelEnemyAIPrefab()
        {
            GameObject enemyImportPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(CCS_AIConstants.EnemyAiImportPrefabPath);
            RuntimeAnimatorController locomotionController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(
                CCS_AIConstants.LocomotionAnimatorControllerPath);
            Avatar enemyAvatar = LoadEnemyAiHumanoidAvatar();

            if (enemyImportPrefab == null)
            {
                Debug.LogError(
                    "[EnemyAI Model Prefab Builder] Missing import prefab at "
                    + CCS_AIConstants.EnemyAiImportPrefabPath);
                return false;
            }

            if (locomotionController == null)
            {
                Debug.LogError("[EnemyAI Model Prefab Builder] Missing locomotion Animator Controller.");
                return false;
            }

            if (enemyAvatar == null)
            {
                Debug.LogError("[EnemyAI Model Prefab Builder] Missing EnemyAI humanoid Avatar.");
                return false;
            }

            string productionPath = CCS_AIConstants.AIBanditModelEnemyAIPrefabPath;
            string directory = Path.GetDirectoryName(productionPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            GameObject existingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(productionPath);
            if (existingPrefab != null)
            {
                return EnsureExistingProductionEnemyAiPrefab(productionPath, locomotionController, enemyAvatar);
            }

            return CreateProductionEnemyAiPrefab(productionPath, enemyImportPrefab, locomotionController, enemyAvatar);
        }

        private static Avatar LoadEnemyAiHumanoidAvatar()
        {
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(CCS_AIConstants.EnemyAiFbxPath);
            for (int i = 0; i < assets.Length; i++)
            {
                if (assets[i] is Avatar avatar && avatar.isValid && avatar.isHuman)
                {
                    return avatar;
                }
            }

            return null;
        }

        private static bool CreateProductionEnemyAiPrefab(
            string productionPath,
            GameObject enemyImportPrefab,
            RuntimeAnimatorController locomotionController,
            Avatar enemyAvatar)
        {
            GameObject wrapper = new GameObject(ProductionPrefabObjectName);
            wrapper.transform.localPosition = new Vector3(0f, WrapperLocalYOffset, 0f);

            GameObject enemyInstance = PrefabUtility.InstantiatePrefab(enemyImportPrefab, wrapper.transform) as GameObject;
            if (enemyInstance == null)
            {
                Object.DestroyImmediate(wrapper);
                Debug.LogError("[EnemyAI Model Prefab Builder] Failed to instantiate EnemyAI import prefab.");
                return false;
            }

            enemyInstance.transform.localPosition = Vector3.zero;
            enemyInstance.transform.localRotation = Quaternion.identity;
            enemyInstance.transform.localScale = Vector3.one;

            ConfigureEnemyAiAnimator(enemyInstance, locomotionController, enemyAvatar);
            CCS_AIBanditVisualEnemyAISwapBuilder.RemoveReallusionPresentationScripts(enemyInstance.transform);

            GameObject savedPrefab = PrefabUtility.SaveAsPrefabAsset(wrapper, productionPath);
            Object.DestroyImmediate(wrapper);

            if (savedPrefab == null)
            {
                Debug.LogError("[EnemyAI Model Prefab Builder] Failed to save production EnemyAI prefab.");
                return false;
            }

            Debug.Log("[EnemyAI Model Prefab Builder] Created production EnemyAI prefab at " + productionPath);
            return true;
        }

        private static bool EnsureExistingProductionEnemyAiPrefab(
            string productionPath,
            RuntimeAnimatorController locomotionController,
            Avatar enemyAvatar)
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
                changed |= ConfigureEnemyAiAnimator(animator.gameObject, locomotionController, enemyAvatar);
            }

            changed |= CCS_AIBanditVisualEnemyAISwapBuilder.RemoveReallusionPresentationScripts(contents.transform);

            if (changed)
            {
                PrefabUtility.SaveAsPrefabAsset(contents, productionPath);
            }

            PrefabUtility.UnloadPrefabContents(contents);
            return changed;
        }

        private static bool ConfigureEnemyAiAnimator(
            GameObject modelObject,
            RuntimeAnimatorController locomotionController,
            Avatar enemyAvatar)
        {
            Animator animator = modelObject.GetComponent<Animator>();
            if (animator == null)
            {
                animator = modelObject.AddComponent<Animator>();
            }

            bool changed = false;
            if (animator.avatar != enemyAvatar)
            {
                animator.avatar = enemyAvatar;
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
