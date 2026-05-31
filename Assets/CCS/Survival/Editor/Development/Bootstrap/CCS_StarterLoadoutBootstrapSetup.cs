using System.Collections.Generic;
using System.IO;
using CCS.Modules.Crafting;
using CCS.Modules.Inventory;
using CCS.Modules.WorldResources;
using CCS.Survival.Composition;
using CCS.Survival.Player.Loadout;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// =============================================================================
// SCRIPT: CCS_StarterLoadoutBootstrapSetup
// CATEGORY: Survival / Editor / Development / Bootstrap
// PURPOSE: Creates starter items, primitive recipes, loadout profile, and bootstrap wiring.
// PLACEMENT: Batch entry for 0.9.1 starter loadout and primitive progression milestone.
// AUTHOR: James Schilz
// CREATED: 2026-05-31
// NOTES: Updates tree harvest to Knife → Branch. Harnesses remain disabled by default.
// =============================================================================

namespace CCS.Survival.Editor.Development
{
    public static class CCS_StarterLoadoutBootstrapSetup
    {
        private const string StarterItemsRoot = "Assets/CCS/Survival/Content/Items/Starter";
        private const string StarterProfileRoot = "Assets/CCS/Survival/Profiles/StarterLoadout";
        private const string StarterProfilePath = StarterProfileRoot + "/CCS_DefaultStarterLoadoutProfile.asset";
        private const string PrimitiveRecipesRoot = "Assets/CCS/Survival/Profiles/Crafting/PrimitiveRecipes";
        private const string InventoryProfilePath = "Assets/CCS/Survival/Profiles/Inventory/CCS_DefaultInventoryProfile.asset";
        private const string TreeResourcePath = "Assets/CCS/Survival/Profiles/WorldResources/TestResources/CCS_TestResource_Tree.asset";
        private const string BootstrapPrefabPath = "Assets/CCS/Survival/Prefabs/PF_CCS_Survival_BootstrapRoot.prefab";
        private const string BootstrapScenePath = "Assets/CCS/Survival/Scenes/SCN_CCS_Survival_Bootstrap.unity";
        private const string LogPrefix = "[CCS_StarterLoadoutBootstrapSetup]";

        #region Public Methods

        public static void ExecuteBatch()
        {
            EnsureFolders();

            CCS_ItemDefinition knife = EnsureStarterItem(
                "CCS_Item_Knife",
                "ccs.survival.item.starter.knife",
                "Knife",
                "Primitive cutting tool for early harvesting.",
                CCS_ItemCategory.Tool,
                1,
                0.5f,
                true,
                CCS_ItemToolType.Knife);

            CCS_ItemDefinition basicFood = EnsureStarterItem(
                "CCS_Item_BasicFood",
                "ccs.survival.item.starter.basicfood",
                "Basic Food",
                "Simple food for early survival.",
                CCS_ItemCategory.Consumable,
                10,
                0.2f,
                false,
                CCS_ItemToolType.None);

            CCS_ItemDefinition coin = EnsureStarterItem(
                "CCS_Item_Coin",
                "ccs.survival.item.starter.coin",
                "Coin",
                "Currency placeholder for future economy systems.",
                CCS_ItemCategory.Generic,
                999,
                0.01f,
                false,
                CCS_ItemToolType.None);

            CCS_ItemDefinition branch = EnsureStarterItem(
                "CCS_Item_Branch",
                "ccs.survival.item.starter.branch",
                "Branch",
                "Primitive wood branch gathered from early tree harvesting.",
                CCS_ItemCategory.Material,
                50,
                0.3f,
                false,
                CCS_ItemToolType.None);

            CCS_ItemDefinition spear = EnsureStarterItem(
                "CCS_Item_Spear",
                "ccs.survival.item.starter.spear",
                "Spear",
                "Primitive spear placeholder.",
                CCS_ItemCategory.Tool,
                1,
                1.5f,
                false,
                CCS_ItemToolType.None);

            CCS_ItemDefinition bowStave = EnsureStarterItem(
                "CCS_Item_BowStave",
                "ccs.survival.item.starter.bowstave",
                "Bow Stave",
                "Primitive bow component placeholder.",
                CCS_ItemCategory.Material,
                5,
                1f,
                false,
                CCS_ItemToolType.None);

            CCS_ItemDefinition arrowShaft = EnsureStarterItem(
                "CCS_Item_ArrowShaft",
                "ccs.survival.item.starter.arrowshaft",
                "Arrow Shaft",
                "Primitive arrow shaft placeholder.",
                CCS_ItemCategory.Material,
                50,
                0.1f,
                false,
                CCS_ItemToolType.None);

            CCS_ItemDefinition campfireKit = EnsureStarterItem(
                "CCS_Item_CampfireKit",
                "ccs.survival.item.starter.campfirekit",
                "Campfire Kit",
                "Primitive campfire kit placeholder.",
                CCS_ItemCategory.Material,
                5,
                2f,
                false,
                CCS_ItemToolType.None);

            UpdateTreeResourceForKnifeHarvest(branch);

            CCS_CraftingRecipeDefinition spearRecipe = EnsurePrimitiveRecipe(
                "CCS_PrimitiveSpearRecipe",
                "ccs.survival.recipe.primitive.spear",
                "Primitive Spear",
                branch,
                3,
                spear,
                1);

            CCS_CraftingRecipeDefinition bowStaveRecipe = EnsurePrimitiveRecipe(
                "CCS_PrimitiveBowStaveRecipe",
                "ccs.survival.recipe.primitive.bowstave",
                "Primitive Bow Stave",
                branch,
                2,
                bowStave,
                1);

            CCS_CraftingRecipeDefinition arrowShaftRecipe = EnsurePrimitiveRecipe(
                "CCS_PrimitiveArrowShaftRecipe",
                "ccs.survival.recipe.primitive.arrowshaft",
                "Primitive Arrow Shafts",
                branch,
                1,
                arrowShaft,
                4);

            CCS_CraftingRecipeDefinition campfireKitRecipe = EnsurePrimitiveRecipe(
                "CCS_PrimitiveCampfireKitRecipe",
                "ccs.survival.recipe.primitive.campfirekit",
                "Primitive Campfire Kit",
                branch,
                5,
                campfireKit,
                1);

            CCS_CraftingRecipeDefinition[] primitiveRecipes =
            {
                spearRecipe,
                bowStaveRecipe,
                arrowShaftRecipe,
                campfireKitRecipe
            };

            EnsureStarterLoadoutProfile(knife, basicFood, coin, primitiveRecipes, 2, 10);
            EnsureInventorySaveRestoreCatalog(
                knife,
                basicFood,
                coin,
                branch,
                spear,
                bowStave,
                arrowShaft,
                campfireKit);
            EnsureBootstrapGameplayServiceHost();
            EnsureHarvestablesUseInventoryTools();
            EnsureCraftingHarnessPrimitiveRecipes(
                spearRecipe,
                bowStaveRecipe,
                arrowShaftRecipe,
                campfireKitRecipe);

            AssetDatabase.SaveAssets();
            Debug.Log($"{LogPrefix} Starter loadout bootstrap setup complete.");
            EditorApplication.Exit(0);
        }

