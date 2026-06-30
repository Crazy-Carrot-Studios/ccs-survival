using System.Collections.Generic;
using CCS.Modules.CharacterController.Local;
using CCS.Modules.Weapons;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_PlayerVisualKevinSwapBuilder
// CATEGORY: Modules / CharacterController / Editor / Builders
// PURPOSE: Swaps networked player VisualRoot/PF_CCS_Player_Visual to Model/PF_CCS_Player_Model_Kevin.
// PLACEMENT: Editor builder invoked from player prefab builder and visual swap batch.
// AUTHOR: James Schilz
// CREATED: 2026-06-25
// NOTES: Preserves presentation scripts on Model root; does not change gameplay root scripts.
// =============================================================================

namespace CCS.Modules.CharacterController.Editor
{
    public static class CCS_PlayerVisualKevinSwapBuilder
    {
        private const string LegacyPlayerVisualPrefabName = "PF_CCS_Player_Visual";

        private const string ProductionKevinPrefabName = "PF_CCS_Player_Model_Kevin";

        private static readonly string[] ReallusionScriptTypeNames =
        {
            "Reallusion.Import.DataLinkActorData",
            "Reallusion.Runtime.BoneDriver",
        };

        public static bool EnsureKevinModelOnNetworkedPlayerPrefab()
        {
            return EnsureKevinModelOnPlayerPrefabContentsFromPath(CCS_PlayerPrefabConstants.NetworkedPlayerPrefabPath);
        }

        public static bool EnsureKevinModelOnPlayerPrefabContents(GameObject prefabRoot)
        {
            if (prefabRoot == null)
            {
                return false;
            }

            return ApplyKevinModelSwap(prefabRoot);
        }

        private static bool EnsureKevinModelOnPlayerPrefabContentsFromPath(string prefabPath)
        {
            GameObject prefabRoot = PrefabUtility.LoadPrefabContents(prefabPath);
            if (prefabRoot == null)
            {
                Debug.LogError("[Kevin Visual Swap Builder] Could not load prefab contents: " + prefabPath);
                return false;
            }

            bool changed = ApplyKevinModelSwap(prefabRoot);
            if (changed)
            {
                PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath);
            }

            PrefabUtility.UnloadPrefabContents(prefabRoot);
            return changed;
        }

        private static bool ApplyKevinModelSwap(GameObject prefabRoot)
        {
            CCS_PlayerModelKevinPrefabBuilder.EnsurePlayerModelKevinPrefab();

            GameObject kevinVisualPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                CCS_CharacterControllerConstants.PlayerModelKevinPrefabPath);
            if (kevinVisualPrefab == null)
            {
                Debug.LogError("[Kevin Visual Swap Builder] Missing production Kevin visual prefab.");
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

            changed |= RemoveLegacyPlayerVisualChildren(modelRoot);
            changed |= EnsureKevinVisualInstance(modelRoot, kevinVisualPrefab);
            changed |= EnsureModelPresentationComponents(modelRoot);
            changed |= RemovePrototypeCapsuleAndGlassesVisuals(prefabRoot.transform);
            changed |= RemoveNestedLegacyVisualPattern(modelRoot);
            return changed;
        }

        public static bool RemovePrototypeCapsuleAndGlassesVisuals(Transform playerRoot)
        {
            if (playerRoot == null)
            {
                return false;
            }

            bool changed = false;
            Transform[] transforms = playerRoot.GetComponentsInChildren<Transform>(true);
            for (int i = transforms.Length - 1; i >= 0; i--)
            {
                Transform candidate = transforms[i];
                if (candidate == null)
                {
                    continue;
                }

                if (candidate.name == CCS_WeaponsConstants.CapsuleVisualName
                    || candidate.name == CCS_WeaponsConstants.GlassesVisualName)
                {
                    Object.DestroyImmediate(candidate.gameObject);
                    changed = true;
                }
            }

            return changed;
        }

