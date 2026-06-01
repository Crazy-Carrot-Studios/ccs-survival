using System.Collections.Generic;
using System.IO;
using CCS.Survival.Composition;
using CCS.Survival.Editor.Development;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// =============================================================================
// SCRIPT: CCS_PlaytestBootstrapSetup
// CATEGORY: Modules / Playtesting / Editor / Validation
// PURPOSE: Creates default playtest profile, composition wiring, and bootstrap HUD.
// PLACEMENT: Batch entry for milestone 1.0.2 manual playtest harness.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-06-01
// NOTES: Enables harness only on bootstrap prefab/scene wiring.
// =============================================================================

namespace CCS.Modules.Playtesting.Editor
{
    public static class CCS_PlaytestBootstrapSetup
    {
        private const string PlaytestProfilesRoot = "Assets/CCS/Survival/Profiles/Playtesting";
        private const string DefaultPlaytestProfilePath = PlaytestProfilesRoot + "/CCS_DefaultPlaytestProfile.asset";
        private const string BootstrapPrefabPath = "Assets/CCS/Survival/Prefabs/PF_CCS_Survival_BootstrapRoot.prefab";
        private const string BootstrapScenePath = "Assets/CCS/Survival/Scenes/SCN_CCS_Survival_Bootstrap.unity";
        private const string ModuleDocPath = "Assets/CCS/Modules/Playtesting/Documentation/CCS_Playtesting_Module.md";
        private const string LogPrefix = "[CCS_PlaytestBootstrapSetup]";

        private const string StickItemId = "ccs.survival.item.resource.stick";
        private const string SpearItemId = "ccs.survival.item.starter.spear";
        private const string CookedRabbitItemId = "ccs.survival.item.food.cookedrabbitmeat";
        private const string FoundationPieceId = "ccs.survival.building.primitive.foundation";

        #region Public Methods

        public static void ExecuteBatch()
        {
            EnsureFolders();
            CCS_PlaytestProfile profile = EnsureDefaultPlaytestProfile();
            EnsureBootstrapGameplayServiceHost(profile);
            EnsureBootstrapPrefabHarness(profile);
            EnsureBootstrapSceneHarness();
            UpdateProjectVersion();
            UpdateModuleDocumentation();

            AssetDatabase.SaveAssets();
            Debug.Log($"{LogPrefix} Playtesting bootstrap setup complete.");
            EditorApplication.Exit(0);
        }

        #endregion

        #region Private Methods

        private static void EnsureFolders()
        {
            EnsureFolder("Assets/CCS/Survival/Profiles");
            EnsureFolder(PlaytestProfilesRoot);
            EnsureFolder("Assets/CCS/Modules/Playtesting/Documentation");
        }

        private static void EnsureFolder(string folderPath)
        {
            if (AssetDatabase.IsValidFolder(folderPath))
            {
                return;
            }

            string parent = Path.GetDirectoryName(folderPath)?.Replace('\\', '/') ?? "Assets";
            string folderName = Path.GetFileName(folderPath);
            AssetDatabase.CreateFolder(parent, folderName);
        }

        private static CCS_PlaytestProfile EnsureDefaultPlaytestProfile()
        {
            CCS_PlaytestProfile profile = AssetDatabase.LoadAssetAtPath<CCS_PlaytestProfile>(DefaultPlaytestProfilePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<CCS_PlaytestProfile>();
                AssetDatabase.CreateAsset(profile, DefaultPlaytestProfilePath);
            }

            SerializedObject serializedProfile = new SerializedObject(profile);
            serializedProfile.FindProperty("profileDisplayName").stringValue = "Default Playtest Harness";
            serializedProfile.FindProperty("profileId").stringValue = "ccs.survival.profile.playtesting.default";
            serializedProfile.FindProperty("profileDescription").stringValue =
                "Bootstrap manual playtest checklist for milestone 1.1.3.";
            serializedProfile.FindProperty("profileVersion").stringValue = "1.1.3";
            serializedProfile.FindProperty("enableHarness").boolValue = true;
            serializedProfile.FindProperty("showDebugLogs").boolValue = true;
            serializedProfile.FindProperty("resetStepStateOnPlayStart").boolValue = true;
            PopulateDefaultSteps(serializedProfile.FindProperty("stepDefinitions"));
            serializedProfile.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
            return profile;
        }

