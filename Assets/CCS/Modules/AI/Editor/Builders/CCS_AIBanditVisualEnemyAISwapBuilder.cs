using System.Collections.Generic;
using CCS.Modules.CharacterController;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_AIBanditVisualEnemyAISwapBuilder
// CATEGORY: Modules / AI / Editor / Builders
// PURPOSE: Swaps AI bandit VisualRoot/PF_CCS_Player_Visual to Model/PF_CCS_AI_Bandit_Model_EnemyAI.
// PLACEMENT: Editor builder invoked from AI bandit prefab builder and visual swap batch.
// AUTHOR: James Schilz
// CREATED: 2026-06-29
// NOTES: Removes player presentation artifacts copied from networked player template.
// =============================================================================

namespace CCS.Modules.AI.Editor
{
    public static class CCS_AIBanditVisualEnemyAISwapBuilder
    {
        private const string LegacyPlayerVisualPrefabName = "PF_CCS_Player_Visual";

        private const string ProductionEnemyAiPrefabName = "PF_CCS_AI_Bandit_Model_EnemyAI";

        private static readonly string[] PlayerPresentationChildNames =
        {
            "WeaponRoot",
            "CCS_WeaponIKTargets",
            "CCS_RevolverArmReticleIKRoot",
            "CCS_FirstPersonHeadlessBody",
        };

        private static readonly string[] ReallusionScriptTypeNames =
        {
            "Reallusion.Import.DataLinkActorData",
            "Reallusion.Runtime.BoneDriver",
        };

        public static bool EnsureEnemyAiModelOnBanditPrefab()
        {
            GameObject prefabRoot = PrefabUtility.LoadPrefabContents(CCS_AIConstants.AIBanditPrefabPath);
            if (prefabRoot == null)
            {
                Debug.LogError("[EnemyAI Visual Swap Builder] Could not load bandit prefab contents.");
                return false;
            }

            bool changed = ApplyEnemyAiModelSwap(prefabRoot);
            if (changed)
            {
                PrefabUtility.SaveAsPrefabAsset(prefabRoot, CCS_AIConstants.AIBanditPrefabPath);
            }

            PrefabUtility.UnloadPrefabContents(prefabRoot);
            return changed;
        }

        public static bool ApplyEnemyAiModelSwap(GameObject prefabRoot)
        {
            CCS_AIBanditModelEnemyAIPrefabBuilder.EnsureBanditModelEnemyAIPrefab();

            GameObject enemyVisualPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(CCS_AIConstants.AIBanditModelEnemyAIPrefabPath);
            if (enemyVisualPrefab == null)
            {
                Debug.LogError("[EnemyAI Visual Swap Builder] Missing production EnemyAI visual prefab.");
                return false;
            }

            bool changed = false;
            changed |= MigrateLegacyVisualRootToModel(prefabRoot.transform);
            Transform modelRoot = CCS_PlayerModelRootUtility.FindModelRoot(prefabRoot.transform);
            if (modelRoot == null)
            {
                modelRoot = CreateModelRoot(prefabRoot.transform);
                changed = true;
            }

            changed |= RemovePlayerPresentationArtifacts(modelRoot);
            changed |= RemoveLegacyPlayerVisualChildren(modelRoot);
            changed |= EnsureEnemyAiVisualInstance(modelRoot, enemyVisualPrefab);
            return changed;
        }

        private static Transform CreateModelRoot(Transform banditRoot)
        {
            GameObject modelObject = new GameObject(CCS_EquipmentConstants.ModelRootObjectName);
            Transform modelRoot = modelObject.transform;
            modelRoot.SetParent(banditRoot, false);
            modelRoot.localPosition = Vector3.zero;
            modelRoot.localRotation = Quaternion.identity;
            modelRoot.localScale = Vector3.one;
            return modelRoot;
        }

        private static bool MigrateLegacyVisualRootToModel(Transform banditRoot)
        {
            Transform legacyVisualRoot = banditRoot.Find(CCS_EquipmentConstants.LegacyVisualRootObjectName);
            Transform modelRoot = banditRoot.Find(CCS_EquipmentConstants.ModelRootObjectName);
            if (legacyVisualRoot == null)
            {
                return false;
            }

            if (modelRoot != null && modelRoot != legacyVisualRoot)
            {
                Debug.LogWarning("[EnemyAI Visual Swap Builder] Both VisualRoot and Model exist on bandit prefab.");
                return false;
            }

            legacyVisualRoot.name = CCS_EquipmentConstants.ModelRootObjectName;
            return true;
        }

