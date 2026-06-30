using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CCS.Modules.Attributes;
using CCS.Modules.CharacterController;
using CCS.Modules.CharacterController.Editor;
using CCS.Project;
using UnityEditor;
using UnityEditor.Animations;
using Unity.Netcode;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_AIBanditVisualModelSwapValidationUtility
// CATEGORY: Modules / AI / Editor / Validation
// PURPOSE: Validates v0.7.7 EnemyAI bandit visual swap and Model root hierarchy.
// PLACEMENT: Editor validation utility. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-29
// =============================================================================

namespace CCS.Modules.AI.Editor
{
    public static class CCS_AIBanditVisualModelSwapValidationUtility
    {
        private const string EquipmentFitStudioWindowPath =
            "Assets/CCS/Modules/CharacterController/Editor/EquipmentFitStudio/CCS_EquipmentFitStudioWindow.cs";

        private const string CharacterControllerTestsRoot =
            "Assets/CCS/Modules/CharacterController/Tests";

        private const string AnimationFitStudioRoot =
            "Assets/CCS/Modules/CharacterController/Editor/AnimationFitStudio";

        public static CCS_SurvivalValidationResult ValidateEnemyAiBanditVisualSwap()
        {
            List<string> failures = new List<string>();
            List<string> warnings = new List<string>();

            CCS_SurvivalValidationResult playerSwapResult =
                CCS_PlayerVisualModelSwapValidationUtility.ValidateKevinPlayerVisualSwap();
            if (!playerSwapResult.IsSuccess)
            {
                failures.Add(playerSwapResult.Message);
            }

            ValidateEnemyAiProductionPrefab(failures);
            ValidateBanditModelHierarchy(failures);
            ValidateLocomotionOnlyAnimatorOnEnemyAi(failures);
            ValidateBanditGameplayComponents(failures);
            ValidateLegacyPlayerVisualDeletionPolicy(failures, warnings);
            ValidateTestsFolderRemoved(failures);
            ValidateAnimationFitStudioNotPresent(failures);
            ValidateEquipmentFitStudioPresent(failures);
            CollectAnimatorBindingWarnings(warnings);
            CollectUnwiredImportWarnings(warnings);

            if (failures.Count > 0)
            {
                return CCS_SurvivalValidationResult.Fail(string.Join(" ", failures));
            }

            string message = "EnemyAI bandit visual swap validated. Model root active on AI bandit prefab.";
            if (warnings.Count > 0)
            {
                message += " Warnings: " + string.Join(" ", warnings);
            }

            return CCS_SurvivalValidationResult.Pass(message);
        }

        private static void ValidateEnemyAiProductionPrefab(List<string> failures)
        {
            AppendIfMissing(
                failures,
                File.Exists(CCS_AIConstants.AIBanditModelEnemyAIPrefabPath),
                "Missing EnemyAI production prefab at " + CCS_AIConstants.AIBanditModelEnemyAIPrefabPath);

            GameObject enemyPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(CCS_AIConstants.AIBanditModelEnemyAIPrefabPath);
            if (enemyPrefab == null)
            {
                return;
            }

            GameObject instance = PrefabUtility.InstantiatePrefab(enemyPrefab) as GameObject;
            try
            {
                Animator animator = instance != null ? instance.GetComponentInChildren<Animator>(true) : null;
                AppendIfMissing(failures, animator != null, "EnemyAI production prefab must contain an Animator.");
                AppendIfMissing(
                    failures,
                    animator != null && animator.avatar != null && animator.avatar.isValid && animator.avatar.isHuman,
                    "EnemyAI Animator must have a valid humanoid Avatar.");

                int missingScripts = CountMissingScripts(instance);
                AppendIfMissing(failures, missingScripts == 0, "EnemyAI production prefab has missing scripts: " + missingScripts);
            }
            finally
            {
                if (instance != null)
                {
                    Object.DestroyImmediate(instance);
                }
            }
        }

