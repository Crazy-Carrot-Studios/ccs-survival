using System.IO;
using CCS.Modules.EnvironmentEffects;
using CCS.Modules.Shelter;
using CCS.Modules.UI;
using CCS.Survival.Composition;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// =============================================================================
// SCRIPT: CCS_ShelterBootstrapSetup
// CATEGORY: Modules / Shelter / Editor / Validation
// PURPOSE: Creates default profile, bootstrap wiring, HUD shelter display, and test volume.
// PLACEMENT: Batch entry for 0.7.5 shelter environmental protection milestone.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-31
// NOTES: Foundation only. No building placement, snapping, or final art.
// =============================================================================

namespace CCS.Modules.Shelter.Editor
{
    public static class CCS_ShelterBootstrapSetup
    {
        private const string ProfilesRoot = "Assets/CCS/Survival/Profiles/Shelter";
        private const string DefaultProfilePath = ProfilesRoot + "/CCS_DefaultShelterProfile.asset";
        private const string BootstrapPrefabPath = "Assets/CCS/Survival/Prefabs/PF_CCS_Survival_BootstrapRoot.prefab";
        private const string BootstrapScenePath = "Assets/CCS/Survival/Scenes/SCN_CCS_Survival_Bootstrap.unity";
        private const string EnvironmentHudPanelObjectName = "EnvironmentHudArea";
        private const string TestShelterVolumeName = "CCS_TestShelterVolume";
        private const string LogPrefix = "[CCS_ShelterBootstrapSetup]";

        #region Public Methods

        public static void ExecuteBatch()
        {
            EnsureFolders();
            EnsureDefaultProfile();
            EnsureBootstrapGameplayServiceHost();
            EnsureBootstrapEnvironmentHudPanel();
            EnsureBootstrapTestShelterVolume();
            EnsureBootstrapShelterTestHarness();

            AssetDatabase.SaveAssets();
            Debug.Log($"{LogPrefix} Shelter bootstrap setup complete.");
            EditorApplication.Exit(0);
        }

        #endregion

        #region Private Methods

        private static void EnsureFolders()
        {
            EnsureFolder("Assets/CCS/Survival/Profiles");
            EnsureFolder(ProfilesRoot);
        }

        private static CCS_ShelterProfile EnsureDefaultProfile()
        {
            CCS_ShelterProfile profile = AssetDatabase.LoadAssetAtPath<CCS_ShelterProfile>(DefaultProfilePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<CCS_ShelterProfile>();
                AssetDatabase.CreateAsset(profile, DefaultProfilePath);
            }

            SerializedObject serializedProfile = new SerializedObject(profile);
            serializedProfile.FindProperty("profileDisplayName").stringValue = "Default Shelter";
            serializedProfile.FindProperty("profileId").stringValue = "ccs.survival.profile.shelter.default";
            serializedProfile.FindProperty("profileDescription").stringValue =
                "Default shelter protection rules for 0.7.5 foundation.";
            serializedProfile.FindProperty("profileVersion").stringValue = "0.7.5";
            serializedProfile.FindProperty("defaultWetnessProtection").floatValue = 1f;
            serializedProfile.FindProperty("defaultExposureProtection").floatValue = 0.6f;
            serializedProfile.FindProperty("defaultTemperatureProtection").floatValue = 1f;
            serializedProfile.FindProperty("defaultProtectionMultiplier").floatValue = 1f;
            serializedProfile.FindProperty("requireTriggerVolume").boolValue = true;
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
            serializedHost.FindProperty("shelterProfile").objectReferenceValue =
                AssetDatabase.LoadAssetAtPath<Object>(DefaultProfilePath);
            serializedHost.ApplyModifiedPropertiesWithoutUndo();

            PrefabUtility.SaveAsPrefabAsset(prefabContents, prefabPath);
            PrefabUtility.UnloadPrefabContents(prefabContents);
        }

