using System.Collections.Generic;
using System.IO;
using CCS.Modules.Crafting;
using CCS.Modules.Inventory;
using CCS.Survival.Composition;
using CCS.Survival.Editor.Development;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// =============================================================================
// SCRIPT: CCS_CraftingProgressionBootstrapSetup
// CATEGORY: Modules / Crafting / Editor / Validation
// PURPOSE: Creates primitive progression items, recipes, workbench, and profile wiring.
// PLACEMENT: Batch entry for milestone 1.1.1 crafting progression foundation.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-06-01
// NOTES: FirePit station type is used for campfire crafting recipes.
// =============================================================================

namespace CCS.Modules.Crafting.Editor
{
    public static class CCS_CraftingProgressionBootstrapSetup
    {
        private const string ProfilesRoot = "Assets/CCS/Survival/Profiles/Crafting";
        private const string ProgressionProfilePath = ProfilesRoot + "/CCS_DefaultCraftingProgressionProfile.asset";
        private const string ProgressionItemsRoot = "Assets/CCS/Survival/Content/Items/Progression";
        private const string ProgressionRecipesRoot = ProfilesRoot + "/ProgressionRecipes";
        private const string PrimitiveItemsRoot = "Assets/CCS/Survival/Content/Items/Resources/Primitive";
        private const string StarterItemsRoot = "Assets/CCS/Survival/Content/Items/Starter";
        private const string FoodItemsRoot = "Assets/CCS/Survival/Content/Items/Food";
        private const string BootstrapPrefabPath = "Assets/CCS/Survival/Prefabs/PF_CCS_Survival_BootstrapRoot.prefab";
        private const string BootstrapScenePath = "Assets/CCS/Survival/Scenes/SCN_CCS_Survival_Bootstrap.unity";
        private const string InventoryProfilePath = "Assets/CCS/Survival/Profiles/Inventory/CCS_DefaultInventoryProfile.asset";
        private const string LogPrefix = "[CCS_CraftingProgressionBootstrapSetup]";
        private const string WorkbenchObjectName = "CCS_TestWorkbench";

        #region Public Methods