        private static void ValidateBanditModelHierarchy(List<string> failures)
        {
            GameObject banditPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(CCS_AIConstants.AIBanditPrefabPath);
            AppendIfMissing(failures, banditPrefab != null, "Missing AI bandit prefab.");

            GameObject instance = banditPrefab != null
                ? PrefabUtility.InstantiatePrefab(banditPrefab) as GameObject
                : null;
            if (instance == null)
            {
                return;
            }

            try
            {
                Transform modelRoot = instance.transform.Find(CCS_EquipmentConstants.ModelRootObjectName);
                AppendIfMissing(failures, modelRoot != null, "AI bandit prefab must contain Model root.");

                Transform legacyVisualRoot = instance.transform.Find(CCS_EquipmentConstants.LegacyVisualRootObjectName);
                AppendIfMissing(failures, legacyVisualRoot == null, "AI bandit prefab must not retain legacy VisualRoot.");

                if (modelRoot != null)
                {
                    bool hasLegacyVisual = false;
                    bool hasEnemyAiVisual = ContainsEnemyAiVisualInHierarchy(modelRoot);
                    Transform[] descendants = modelRoot.GetComponentsInChildren<Transform>(true);
                    for (int i = 0; i < descendants.Length; i++)
                    {
                        if (descendants[i] == modelRoot)
                        {
                            continue;
                        }

                        if (IsLegacyPlayerVisualInstance(descendants[i].gameObject))
                        {
                            hasLegacyVisual = true;
                        }
                    }

                    AppendIfMissing(failures, !hasLegacyVisual, "Model root must not contain nested PF_CCS_Player_Visual.");
                    AppendIfMissing(
                        failures,
                        hasEnemyAiVisual,
                        "Model root must contain PF_CCS_AI_Bandit_Model_EnemyAI instance.");
                }

                int missingScripts = CountMissingScripts(instance);
                AppendIfMissing(failures, missingScripts == 0, "AI bandit prefab has missing scripts: " + missingScripts);
            }
            finally
            {
                Object.DestroyImmediate(instance);
            }
        }

        private static void ValidateLocomotionOnlyAnimatorOnEnemyAi(List<string> failures)
        {
            GameObject enemyPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(CCS_AIConstants.AIBanditModelEnemyAIPrefabPath);
            if (enemyPrefab == null)
            {
                return;
            }

            GameObject instance = PrefabUtility.InstantiatePrefab(enemyPrefab) as GameObject;
            try
            {
                Animator animator = instance != null ? instance.GetComponentInChildren<Animator>(true) : null;
                if (animator == null || animator.runtimeAnimatorController == null)
                {
                    failures.Add("EnemyAI Animator Controller is missing.");
                    return;
                }

                string expectedPath = CCS_AIConstants.LocomotionAnimatorControllerPath;
                string actualPath = AssetDatabase.GetAssetPath(animator.runtimeAnimatorController);
                AppendIfMissing(
                    failures,
                    actualPath == expectedPath,
                    "EnemyAI Animator Controller must be locomotion-only at " + expectedPath);

                AnimatorController controller = animator.runtimeAnimatorController as AnimatorController;
                if (controller == null)
                {
                    return;
                }

                AppendIfMissing(failures, controller.layers.Length == 1, "EnemyAI Animator must remain locomotion-only (one layer).");
            }
            finally
            {
                if (instance != null)
                {
                    Object.DestroyImmediate(instance);
                }
            }
        }

        private static void ValidateBanditGameplayComponents(List<string> failures)
        {
            GameObject banditPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(CCS_AIConstants.AIBanditPrefabPath);
            if (banditPrefab == null)
            {
                return;
            }

            AppendIfMissing(failures, banditPrefab.GetComponent<CCS_AIBanditBrain>() != null, "Missing CCS_AIBanditBrain.");
            AppendIfMissing(failures, banditPrefab.GetComponent<CCS_AIBanditController>() != null, "Missing CCS_AIBanditController.");
            AppendIfMissing(failures, banditPrefab.GetComponent<CCS_NetworkHealth>() != null, "Missing CCS_NetworkHealth.");
            AppendIfMissing(failures, banditPrefab.GetComponent<CCS_AIAnimatorDriver>() != null, "Missing CCS_AIAnimatorDriver.");
            AppendIfMissing(failures, banditPrefab.GetComponentInChildren<CCS_AIBanditDamageHitbox>(true) != null, "Missing CCS_AIBanditDamageHitbox.");
            AppendIfMissing(failures, banditPrefab.GetComponent<NetworkObject>() != null, "Missing NetworkObject on AI bandit.");

            CCS_SurvivalValidationResult aiResult = CCS_AIBanditValidationUtility.ValidateMilestoneB13Foundation();
            if (!aiResult.IsSuccess)
            {
                failures.Add(aiResult.Message);
            }
        }

        private static void ValidateLegacyPlayerVisualDeletionPolicy(List<string> failures, List<string> warnings)
        {
            bool legacyExists = File.Exists(CCS_AIConstants.LegacyPlayerVisualPrefabPath);
            if (!legacyExists)
            {
                return;
            }

            int referenceCount = CCS_PlayerVisualModelSwapValidationUtility.CountProjectReferencesToAsset(
                CCS_AIConstants.LegacyPlayerVisualPrefabPath);
            if (referenceCount > 0)
            {
                warnings.Add(
                    "PF_CCS_Player_Visual retained with "
                    + referenceCount
                    + " reference(s); deletion deferred.");
                return;
            }

            warnings.Add("PF_CCS_Player_Visual has zero references and may be deleted.");
        }