        private static Transform CreateModelRoot(Transform playerRoot)
        {
            GameObject modelObject = new GameObject(CCS_EquipmentConstants.ModelRootObjectName);
            Transform modelRoot = modelObject.transform;
            modelRoot.SetParent(playerRoot, false);
            modelRoot.localPosition = Vector3.zero;
            modelRoot.localRotation = Quaternion.identity;
            modelRoot.localScale = Vector3.one;
            return modelRoot;
        }

        private static bool MigrateLegacyVisualRootToModel(Transform playerRoot)
        {
            Transform legacyVisualRoot = playerRoot.Find(CCS_EquipmentConstants.LegacyVisualRootObjectName);
            Transform modelRoot = playerRoot.Find(CCS_EquipmentConstants.ModelRootObjectName);
            if (legacyVisualRoot == null)
            {
                return false;
            }

            if (modelRoot != null && modelRoot != legacyVisualRoot)
            {
                Debug.LogWarning(
                    "[Kevin Visual Swap Builder] Both VisualRoot and Model exist. Manual review required.");
                return false;
            }

            legacyVisualRoot.name = CCS_EquipmentConstants.ModelRootObjectName;
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
            return sourcePath == CCS_CharacterControllerConstants.PlayerVisualPrefabPath;
        }

        private static bool EnsureKevinVisualInstance(Transform modelRoot, GameObject kevinVisualPrefab)
        {
            for (int i = 0; i < modelRoot.childCount; i++)
            {
                Transform child = modelRoot.GetChild(i);
                if (IsKevinVisualChild(child))
                {
                    return false;
                }
            }

            GameObject instance = PrefabUtility.InstantiatePrefab(kevinVisualPrefab, modelRoot) as GameObject;
            if (instance == null)
            {
                Debug.LogError("[Kevin Visual Swap Builder] Failed to instantiate PF_CCS_Player_Model_Kevin.");
                return false;
            }

            instance.name = ProductionKevinPrefabName;
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.identity;
            instance.transform.localScale = Vector3.one;
            return true;
        }

        private static bool IsKevinVisualChild(Transform child)
        {
            if (child == null)
            {
                return false;
            }

            if (string.Equals(child.name, ProductionKevinPrefabName))
            {
                return true;
            }

            GameObject source = PrefabUtility.GetCorrespondingObjectFromSource(child.gameObject);
            if (source == null)
            {
                return false;
            }

            string sourcePath = AssetDatabase.GetAssetPath(source);
            return sourcePath == CCS_CharacterControllerConstants.PlayerModelKevinPrefabPath;
        }

        private static bool RemoveNestedLegacyVisualPattern(Transform modelRoot)
        {
            bool changed = false;
            for (int i = 0; i < modelRoot.childCount; i++)
            {
                Transform child = modelRoot.GetChild(i);
                if (!IsKevinVisualChild(child))
                {
                    continue;
                }

                for (int nestedIndex = child.childCount - 1; nestedIndex >= 0; nestedIndex--)
                {
                    Transform nestedChild = child.GetChild(nestedIndex);
                    if (IsLegacyPlayerVisualChild(nestedChild))
                    {
                        Object.DestroyImmediate(nestedChild.gameObject, true);
                        changed = true;
                    }
                }
            }

            return changed;
        }

        private static bool EnsureModelPresentationComponents(Transform modelRoot)
        {
            bool changed = false;
            changed |= EnsureComponent<CCS_PlayerLocomotionAnimator>(modelRoot.gameObject);
            changed |= EnsureComponent<CCS_PlayerInteractionAnimator>(modelRoot.gameObject);
            changed |= EnsureComponent<CCS_SingleRevolverAimAnimator>(modelRoot.gameObject);
            changed |= EnsureComponent<CCS_RevolverArmReticleIK>(modelRoot.gameObject);
            changed |= EnsureComponent<CCS_RevolverBodyAimFollowController>(modelRoot.gameObject);
            return changed;
        }

        private static bool EnsureComponent<T>(GameObject target) where T : Component
        {
            if (target.GetComponent<T>() != null)
            {
                return false;
            }

            target.AddComponent<T>();
            return true;
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
