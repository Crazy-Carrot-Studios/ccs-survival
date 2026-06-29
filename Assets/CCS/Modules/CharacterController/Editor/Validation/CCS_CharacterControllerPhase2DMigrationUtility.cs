using System.IO;
using CCS.Modules.Attributes.Tests;
using CCS.Modules.CharacterController.Diagnostics;
using CCS.Modules.CharacterController.Local;
using CCS.Project;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// =============================================================================
// SCRIPT: CCS_CharacterControllerPhase2DMigrationUtility
// CATEGORY: Modules / CharacterController / Editor / Validation
// PURPOSE: Applies Phase 2D test-only separation (v0.7.1f) to scenes and prefabs.
// PLACEMENT: Editor migration utility. Invoked from Master Test builder/batch paths.
// AUTHOR: James Schilz
// CREATED: 2026-06-29
// NOTES: Safe, incremental migration only. No gameplay rewrite.
// =============================================================================

namespace CCS.Modules.CharacterController.Editor
{
    public static class CCS_CharacterControllerPhase2DMigrationUtility
    {
        private static readonly string[] LegacyTestingManagerTypeNames =
        {
            "CCS_MasterTestSceneTestingManager",
            "CCS_CharacterControllerTestingManager",
        };

        public static bool ApplyPhase2DSeparation(Scene masterTestScene)
        {
            if (!masterTestScene.IsValid())
            {
                return false;
            }

            bool changed = false;
            changed |= MigrateTestingManagerComponent(masterTestScene);
            changed |= EnsureSceneTestOnlyComponents(masterTestScene);
            changed |= RemoveTestOnlyRootComponentsFromNetworkedPlayerPrefab();
            if (changed && masterTestScene.isLoaded)
            {
                EditorSceneManager.MarkSceneDirty(masterTestScene);
            }

            return changed;
        }

        public static bool MigrateTestingManagerComponent(Scene masterTestScene)
        {
            GameObject testingManagerObject = FindRootObject(
                masterTestScene,
                CCS_ProjectAudioConstants.MasterTestTestingManagerObjectName);
            if (testingManagerObject == null)
            {
                return false;
            }

            bool changed = false;
            MonoBehaviour legacyWrapper = FindLegacyTestingManagerWrapper(testingManagerObject);
            if (legacyWrapper != null)
            {
                CCS_CharacterControllerDiagnosticsManager manager =
                    testingManagerObject.GetComponent<CCS_CharacterControllerDiagnosticsManager>();
                if (manager == null)
                {
                    manager = testingManagerObject.AddComponent<CCS_CharacterControllerDiagnosticsManager>();
                }

                EditorUtility.CopySerialized(legacyWrapper, manager);
                Object.DestroyImmediate(legacyWrapper, true);
                changed = true;
            }

            if (GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(testingManagerObject) > 0)
            {
                GameObjectUtility.RemoveMonoBehavioursWithMissingScript(testingManagerObject);
                changed = true;
            }

            changed |= EnsureDirectTestingManagerComponent(testingManagerObject);
            return changed;
        }

        public static bool EnsureSceneTestOnlyComponents(Scene masterTestScene)
        {
            GameObject testingManagerObject = FindRootObject(
                masterTestScene,
                CCS_ProjectAudioConstants.MasterTestTestingManagerObjectName);
            if (testingManagerObject == null)
            {
                return false;
            }

            bool changed = false;
            if (testingManagerObject.GetComponent<CCS_LocalPlayerOfflineBootstrapper>() == null)
            {
                testingManagerObject.AddComponent<CCS_LocalPlayerOfflineBootstrapper>();
                changed = true;
            }

            if (testingManagerObject.GetComponent<CCS_PlayerDiagnosticsInputRouter>() == null)
            {
                testingManagerObject.AddComponent<CCS_PlayerDiagnosticsInputRouter>();
                changed = true;
            }

            CCS_PlayerDisplayProfile displayProfile = AssetDatabase.LoadAssetAtPath<CCS_PlayerDisplayProfile>(
                CCS_PlayerPrefabConstants.DefaultDisplayProfilePath);
            CCS_LocalPlayerOfflineBootstrapper bootstrapper =
                testingManagerObject.GetComponent<CCS_LocalPlayerOfflineBootstrapper>();
            if (bootstrapper != null && displayProfile != null)
            {
                SerializedObject serializedBootstrapper = new SerializedObject(bootstrapper);
                SerializedProperty profileProperty = serializedBootstrapper.FindProperty("displayProfile");
                if (profileProperty != null && profileProperty.objectReferenceValue != displayProfile)
                {
                    profileProperty.objectReferenceValue = displayProfile;
                    serializedBootstrapper.ApplyModifiedPropertiesWithoutUndo();
                    changed = true;
                }
            }

            return changed;
        }

