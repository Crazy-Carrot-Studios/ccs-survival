using System.IO;
using CCS.Modules.Cooking;
using CCS.Modules.Gathering;
using CCS.Modules.PlayerDeath;
using CCS.Modules.SaveSystem;
using CCS.Survival.Composition;
using CCS.Survival.Editor.Development;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// =============================================================================
// SCRIPT: CCS_SaveBootstrapSetup
// CATEGORY: Modules / SaveSystem / Editor / Validation
// PURPOSE: Creates save/death profiles, bootstrap wiring, respawn point, and save node ids.
// PLACEMENT: Batch entry for milestone 1.0.1 death, respawn, and save foundation.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-06-01
// NOTES: Also wires CCS_PlayerDeathProfile and scene respawn markers.
// =============================================================================

namespace CCS.Modules.SaveSystem.Editor
{
    public static class CCS_SaveBootstrapSetup
    {
        private const string SaveProfilesRoot = "Assets/CCS/Survival/Profiles/SaveSystem";
        private const string DefaultSaveProfilePath = SaveProfilesRoot + "/CCS_DefaultSaveProfile.asset";
        private const string PlayerDeathProfilesRoot = "Assets/CCS/Survival/Profiles/PlayerDeath";
        private const string DefaultPlayerDeathProfilePath = PlayerDeathProfilesRoot + "/CCS_DefaultPlayerDeathProfile.asset";
        private const string BootstrapPrefabPath = "Assets/CCS/Survival/Prefabs/PF_CCS_Survival_BootstrapRoot.prefab";
        private const string BootstrapScenePath = "Assets/CCS/Survival/Scenes/SCN_CCS_Survival_Bootstrap.unity";
        private const string CompositionRegistrationPath =
            "Assets/CCS/Survival/Runtime/Composition/CCS_SurvivalGameplayServiceRegistration.cs";
        private const string LogPrefix = "[CCS_SaveBootstrapSetup]";

        #region Public Methods

        public static void ExecuteBatch()
        {
            EnsureFolders();
            CCS_SaveProfile saveProfile = EnsureDefaultSaveProfile();
            CCS_PlayerDeathProfile playerDeathProfile = EnsureDefaultPlayerDeathProfile();
            EnsureBootstrapGameplayServiceHost(saveProfile, playerDeathProfile);
            EnsureBootstrapPrefabComponents();
            EnsureBootstrapSceneObjects();
            UpdateProjectVersion();

            AssetDatabase.SaveAssets();
            Debug.Log($"{LogPrefix} Save system bootstrap setup complete.");
            EditorApplication.Exit(0);
        }

        #endregion

        #region Private Methods

        private static void EnsureFolders()
        {
            EnsureFolder("Assets/CCS/Survival/Profiles");
            EnsureFolder(SaveProfilesRoot);
            EnsureFolder(PlayerDeathProfilesRoot);
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

        private static CCS_SaveProfile EnsureDefaultSaveProfile()
        {
            CCS_SaveProfile profile = AssetDatabase.LoadAssetAtPath<CCS_SaveProfile>(DefaultSaveProfilePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<CCS_SaveProfile>();
                AssetDatabase.CreateAsset(profile, DefaultSaveProfilePath);
            }

            SerializedObject serializedProfile = new SerializedObject(profile);
            serializedProfile.FindProperty("profileDisplayName").stringValue = "Default Save System";
            serializedProfile.FindProperty("profileId").stringValue = "ccs.survival.profile.savesystem.default";
            serializedProfile.FindProperty("profileDescription").stringValue =
                "Unified survival save file for milestone 1.0.1.";
            serializedProfile.FindProperty("profileVersion").stringValue = "1.0.1";
            serializedProfile.FindProperty("saveFileName").stringValue = "CCS_Survival_Save.json";
            serializedProfile.FindProperty("autoSaveEnabled").boolValue = true;
            serializedProfile.FindProperty("autoSaveIntervalSeconds").floatValue = 120f;
            serializedProfile.FindProperty("enableDebugLogging").boolValue = true;
            serializedProfile.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
            return profile;
        }

        private static CCS_PlayerDeathProfile EnsureDefaultPlayerDeathProfile()
        {
            CCS_PlayerDeathProfile profile =
                AssetDatabase.LoadAssetAtPath<CCS_PlayerDeathProfile>(DefaultPlayerDeathProfilePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<CCS_PlayerDeathProfile>();
                AssetDatabase.CreateAsset(profile, DefaultPlayerDeathProfilePath);
            }

            SerializedObject serializedProfile = new SerializedObject(profile);
            serializedProfile.FindProperty("profileDisplayName").stringValue = "Default Player Death";
            serializedProfile.FindProperty("profileId").stringValue = "ccs.survival.profile.playerdeath.default";
            serializedProfile.FindProperty("profileDescription").stringValue =
                "Starvation and dehydration death rules for milestone 1.0.1.";
            serializedProfile.FindProperty("profileVersion").stringValue = "1.0.1";
            serializedProfile.FindProperty("respawnHunger").floatValue = 50f;
            serializedProfile.FindProperty("respawnThirst").floatValue = 50f;
            serializedProfile.FindProperty("respawnStamina").floatValue = 100f;
            serializedProfile.FindProperty("defaultSpawnId").stringValue = "ccs.survival.spawn.bootstrap";
            serializedProfile.FindProperty("enableDebugLogging").boolValue = true;
            serializedProfile.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
            return profile;
        }

        private static void EnsureBootstrapGameplayServiceHost(
            CCS_SaveProfile saveProfile,
            CCS_PlayerDeathProfile playerDeathProfile)
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
            serializedHost.FindProperty("saveProfile").objectReferenceValue = saveProfile;
            serializedHost.FindProperty("playerDeathProfile").objectReferenceValue = playerDeathProfile;
            serializedHost.ApplyModifiedPropertiesWithoutUndo();

            PrefabUtility.SaveAsPrefabAsset(prefabContents, prefabPath);
            PrefabUtility.UnloadPrefabContents(prefabContents);
        }

