using System.IO;
using CCS.Modules.Storage;
using CCS.Survival.Composition;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// =============================================================================
// SCRIPT: CCS_StorageBootstrapSetup
// CATEGORY: Modules / Storage / Editor / Validation
// PURPOSE: Creates primitive storage crate content, profile wiring, and bootstrap test object.
// PLACEMENT: Batch entry for milestone 1.1.2 storage container foundation.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-06-01
// NOTES: Primitives only. No real art assets.
// =============================================================================

namespace CCS.Modules.Storage.Editor
{
    public static class CCS_StorageBootstrapSetup
    {
        private const string ContentRoot = "Assets/CCS/Survival/Content/Storage/Primitive";
        private const string PrefabsRoot = ContentRoot + "/Prefabs";
        private const string DefinitionPath = ContentRoot + "/CCS_PrimitiveStorageCrateDefinition.asset";
        private const string PrefabPath = PrefabsRoot + "/PF_CCS_PrimitiveStorageCrate.prefab";
        private const string ProfilePath = "Assets/CCS/Survival/Profiles/Storage/CCS_DefaultStorageProfile.asset";
        private const string BootstrapPrefabPath = "Assets/CCS/Survival/Prefabs/PF_CCS_Survival_BootstrapRoot.prefab";
        private const string BootstrapScenePath = "Assets/CCS/Survival/Scenes/SCN_CCS_Survival_Bootstrap.unity";
        private const string TestCrateObjectName = "CCS_TestStorageCrate";
        private const string TestCrateInstanceId = "ccs.survival.storage.instance.test.crate";
        private const string ContainerDefinitionId = "ccs.survival.storage.primitive.crate";
        private const string LogPrefix = "[CCS_StorageBootstrapSetup]";

        #region Public Methods

        public static void ExecuteBatch()
        {
            AssetDatabase.Refresh();
            EnsureFolders();

            GameObject cratePrefab = EnsurePrimitiveStorageCratePrefab();
            CCS_StorageContainerDefinition definition = EnsureContainerDefinition(cratePrefab);
            CCS_StorageProfile profile = EnsureStorageProfile(definition);
            EnsureBootstrapGameplayServiceHost(profile);
            EnsureBootstrapTestStorageCrate(definition, cratePrefab);
            UpdateProjectVersion();

            AssetDatabase.SaveAssets();
            Debug.Log($"{LogPrefix} Storage bootstrap setup complete.");
            EditorApplication.Exit(0);
        }

        #endregion

        #region Private Methods

        private static void EnsureFolders()
        {
            EnsureFolder("Assets/CCS/Survival/Content/Storage");
            EnsureFolder(ContentRoot);
            EnsureFolder(PrefabsRoot);
            EnsureFolder("Assets/CCS/Survival/Profiles/Storage");
        }

        private static void EnsureFolder(string folderPath)
        {
            if (AssetDatabase.IsValidFolder(folderPath))
            {
                return;
            }

            string parent = Path.GetDirectoryName(folderPath)?.Replace('\\', '/');
            string folderName = Path.GetFileName(folderPath);
            if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent))
            {
                EnsureFolder(parent);
            }