        public static void ExecuteBatch()
        {
            AssetDatabase.Refresh();
            EnsureFolders();

            CCS_ItemDefinition woodItem = LoadItem(PrimitiveItemsRoot + "/CCS_Item_Wood.asset");
            CCS_ItemDefinition stickItem = LoadItem(PrimitiveItemsRoot + "/CCS_Item_Stick.asset");
            CCS_ItemDefinition stoneItem = LoadItem(PrimitiveItemsRoot + "/CCS_Item_Stone.asset");
            CCS_ItemDefinition fiberItem = LoadItem(PrimitiveItemsRoot + "/CCS_Item_Fiber.asset");
            CCS_ItemDefinition spearItem = LoadItem(StarterItemsRoot + "/CCS_Item_Spear.asset");
            CCS_ItemDefinition bedrollItem = LoadItem(StarterItemsRoot + "/CCS_Item_Bedroll.asset");
            CCS_ItemDefinition cookedRabbitItem = LoadItem(FoodItemsRoot + "/CCS_Item_CookedRabbitMeat.asset");
            CCS_ItemDefinition cookedVenisonItem = LoadItem(FoodItemsRoot + "/CCS_Item_CookedVenison.asset");

            if (woodItem == null || stickItem == null || stoneItem == null || fiberItem == null
                || spearItem == null || bedrollItem == null || cookedRabbitItem == null || cookedVenisonItem == null)
            {
                Debug.LogError(
                    $"{LogPrefix} Missing prerequisite items. wood={woodItem != null} stick={stickItem != null} "
                    + $"stone={stoneItem != null} fiber={fiberItem != null} spear={spearItem != null} "
                    + $"bedroll={bedrollItem != null} rabbit={cookedRabbitItem != null} venison={cookedVenisonItem != null}");
                EditorApplication.Exit(1);
                return;
            }

            CCS_ItemDefinition basicBandageItem = EnsureProgressionItem(
                "CCS_Item_BasicBandage",
                "ccs.survival.item.progression.basicbandage",
                "Basic Bandage",
                CCS_ItemCategory.Consumable,
                "Primitive bandage consumable crafted by hand.");
            CCS_ItemDefinition primitiveTorchItem = EnsureProgressionItem(
                "CCS_Item_PrimitiveTorch",
                "ccs.survival.item.progression.primitivetorch",
                "Primitive Torch",
                CCS_ItemCategory.Tool,
                "Hand-crafted torch placeholder.");
            CCS_ItemDefinition charcoalItem = EnsureProgressionItem(
                "CCS_Item_Charcoal",
                "ccs.survival.item.progression.charcoal",
                "Charcoal",
                CCS_ItemCategory.Material,
                "Campfire byproduct used for future fuel systems.");
            CCS_ItemDefinition ashItem = EnsureProgressionItem(
                "CCS_Item_Ash",
                "ccs.survival.item.progression.ash",
                "Ash",
                CCS_ItemCategory.Material,
                "Campfire residue material.");
            CCS_ItemDefinition driedMeatItem = EnsureProgressionItem(
                "CCS_Item_DriedMeat",
                "ccs.survival.item.progression.driedmeat",
                "Dried Meat",
                CCS_ItemCategory.Consumable,
                "Preserved meat crafted at a campfire.");
            CCS_ItemDefinition reinforcedSpearItem = EnsureReinforcedSpearItem(spearItem);
            CCS_ItemDefinition storageCrateItem = EnsureProgressionItem(
                "CCS_Item_StorageCrate",
                "ccs.survival.item.progression.storagecrate",
                "Storage Crate",
                CCS_ItemCategory.Material,
                "Workbench placeable storage crate placeholder for future building placement.");

            List<CCS_CraftingRecipeDefinition> progressionRecipes = new List<CCS_CraftingRecipeDefinition>
            {
                EnsureRecipe("CCS_ProgressionSpearRecipe", "ccs.survival.recipe.progression.spear", "Primitive Spear",
                    CCS_CraftingStationType.Hand, 2f, 1,
                    new[] { (stickItem, 2), (stoneItem, 1) },
                    new[] { (spearItem, 1) }),
                EnsureRecipe("CCS_ProgressionBasicBandageRecipe", "ccs.survival.recipe.progression.basicbandage",
                    "Basic Bandage", CCS_CraftingStationType.Hand, 1.5f, 1,
                    new[] { (fiberItem, 2) },
                    new[] { (basicBandageItem, 1) }),
                EnsureRecipe("CCS_ProgressionPrimitiveTorchRecipe", "ccs.survival.recipe.progression.primitivetorch",
                    "Primitive Torch", CCS_CraftingStationType.Hand, 1.5f, 1,
                    new[] { (stickItem, 1), (fiberItem, 1) },
                    new[] { (primitiveTorchItem, 1) }),
                EnsureRecipe("CCS_ProgressionCharcoalRecipe", "ccs.survival.recipe.progression.charcoal", "Charcoal",
                    CCS_CraftingStationType.FirePit, 3f, 1,
                    new[] { (woodItem, 1) },
                    new[] { (charcoalItem, 1) }),
                EnsureRecipe("CCS_ProgressionAshRecipe", "ccs.survival.recipe.progression.ash", "Ash",
                    CCS_CraftingStationType.FirePit, 2f, 1,
                    new[] { (woodItem, 1) },
                    new[] { (ashItem, 1) }),
                EnsureRecipe("CCS_ProgressionDriedMeatRabbitRecipe",
                    "ccs.survival.recipe.progression.driedmeat.rabbit", "Dried Rabbit Meat",
                    CCS_CraftingStationType.FirePit, 4f, 1,
                    new[] { (cookedRabbitItem, 1) },
                    new[] { (driedMeatItem, 1) }),
                EnsureRecipe("CCS_ProgressionDriedMeatVenisonRecipe",
                    "ccs.survival.recipe.progression.driedmeat.venison", "Dried Venison",
                    CCS_CraftingStationType.FirePit, 4f, 1,
                    new[] { (cookedVenisonItem, 1) },
                    new[] { (driedMeatItem, 1) }),
                EnsureRecipe("CCS_ProgressionReinforcedSpearRecipe",
                    "ccs.survival.recipe.progression.reinforcedspear", "Reinforced Spear",
                    CCS_CraftingStationType.Workbench, 5f, 2,
                    new[] { (spearItem, 1), (woodItem, 2), (stoneItem, 2) },
                    new[] { (reinforcedSpearItem, 1) }),
                EnsureRecipe("CCS_ProgressionStorageCrateRecipe",
                    "ccs.survival.recipe.progression.storagecrate", "Storage Crate",
                    CCS_CraftingStationType.Workbench, 4f, 2,
                    new[] { (woodItem, 6) },
                    new[] { (storageCrateItem, 1) }),
                EnsureRecipe("CCS_ProgressionBedrollRecipe", "ccs.survival.recipe.progression.bedroll", "Bedroll",
                    CCS_CraftingStationType.Workbench, 3f, 2,
                    new[] { (fiberItem, 4), (stickItem, 2) },
                    new[] { (bedrollItem, 1) })
            };

            EnsureProgressionProfile(progressionRecipes);
            MergeInventoryCatalog(
                woodItem,
                stickItem,
                stoneItem,
                fiberItem,
                spearItem,
                bedrollItem,
                cookedRabbitItem,
                cookedVenisonItem,
                basicBandageItem,
                primitiveTorchItem,
                charcoalItem,
                ashItem,
                driedMeatItem,
                reinforcedSpearItem,
                storageCrateItem);
            EnsureBootstrapGameplayServiceHost();
            EnsureBootstrapWorkbench();
            EnsureCampfireCraftingStation();
            UpdateProjectVersion();

            AssetDatabase.SaveAssets();
            Debug.Log($"{LogPrefix} Crafting progression bootstrap setup complete.");
            EditorApplication.Exit(0);
        }

