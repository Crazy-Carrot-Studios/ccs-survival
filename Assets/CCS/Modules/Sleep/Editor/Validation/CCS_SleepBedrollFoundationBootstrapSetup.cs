using System.IO;
using CCS.Modules.Sleep;
using CCS.Survival.Composition;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// =============================================================================
// SCRIPT: CCS_SleepBedrollFoundationBootstrapSetup
// CATEGORY: Modules / Sleep / Editor / Validation
// PURPOSE: Creates primitive bedroll prefab, spot definition, profile wiring, and test object.
// PLACEMENT: Batch entry for milestone 1.1.3 sleep and bedroll foundation.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-06-01
// NOTES: Primitives only. Run after CCS_SleepBootstrapSetup when bedroll item exists.
// =============================================================================

namespace CCS.Modules.Sleep.Editor
{
    public static class CCS_SleepBedrollFoundationBootstrapSetup
    {
        private const string ContentRoot = "Assets/CCS/Survival/Content/Sleep/Primitive";
        private const string PrefabsRoot = ContentRoot + "/Prefabs";
        private const string DefinitionPath = ContentRoot + "/CCS_PrimitiveBedrollSleepSpotDefinition.asset";
        private const string PrefabPath = PrefabsRoot + "/PF_CCS_PrimitiveBedroll.prefab";
        private const string DefaultProfilePath = "Assets/CCS/Survival/Profiles/Sleep/CCS_DefaultSleepProfile.asset";
        private const string BootstrapScenePath = "Assets/CCS/Survival/Scenes/SCN_CCS_Survival_Bootstrap.unity";
        private const string TestBedrollObjectName = "CCS_TestBedroll";
        private const string TestBedrollInstanceId = "ccs.survival.sleep.instance.test.bedroll";
        private const string SleepSpotDefinitionId = "ccs.survival.sleep.primitive.bedroll";
        private const string LogPrefix = "[CCS_SleepBedrollFoundationBootstrapSetup]";

        #region Public Methods

        public static void ExecuteBatch()
        {
            AssetDatabase.Refresh();
            EnsureFolders();

            GameObject bedrollPrefab = EnsurePrimitiveBedrollPrefab();
            CCS_SleepSpotDefinition definition = EnsureSleepSpotDefinition(bedrollPrefab);
            EnsureSleepProfile(definition);
            EnsureBootstrapTestBedroll(definition, bedrollPrefab);
            UpdateProjectVersion();

            AssetDatabase.SaveAssets();
            Debug.Log($"{LogPrefix} Sleep bedroll foundation bootstrap complete.");
            EditorApplication.Exit(0);
        }

        #endregion

        #region Private Methods

        private static void EnsureFolders()
        {
            EnsureFolder("Assets/CCS/Survival/Content/Sleep");
            EnsureFolder(ContentRoot);
            EnsureFolder(PrefabsRoot);
        }

        private static void EnsureFolder(string folderPath)
        {
            if (AssetDatabase.IsValidFolder(folderPath))
            {
                return;
            }

            string parent = Path.GetDirectoryName(folderPath)?.Replace("\\", "/");
            string folderName = Path.GetFileName(folderPath);
            if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent))
            {
                EnsureFolder(parent);
            }