        private static void PopulateDefaultSteps(SerializedProperty stepListProperty)
        {
            stepListProperty.ClearArray();
            AddStep(
                stepListProperty,
                "ccs.survival.playtest.spawn",
                "Spawn into bootstrap",
                CCS_PlaytestStepType.Spawn,
                "Enter play mode in SCN_CCS_Survival_Bootstrap and confirm the player spawns.");
            AddStep(
                stepListProperty,
                "ccs.survival.playtest.gather",
                "Gather sticks or wood",
                CCS_PlaytestStepType.GatherResource,
                "Gather from CCS_TestGatheringSmallTree or CCS_TestGatheringBush (interact).",
                string.Empty,
                requiredCount: 1);
            AddStep(
                stepListProperty,
                "ccs.survival.playtest.equip",
                "Equip spear",
                CCS_PlaytestStepType.EquipWeapon,
                "Press F6 to equip the starter spear (or equip manually when UI exists).",
                SpearItemId);
            AddStep(
                stepListProperty,
                "ccs.survival.playtest.hunt",
                "Hunt wildlife",
                CCS_PlaytestStepType.HuntWildlife,
                "Kill CCS_TestRabbit or CCS_TestDeer with primary attack while spear is equipped.");
            AddStep(
                stepListProperty,
                "ccs.survival.playtest.harvest",
                "Harvest carcass",
                CCS_PlaytestStepType.HarvestCarcass,
                "Harvest meat from the wildlife carcass.");
            AddStep(
                stepListProperty,
                "ccs.survival.playtest.cook",
                "Cook meat at campfire",
                CCS_PlaytestStepType.CookFood,
                "Cook raw meat at the test campfire station.");
            AddStep(
                stepListProperty,
                "ccs.survival.playtest.eat",
                "Eat cooked meat",
                CCS_PlaytestStepType.EatFood,
                "Press F to consume cooked rabbit or venison from inventory.");
            AddStep(
                stepListProperty,
                "ccs.survival.playtest.shelter",
                "Build shelter",
                CCS_PlaytestStepType.BuildShelter,
                "Place primitive foundation, at least one wall, and one roof (B places foundation; snap wall/roof).");
            AddStep(
                stepListProperty,
                "ccs.survival.playtest.workbench",
                "Craft item at workbench",
                CCS_PlaytestStepType.CraftAtWorkbench,
                "Interact with CCS_TestWorkbench; F4 seeds mats; F3 crafts storage crate.");
            AddStep(
                stepListProperty,
                "ccs.survival.playtest.storage",
                "Use storage crate",
                CCS_PlaytestStepType.UseStorageCrate,
                "F2 place/open crate near player; F1 deposit item; F5 save; F9 load to verify persistence.");
            AddStep(
                stepListProperty,
                "ccs.survival.playtest.bedroll",
                "Place and sleep at bedroll",
                CCS_PlaytestStepType.PlaceAndSleepAtBedroll,
                "Shift+F2 place/sleep at bedroll; interact to sleep; F5 save; F9 load to verify bedroll restore.");
            AddStep(
                stepListProperty,
                "ccs.survival.playtest.save",
                "Save game",
                CCS_PlaytestStepType.SaveGame,
                "Press F5 to save the unified survival file.");
            AddStep(
                stepListProperty,
                "ccs.survival.playtest.load",
                "Load game",
                CCS_PlaytestStepType.LoadGame,
                "Press F9 to load the unified survival file.");
            AddStep(
                stepListProperty,
                "ccs.survival.playtest.death",
                "Trigger death",
                CCS_PlaytestStepType.TriggerDeath,
                "Press F7 to drain needs or die from starvation/dehydration.");
            AddStep(
                stepListProperty,
                "ccs.survival.playtest.respawn",
                "Respawn",
                CCS_PlaytestStepType.Respawn,
                "Confirm respawn at the bootstrap respawn point.");
        }

        private static void AddStep(
            SerializedProperty stepListProperty,
            string stepId,
            string displayName,
            CCS_PlaytestStepType stepType,
            string instructionText,
            string targetItemId = "",
            string targetObjectId = "",
            int requiredCount = 1,
            float timeoutSeconds = 0f)
        {
            int index = stepListProperty.arraySize;
            stepListProperty.InsertArrayElementAtIndex(index);
            SerializedProperty stepProperty = stepListProperty.GetArrayElementAtIndex(index);
            stepProperty.FindPropertyRelative("stepId").stringValue = stepId;
            stepProperty.FindPropertyRelative("displayName").stringValue = displayName;
            stepProperty.FindPropertyRelative("stepType").enumValueIndex = (int)stepType;
            stepProperty.FindPropertyRelative("instructionText").stringValue = instructionText;
            stepProperty.FindPropertyRelative("targetItemId").stringValue = targetItemId ?? string.Empty;
            stepProperty.FindPropertyRelative("targetObjectId").stringValue = targetObjectId ?? string.Empty;
            stepProperty.FindPropertyRelative("requiredCount").intValue = requiredCount;
            stepProperty.FindPropertyRelative("timeoutSeconds").floatValue = timeoutSeconds;
        }

