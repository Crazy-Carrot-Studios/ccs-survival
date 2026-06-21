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
            GameObject existingTarget = FindDamageTargetInScene();
            if (existingTarget != null)
            {
                return false;
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

            Transform spawnOrigin = FindSpawnOriginInScene();
            if (spawnOrigin == null)
            {
                Debug.LogError(
                    "[Weapons Builder] Missing spawn origin: "
                    + CCS_WeaponsConstants.MasterTestSpawnOriginObjectPath);
                return false;
            }

            Vector3 forward = spawnOrigin.forward;
            Vector3 right = spawnOrigin.right;
            Vector3 position = spawnOrigin.position
                + (forward * CCS_WeaponsConstants.TestDamageTargetForwardDistance)
                + (right * CCS_WeaponsConstants.TestDamageTargetLateralOffset)
                + (Vector3.up * CCS_WeaponsConstants.TestDamageTargetHeightOffset);

            GameObject instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            if (instance == null)
            {
                return false;
            }

            instance.name = CCS_WeaponsConstants.TestDamageTargetObjectName;
            instance.transform.SetPositionAndRotation(position, Quaternion.LookRotation(-forward, Vector3.up));
            return true;
        }

        private static GameObject FindDamageTargetInScene()
        {
            CCS_TestDamageTarget[] targets = Object.FindObjectsByType<CCS_TestDamageTarget>(FindObjectsSortMode.None);
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