        private static bool RemovePlayerPresentationArtifacts(Transform modelRoot)
        {
            bool changed = false;

            for (int i = 0; i < PlayerPresentationChildNames.Length; i++)
            {
                Transform child = modelRoot.Find(PlayerPresentationChildNames[i]);
                if (child != null)
                {
                    Object.DestroyImmediate(child.gameObject, true);
                    changed = true;
                }
            }

            changed |= DestroyIfPresent<CCS.Modules.Weapons.CCS_RevolverArmReticleIK>(modelRoot.gameObject);
            changed |= DestroyIfPresent<CCS.Modules.Weapons.CCS_RevolverBodyAimFollowController>(modelRoot.gameObject);
            changed |= DestroyIfPresent<CCS_PlayerLocomotionAnimator>(modelRoot.gameObject);
            changed |= DestroyIfPresent<CCS_PlayerInteractionAnimator>(modelRoot.gameObject);
            return changed;
        }

        private static bool DestroyIfPresent<T>(GameObject target) where T : Component
        {
            T component = target.GetComponent<T>();
            if (component == null)
            {
                return false;
            }

            Object.DestroyImmediate(component, true);
            return true;
        }

        private static bool RemoveLegacyPlayerVisualChildren(Transform modelRoot)
        {
            bool changed = false;
            List<GameObject> toDestroy = new List<GameObject>();

            for (int i = 0; i < modelRoot.childCount; i++)
            {
                Transform child = modelRoot.GetChild(i);
                if (IsLegacyPlayerVisualChild(child))
                {
                    toDestroy.Add(child.gameObject);
                }
            }

            for (int i = 0; i < toDestroy.Count; i++)
            {
                Object.DestroyImmediate(toDestroy[i], true);
                changed = true;
            }

            return changed;
        }

        private static bool IsLegacyPlayerVisualChild(Transform child)
        {
            if (child == null)
            {
                return false;
            }

            if (string.Equals(child.name, LegacyPlayerVisualPrefabName))
            {
                return true;
            }

            GameObject source = PrefabUtility.GetCorrespondingObjectFromSource(child.gameObject);
            if (source == null)
            {
                return false;
            }

            string sourcePath = AssetDatabase.GetAssetPath(source);
            return sourcePath == CCS_AIConstants.LegacyPlayerVisualPrefabPath;
        }

        private static bool EnsureEnemyAiVisualInstance(Transform modelRoot, GameObject enemyVisualPrefab)
        {
            for (int i = 0; i < modelRoot.childCount; i++)
            {
                Transform child = modelRoot.GetChild(i);
                if (IsEnemyAiVisualChild(child))
                {
                    return false;
                }
            }

            GameObject instance = PrefabUtility.InstantiatePrefab(enemyVisualPrefab, modelRoot) as GameObject;
            if (instance == null)
            {
                Debug.LogError("[EnemyAI Visual Swap Builder] Failed to instantiate PF_CCS_AI_Bandit_Model_EnemyAI.");
                return false;
            }

            instance.name = ProductionEnemyAiPrefabName;
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.identity;
            instance.transform.localScale = Vector3.one;
            return true;
        }

        private static bool IsEnemyAiVisualChild(Transform child)
        {
            if (child == null)
            {
                return false;
            }

            if (string.Equals(child.name, ProductionEnemyAiPrefabName))
            {
                return true;
            }

            GameObject source = PrefabUtility.GetCorrespondingObjectFromSource(child.gameObject);
            if (source == null)
            {
                return false;
            }

            return AssetDatabase.GetAssetPath(source) == CCS_AIConstants.AIBanditModelEnemyAIPrefabPath;
        }

        public static bool RemoveReallusionPresentationScripts(Transform root)
        {
            if (root == null)
            {
                return false;
            }

            bool changed = false;
            MonoBehaviour[] behaviours = root.GetComponentsInChildren<MonoBehaviour>(true);
            for (int i = 0; i < behaviours.Length; i++)
            {
                MonoBehaviour behaviour = behaviours[i];
                if (behaviour == null)
                {
                    continue;
                }

                string typeName = behaviour.GetType().FullName;
                for (int scriptIndex = 0; scriptIndex < ReallusionScriptTypeNames.Length; scriptIndex++)
                {
                    if (typeName == ReallusionScriptTypeNames[scriptIndex])
                    {
                        Object.DestroyImmediate(behaviour, true);
                        changed = true;
                        break;
                    }
                }
            }

            return changed;
        }
    }
}