        private static void EnsureBootstrapGameplayServiceHost(CCS_PlaytestProfile profile)
        {
            GameObject prefabRoot = AssetDatabase.LoadAssetAtPath<GameObject>(BootstrapPrefabPath);
            if (prefabRoot == null)
            {
                Debug.LogWarning($"{LogPrefix} Missing bootstrap prefab: {BootstrapPrefabPath}");
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
            serializedHost.FindProperty("playtestProfile").objectReferenceValue = profile;
            serializedHost.ApplyModifiedPropertiesWithoutUndo();

            PrefabUtility.SaveAsPrefabAsset(prefabContents, prefabPath);
            PrefabUtility.UnloadPrefabContents(prefabContents);
        }

        private static void EnsureBootstrapPrefabHarness(CCS_PlaytestProfile profile)
        {
            GameObject prefabRoot = AssetDatabase.LoadAssetAtPath<GameObject>(BootstrapPrefabPath);
            if (prefabRoot == null)
            {
                return;
            }

            string prefabPath = AssetDatabase.GetAssetPath(prefabRoot);
            GameObject prefabContents = PrefabUtility.LoadPrefabContents(prefabPath);

            Transform harnessRoot = prefabContents.transform.Find("PlaytestHarness");
            if (harnessRoot == null)
            {
                GameObject harnessObject = new GameObject("PlaytestHarness");
                harnessObject.transform.SetParent(prefabContents.transform, false);
                harnessRoot = harnessObject.transform;
            }

            CCS_PlaytestHud hud = harnessRoot.GetComponent<CCS_PlaytestHud>();
            if (hud == null)
            {
                hud = harnessRoot.gameObject.AddComponent<CCS_PlaytestHud>();
            }

            SerializedObject serializedHud = new SerializedObject(hud);
            serializedHud.FindProperty("enableHud").boolValue = profile != null && profile.EnableHarness;
            serializedHud.ApplyModifiedPropertiesWithoutUndo();

            PrefabUtility.SaveAsPrefabAsset(prefabContents, prefabPath);
            PrefabUtility.UnloadPrefabContents(prefabContents);
        }

        private static void EnsureBootstrapSceneHarness()
        {
            Scene scene = EditorSceneManager.OpenScene(BootstrapScenePath, OpenSceneMode.Single);
            bool sceneDirty = false;

            foreach (GameObject root in scene.GetRootGameObjects())
            {
                CCS_PlaytestHud[] huds = root.GetComponentsInChildren<CCS_PlaytestHud>(true);
                if (huds != null && huds.Length > 0)
                {
                    continue;
                }
            }

            GameObject prefabInstance = null;
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                if (root.name.Contains("Bootstrap") || root.GetComponentInChildren<CCS_SurvivalGameplayServiceHost>(true) != null)
                {
                    prefabInstance = root;
                    break;
                }
            }

            if (prefabInstance != null)
            {
                CCS_PlaytestHud hud = prefabInstance.GetComponentInChildren<CCS_PlaytestHud>(true);
                if (hud == null)
                {
                    Transform harnessRoot = prefabInstance.transform.Find("PlaytestHarness");
                    if (harnessRoot == null)
                    {
                        GameObject harnessObject = new GameObject("PlaytestHarness");
                        harnessObject.transform.SetParent(prefabInstance.transform, false);
                        harnessRoot = harnessObject.transform;
                    }

                    harnessRoot.gameObject.AddComponent<CCS_PlaytestHud>();
                    sceneDirty = true;
                }
            }

            if (sceneDirty)
            {
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
            }
        }

        private static void UpdateProjectVersion()
        {
            string projectSettingsPath = "ProjectSettings/ProjectSettings.asset";
            string text = File.ReadAllText(projectSettingsPath);
            text = System.Text.RegularExpressions.Regex.Replace(
                text,
                @"bundleVersion: [0-9]+\.[0-9]+\.[0-9]+",
                "bundleVersion: 1.1.3");
            File.WriteAllText(projectSettingsPath, text);
        }

        private static void UpdateModuleDocumentation()
        {
            if (File.Exists(ModuleDocPath))
            {
                return;
            }

            File.WriteAllText(
                ModuleDocPath,
                "# CCS Playtesting Module\n\n"
                + "Milestone **1.0.2** manual playtest harness for bootstrap development.\n\n"
                + "## Hotkeys\n\n"
                + "| Key | Action |\n"
                + "|-----|--------|\n"
                + "| F7 | Force hunger/thirst to zero (test death) |\n"
                + "| F10 | Toggle playtest HUD |\n"
                + "| F11 | Advance active checklist step |\n"
                + "| F12 | Reset checklist |\n"
                + "| F5 | Save (SaveSystem debug) |\n"
                + "| F9 | Load (SaveSystem debug) |\n\n"
                + "Batch: `CCS.Modules.Playtesting.Editor.CCS_PlaytestBootstrapSetup.ExecuteBatch`\n");
        }

        #endregion
    }
}