        public static bool RemoveTestOnlyRootComponentsFromNetworkedPlayerPrefab()
        {
            string prefabPath = CCS_PlayerPrefabConstants.NetworkedPlayerPrefabPath;
            if (!File.Exists(prefabPath))
            {
                return false;
            }

            GameObject prefabRoot = PrefabUtility.LoadPrefabContents(prefabPath);
            if (prefabRoot == null)
            {
                return false;
            }

            bool changed = false;
            changed |= DestroyComponentIfPresent<CCS_LocalPlayerOfflineBootstrap>(prefabRoot);
            changed |= DestroyComponentIfPresent<CCS_TestPlayerAttributeDebugInput>(prefabRoot);

            if (changed)
            {
                PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath);
            }

            PrefabUtility.UnloadPrefabContents(prefabRoot);
            return changed;
        }

        private static bool EnsureDirectTestingManagerComponent(GameObject testingManagerObject)
        {
            if (testingManagerObject.GetComponent<CCS_CharacterControllerDiagnosticsManager>() != null)
            {
                return false;
            }

            testingManagerObject.AddComponent<CCS_CharacterControllerDiagnosticsManager>();
            return true;
        }

        private static GameObject FindRootObject(Scene scene, string objectName)
        {
            GameObject[] roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                GameObject root = roots[i];
                if (root != null && root.name == objectName)
                {
                    return root;
                }
            }

            return null;
        }

        private static bool DestroyComponentIfPresent<T>(GameObject target) where T : Component
        {
            T component = target.GetComponent<T>();
            if (component == null)
            {
                return false;
            }

            Object.DestroyImmediate(component, true);
            return true;
        }

        private static MonoBehaviour FindLegacyTestingManagerWrapper(GameObject testingManagerObject)
        {
            MonoBehaviour[] behaviours = testingManagerObject.GetComponents<MonoBehaviour>();
            for (int i = 0; i < behaviours.Length; i++)
            {
                MonoBehaviour behaviour = behaviours[i];
                if (behaviour == null)
                {
                    continue;
                }

                if (IsLegacyTestingManagerTypeName(behaviour.GetType().Name))
                {
                    return behaviour;
                }
            }

            return null;
        }

        public static int CountLegacyTestingManagerWrappersInScene(Scene scene)
        {
            int count = 0;
            GameObject[] roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                count += CountLegacyTestingManagerWrappersRecursive(roots[i].transform);
            }

            return count;
        }

        private static int CountLegacyTestingManagerWrappersRecursive(Transform current)
        {
            int count = 0;
            MonoBehaviour[] behaviours = current.GetComponents<MonoBehaviour>();
            for (int i = 0; i < behaviours.Length; i++)
            {
                MonoBehaviour behaviour = behaviours[i];
                if (behaviour != null && IsLegacyTestingManagerTypeName(behaviour.GetType().Name))
                {
                    count++;
                }
            }

            for (int childIndex = 0; childIndex < current.childCount; childIndex++)
            {
                count += CountLegacyTestingManagerWrappersRecursive(current.GetChild(childIndex));
            }

            return count;
        }

        private static bool IsLegacyTestingManagerTypeName(string typeName)
        {
            for (int i = 0; i < LegacyTestingManagerTypeNames.Length; i++)
            {
                if (typeName == LegacyTestingManagerTypeNames[i])
                {
                    return true;
                }
            }

            return false;
        }
    }
}
