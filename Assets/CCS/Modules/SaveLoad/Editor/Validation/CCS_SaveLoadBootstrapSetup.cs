using System.IO;
using CCS.Modules.SaveLoad;
using CCS.Survival.Composition;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// =============================================================================
// SCRIPT: CCS_SaveLoadBootstrapSetup
// CATEGORY: Modules / SaveLoad / Editor / Validation
// PURPOSE: Creates default profile, bootstrap wiring, and development test saveable.
// PLACEMENT: Batch entry for 0.6.0 save/load foundation milestone.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Framework setup only. No gameplay module persistence yet.
// =============================================================================

namespace CCS.Modules.SaveLoad.Editor
{
    public static class CCS_SaveLoadBootstrapSetup
    {
        private const string ProfilesRoot = "Assets/CCS/Survival/Profiles/SaveLoad";
        private const string DefaultProfilePath = ProfilesRoot + "/CCS_DefaultSaveLoadProfile.asset";
        private const string BootstrapPrefabPath = "Assets/CCS/Survival/Prefabs/PF_CCS_Survival_BootstrapRoot.prefab";
        private const string BootstrapScenePath = "Assets/CCS/Survival/Scenes/SCN_CCS_Survival_Bootstrap.unity";
        private const string TestSaveableObjectName = "CCS_TestSaveableComponent";
        private const string LogPrefix = "[CCS_SaveLoadBootstrapSetup]";

        #region Public Methods

        public static void ExecuteBatch()
        {
            EnsureFolders();
            EnsureDefaultProfile();
            EnsureBootstrapGameplayServiceHost();
            EnsureBootstrapTestSaveable();

            AssetDatabase.SaveAssets();
            Debug.Log($"{LogPrefix} Save/load bootstrap setup complete.");
            EditorApplication.Exit(0);
        }

        #endregion

        #region Private Methods

        private static void EnsureFolders()
        {
            EnsureFolder("Assets/CCS/Survival/Profiles");
            EnsureFolder(ProfilesRoot);
        }

        private static CCS_SaveLoadProfile EnsureDefaultProfile()
        {
            CCS_SaveLoadProfile profile = AssetDatabase.LoadAssetAtPath<CCS_SaveLoadProfile>(DefaultProfilePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<CCS_SaveLoadProfile>();
                AssetDatabase.CreateAsset(profile, DefaultProfilePath);
            }

            SerializedObject serializedProfile = new SerializedObject(profile);
            serializedProfile.FindProperty("profileDisplayName").stringValue = "Default Save Load";
            serializedProfile.FindProperty("profileId").stringValue = "ccs.survival.profile.saveload.default";
            serializedProfile.FindProperty("profileDescription").stringValue =
                "Default save/load rules for 0.6.0 foundation.";
            serializedProfile.FindProperty("profileVersion").stringValue = "0.6.0";
            serializedProfile.FindProperty("autoSaveEnabled").boolValue = false;
            serializedProfile.FindProperty("autoSaveIntervalSeconds").floatValue = 300f;
            serializedProfile.FindProperty("maxSaveSlots").intValue = 10;
            serializedProfile.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
            return profile;
        }

        private static void EnsureBootstrapGameplayServiceHost()
        {
            GameObject prefabRoot = AssetDatabase.LoadAssetAtPath<GameObject>(BootstrapPrefabPath);
            if (prefabRoot == null)
            {
                Debug.LogError($"{LogPrefix} Missing bootstrap prefab: {BootstrapPrefabPath}");
                EditorApplication.Exit(1);
                return;
            }

            string prefabPath = AssetDatabase.GetAssetPath(prefabRoot);
            GameObject prefabContents = PrefabUtility.LoadPrefabContents(prefabPath);
            CCS_SurvivalGameplayServiceHost host = prefabContents.GetComponent<CCS_SurvivalGameplayServiceHost>();
            if (host == null)
            {
                host = prefabContents.AddComponent<CCS_SurvivalGameplayServiceHost>();
            }

            SerializedObject serializedHost = new SerializedObject(host);
            serializedHost.FindProperty("saveLoadProfile").objectReferenceValue =
                AssetDatabase.LoadAssetAtPath<Object>(DefaultProfilePath);
            serializedHost.ApplyModifiedPropertiesWithoutUndo();

            PrefabUtility.SaveAsPrefabAsset(prefabContents, prefabPath);
            PrefabUtility.UnloadPrefabContents(prefabContents);
        }

        private static void EnsureBootstrapTestSaveable()
        {
            Scene scene = EditorSceneManager.OpenScene(BootstrapScenePath, OpenSceneMode.Single);
            Transform sceneRoot = FindSceneRoot();
            if (sceneRoot == null)
            {
                Debug.LogError($"{LogPrefix} Could not find CCS_BuildVerificationScene root.");
                EditorApplication.Exit(1);
                return;
            }

            Transform existing = sceneRoot.Find(TestSaveableObjectName);
            GameObject saveableObject = existing != null
                ? existing.gameObject
                : new GameObject(TestSaveableObjectName);

            if (existing == null)
            {
                saveableObject.transform.SetParent(sceneRoot, false);
            }

            CCS_TestSaveableComponent testSaveable = saveableObject.GetComponent<CCS_TestSaveableComponent>();
            if (testSaveable == null)
            {
                testSaveable = saveableObject.AddComponent<CCS_TestSaveableComponent>();
            }

            SerializedObject serializedSaveable = new SerializedObject(testSaveable);
            serializedSaveable.FindProperty("enableTestSaveable").boolValue = true;
            serializedSaveable.FindProperty("testString").stringValue = "bootstrap-test";
            serializedSaveable.FindProperty("testInteger").intValue = 42;
            serializedSaveable.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static Transform FindSceneRoot()
        {
            GameObject[] roots = SceneManager.GetActiveScene().GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                if (roots[i].name == "CCS_BuildVerificationScene")
                {
                    return roots[i].transform;
                }
            }

            return null;
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
            {
                return;
            }

            string parent = Path.GetDirectoryName(path)?.Replace("\\", "/");
            string folderName = Path.GetFileName(path);
            if (string.IsNullOrEmpty(parent) || string.IsNullOrEmpty(folderName))
            {
                return;
            }

            if (!AssetDatabase.IsValidFolder(parent))
            {
                EnsureFolder(parent);
            }

            AssetDatabase.CreateFolder(parent, folderName);
        }

        #endregion
    }
}
