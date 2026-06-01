using System.IO;
using CCS.Modules.UI;
using CCS.Modules.Wildlife;
using CCS.Survival.Composition;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// =============================================================================
// SCRIPT: CCS_WildlifeAiBootstrapSetup
// CATEGORY: Modules / Wildlife / Editor / Validation
// PURPOSE: Creates wildlife AI profile, living bootstrap wildlife, and HUD debug wiring.
// PLACEMENT: Batch entry for 0.9.7 passive wildlife AI foundation milestone.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: Keeps existing carcass harvest placeholders. Primitive living wildlife only.
// =============================================================================

namespace CCS.Modules.Wildlife.Editor
{
    public static class CCS_WildlifeAiBootstrapSetup
    {
        private const string ProfilesRoot = "Assets/CCS/Survival/Profiles/Wildlife";
        private const string DefaultAiProfilePath = ProfilesRoot + "/CCS_DefaultWildlifeAiProfile.asset";
        private const string BootstrapScenePath = "Assets/CCS/Survival/Scenes/SCN_CCS_Survival_Bootstrap.unity";
        private const string BootstrapPrefabPath = "Assets/CCS/Survival/Prefabs/PF_CCS_Survival_BootstrapRoot.prefab";
        private const string HudPrefabPath = "Assets/CCS/Modules/UI/Prefabs/PF_CCS_HUD_Root.prefab";
        private const string TestAreaObjectName = "CCS_WildlifeTestArea";
        private const string RabbitObjectName = "CCS_TestRabbit";
        private const string DeerObjectName = "CCS_TestDeer";
        private const string LogPrefix = "[CCS_WildlifeAiBootstrapSetup]";

        #region Public Methods

        public static void ExecuteBatch()
        {
            EnsureFolders();
            CCS_WildlifeAiProfile aiProfile = EnsureDefaultAiProfile();
            EnsureBootstrapPrefabAiProfile(aiProfile);
            EnsureBootstrapLivingWildlife(aiProfile);
            EnsureHudWildlifeAiDebugPanel();

            AssetDatabase.SaveAssets();
            Debug.Log($"{LogPrefix} Wildlife AI bootstrap setup complete.");
            EditorApplication.Exit(0);
        }

        #endregion

        #region Private Methods

        private static void EnsureFolders()
        {
            if (!AssetDatabase.IsValidFolder("Assets/CCS/Survival/Profiles"))
            {
                AssetDatabase.CreateFolder("Assets/CCS/Survival", "Profiles");
            }

            if (!AssetDatabase.IsValidFolder(ProfilesRoot))
            {
                AssetDatabase.CreateFolder("Assets/CCS/Survival/Profiles", "Wildlife");
            }
        }

        private static CCS_WildlifeAiProfile EnsureDefaultAiProfile()
        {
            CCS_WildlifeAiProfile profile = AssetDatabase.LoadAssetAtPath<CCS_WildlifeAiProfile>(DefaultAiProfilePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<CCS_WildlifeAiProfile>();
                AssetDatabase.CreateAsset(profile, DefaultAiProfilePath);
            }

            SerializedObject serializedProfile = new SerializedObject(profile);
            serializedProfile.FindProperty("profileDisplayName").stringValue = "Default Wildlife AI";
            serializedProfile.FindProperty("profileId").stringValue = "ccs.survival.profile.wildlifeai.default";
            serializedProfile.FindProperty("profileDescription").stringValue =
                "Default passive wildlife AI tuning for 0.9.7 foundation.";
            serializedProfile.FindProperty("profileVersion").stringValue = "0.9.7";
            serializedProfile.FindProperty("minimumIdleSeconds").floatValue = 1f;
            serializedProfile.FindProperty("maximumIdleSeconds").floatValue = 3f;
            serializedProfile.FindProperty("alertDurationSeconds").floatValue = 0.25f;
            serializedProfile.FindProperty("fleeDestinationDistance").floatValue = 6f;

            ApplySpeciesSettings(serializedProfile.FindProperty("rabbitSettings"), 10f, 8f, 4f);
            ApplySpeciesSettings(serializedProfile.FindProperty("deerSettings"), 20f, 15f, 6f);
            serializedProfile.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
            return profile;
        }

        private static void ApplySpeciesSettings(
            SerializedProperty speciesProperty,
            float wanderRadius,
            float fleeRadius,
            float moveSpeed)
        {
            speciesProperty.FindPropertyRelative("wanderRadius").floatValue = wanderRadius;
            speciesProperty.FindPropertyRelative("fleeRadius").floatValue = fleeRadius;
            speciesProperty.FindPropertyRelative("moveSpeed").floatValue = moveSpeed;
        }

        private static void EnsureBootstrapPrefabAiProfile(CCS_WildlifeAiProfile profile)
        {
            GameObject prefabRoot = AssetDatabase.LoadAssetAtPath<GameObject>(BootstrapPrefabPath);
            if (prefabRoot == null)
            {
                Debug.LogError($"{LogPrefix} Missing bootstrap prefab: {BootstrapPrefabPath}");
                EditorApplication.Exit(1);
                return;
            }

            CCS_SurvivalGameplayServiceHost serviceHost =
                prefabRoot.GetComponent<CCS_SurvivalGameplayServiceHost>();
            if (serviceHost == null)
            {
                Debug.LogError($"{LogPrefix} Bootstrap prefab is missing CCS_SurvivalGameplayServiceHost.");
                EditorApplication.Exit(1);
                return;
            }

            SerializedObject serializedHost = new SerializedObject(serviceHost);
            serializedHost.FindProperty("wildlifeAiProfile").objectReferenceValue = profile;
            serializedHost.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(prefabRoot);
        }