            AssetDatabase.CreateFolder(parent, folderName);
        }

        private static GameObject EnsurePrimitiveBedrollPrefab()
        {
            GameObject existingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            if (existingPrefab != null)
            {
                return existingPrefab;
            }

            GameObject capsule = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            capsule.name = "PF_CCS_PrimitiveBedroll";
            capsule.transform.localScale = new Vector3(1.2f, 0.35f, 1.2f);

            if (capsule.GetComponent<CCS_SleepSpot>() == null)
            {
                capsule.AddComponent<CCS_SleepSpot>();
            }

            if (capsule.GetComponent<CCS_SleepSpotInteractable>() == null)
            {
                capsule.AddComponent<CCS_SleepSpotInteractable>();
            }

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(capsule, PrefabPath);
            Object.DestroyImmediate(capsule);
            return prefab;
        }

        private static CCS_SleepSpotDefinition EnsureSleepSpotDefinition(GameObject prefab)
        {
            CCS_SleepSpotDefinition definition =
                AssetDatabase.LoadAssetAtPath<CCS_SleepSpotDefinition>(DefinitionPath);
            if (definition == null)
            {
                definition = ScriptableObject.CreateInstance<CCS_SleepSpotDefinition>();
                AssetDatabase.CreateAsset(definition, DefinitionPath);
            }

            SerializedObject serialized = new SerializedObject(definition);
            serialized.FindProperty("sleepSpotId").stringValue = SleepSpotDefinitionId;
            serialized.FindProperty("displayName").stringValue = "Bedroll";
            serialized.FindProperty("prefabReference").objectReferenceValue = prefab;
            serialized.FindProperty("enableDebugLogging").boolValue = false;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(definition);
            return definition;
        }

        private static void EnsureSleepProfile(CCS_SleepSpotDefinition definition)
        {
            CCS_SleepProfile profile = AssetDatabase.LoadAssetAtPath<CCS_SleepProfile>(DefaultProfilePath);
            if (profile == null)
            {
                Debug.LogError($"{LogPrefix} Missing default sleep profile. Run CCS_SleepBootstrapSetup first.");
                EditorApplication.Exit(1);
                return;
            }

            SerializedObject serialized = new SerializedObject(profile);
            serialized.FindProperty("profileVersion").stringValue = "1.1.3";
            serialized.FindProperty("profileDescription").stringValue =
                "Sleep and placeable bedroll foundation for milestone 1.1.3.";
            serialized.FindProperty("defaultSleepSpotDefinition").objectReferenceValue = definition;
            serialized.FindProperty("sleepDurationSeconds").floatValue = 30f;
            serialized.FindProperty("hungerRecoveryAmount").floatValue = 20f;
            serialized.FindProperty("thirstRecoveryAmount").floatValue = 20f;
            serialized.FindProperty("staminaRecoveryAmount").floatValue = 100f;
            serialized.FindProperty("assignRespawnPointOnSleep").boolValue = true;
            serialized.FindProperty("enableDebugLogging").boolValue = false;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
        }

        private static void EnsureBootstrapTestBedroll(
            CCS_SleepSpotDefinition definition,
            GameObject prefab)
        {
            Scene scene = EditorSceneManager.OpenScene(BootstrapScenePath, OpenSceneMode.Single);
            Transform sceneRoot = FindSceneRoot();
            if (sceneRoot == null)
            {
                Debug.LogError($"{LogPrefix} Could not find CCS_BuildVerificationScene root.");
                EditorApplication.Exit(1);
                return;
            }

            Transform testArea = sceneRoot.Find("CCS_SleepTestArea");
            if (testArea == null)
            {
                GameObject testAreaObject = new GameObject("CCS_SleepTestArea");
                testAreaObject.transform.SetParent(sceneRoot, false);
                testAreaObject.transform.localPosition = new Vector3(-6f, 0f, 8f);
                testArea = testAreaObject.transform;
            }

            Transform existing = testArea.Find(TestBedrollObjectName);
            if (existing != null)
            {
                Object.DestroyImmediate(existing.gameObject);
            }

            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, testArea);
            instance.name = TestBedrollObjectName;
            instance.transform.localPosition = new Vector3(2f, 0.1f, 0f);
            instance.transform.localRotation = Quaternion.identity;

            CCS_SleepSpot sleepSpot = instance.GetComponent<CCS_SleepSpot>();
            if (sleepSpot != null)
            {
                sleepSpot.ConfigureFromDefinition(definition, TestBedrollInstanceId);
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static Transform FindSceneRoot()
        {
            GameObject[] roots = SceneManager.GetActiveScene().GetRootGameObjects();
            for (int index = 0; index < roots.Length; index++)
            {
                if (roots[index].name == "CCS_BuildVerificationScene")
                {
                    return roots[index].transform;
                }
            }

            return null;
        }

        private static void UpdateProjectVersion()
        {
            string projectSettingsPath = "ProjectSettings/ProjectSettings.asset";
            if (!File.Exists(projectSettingsPath))
            {
                return;
            }

            string text = File.ReadAllText(projectSettingsPath);
            text = System.Text.RegularExpressions.Regex.Replace(
                text,
                @"bundleVersion: [0-9]+\.[0-9]+\.[0-9]+",
                "bundleVersion: 1.1.3");
            File.WriteAllText(projectSettingsPath, text);
        }

        #endregion
    }
}