        #endregion

        #region Private Methods

        private static void EnsureFolders()
        {
            EnsureFolder("Assets/CCS/Survival/Content/Items");
            EnsureFolder(ProgressionItemsRoot);
            EnsureFolder(ProgressionRecipesRoot);
            EnsureFolder(ProfilesRoot);
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

        private static CCS_ItemDefinition LoadItem(string assetPath)
        {
            CCS_ItemDefinition item = AssetDatabase.LoadAssetAtPath<CCS_ItemDefinition>(assetPath);
            if (item == null)
            {
                Debug.LogWarning($"{LogPrefix} Could not load item at {assetPath}");
            }

            return item;
        }

        private static CCS_ItemDefinition EnsureProgressionItem(
            string assetName,
            string itemId,
            string displayName,
            CCS_ItemCategory category,
            string description)
        {
            string assetPath = $"{ProgressionItemsRoot}/{assetName}.asset";
            CCS_ItemDefinition item = AssetDatabase.LoadAssetAtPath<CCS_ItemDefinition>(assetPath);
            if (item == null)
            {
                item = ScriptableObject.CreateInstance<CCS_ItemDefinition>();
                AssetDatabase.CreateAsset(item, assetPath);
            }

            SerializedObject serialized = new SerializedObject(item);
            serialized.FindProperty("itemId").stringValue = itemId;
            serialized.FindProperty("displayName").stringValue = displayName;
            serialized.FindProperty("description").stringValue = description;
            serialized.FindProperty("category").enumValueIndex = (int)category;
            serialized.FindProperty("maxStackSize").intValue = category == CCS_ItemCategory.Consumable ? 20 : 99;
            serialized.FindProperty("isStackable").boolValue = category != CCS_ItemCategory.Tool;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(item);
            return item;
        }

        private static CCS_ItemDefinition EnsureReinforcedSpearItem(CCS_ItemDefinition spearReference)
        {
            string assetPath = $"{ProgressionItemsRoot}/CCS_Item_ReinforcedSpear.asset";
            CCS_ItemDefinition item = AssetDatabase.LoadAssetAtPath<CCS_ItemDefinition>(assetPath);
            if (item == null)
            {
                item = ScriptableObject.CreateInstance<CCS_ItemDefinition>();
                AssetDatabase.CreateAsset(item, assetPath);
            }

            float baseDamage = spearReference != null ? spearReference.MeleeDamage : 20f;
            float baseRange = spearReference != null ? spearReference.MeleeRange : 3f;

            SerializedObject serialized = new SerializedObject(item);
            serialized.FindProperty("itemId").stringValue = "ccs.survival.item.progression.reinforcedspear";
            serialized.FindProperty("displayName").stringValue = "Reinforced Spear";
            serialized.FindProperty("description").stringValue =
                "Workbench upgraded spear with higher melee damage.";
            serialized.FindProperty("category").enumValueIndex = (int)CCS_ItemCategory.Tool;
            serialized.FindProperty("maxStackSize").intValue = 1;
            serialized.FindProperty("isStackable").boolValue = false;
            serialized.FindProperty("gameplayKind").enumValueIndex = (int)CCS_ItemGameplayKind.Weapon;
            serialized.FindProperty("hasWeaponIdentity").boolValue = true;
            serialized.FindProperty("weaponArchetype").enumValueIndex = (int)CCS_WeaponArchetype.Spear;
            serialized.FindProperty("weaponType").enumValueIndex = (int)CCS_WeaponType.Melee;
            serialized.FindProperty("damageType").enumValueIndex = (int)CCS_DamageType.Pierce;
            serialized.FindProperty("rangeType").enumValueIndex = (int)CCS_RangeType.Melee;
            serialized.FindProperty("meleeDamage").floatValue = baseDamage + 15f;
            serialized.FindProperty("meleeRange").floatValue = baseRange + 0.5f;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(item);
            return item;
        }

        private static CCS_CraftingRecipeDefinition EnsureRecipe(
            string assetName,
            string recipeId,
            string displayName,
            CCS_CraftingStationType stationType,
            float craftTimeSeconds,
            int unlockTier,
            (CCS_ItemDefinition item, int quantity)[] ingredients,
            (CCS_ItemDefinition item, int quantity)[] results)
        {
            string assetPath = $"{ProgressionRecipesRoot}/{assetName}.asset";
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
                $"Milestone 1.1.1 progression recipe (tier {unlockTier}).";
            serializedRecipe.FindProperty("requiredStationType").enumValueIndex = (int)stationType;
            serializedRecipe.FindProperty("craftTimeSeconds").floatValue = craftTimeSeconds;
            serializedRecipe.FindProperty("isUnlockedByDefault").boolValue = true;

            SerializedProperty ingredientProperty = serializedRecipe.FindProperty("ingredients");
            ingredientProperty.arraySize = ingredients.Length;
            for (int index = 0; index < ingredients.Length; index++)
            {
                SerializedProperty entry = ingredientProperty.GetArrayElementAtIndex(index);
                entry.FindPropertyRelative("itemDefinition").objectReferenceValue = ingredients[index].item;
                entry.FindPropertyRelative("quantity").intValue = ingredients[index].quantity;
            }

            SerializedProperty resultProperty = serializedRecipe.FindProperty("results");
            resultProperty.arraySize = results.Length;
            for (int index = 0; index < results.Length; index++)
            {
                SerializedProperty entry = resultProperty.GetArrayElementAtIndex(index);
                entry.FindPropertyRelative("itemDefinition").objectReferenceValue = results[index].item;
                entry.FindPropertyRelative("quantity").intValue = results[index].quantity;
            }

            serializedRecipe.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(recipe);
            return recipe;
        }

        private static void EnsureProgressionProfile(List<CCS_CraftingRecipeDefinition> recipes)
        {
            CCS_CraftingProgressionProfile profile =
                AssetDatabase.LoadAssetAtPath<CCS_CraftingProgressionProfile>(ProgressionProfilePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<CCS_CraftingProgressionProfile>();
                AssetDatabase.CreateAsset(profile, ProgressionProfilePath);
            }

            SerializedObject serialized = new SerializedObject(profile);
            serialized.FindProperty("profileDisplayName").stringValue = "Default Crafting Progression";
            serialized.FindProperty("profileId").stringValue = "ccs.survival.profile.crafting.progression.default";
            serialized.FindProperty("profileDescription").stringValue =
                "Primitive hand, campfire, and workbench crafting progression for milestone 1.1.1.";
            serialized.FindProperty("profileVersion").stringValue = "1.1.1";
            serialized.FindProperty("progressionEnabled").boolValue = true;
            serialized.FindProperty("enableDebugLogging").boolValue = true;
            serialized.FindProperty("workbenchPlaytestRecipeId").stringValue =
                "ccs.survival.recipe.progression.storagecrate";

            SerializedProperty recipeList = serialized.FindProperty("progressionRecipes");
            recipeList.ClearArray();
            for (int index = 0; index < recipes.Count; index++)
            {
                recipeList.InsertArrayElementAtIndex(index);
                SerializedProperty entry = recipeList.GetArrayElementAtIndex(index);
                entry.FindPropertyRelative("recipeDefinition").objectReferenceValue = recipes[index];
                entry.FindPropertyRelative("unlockTier").intValue = GetUnlockTier(recipes[index]);
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
        }

        private static int GetUnlockTier(CCS_CraftingRecipeDefinition recipe)
        {
            if (recipe == null)
            {
                return 1;
            }

            switch (recipe.RequiredStationType)
            {
                case CCS_CraftingStationType.Workbench:
                    return 2;
                case CCS_CraftingStationType.FirePit:
                    return 1;
                default:
                    return 1;
            }
        }

        private static void EnsureBootstrapGameplayServiceHost()
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
            serializedHost.FindProperty("craftingProgressionProfile").objectReferenceValue =
                AssetDatabase.LoadAssetAtPath<CCS_CraftingProgressionProfile>(ProgressionProfilePath);
            serializedHost.ApplyModifiedPropertiesWithoutUndo();
            PrefabUtility.SaveAsPrefabAsset(prefabContents, prefabPath);
            PrefabUtility.UnloadPrefabContents(prefabContents);
        }

        private static void EnsureBootstrapWorkbench()
        {
            Scene scene = EditorSceneManager.OpenScene(BootstrapScenePath, OpenSceneMode.Single);
            Transform sceneRoot = FindSceneRoot();
            if (sceneRoot == null)
            {
                Debug.LogError($"{LogPrefix} Could not find CCS_BuildVerificationScene root.");
                EditorApplication.Exit(1);
                return;
            }

            Transform craftingArea = sceneRoot.Find("CCS_CraftingTestArea");
            Transform parent = craftingArea != null ? craftingArea : sceneRoot;

            Transform existing = parent.Find(WorkbenchObjectName);
            GameObject workbenchObject;
            if (existing != null)
            {
                workbenchObject = existing.gameObject;
            }
            else
            {
                workbenchObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                workbenchObject.name = WorkbenchObjectName;
                workbenchObject.transform.SetParent(parent, false);
                workbenchObject.transform.localPosition = new Vector3(6f, 0.5f, 4f);
                workbenchObject.transform.localScale = new Vector3(1.5f, 1f, 1f);
            }

            CCS_CraftingStationInteractable stationInteractable =
                workbenchObject.GetComponent<CCS_CraftingStationInteractable>();
            if (stationInteractable == null)
            {
                stationInteractable = workbenchObject.AddComponent<CCS_CraftingStationInteractable>();
            }

            stationInteractable.ConfigureRuntime(
                CCS_CraftingStationType.Workbench,
                "ccs.survival.station.test.workbench",
                "Test Workbench");

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static void EnsureCampfireCraftingStation()
        {
            Scene scene = EditorSceneManager.GetActiveScene();
            GameObject campfire = GameObject.Find("CCS_TestCampfire");
            if (campfire == null)
            {
                return;
            }

            CCS_CraftingStationInteractable stationInteractable =
                campfire.GetComponent<CCS_CraftingStationInteractable>();
            if (stationInteractable == null)
            {
                stationInteractable = campfire.AddComponent<CCS_CraftingStationInteractable>();
            }

            stationInteractable.ConfigureRuntime(
                CCS_CraftingStationType.FirePit,
                "ccs.survival.station.test.campfire",
                "Campfire");

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

        private static void MergeInventoryCatalog(params CCS_ItemDefinition[] itemDefinitions)
        {
            CCS_InventoryProfile inventoryProfile =
                AssetDatabase.LoadAssetAtPath<CCS_InventoryProfile>(InventoryProfilePath);
            if (inventoryProfile == null)
            {
                return;
            }

            SerializedObject serializedProfile = new SerializedObject(inventoryProfile);
            SerializedProperty catalog = serializedProfile.FindProperty("saveRestoreItemDefinitions");
            for (int index = 0; index < itemDefinitions.Length; index++)
            {
                AddCatalogEntryIfMissing(catalog, itemDefinitions[index]);
            }

            serializedProfile.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(inventoryProfile);
        }

        private static void AddCatalogEntryIfMissing(SerializedProperty catalog, CCS_ItemDefinition itemDefinition)
        {
            if (itemDefinition == null)
            {
                return;
            }

            for (int index = 0; index < catalog.arraySize; index++)
            {
                if (catalog.GetArrayElementAtIndex(index).objectReferenceValue == itemDefinition)
                {
                    return;
                }
            }

            int newIndex = catalog.arraySize;
            catalog.InsertArrayElementAtIndex(newIndex);
            catalog.GetArrayElementAtIndex(newIndex).objectReferenceValue = itemDefinition;
        }

        private static void UpdateProjectVersion()
        {
            CCS_SurvivalBootstrapVersionUtility.EnsureBundleVersionAtLeast(
                CCS_SurvivalBootstrapVersionUtility.CurrentMilestoneVersion);
        }

        #endregion
    }
}