        private static void EnsureBootstrapPrefabComponents()
        {
            GameObject prefabRoot = AssetDatabase.LoadAssetAtPath<GameObject>(BootstrapPrefabPath);
            if (prefabRoot == null)
            {
                return;
            }

            string prefabPath = AssetDatabase.GetAssetPath(prefabRoot);
            GameObject prefabContents = PrefabUtility.LoadPrefabContents(prefabPath);

            if (prefabContents.GetComponent<CCS_SaveStartupLoader>() == null)
            {
                prefabContents.AddComponent<CCS_SaveStartupLoader>();
            }

            Transform debugArea = prefabContents.transform.Find("SaveSystemDebugArea");
            if (debugArea == null)
            {
                GameObject debugObject = new GameObject("SaveSystemDebugArea");
                debugObject.transform.SetParent(prefabContents.transform, false);
                debugArea = debugObject.transform;
            }

            if (debugArea.GetComponent<CCS_SaveDebugController>() == null)
            {
                debugArea.gameObject.AddComponent<CCS_SaveDebugController>();
            }

            PrefabUtility.SaveAsPrefabAsset(prefabContents, prefabPath);
            PrefabUtility.UnloadPrefabContents(prefabContents);
        }

        private static void EnsureBootstrapSceneObjects()
        {
            Scene scene = EditorSceneManager.OpenScene(BootstrapScenePath, OpenSceneMode.Single);
            Transform sceneRoot = FindSceneRoot();
            if (sceneRoot == null)
            {
                Debug.LogWarning($"{LogPrefix} Could not find scene root in bootstrap scene.");
                return;
            }

            EnsureRespawnPoint(sceneRoot);
            ConfigureGatheringSaveIds(sceneRoot);
            ConfigureCampfireSaveId(sceneRoot);
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

        private static void EnsureRespawnPoint(Transform sceneRoot)
        {
            const string respawnObjectName = "CCS_PlayerRespawnPoint_Bootstrap";
            Transform existing = sceneRoot.Find(respawnObjectName);
            GameObject respawnObject = existing != null ? existing.gameObject : new GameObject(respawnObjectName);
            respawnObject.transform.SetParent(sceneRoot, false);
            respawnObject.transform.position = new Vector3(0f, 1f, 2f);

            CCS_PlayerRespawnPoint respawnPoint = respawnObject.GetComponent<CCS_PlayerRespawnPoint>();
            if (respawnPoint == null)
            {
                respawnPoint = respawnObject.AddComponent<CCS_PlayerRespawnPoint>();
            }

            respawnPoint.ConfigureRuntime("ccs.survival.spawn.bootstrap");
            EditorUtility.SetDirty(respawnObject);
        }

        private static void ConfigureGatheringSaveIds(Transform sceneRoot)
        {
            ConfigureGatheringNode(sceneRoot, "CCS_TestGatheringSmallTree", "ccs.survival.gathering.node.smalltree");
            ConfigureGatheringNode(sceneRoot, "CCS_TestGatheringRock", "ccs.survival.gathering.node.rock");
            ConfigureGatheringNode(sceneRoot, "CCS_TestGatheringBush", "ccs.survival.gathering.node.bush");
        }

        private static void ConfigureGatheringNode(Transform parent, string objectName, string saveNodeId)
        {
            Transform nodeTransform = parent.Find(objectName);
            if (nodeTransform == null)
            {
                return;
            }

            CCS_GatheringNode node = nodeTransform.GetComponent<CCS_GatheringNode>();
            if (node == null)
            {
                return;
            }

            node.ConfigureSaveNodeId(saveNodeId);
            EditorUtility.SetDirty(nodeTransform.gameObject);
        }

        private static void ConfigureCampfireSaveId(Transform sceneRoot)
        {
            Transform campfireArea = sceneRoot.Find("CCS_CampfireTestArea");
            if (campfireArea == null)
            {
                return;
            }

            Transform campfire = campfireArea.Find("CCS_TestCampfire");
            if (campfire == null)
            {
                return;
            }

            CCS_CookingStation station = campfire.GetComponent<CCS_CookingStation>();
            if (station != null)
            {
                station.ConfigureSaveStationId("ccs.survival.cooking.station.testcampfire");
                EditorUtility.SetDirty(campfire.gameObject);
            }
        }

        private static void UpdateProjectVersion()
        {
            string projectSettingsPath = "ProjectSettings/ProjectSettings.asset";
            string text = File.ReadAllText(projectSettingsPath);
            text = System.Text.RegularExpressions.Regex.Replace(
                text,
                @"bundleVersion: [0-9]+\.[0-9]+\.[0-9]+",
                "bundleVersion: 1.0.1");
            File.WriteAllText(projectSettingsPath, text);
        }

        #endregion
    }
}