        private static void EnsureBootstrapLivingWildlife(CCS_WildlifeAiProfile profile)
        {
            Scene scene = EditorSceneManager.OpenScene(BootstrapScenePath, OpenSceneMode.Single);
            Transform sceneRoot = FindSceneRoot();
            if (sceneRoot == null)
            {
                Debug.LogError($"{LogPrefix} Could not find CCS_BuildVerificationScene root.");
                EditorApplication.Exit(1);
                return;
            }

            Transform testArea = sceneRoot.Find(TestAreaObjectName);
            if (testArea == null)
            {
                GameObject testAreaObject = new GameObject(TestAreaObjectName);
                testAreaObject.transform.SetParent(sceneRoot, false);
                testAreaObject.transform.localPosition = new Vector3(4f, 0f, 4f);
                testArea = testAreaObject.transform;
            }

            Transform playerTransform = FindPlayerTransform(sceneRoot);
            EnsureLivingWildlife(
                testArea,
                RabbitObjectName,
                PrimitiveType.Sphere,
                new Vector3(-1f, 0.35f, -2f),
                new Vector3(0.5f, 0.5f, 0.5f),
                "Rabbit",
                CCS_WildlifeAiSpecies.Rabbit,
                profile,
                playerTransform);

            EnsureLivingWildlife(
                testArea,
                DeerObjectName,
                PrimitiveType.Capsule,
                new Vector3(1.5f, 0.75f, -2f),
                new Vector3(0.8f, 1.5f, 0.8f),
                "Deer",
                CCS_WildlifeAiSpecies.Deer,
                profile,
                playerTransform);

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

        private static Transform FindPlayerTransform(Transform sceneRoot)
        {
            Transform player = sceneRoot.Find("PF_CCS_Player");
            if (player != null)
            {
                return player;
            }

            UnityEngine.CharacterController[] characterControllers =
                Object.FindObjectsByType<UnityEngine.CharacterController>(
                    FindObjectsInactive.Exclude,
                    FindObjectsSortMode.None);
            if (characterControllers == null || characterControllers.Length == 0)
            {
                return null;
            }

            for (int index = 0; index < characterControllers.Length; index++)
            {
                if (characterControllers[index].gameObject.name == "PF_CCS_Player")
                {
                    return characterControllers[index].transform;
                }
            }

            return characterControllers[0].transform;
        }

        private static void EnsureLivingWildlife(
            Transform parent,
            string objectName,
            PrimitiveType primitiveType,
            Vector3 localPosition,
            Vector3 localScale,
            string displayName,
            CCS_WildlifeAiSpecies species,
            CCS_WildlifeAiProfile profile,
            Transform playerTransform)
        {
            Transform existing = parent.Find(objectName);
            GameObject wildlifeObject;

            if (existing != null)
            {
                wildlifeObject = existing.gameObject;
            }
            else
            {
                wildlifeObject = GameObject.CreatePrimitive(primitiveType);
                wildlifeObject.name = objectName;
                wildlifeObject.transform.SetParent(parent, false);
            }

            wildlifeObject.transform.localPosition = localPosition;
            wildlifeObject.transform.localScale = localScale;

            Rigidbody rigidbody = wildlifeObject.GetComponent<Rigidbody>();
            if (rigidbody != null)
            {
                Object.DestroyImmediate(rigidbody);
            }

            Collider collider = wildlifeObject.GetComponent<Collider>();
            if (collider != null)
            {
                collider.isTrigger = false;
                collider.enabled = true;
            }

            CCS_WildlifeAgent agent = wildlifeObject.GetComponent<CCS_WildlifeAgent>();
            if (agent == null)
            {
                agent = wildlifeObject.AddComponent<CCS_WildlifeAgent>();
            }

            agent.ConfigureForBootstrap(displayName, species, profile, playerTransform);
            EditorUtility.SetDirty(wildlifeObject);
        }

        private static void EnsureHudWildlifeAiDebugPanel()
        {
            if (!File.Exists(HudPrefabPath))
            {
                Debug.LogWarning($"{LogPrefix} HUD prefab missing. Run UI HUD bootstrap to create wildlife debug panel.");
                return;
            }

            GameObject prefabRoot = PrefabUtility.LoadPrefabContents(HudPrefabPath);
            if (prefabRoot == null)
            {
                return;
            }

            CCS_HudRootPresenter rootPresenter = prefabRoot.GetComponent<CCS_HudRootPresenter>();
            CCS_WildlifeAiDebugPresenter existingPresenter =
                prefabRoot.GetComponentInChildren<CCS_WildlifeAiDebugPresenter>(true);

            if (existingPresenter == null)
            {
                CCS.Modules.UI.Editor.CCS_UIHudBootstrapSetup.EnsureProfileFoldersPublic();
                CCS_HudProfile profile =
                    AssetDatabase.LoadAssetAtPath<CCS_HudProfile>(
                        "Assets/CCS/Survival/Profiles/UI/CCS_DefaultHudProfile.asset");
                if (profile != null)
                {
                    CCS.Modules.UI.Editor.CCS_UIHudBootstrapSetup.BuildHudPrefab(profile, HudPrefabPath);
                }

                PrefabUtility.UnloadPrefabContents(prefabRoot);
                return;
            }

            SerializedObject serializedPresenter = new SerializedObject(rootPresenter);
            serializedPresenter.FindProperty("wildlifeAiDebugPresenter").objectReferenceValue = existingPresenter;
            serializedPresenter.FindProperty("wildlifeAiDebugArea").objectReferenceValue =
                existingPresenter.GetComponent<RectTransform>();
            serializedPresenter.ApplyModifiedPropertiesWithoutUndo();
            PrefabUtility.SaveAsPrefabAsset(prefabRoot, HudPrefabPath);
            PrefabUtility.UnloadPrefabContents(prefabRoot);
        }

        #endregion
    }
}