        private static void EnsureBootstrapEnvironmentHudPanel()
        {
            Scene scene = EditorSceneManager.OpenScene(BootstrapScenePath, OpenSceneMode.Single);
            CCS_HudRootPresenter hudRoot = Object.FindFirstObjectByType<CCS_HudRootPresenter>();
            if (hudRoot == null)
            {
                Debug.LogError($"{LogPrefix} Bootstrap scene is missing PF_CCS_HUD_Root instance.");
                EditorApplication.Exit(1);
                return;
            }

            Transform existingPanel = hudRoot.transform.Find(EnvironmentHudPanelObjectName);
            if (existingPanel == null)
            {
                Debug.LogError($"{LogPrefix} Bootstrap scene is missing EnvironmentHudArea panel.");
                EditorApplication.Exit(1);
                return;
            }

            RectTransform panelRect = existingPanel.GetComponent<RectTransform>();
            if (panelRect != null)
            {
                panelRect.sizeDelta = new Vector2(190f, 210f);
            }

            Text statusText = existingPanel.Find("StatusText")?.GetComponent<Text>();
            if (statusText != null)
            {
                statusText.fontSize = 11;
                statusText.text =
                    "Env Temp: 0\nWetness: 0\nExposure: 0\n" +
                    "Sheltered: No\nShelter Wet: 0\nShelter Exp: 0\nShelter Temp: 0\n" +
                    "Temp Res: 0\nWet Res: 0\nExp Res: 0\n" +
                    "Eff Temp: 0\nEff Wet: 0\nEff Exp: 0";
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static void EnsureBootstrapTestShelterVolume()
        {
            Scene scene = EditorSceneManager.OpenScene(BootstrapScenePath, OpenSceneMode.Single);
            GameObject existingVolume = GameObject.Find(TestShelterVolumeName);
            GameObject volumeObject = existingVolume != null
                ? existingVolume
                : GameObject.CreatePrimitive(PrimitiveType.Cube);

            if (existingVolume == null)
            {
                volumeObject.name = TestShelterVolumeName;
            }

            volumeObject.transform.position = new Vector3(4f, 1.5f, 2f);
            volumeObject.transform.localScale = new Vector3(3f, 3f, 3f);

            BoxCollider boxCollider = volumeObject.GetComponent<BoxCollider>();
            if (boxCollider == null)
            {
                boxCollider = volumeObject.AddComponent<BoxCollider>();
            }

            boxCollider.isTrigger = true;

            CCS_ShelterVolume shelterVolume = volumeObject.GetComponent<CCS_ShelterVolume>();
            if (shelterVolume == null)
            {
                shelterVolume = volumeObject.AddComponent<CCS_ShelterVolume>();
            }

            SerializedObject serializedVolume = new SerializedObject(shelterVolume);
            serializedVolume.FindProperty("shelterId").stringValue = "ccs.survival.shelter.test.bootstrap";
            serializedVolume.FindProperty("displayName").stringValue = "Test Shelter Volume";
            serializedVolume.FindProperty("wetnessProtection").floatValue = 1f;
            serializedVolume.FindProperty("exposureProtection").floatValue = 0.6f;
            serializedVolume.FindProperty("temperatureProtection").floatValue = 1f;
            serializedVolume.FindProperty("protectionMultiplier").floatValue = 1f;
            serializedVolume.FindProperty("acceptAnyTriggerSubject").boolValue = false;
            serializedVolume.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static void EnsureBootstrapShelterTestHarness()
        {
            Scene scene = EditorSceneManager.OpenScene(BootstrapScenePath, OpenSceneMode.Single);
            GameObject bootstrapRoot = GameObject.Find("PF_CCS_Survival_BootstrapRoot");
            if (bootstrapRoot == null)
            {
                CCS_SurvivalGameplayServiceHost host = Object.FindFirstObjectByType<CCS_SurvivalGameplayServiceHost>();
                bootstrapRoot = host != null ? host.gameObject : null;
            }

            if (bootstrapRoot == null)
            {
                Debug.LogError($"{LogPrefix} Bootstrap scene is missing PF_CCS_Survival_BootstrapRoot.");
                EditorApplication.Exit(1);
                return;
            }

            CCS_ShelterTestHarness harness = bootstrapRoot.GetComponent<CCS_ShelterTestHarness>();
            if (harness == null)
            {
                harness = bootstrapRoot.AddComponent<CCS_ShelterTestHarness>();
            }

            SerializedObject serializedHarness = new SerializedObject(harness);
            serializedHarness.FindProperty("enableHarness").boolValue = true;
            serializedHarness.FindProperty("toggleIntervalSeconds").floatValue = 5f;
            serializedHarness.FindProperty("testShelterId").stringValue = "ccs.survival.shelter.test.harness";
            serializedHarness.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
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
