using System.Collections.Generic;
using System.IO;
using System.Linq;
using CCS.Modules.CharacterController.Local;
using CCS.Project;
using UnityEditor;
using UnityEditor.Animations;
using Unity.Netcode;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_PlayerVisualModelSwapValidationUtility
// CATEGORY: Modules / CharacterController / Editor / Validation
// PURPOSE: Validates v0.7.6 Kevin player visual swap and Model root hierarchy.
// PLACEMENT: Editor validation utility. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-25
// =============================================================================

namespace CCS.Modules.CharacterController.Editor
{
    public static class CCS_PlayerVisualModelSwapValidationUtility
    {
        private const string EquipmentFitStudioWindowPath =
            "Assets/CCS/Modules/CharacterController/Editor/EquipmentFitStudio/CCS_EquipmentFitStudioWindow.cs";

        private const string CharacterControllerTestsRoot =
            "Assets/CCS/Modules/CharacterController/Tests";

        private const string AnimationFitStudioRoot =
            "Assets/CCS/Modules/CharacterController/Editor/AnimationFitStudio";

        public static CCS_SurvivalValidationResult ValidateKevinPlayerVisualSwap()
        {
            List<string> failures = new List<string>();
            List<string> warnings = new List<string>();

            CCS_SurvivalValidationResult phase3DResult =
                CCS_CharacterControllerPhase3DValidationUtility.ValidatePhase3DPlayerPrefabHierarchyArchitecture();
            if (!phase3DResult.IsSuccess)
            {
                failures.Add(phase3DResult.Message);
            }

            ValidateKevinProductionPrefab(failures);
            ValidateNetworkedPlayerModelHierarchy(failures);
            ValidateLocomotionOnlyAnimatorOnKevin(failures);
            ValidateRequiredSockets(failures);
            ValidateNetworkManagerPlayerPrefabReference(failures);
            ValidateTestsFolderRemoved(failures);
            ValidateAnimationFitStudioNotPresent(failures);
            ValidateEquipmentFitStudioPresent(failures);
            CollectDeferredDeletionWarnings(warnings);
            CollectUnwiredImportWarnings(warnings);

            if (failures.Count > 0)
            {
                return CCS_SurvivalValidationResult.Fail(string.Join(" ", failures));
            }

            string message = "Kevin player visual swap validated. Model root active on networked player prefab.";
            if (warnings.Count > 0)
            {
                message += " Warnings: " + string.Join(" ", warnings);
            }

            return CCS_SurvivalValidationResult.Pass(message);
        }

        private static void ValidateKevinProductionPrefab(List<string> failures)
        {
            AppendIfMissing(
                failures,
                File.Exists(CCS_CharacterControllerConstants.PlayerModelKevinPrefabPath),
                "Missing Kevin production prefab at " + CCS_CharacterControllerConstants.PlayerModelKevinPrefabPath);

            GameObject kevinPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                CCS_CharacterControllerConstants.PlayerModelKevinPrefabPath);
            if (kevinPrefab == null)
            {
                return;
            }

            GameObject instance = PrefabUtility.InstantiatePrefab(kevinPrefab) as GameObject;
            try
            {
                Animator animator = instance != null ? instance.GetComponentInChildren<Animator>(true) : null;
                AppendIfMissing(failures, animator != null, "Kevin production prefab must contain an Animator.");
                if (animator == null)
                {
                    return;
                }

                AppendIfMissing(
                    failures,
                    animator.avatar != null && animator.avatar.isValid && animator.avatar.isHuman,
                    "Kevin Animator must have a valid humanoid Avatar.");

                int missingScripts = CountMissingScripts(instance);
                AppendIfMissing(failures, missingScripts == 0, "Kevin production prefab has missing scripts: " + missingScripts);
            }
            finally
            {
                if (instance != null)
                {
                    Object.DestroyImmediate(instance);
                }
            }
        }

        private static void ValidateNetworkedPlayerModelHierarchy(List<string> failures)
        {
            GameObject playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                CCS_PlayerPrefabConstants.NetworkedPlayerPrefabPath);
            AppendIfMissing(failures, playerPrefab != null, "Missing networked player prefab.");

            GameObject instance = playerPrefab != null
                ? PrefabUtility.InstantiatePrefab(playerPrefab) as GameObject
                : null;
            if (instance == null)
            {
                return;
            }