        private static void CollectAnimatorBindingWarnings(List<string> warnings)
        {
            AppendAnimatorBindingWarning(warnings, CCS_AIConstants.AIBanditModelEnemyAIPrefabPath, "EnemyAI");
            AppendAnimatorBindingWarning(
                warnings,
                "Assets/CCS/Modules/CharacterController/Characters/Player/Prefabs/PF_CCS_Player_Model_Kevin.prefab",
                "Kevin");
        }

        private static void AppendAnimatorBindingWarning(List<string> warnings, string prefabPath, string label)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null)
            {
                return;
            }

            GameObject instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            try
            {
                Animator animator = instance != null ? instance.GetComponentInChildren<Animator>(true) : null;
                if (animator == null || animator.avatar == null || !animator.avatar.isHuman)
                {
                    return;
                }

                warnings.Add(
                    label
                    + " uses Humanoid avatar with locomotion-only controller; generic clip binding warnings may appear in Inspector until animation rebuild milestone.");
            }
            finally
            {
                if (instance != null)
                {
                    Object.DestroyImmediate(instance);
                }
            }
        }

        private static void CollectUnwiredImportWarnings(List<string> warnings)
        {
            if (Directory.Exists(CCS_CharacterControllerConstants.CamilaImportRootPath))
            {
                warnings.Add("Camila import present but intentionally not wired in v0.7.7.");
            }
        }

        private static void ValidateTestsFolderRemoved(List<string> failures)
        {
            if (Directory.Exists(CharacterControllerTestsRoot))
            {
                failures.Add("CharacterController Tests folder must not return.");
            }
        }

        private static void ValidateAnimationFitStudioNotPresent(List<string> failures)
        {
            if (Directory.Exists(AnimationFitStudioRoot))
            {
                failures.Add("Animation Fit Studio must remain removed.");
            }
        }

        private static void ValidateEquipmentFitStudioPresent(List<string> failures)
        {
            AppendIfMissing(failures, File.Exists(EquipmentFitStudioWindowPath), "Equipment Fit Studio window missing.");
        }

        private static bool ContainsEnemyAiVisualInHierarchy(Transform modelRoot)
        {
            Transform[] descendants = modelRoot.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < descendants.Length; i++)
            {
                if (descendants[i] == modelRoot)
                {
                    continue;
                }

                if (IsEnemyAiVisualInstance(descendants[i].gameObject))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsLegacyPlayerVisualInstance(GameObject gameObject)
        {
            if (gameObject.name == "PF_CCS_Player_Visual")
            {
                return true;
            }

            GameObject source = PrefabUtility.GetCorrespondingObjectFromSource(gameObject);
            return source != null
                && AssetDatabase.GetAssetPath(source) == CCS_AIConstants.LegacyPlayerVisualPrefabPath;
        }

        private static bool IsEnemyAiVisualInstance(GameObject gameObject)
        {
            if (gameObject.name == "PF_CCS_AI_Bandit_Model_EnemyAI")
            {
                return true;
            }

            GameObject nearestInstanceRoot = PrefabUtility.GetNearestPrefabInstanceRoot(gameObject);
            if (nearestInstanceRoot != null)
            {
                GameObject sourceRoot = PrefabUtility.GetCorrespondingObjectFromSource(nearestInstanceRoot);
                if (sourceRoot != null
                    && AssetDatabase.GetAssetPath(sourceRoot) == CCS_AIConstants.AIBanditModelEnemyAIPrefabPath)
                {
                    return true;
                }
            }

            GameObject source = PrefabUtility.GetCorrespondingObjectFromSource(gameObject);
            return source != null
                && AssetDatabase.GetAssetPath(source) == CCS_AIConstants.AIBanditModelEnemyAIPrefabPath;
        }

        private static int CountMissingScripts(GameObject root)
        {
            int missing = 0;
            Component[] components = root.GetComponentsInChildren<Component>(true);
            for (int i = 0; i < components.Length; i++)
            {
                if (components[i] == null)
                {
                    missing++;
                }
            }

            return missing;
        }

        private static void AppendIfMissing(List<string> failures, bool condition, string message)
        {
            if (!condition)
            {
                failures.Add(message);
            }
        }
    }
}