        #endregion

        #region Private Methods

        private static void EnsureFolders()
        {
            EnsureFolder("Assets/CCS/Survival/Content");
            EnsureFolder("Assets/CCS/Survival/Content/Items");
            EnsureFolder(StarterItemsRoot);
            EnsureFolder(StarterProfileRoot);
            EnsureFolder("Assets/CCS/Survival/Profiles/Crafting");
            EnsureFolder(PrimitiveRecipesRoot);
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

        private static CCS_ItemDefinition EnsureStarterItem(
            string assetName,
            string itemId,
            string displayName,
            string description,
            CCS_ItemCategory category,
            int maxStackSize,
            float weight,
            bool hasToolIdentity,
            CCS_ItemToolType toolType)
        {
            string assetPath = $"{StarterItemsRoot}/{assetName}.asset";
            CCS_ItemDefinition itemDefinition = AssetDatabase.LoadAssetAtPath<CCS_ItemDefinition>(assetPath);
            if (itemDefinition == null)
            {
                itemDefinition = ScriptableObject.CreateInstance<CCS_ItemDefinition>();
                AssetDatabase.CreateAsset(itemDefinition, assetPath);
            }

            SerializedObject serializedItem = new SerializedObject(itemDefinition);
            serializedItem.FindProperty("itemId").stringValue = itemId;
            serializedItem.FindProperty("displayName").stringValue = displayName;
            serializedItem.FindProperty("description").stringValue = description;
            serializedItem.FindProperty("category").enumValueIndex = (int)category;
            serializedItem.FindProperty("maxStackSize").intValue = maxStackSize;
            serializedItem.FindProperty("isStackable").boolValue = maxStackSize > 1;
            serializedItem.FindProperty("weight").floatValue = weight;
            serializedItem.FindProperty("hasToolIdentity").boolValue = hasToolIdentity;
            serializedItem.FindProperty("toolType").enumValueIndex = (int)toolType;
            serializedItem.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(itemDefinition);
            return itemDefinition;
        }

        private static void UpdateTreeResourceForKnifeHarvest(CCS_ItemDefinition branchItem)
        {
            CCS_ResourceDefinition treeDefinition =
                AssetDatabase.LoadAssetAtPath<CCS_ResourceDefinition>(TreeResourcePath);
            if (treeDefinition == null)
            {
                Debug.LogError($"{LogPrefix} Missing tree resource definition: {TreeResourcePath}");
                EditorApplication.Exit(1);
                return;
            }

            SerializedObject serializedTree = new SerializedObject(treeDefinition);
            serializedTree.FindProperty("requiredToolType").enumValueIndex = (int)CCS_RequiredToolType.Knife;

            SerializedProperty dropList = serializedTree.FindProperty("dropDefinitions");
            dropList.ClearArray();
            dropList.InsertArrayElementAtIndex(0);
            SerializedProperty drop = dropList.GetArrayElementAtIndex(0);
            drop.FindPropertyRelative("itemDefinition").objectReferenceValue = branchItem;
            drop.FindPropertyRelative("minQuantity").intValue = 2;
            drop.FindPropertyRelative("maxQuantity").intValue = 4;

            serializedTree.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(treeDefinition);
        }

        private static CCS_CraftingRecipeDefinition EnsurePrimitiveRecipe(
            string assetName,
            string recipeId,
            string displayName,
            CCS_ItemDefinition ingredient,
            int ingredientQuantity,
            CCS_ItemDefinition result,
            int resultQuantity)
        {
            string assetPath = $"{PrimitiveRecipesRoot}/{assetName}.asset";
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
            serializedRecipe.FindProperty("description").stringValue =
                "Primitive hand crafting recipe for early-game progression.";
            serializedRecipe.FindProperty("requiredStationType").enumValueIndex = (int)CCS_CraftingStationType.Hand;
            serializedRecipe.FindProperty("craftTimeSeconds").floatValue = 0f;
            serializedRecipe.FindProperty("isUnlockedByDefault").boolValue = true;

            SerializedProperty ingredients = serializedRecipe.FindProperty("ingredients");
            ingredients.ClearArray();
            ingredients.InsertArrayElementAtIndex(0);
            SerializedProperty ingredientEntry = ingredients.GetArrayElementAtIndex(0);
            ingredientEntry.FindPropertyRelative("itemDefinition").objectReferenceValue = ingredient;
            ingredientEntry.FindPropertyRelative("quantity").intValue = ingredientQuantity;

            SerializedProperty results = serializedRecipe.FindProperty("results");
            results.ClearArray();
            results.InsertArrayElementAtIndex(0);
            SerializedProperty resultEntry = results.GetArrayElementAtIndex(0);
            resultEntry.FindPropertyRelative("itemDefinition").objectReferenceValue = result;
            resultEntry.FindPropertyRelative("quantity").intValue = resultQuantity;

            serializedRecipe.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(recipe);
            return recipe;
        }

        private static void EnsureStarterLoadoutProfile(
            CCS_ItemDefinition knife,
            CCS_ItemDefinition basicFood,
            CCS_ItemDefinition coin,
            CCS_CraftingRecipeDefinition[] primitiveRecipes,
            int basicFoodQuantity,
            int startingCurrencyAmount)
        {
            CCS_StarterLoadoutProfile profile =
                AssetDatabase.LoadAssetAtPath<CCS_StarterLoadoutProfile>(StarterProfilePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<CCS_StarterLoadoutProfile>();
                AssetDatabase.CreateAsset(profile, StarterProfilePath);
            }

            SerializedObject serializedProfile = new SerializedObject(profile);
            serializedProfile.FindProperty("profileDisplayName").stringValue = "Default Starter Loadout";
            serializedProfile.FindProperty("profileId").stringValue = "ccs.survival.profile.starterloadout.default";
            serializedProfile.FindProperty("profileDescription").stringValue =
                "Starter loadout and primitive progression for 0.9.1.";
            serializedProfile.FindProperty("profileVersion").stringValue = "0.9.1";
            serializedProfile.FindProperty("startingCurrencyAmount").intValue = startingCurrencyAmount;
            serializedProfile.FindProperty("currencyItemDefinition").objectReferenceValue = coin;
            serializedProfile.FindProperty("applyWhenInventoryEmpty").boolValue = true;

            SerializedProperty startingItems = serializedProfile.FindProperty("startingItems");
            startingItems.ClearArray();

            AddLoadoutEntry(startingItems, 0, knife, 1);
            AddLoadoutEntry(startingItems, 1, basicFood, basicFoodQuantity);

            SerializedProperty recipeList = serializedProfile.FindProperty("primitiveRecipes");
            recipeList.ClearArray();
            for (int index = 0; index < primitiveRecipes.Length; index++)
            {
                recipeList.InsertArrayElementAtIndex(index);
                recipeList.GetArrayElementAtIndex(index).objectReferenceValue = primitiveRecipes[index];
            }

            serializedProfile.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
        }

        private static void AddLoadoutEntry(
            SerializedProperty startingItems,
            int index,
            CCS_ItemDefinition itemDefinition,
            int quantity)
        {
            startingItems.InsertArrayElementAtIndex(index);
            SerializedProperty entry = startingItems.GetArrayElementAtIndex(index);
            entry.FindPropertyRelative("itemDefinition").objectReferenceValue = itemDefinition;
            entry.FindPropertyRelative("quantity").intValue = quantity;
        }

        private static void EnsureInventorySaveRestoreCatalog(params CCS_ItemDefinition[] itemDefinitions)
        {
            CCS_InventoryProfile inventoryProfile =
                AssetDatabase.LoadAssetAtPath<CCS_InventoryProfile>(InventoryProfilePath);
            if (inventoryProfile == null)
            {
                Debug.LogError($"{LogPrefix} Missing inventory profile: {InventoryProfilePath}");
                EditorApplication.Exit(1);
                return;
            }

            List<CCS_ItemDefinition> mergedDefinitions = new List<CCS_ItemDefinition>();
            CCS_ItemDefinition[] existingDefinitions = inventoryProfile.SaveRestoreItemDefinitions;
            for (int index = 0; index < existingDefinitions.Length; index++)
            {
                CCS_ItemDefinition existingDefinition = existingDefinitions[index];
                if (existingDefinition != null && !mergedDefinitions.Contains(existingDefinition))
                {
                    mergedDefinitions.Add(existingDefinition);
                }
            }

            for (int index = 0; index < itemDefinitions.Length; index++)
            {
                CCS_ItemDefinition itemDefinition = itemDefinitions[index];
                if (itemDefinition != null && !mergedDefinitions.Contains(itemDefinition))
                {
                    mergedDefinitions.Add(itemDefinition);
                }
            }

            SerializedObject serializedProfile = new SerializedObject(inventoryProfile);
            SerializedProperty restoreList = serializedProfile.FindProperty("saveRestoreItemDefinitions");
            restoreList.ClearArray();
            for (int index = 0; index < mergedDefinitions.Count; index++)
            {
                restoreList.InsertArrayElementAtIndex(index);
                restoreList.GetArrayElementAtIndex(index).objectReferenceValue = mergedDefinitions[index];
            }

            serializedProfile.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(inventoryProfile);
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
            serializedHost.FindProperty("starterLoadoutProfile").objectReferenceValue =
                AssetDatabase.LoadAssetAtPath<Object>(StarterProfilePath);
            serializedHost.ApplyModifiedPropertiesWithoutUndo();

            PrefabUtility.SaveAsPrefabAsset(prefabContents, prefabPath);
            PrefabUtility.UnloadPrefabContents(prefabContents);
        }

        private static void EnsureHarvestablesUseInventoryTools()
        {
            Scene scene = EditorSceneManager.OpenScene(BootstrapScenePath, OpenSceneMode.Single);
            CCS_HarvestableResource[] harvestables =
                Object.FindObjectsByType<CCS_HarvestableResource>(FindObjectsSortMode.None);

            for (int index = 0; index < harvestables.Length; index++)
            {
                SerializedObject serializedHarvestable = new SerializedObject(harvestables[index]);
                serializedHarvestable.FindProperty("assumeRequiredToolEquipped").boolValue = false;
                serializedHarvestable.ApplyModifiedPropertiesWithoutUndo();
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static void EnsureCraftingHarnessPrimitiveRecipes(
            CCS_CraftingRecipeDefinition spearRecipe,
            CCS_CraftingRecipeDefinition bowStaveRecipe,
            CCS_CraftingRecipeDefinition arrowShaftRecipe,
            CCS_CraftingRecipeDefinition campfireKitRecipe)
        {
            Scene scene = EditorSceneManager.GetActiveScene();
            CCS_CraftingTestHarness[] harnesses =
                Object.FindObjectsByType<CCS_CraftingTestHarness>(FindObjectsSortMode.None);

            for (int index = 0; index < harnesses.Length; index++)
            {
                SerializedObject serializedHarness = new SerializedObject(harnesses[index]);
                serializedHarness.FindProperty("enableHarness").boolValue = false;
                serializedHarness.FindProperty("spearRecipe").objectReferenceValue = spearRecipe;
                serializedHarness.FindProperty("bowStaveRecipe").objectReferenceValue = bowStaveRecipe;
                serializedHarness.FindProperty("arrowShaftRecipe").objectReferenceValue = arrowShaftRecipe;
                serializedHarness.FindProperty("campfireKitRecipe").objectReferenceValue = campfireKitRecipe;
                serializedHarness.ApplyModifiedPropertiesWithoutUndo();
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        #endregion
    }
}