            try
            {
                Transform modelRoot = instance.transform.Find(CCS_EquipmentConstants.ModelRootObjectName);
                AppendIfMissing(failures, modelRoot != null, "Networked player prefab must contain Model root.");

                Transform legacyVisualRoot = instance.transform.Find(CCS_EquipmentConstants.LegacyVisualRootObjectName);
                AppendIfMissing(
                    failures,
                    legacyVisualRoot == null,
                    "Networked player prefab must not retain legacy VisualRoot object.");

                if (modelRoot != null)
                {
                    bool hasLegacyNestedVisual = false;
                    bool hasKevinVisual = ContainsKevinVisualInHierarchy(modelRoot);
                    Transform[] descendants = modelRoot.GetComponentsInChildren<Transform>(true);
                    for (int i = 0; i < descendants.Length; i++)
                    {
                        Transform child = descendants[i];
                        if (child == modelRoot)
                        {
                            continue;
                        }

                        if (IsLegacyPlayerVisualInstance(child.gameObject))
                        {
                            hasLegacyNestedVisual = true;
                        }
                    }

                    AppendIfMissing(
                        failures,
                        !hasLegacyNestedVisual,
                        "Model root must not contain nested PF_CCS_Player_Visual.");
                    AppendIfMissing(
                        failures,
                        hasKevinVisual,
                        "Model root must contain PF_CCS_Player_Model_Kevin instance.");
                }

                int missingScripts = CountMissingScripts(instance);
                AppendIfMissing(failures, missingScripts == 0, "Networked player prefab has missing scripts: " + missingScripts);
            }
            finally
            {
                Object.DestroyImmediate(instance);
            }
        }

        private static void ValidateLocomotionOnlyAnimatorOnKevin(List<string> failures)
        {
            GameObject kevinPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                CCS_CharacterControllerConstants.PlayerModelKevinPrefabPath);
            if (kevinPrefab == null)
            {
                return;
            }

            GameObject instance = PrefabUtility.InstantiatePrefab(kevinPrefab) as GameObject;
            try
            {
                Animator animator = instance != null ? instance.GetComponentInChildren<Animator>(true) : null;
                if (animator == null || animator.runtimeAnimatorController == null)
                {
                    failures.Add("Kevin Animator Controller is missing.");
                    return;
                }

                string expectedPath = CCS_CharacterControllerConstants.PlayerLocomotionAnimatorControllerPath;
                string actualPath = AssetDatabase.GetAssetPath(animator.runtimeAnimatorController);
                AppendIfMissing(
                    failures,
                    actualPath == expectedPath,
                    "Kevin Animator Controller must be locomotion-only at " + expectedPath + " (found " + actualPath + ").");

                AnimatorController controller = animator.runtimeAnimatorController as AnimatorController;
                if (controller == null)
                {
                    return;
                }

                AppendIfMissing(failures, controller.layers.Length == 1, "Kevin Animator must remain locomotion-only (one layer).");
                if (controller.layers.Length > 0)
                {
                    AppendIfMissing(
                        failures,
                        controller.layers[0].name == "Base Layer",
                        "Kevin Animator base layer must remain Base Layer only.");
                }

                for (int i = 0; i < controller.parameters.Length; i++)
                {
                    string parameterName = controller.parameters[i].name;
                    if (parameterName.Contains("Revolver") || parameterName.Contains("Interaction"))
                    {
                        failures.Add("Kevin Animator must not contain weapon/interaction parameters: " + parameterName);
                    }
                }
            }
            finally
            {
                if (instance != null)
                {
                    Object.DestroyImmediate(instance);
                }
            }
        }

        private static void ValidateRequiredSockets(List<string> failures)
        {
            GameObject playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                CCS_PlayerPrefabConstants.NetworkedPlayerPrefabPath);
            if (playerPrefab == null)
            {
                return;
            }

            GameObject instance = PrefabUtility.InstantiatePrefab(playerPrefab) as GameObject;
            try
            {
                CCS_EquipmentSocketRegistry registry = instance.GetComponentInChildren<CCS_EquipmentSocketRegistry>(true);
                AppendIfMissing(failures, registry != null, "Player prefab must contain CCS_EquipmentSocketRegistry.");

                for (int i = 0; i < CCS_EquipmentConstants.RequiredSocketIds.Length; i++)
                {
                    string socketId = CCS_EquipmentConstants.RequiredSocketIds[i];
                    CCS_EquipmentSocketAnchor anchor = instance.GetComponentsInChildren<CCS_EquipmentSocketAnchor>(true)
                        .FirstOrDefault(candidate => candidate != null && candidate.SocketId == socketId);
                    AppendIfMissing(failures, anchor != null, "Missing equipment socket: " + socketId);
                }
            }
            finally
            {
                if (instance != null)
                {
                    Object.DestroyImmediate(instance);
                }
            }
        }

        private static void ValidateNetworkManagerPlayerPrefabReference(List<string> failures)
        {
            CCS_SurvivalValidationResult result =
                CCS_CharacterControllerPlayerPrefabAuditUtility.ValidateNetworkManagerPlayerPrefabReference();
            if (!result.IsSuccess)
            {
                failures.Add(result.Message);
            }
        }

        private static void ValidateTestsFolderRemoved(List<string> failures)
        {
            if (Directory.Exists(CharacterControllerTestsRoot))
            {
                failures.Add("CharacterController Tests folder must not return: " + CharacterControllerTestsRoot);
            }
        }

        private static void ValidateAnimationFitStudioNotPresent(List<string> failures)
        {
            if (Directory.Exists(AnimationFitStudioRoot))
            {
                failures.Add("Animation Fit Studio must remain removed: " + AnimationFitStudioRoot);
            }
        }

        private static void ValidateEquipmentFitStudioPresent(List<string> failures)
        {
            AppendIfMissing(
                failures,
                File.Exists(EquipmentFitStudioWindowPath),
                "Equipment Fit Studio window missing at " + EquipmentFitStudioWindowPath);
        }

        private static void CollectDeferredDeletionWarnings(List<string> warnings)
        {
            if (!File.Exists(CCS_CharacterControllerConstants.PlayerVisualPrefabPath))
            {
                return;
            }

            int referenceCount = CountProjectReferencesToAsset(CCS_CharacterControllerConstants.PlayerVisualPrefabPath);
            if (referenceCount > 0)
            {
                warnings.Add(
                    "PF_CCS_Player_Visual retained with "
                    + referenceCount
                    + " reference(s); deletion deferred until references are removed.");
            }
        }

        private static void CollectUnwiredImportWarnings(List<string> warnings)
        {
            if (Directory.Exists(CCS_CharacterControllerConstants.EnemyAiImportRootPath))
            {
                warnings.Add("EnemyAI import present but intentionally not wired in v0.7.6.");
            }

            if (Directory.Exists(CCS_CharacterControllerConstants.CamilaImportRootPath))
            {
                warnings.Add("Camila import present but intentionally not wired in v0.7.6.");
            }

            GameObject kevinImport = AssetDatabase.LoadAssetAtPath<GameObject>(
                CCS_CharacterControllerConstants.KevinImportPrefabPath);
            if (kevinImport != null)
            {
                GameObject temp = PrefabUtility.InstantiatePrefab(kevinImport) as GameObject;
                try
                {
                    int materialCount = temp != null
                        ? temp.GetComponentsInChildren<Renderer>(true).SelectMany(renderer => renderer.sharedMaterials).Count(material => material != null)
                        : 0;
                    if (materialCount > 12)
                    {
                        warnings.Add("Kevin material count is high (" + materialCount + ").");
                    }
                }
                finally
                {
                    if (temp != null)
                    {
                        Object.DestroyImmediate(temp);
                    }
                }
            }
        }

        public static int CountProjectReferencesToAsset(string assetPath)
        {
            if (!File.Exists(assetPath))
            {
                return 0;
            }

            string[] candidateGuids = AssetDatabase.FindAssets("t:Prefab t:Scene t:ScriptableObject");
            int count = 0;
            for (int i = 0; i < candidateGuids.Length; i++)
            {
                string candidatePath = AssetDatabase.GUIDToAssetPath(candidateGuids[i]);
                if (string.IsNullOrEmpty(candidatePath) || candidatePath == assetPath)
                {
                    continue;
                }

                string[] dependencies = AssetDatabase.GetDependencies(candidatePath, false);
                for (int dependencyIndex = 0; dependencyIndex < dependencies.Length; dependencyIndex++)
                {
                    if (dependencies[dependencyIndex] == assetPath)
                    {
                        count++;
                        break;
                    }
                }
            }

            return count;
        }

        private static bool IsLegacyPlayerVisualInstance(GameObject gameObject)
        {
            GameObject source = PrefabUtility.GetCorrespondingObjectFromSource(gameObject);
            if (source == null)
            {
                return gameObject.name == "PF_CCS_Player_Visual";
            }

            return AssetDatabase.GetAssetPath(source) == CCS_CharacterControllerConstants.PlayerVisualPrefabPath;
        }

        private static bool ContainsKevinVisualInHierarchy(Transform modelRoot)
        {
            Transform[] descendants = modelRoot.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < descendants.Length; i++)
            {
                if (descendants[i] == modelRoot)
                {
                    continue;
                }

                if (IsKevinVisualInstance(descendants[i].gameObject))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsKevinVisualInstance(GameObject gameObject)
        {
            if (gameObject.name == "PF_CCS_Player_Model_Kevin")
            {
                return true;
            }

            GameObject nearestInstanceRoot = PrefabUtility.GetNearestPrefabInstanceRoot(gameObject);
            if (nearestInstanceRoot != null)
            {
                GameObject sourceRoot = PrefabUtility.GetCorrespondingObjectFromSource(nearestInstanceRoot);
                if (sourceRoot != null
                    && AssetDatabase.GetAssetPath(sourceRoot) == CCS_CharacterControllerConstants.PlayerModelKevinPrefabPath)
                {
                    return true;
                }
            }

            GameObject source = PrefabUtility.GetCorrespondingObjectFromSource(gameObject);
            if (source == null)
            {
                return false;
            }

            return AssetDatabase.GetAssetPath(source) == CCS_CharacterControllerConstants.PlayerModelKevinPrefabPath;
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