            AssetDatabase.CreateFolder(parent, folderName);
        }

        private static GameObject EnsurePrimitiveStorageCratePrefab()
        {
            GameObject existingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            if (existingPrefab != null)
            {
                return existingPrefab;
            }

            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = "PF_CCS_PrimitiveStorageCrate";
            cube.transform.localScale = new Vector3(1.1f, 0.9f, 1.1f);

            if (cube.GetComponent<CCS_StorageContainer>() == null)
            {
                cube.AddComponent<CCS_StorageContainer>();
            }

            if (cube.GetComponent<CCS_StorageContainerInteractable>() == null)
            {
                cube.AddComponent<CCS_StorageContainerInteractable>();
            }

            PrefabUtility.SaveAsPrefabAsset(cube, PrefabPath);
            Object.DestroyImmediate(cube);
            return AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        }

        private static CCS_StorageContainerDefinition EnsureContainerDefinition(GameObject prefabReference)
        {
            CCS_StorageContainerDefinition definition =
                AssetDatabase.LoadAssetAtPath<CCS_StorageContainerDefinition>(DefinitionPath);
            if (definition == null)
            {
                definition = ScriptableObject.CreateInstance<CCS_StorageContainerDefinition>();
                AssetDatabase.CreateAsset(definition, DefinitionPath);
            }

            SerializedObject serialized = new SerializedObject(definition);
            serialized.FindProperty("containerId").stringValue = ContainerDefinitionId;
            serialized.FindProperty("displayName").stringValue = "Storage Crate";
            serialized.FindProperty("slotCount").intValue = 8;
            serialized.FindProperty("maxWeight").floatValue = 0f;
            serialized.FindProperty("prefabReference").objectReferenceValue = prefabReference;
            serialized.FindProperty("enableDebugLogging").boolValue = true;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(definition);
            return definition;
        }

        private static CCS_StorageProfile EnsureStorageProfile(CCS_StorageContainerDefinition definition)
        {
            CCS_StorageProfile profile = AssetDatabase.LoadAssetAtPath<CCS_StorageProfile>(ProfilePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<CCS_StorageProfile>();
                AssetDatabase.CreateAsset(profile, ProfilePath);
            }

            SerializedObject serialized = new SerializedObject(profile);
            serialized.FindProperty("profileDisplayName").stringValue = "Default Storage";
            serialized.FindProperty("profileId").stringValue = "ccs.survival.profile.storage.default";
            serialized.FindProperty("profileDescription").stringValue =
                "Primitive storage container foundation for milestone 1.1.2.";
            serialized.FindProperty("profileVersion").stringValue = "1.1.2";
            serialized.FindProperty("defaultContainerDefinition").objectReferenceValue = definition;
            serialized.FindProperty("enableDebugLogging").boolValue = true;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
            return profile;
        }

        private static void EnsureBootstrapGameplayServiceHost(CCS_StorageProfile profile)
        {
            GameObject prefabRoot = AssetDatabase.LoadAssetAtPath<GameObject>(BootstrapPrefabPath);
            if (prefabRoot == null)
            {
                return;
            }

            string prefabPath = AssetDatabase.GetAssetPath(prefabRoot);
            GameObject prefabContents = PrefabUtility.LoadPrefabContents(prefabPath);
            CCS_SurvivalGameplayServiceHost host = prefabContents.GetComponent<CCS_SurvivalGameplayServiceHost>();
            if (host == null)
            {
                PrefabUtility.UnloadPrefabContents(prefabContents);
                return;
            }

            SerializedObject serializedHost = new SerializedObject(host);
            serializedHost.FindProperty("storageProfile").objectReferenceValue = profile;
            serializedHost.ApplyModifiedPropertiesWithoutUndo();
            PrefabUtility.SaveAsPrefabAsset(prefabContents, prefabPath);
            PrefabUtility.UnloadPrefabContents(prefabContents);
        }

        private static void EnsureBootstrapTestStorageCrate(
            CCS_StorageContainerDefinition definition,
            GameObject prefabReference)
        {
            Scene scene = EditorSceneManager.OpenScene(BootstrapScenePath, OpenSceneMode.Single);
            Transform sceneRoot = FindSceneRoot();
            if (sceneRoot == null)
            {
                Debug.LogError($"{LogPrefix} Could not find bootstrap scene root.");
                EditorApplication.Exit(1);
                return;
            }

            Transform existing = sceneRoot.Find(TestCrateObjectName);
            GameObject crateObject;
            if (existing != null)
            {
                crateObject = existing.gameObject;
            }
            else
            {
                crateObject = (GameObject)PrefabUtility.InstantiatePrefab(prefabReference, scene);
                crateObject.name = TestCrateObjectName;
                crateObject.transform.SetParent(sceneRoot, false);
                crateObject.transform.position = new Vector3(8f, 0.45f, 2f);
            }

            CCS_StorageContainer container = crateObject.GetComponent<CCS_StorageContainer>();
            if (container == null)
            {
                container = crateObject.AddComponent<CCS_StorageContainer>();
            }

            container.ConfigureFromDefinition(definition, TestCrateInstanceId);

            if (crateObject.GetComponent<CCS_StorageContainerInteractable>() == null)
            {
                crateObject.AddComponent<CCS_StorageContainerInteractable>();
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static Transform FindSceneRoot()
        {
            Scene activeScene = EditorSceneManager.GetActiveScene();
            GameObject[] roots = activeScene.GetRootGameObjects();
            for (int index = 0; index < roots.Length; index++)
            {
                GameObject root = roots[index];
                if (root != null && root.name.Contains("Bootstrap"))
                {
                    return root.transform;
                }
            }

            return roots.Length > 0 ? roots[0].transform : null;
        }

        private static void UpdateProjectVersion()
        {
            string projectSettingsPath = "ProjectSettings/ProjectSettings.asset";
            if (!File.Exists(projectSettingsPath))
            {
                return;
            }

            string projectSettingsText = File.ReadAllText(projectSettingsPath);
            projectSettingsText = System.Text.RegularExpressions.Regex.Replace(
                projectSettingsText,
                @"bundleVersion: [0-9]+\.[0-9]+\.[0-9]+",
                "bundleVersion: 1.1.2");
            File.WriteAllText(projectSettingsPath, projectSettingsText);
        }

        #endregion
    }
}
