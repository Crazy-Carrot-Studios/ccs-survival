using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// =============================================================================
// SCRIPT: CCS_WeaponsMasterTestBuilder
// CATEGORY: Modules / Weapons / Editor
// PURPOSE: Places test weapon damage target in Master Test scene at safe range.
// PLACEMENT: Editor utility invoked from Weapons validation and master test setup.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Keeps weapon test content separate from interaction pickup/door flow.
// =============================================================================

namespace CCS.Modules.Weapons.Editor
{
    public static class CCS_WeaponsMasterTestBuilder
    {
        #region Public Methods

        public static bool EnsureMasterTestWeaponTarget()
        {
            bool changed = false;
            changed |= CCS_WeaponsAssetBuilder.EnsureWeaponsAssets();
            changed |= CCS_WeaponsTestPlayerPrefabBuilder.EnsureTestPlayerWeaponWiring();

            Scene scene = EditorSceneManager.OpenScene(
                CCS_WeaponsConstants.MasterTestScenePath,
                OpenSceneMode.Single);
            if (!scene.IsValid())
            {
                Debug.LogError(
                    "[Weapons Builder] Could not open "
                    + CCS_WeaponsConstants.MasterTestScenePath);
                return changed;
            }

            changed |= EnsureDamageTargetInScene();

            if (changed)
            {
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
            }

            return changed;
        }

        #endregion

        #region Private Methods

        private static bool EnsureDamageTargetInScene()
        {
            Transform spawnOrigin = FindSpawnOriginInScene();
            if (spawnOrigin == null)
            {
                Debug.LogError(
                    "[Weapons Builder] Missing spawn origin: "
                    + CCS_WeaponsConstants.MasterTestSpawnOriginObjectPath);
                return false;
            }

            if (!TryGetDamageTargetWorldPose(spawnOrigin, out Vector3 position, out Quaternion rotation))
            {
                return false;
            }

            GameObject existingTarget = FindDamageTargetInScene();
            if (existingTarget != null)
            {
                return ApplyDamageTargetWorldPose(existingTarget, position, rotation);
            }

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                CCS_WeaponsConstants.TestDamageTargetPrefabPath);
            if (prefab == null)
            {
                Debug.LogError(
                    "[Weapons Builder] Missing damage target prefab at "
                    + CCS_WeaponsConstants.TestDamageTargetPrefabPath);
                return false;
            }

            GameObject instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            if (instance == null)
            {
                return false;
            }

            instance.name = CCS_WeaponsConstants.TestDamageTargetObjectName;
            ApplyDamageTargetWorldPose(instance, position, rotation);
            return true;
        }

        private static bool TryGetDamageTargetWorldPose(
            Transform spawnOrigin,
            out Vector3 position,
            out Quaternion rotation)
        {
            position = Vector3.zero;
            rotation = Quaternion.identity;

            if (spawnOrigin == null)
            {
                return false;
            }

            Vector3 forward = spawnOrigin.forward;
            Vector3 right = spawnOrigin.right;
            position = spawnOrigin.position
                + (forward * CCS_WeaponsConstants.TestDamageTargetForwardDistance)
                + (right * CCS_WeaponsConstants.TestDamageTargetLateralOffset)
                + (Vector3.up * CCS_WeaponsConstants.TestDamageTargetHeightOffset);
            rotation = Quaternion.LookRotation(-forward, Vector3.up);
            return true;
        }

        private static bool ApplyDamageTargetWorldPose(
            GameObject targetObject,
            Vector3 position,
            Quaternion rotation)
        {
            if (targetObject == null)
            {
                return false;
            }

            bool changed = false;
            Transform targetTransform = targetObject.transform;
            if (targetTransform.position != position)
            {
                targetTransform.position = position;
                changed = true;
            }

            if (targetTransform.rotation != rotation)
            {
                targetTransform.rotation = rotation;
                changed = true;
            }

            if (targetObject.name != CCS_WeaponsConstants.TestDamageTargetObjectName)
            {
                targetObject.name = CCS_WeaponsConstants.TestDamageTargetObjectName;
                changed = true;
            }

            return changed;
        }

        private static GameObject FindDamageTargetInScene()
        {
            CCS_TestDamageTarget[] targets = Object.FindObjectsByType<CCS_TestDamageTarget>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);
            for (int i = 0; i < targets.Length; i++)
            {
                CCS_TestDamageTarget target = targets[i];
                if (target != null && target.gameObject.scene.IsValid())
                {
                    return target.gameObject;
                }
            }

            return null;
        }

        private static Transform FindSpawnOriginInScene()
        {
            GameObject[] roots = SceneManager.GetActiveScene().GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                Transform match = FindDeepChild(roots[i].transform, "TP_Spawn_Host");
                if (match != null)
                {
                    return match;
                }
            }

            return null;
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

        #endregion
    }
}
