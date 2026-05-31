using System.IO;
using CCS.Modules.Crafting;
using CCS.Modules.Inventory;
using CCS.Survival.Composition;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// =============================================================================
// SCRIPT: CCS_CraftingBootstrapSetup
// CATEGORY: Modules / Crafting / Editor / Validation
// PURPOSE: Creates test recipes, items, bootstrap harness, and gameplay service wiring.
// PLACEMENT: Batch entry for 0.5.3 crafting gameplay integration milestone.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Hand crafting only. No final station art or crafting UI.
// =============================================================================

namespace CCS.Modules.Crafting.Editor
{
    public static class CCS_CraftingBootstrapSetup
    {
        private const string ProfilesRoot = "Assets/CCS/Survival/Profiles/Crafting";
        private const string DefaultProfilePath = ProfilesRoot + "/CCS_DefaultCraftingProfile.asset";
        private const string TestItemsRoot = ProfilesRoot + "/TestItems";
        private const string TestRecipesRoot = ProfilesRoot + "/TestRecipes";
        private const string WorldTestItemsRoot = "Assets/CCS/Survival/Profiles/WorldResources/TestItems";
        private const string BootstrapPrefabPath = "Assets/CCS/Survival/Prefabs/PF_CCS_Survival_BootstrapRoot.prefab";
        private const string BootstrapScenePath = "Assets/CCS/Survival/Scenes/SCN_CCS_Survival_Bootstrap.unity";
        private const string LogPrefix = "[CCS_CraftingBootstrapSetup]";

        private const string HarnessObjectName = "CCS_CraftingTestHarness";

        #region Public Methods

        public static void ExecuteBatch()
        {
            EnsureFolders();

            CCS_ItemDefinition woodItem = LoadWorldTestItem("CCS_TestItem_Wood");
            CCS_ItemDefinition stoneItem = LoadWorldTestItem("CCS_TestItem_Stone");
            CCS_ItemDefinition fiberItem = LoadWorldTestItem("CCS_TestItem_Fiber");

            if (woodItem == null || stoneItem == null || fiberItem == null)
            {
                Debug.LogError($"{LogPrefix} Missing world resource test items. Run world resource bootstrap setup first.");
                EditorApplication.Exit(1);
                return;
            }

            CCS_ItemDefinition campfireKitItem = EnsureTestItem(
                "CCS_TestItem_CampfireKit",
                "ccs.survival.item.test.campfirekit",
                "Campfire Kit");

            CCS_ItemDefinition bandageItem = EnsureTestItem(
                "CCS_TestItem_Bandage",
                "ccs.survival.item.test.bandage",
                "Bandage");

            CCS_CraftingRecipeDefinition bandageRecipe = EnsureTestRecipe(
                "CCS_TestBandageRecipe",
                "ccs.survival.recipe.test.bandage",
                "Test Bandage Recipe",
                CCS_CraftingStationType.Hand,
                new[]
                {
                    (fiberItem, 2)
                },
                new[]
                {
                    (bandageItem, 1)
                });

            CCS_CraftingRecipeDefinition campfireRecipe = EnsureTestRecipe(
                "CCS_TestCampfireRecipe",
                "ccs.survival.recipe.test.campfire",
                "Test Campfire Recipe",
                CCS_CraftingStationType.Hand,
                new[]
                {
                    (woodItem, 3),
                    (stoneItem, 2)
                },
                new[]
                {
                    (campfireKitItem, 1)
                });

            EnsureDefaultProfileUpdated();
            EnsureBootstrapGameplayServiceHost();
            EnsureBootstrapCraftingHarness(bandageRecipe, campfireRecipe);

            AssetDatabase.SaveAssets();
            Debug.Log($"{LogPrefix} Crafting bootstrap setup complete.");
            EditorApplication.Exit(0);
        }

        #endregion

        #region Private Methods

        private static void EnsureFolders()
        {
            EnsureFolder("Assets/CCS/Survival/Profiles");
            EnsureFolder(ProfilesRoot);
            EnsureFolder(TestItemsRoot);
            EnsureFolder(TestRecipesRoot);
        }

        private static CCS_ItemDefinition LoadWorldTestItem(string assetName)
        {
            return AssetDatabase.LoadAssetAtPath<CCS_ItemDefinition>($"{WorldTestItemsRoot}/{assetName}.asset");
        }

        private static CCS_ItemDefinition EnsureTestItem(string assetName, string itemId, string displayName)
        {
            string assetPath = $"{TestItemsRoot}/{assetName}.asset";
            CCS_ItemDefinition itemDefinition = AssetDatabase.LoadAssetAtPath<CCS_ItemDefinition>(assetPath);
            if (itemDefinition != null)
            {
                return itemDefinition;
            }

            itemDefinition = ScriptableObject.CreateInstance<CCS_ItemDefinition>();
            SerializedObject serializedItem = new SerializedObject(itemDefinition);
            serializedItem.FindProperty("itemId").stringValue = itemId;
            serializedItem.FindProperty("displayName").stringValue = displayName;
            serializedItem.FindProperty("description").stringValue = "Bootstrap verification test craft output.";
            serializedItem.FindProperty("maxStackSize").intValue = 99;
            serializedItem.FindProperty("isStackable").boolValue = true;
            serializedItem.ApplyModifiedPropertiesWithoutUndo();

            AssetDatabase.CreateAsset(itemDefinition, assetPath);
            return itemDefinition;
        }

