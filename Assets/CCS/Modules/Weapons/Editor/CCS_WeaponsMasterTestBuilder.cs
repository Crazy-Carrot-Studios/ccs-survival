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
            changed |= RemoveLooseRevolverSceneObjects();
            changed |= EnsureRevolverWorldPickupInScene();

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

        private static bool RemoveLooseRevolverSceneObjects()
        {
            bool changed = false;
            GameObject[] roots = SceneManager.GetActiveScene().GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                changed |= DestroyLooseRevolverHierarchy(roots[i]);
            }

            return changed;
        }

        private static bool DestroyLooseRevolverHierarchy(GameObject root)
        {
            if (root == null)
            {
                return false;
            }

            bool changed = false;
            if (IsLooseRevolverSceneObject(root))
            {
                Object.DestroyImmediate(root);
                return true;
            }

            for (int i = root.transform.childCount - 1; i >= 0; i--)
            {
                changed |= DestroyLooseRevolverHierarchy(root.transform.GetChild(i).gameObject);
            }

            return changed;
        }

        private static bool IsLooseRevolverSceneObject(GameObject sceneObject)
        {
            if (sceneObject == null || sceneObject.GetComponent<CCS_WeaponPickupInteractable>() != null)
            {
                return false;
            }

            string objectName = sceneObject.name;
            if (objectName == "ReichsrevolverM1879"
                || objectName == "ReichsrevolverM1879Shell"
                || objectName.StartsWith("ReichsrevolverM1879"))
            {
                return true;
            }

            if (objectName == "PF_CCS_RevolverM1879_Holstered_Instance"
                || objectName == "PF_CCS_RevolverM1879_Equipped_Instance")
            {
                return true;
            }

            return PrefabUtility.GetCorrespondingObjectFromSource(sceneObject) != null
                && PrefabUtility.GetCorrespondingObjectFromSource(sceneObject).name == "ReichsrevolverM1879";
        }

        private static bool EnsureRevolverWorldPickupInScene()
        {
            bool changed = false;
            Transform spawnOrigin = FindSpawnOriginInScene();
            if (spawnOrigin == null)
            {
                return false;
            }

            Vector3 position = spawnOrigin.position
                + (spawnOrigin.forward * CCS_WeaponsConstants.RevolverWorldPickupForwardDistance)
                + (spawnOrigin.right * CCS_WeaponsConstants.RevolverWorldPickupRightDistance)
                + (Vector3.up * CCS_WeaponsConstants.RevolverWorldPickupHeightOffset);
            Quaternion rotation = Quaternion.Euler(0f, spawnOrigin.eulerAngles.y + 35f, 0f);

            CCS_WeaponPickupInteractable[] existingPickups = Object.FindObjectsByType<CCS_WeaponPickupInteractable>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);
            GameObject existingPickup = null;
            for (int i = 0; i < existingPickups.Length; i++)
            {
                CCS_WeaponPickupInteractable pickup = existingPickups[i];
                if (pickup == null || !pickup.gameObject.scene.IsValid())
                {
                    continue;
                }

                if (existingPickup == null)
                {
                    existingPickup = pickup.gameObject;
                    continue;
                }

                Object.DestroyImmediate(pickup.gameObject);
                changed = true;
            }

            if (existingPickup != null)
            {
                return ApplyWorldPickupPose(existingPickup, position, rotation) || changed;
            }

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                CCS_WeaponsConstants.RevolverM1879WorldPickupPrefabPath);
            if (prefab == null)
            {
                Debug.LogError(
                    "[Weapons Builder] Missing revolver world pickup prefab at "
                    + CCS_WeaponsConstants.RevolverM1879WorldPickupPrefabPath);
                return false;
            }

            GameObject instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            if (instance == null)
            {
                return false;
            }

            instance.name = CCS_WeaponsConstants.RevolverM1879WorldPickupInstanceName;
            ApplyWorldPickupPose(instance, position, rotation);
            return true;
        }

        private static bool ApplyWorldPickupPose(GameObject pickupObject, Vector3 position, Quaternion rotation)
        {
            if (pickupObject == null)
            {
                return false;
            }

            bool changed = false;
            Transform pickupTransform = pickupObject.transform;
            if (pickupTransform.position != position)
            {
                pickupTransform.position = position;
                changed = true;
            }

            if (pickupTransform.rotation != rotation)
            {
                pickupTransform.rotation = rotation;
                changed = true;
            }

            if (pickupObject.name != CCS_WeaponsConstants.RevolverM1879WorldPickupInstanceName)
            {
                pickupObject.name = CCS_WeaponsConstants.RevolverM1879WorldPickupInstanceName;
                changed = true;
            }

            return changed;
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