        private static CCS_CraftingRecipeDefinition EnsureTestRecipe(
            string assetName,
            string recipeId,
            string displayName,
            CCS_CraftingStationType stationType,
            (CCS_ItemDefinition item, int quantity)[] ingredients,
            (CCS_ItemDefinition item, int quantity)[] results)
        {
            string assetPath = $"{TestRecipesRoot}/{assetName}.asset";
            CCS_CraftingRecipeDefinition recipe =
                AssetDatabase.LoadAssetAtPath<CCS_CraftingRecipeDefinition>(assetPath);

            if (recipe == null)
            {
                recipe = ScriptableObject.CreateInstance<CCS_CraftingRecipeDefinition>();
                AssetDatabase.CreateAsset(recipe, assetPath);
            }

            SerializedObject serializedRecipe = new SerializedObject(recipe);
            serializedRecipe.FindProperty("recipeId").stringValue = recipeId;
            serializedRecipe.FindProperty("displayName").stringValue = displayName;
            serializedRecipe.FindProperty("description").stringValue = "Bootstrap verification test recipe.";
            serializedRecipe.FindProperty("requiredStationType").enumValueIndex = (int)stationType;
            serializedRecipe.FindProperty("craftTimeSeconds").floatValue = 0f;
            serializedRecipe.FindProperty("isUnlockedByDefault").boolValue = true;

            SerializedProperty ingredientProperty = serializedRecipe.FindProperty("ingredients");
            ingredientProperty.arraySize = ingredients.Length;
            for (int i = 0; i < ingredients.Length; i++)
            {
                SerializedProperty entry = ingredientProperty.GetArrayElementAtIndex(i);
                entry.FindPropertyRelative("itemDefinition").objectReferenceValue = ingredients[i].item;
                entry.FindPropertyRelative("quantity").intValue = ingredients[i].quantity;
            }

            SerializedProperty resultProperty = serializedRecipe.FindProperty("results");
            resultProperty.arraySize = results.Length;
            for (int i = 0; i < results.Length; i++)
            {
                SerializedProperty entry = resultProperty.GetArrayElementAtIndex(i);
                entry.FindPropertyRelative("itemDefinition").objectReferenceValue = results[i].item;
                entry.FindPropertyRelative("quantity").intValue = results[i].quantity;
            }

            serializedRecipe.ApplyModifiedPropertiesWithoutUndo();
            return recipe;
        }

        private static void EnsureDefaultProfileUpdated()
        {
            CCS_CraftingProfile profile = AssetDatabase.LoadAssetAtPath<CCS_CraftingProfile>(DefaultProfilePath);
            if (profile == null)
            {
                Debug.LogError($"{LogPrefix} Missing default crafting profile: {DefaultProfilePath}");
                EditorApplication.Exit(1);
                return;
            }

            SerializedObject serializedProfile = new SerializedObject(profile);
            serializedProfile.FindProperty("profileDescription").stringValue =
                "Default crafting rules for 0.5.3 gameplay integration.";
            serializedProfile.FindProperty("profileVersion").stringValue = "0.5.3";
            serializedProfile.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
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
            serializedHost.FindProperty("craftingProfile").objectReferenceValue =
                AssetDatabase.LoadAssetAtPath<Object>(DefaultProfilePath);
            serializedHost.ApplyModifiedPropertiesWithoutUndo();

            PrefabUtility.SaveAsPrefabAsset(prefabContents, prefabPath);
            PrefabUtility.UnloadPrefabContents(prefabContents);
        }

        private static void EnsureBootstrapCraftingHarness(
            CCS_CraftingRecipeDefinition bandageRecipe,
            CCS_CraftingRecipeDefinition campfireRecipe)
        {
            Scene scene = EditorSceneManager.OpenScene(BootstrapScenePath, OpenSceneMode.Single);
            Transform sceneRoot = FindSceneRoot();
            if (sceneRoot == null)
            {
                Debug.LogError($"{LogPrefix} Could not find CCS_BuildVerificationScene root.");
                EditorApplication.Exit(1);
                return;
            }

            Transform existingHarness = sceneRoot.Find(HarnessObjectName);
            GameObject harnessObject = existingHarness != null
                ? existingHarness.gameObject
                : new GameObject(HarnessObjectName);

            if (existingHarness == null)
            {
                harnessObject.transform.SetParent(sceneRoot, false);
            }

            CCS_CraftingTestHarness harness = harnessObject.GetComponent<CCS_CraftingTestHarness>();
            if (harness == null)
            {
                harness = harnessObject.AddComponent<CCS_CraftingTestHarness>();
            }

            SerializedObject serializedHarness = new SerializedObject(harness);
            serializedHarness.FindProperty("enableHarness").boolValue = true;
            serializedHarness.FindProperty("craftAttemptIntervalSeconds").floatValue = 5f;
            serializedHarness.FindProperty("bandageRecipe").objectReferenceValue = bandageRecipe;
            serializedHarness.FindProperty("campfireRecipe").objectReferenceValue = campfireRecipe;
            serializedHarness.ApplyModifiedPropertiesWithoutUndo();

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
