using System.IO;
using CCS.Modules.Crafting;
using CCS.Modules.Economy;
using CCS.Modules.Inventory;
using CCS.Modules.Playtesting;
using CCS.Modules.Shelter;
using CCS.Survival.Composition;
using CCS.Survival.Editor.Development;
using UnityEditor;
using UnityEngine;

namespace CCS.Modules.Shelter.Editor
{
    public static class CCS_FrontierShelterBootstrapSetup
    {
        private const string LogPrefix = "[CCS_FrontierShelterBootstrapSetup]";
        private const string FrontierStructuresRoot = "Assets/CCS/Survival/Content/Structures/Frontier";
        private const string FrontierItemsRoot = "Assets/CCS/Survival/Content/Items/Frontier";
        private const string CampProfilePath = "Assets/CCS/Survival/Profiles/Camp/CCS_DefaultCampDefinition.asset";
        private const string FrontierRecipesRoot = "Assets/CCS/Survival/Profiles/Crafting/FrontierPrimitiveRecipes";
        private const string CraftingProgressionPath =
            "Assets/CCS/Survival/Profiles/Crafting/CCS_DefaultCraftingProgressionProfile.asset";
        private const string GeneralStoreVendorPath = "Assets/CCS/Survival/Content/Vendors/CCS_Vendor_GeneralStore.asset";
        private const string BootstrapRootPrefabPath = "Assets/CCS/Survival/Prefabs/PF_CCS_Survival_BootstrapRoot.prefab";
        private const string DefaultPlaytestProfilePath = "Assets/CCS/Survival/Profiles/Playtesting/CCS_DefaultPlaytestProfile.asset";
        private const string CookingCurrencyStepId = "ccs.survival.playtest.cooking.currency.verify";

        private const string WoodItemId = "ccs.survival.item.resource.wood";
        private const string SaplingItemId = "ccs.survival.item.resource.sapling";
        private const string CordageItemId = "ccs.survival.item.frontier.cordage";
        private const string CanvasItemId = "ccs.survival.item.resource.canvas";
        private const string HideItemId = "ccs.survival.item.resource.hide";
        private const string BoneHatchetItemId = "ccs.survival.item.tool.hatchet.bone";

        public static void ExecuteBatch()
        {
            UpdateProjectVersion();
            EnsureFolders();

            CCS_ItemDefinition wood = LoadItem("CCS_Item_Wood", WoodItemId);
            CCS_ItemDefinition sapling = LoadItem("CCS_Item_Sapling", SaplingItemId);
            CCS_ItemDefinition cordage = LoadItem("CCS_Item_Cordage", CordageItemId);
            CCS_ItemDefinition canvas = LoadItem("CCS_Item_Canvas", CanvasItemId);
            CCS_ItemDefinition hide = LoadItem("CCS_Item_Hide", HideItemId);
            CCS_ItemDefinition largeWood = EnsureLargeWoodItem();

            CCS_ItemDefinition leanToKit = EnsureShelterKitItem(
                "CCS_Item_LeanToKit",
                "ccs.survival.item.frontier.leantokit",
                "Lean-To Kit");
            CCS_ItemDefinition tarpKit = EnsureShelterKitItem(
                "CCS_Item_TarpShelterKit",
                "ccs.survival.item.frontier.tarpshelterkit",
                "Primitive Tarp Shelter Kit");
            CCS_ItemDefinition trapperKit = EnsureShelterKitItem(
                "CCS_Item_TrapperShelterKit",
                "ccs.survival.item.frontier.trappershelterkit",
                "Trapper Shelter Kit");
            CCS_ItemDefinition logCabinKit = EnsureShelterKitItem(
                "CCS_Item_LogCabinFoundationKit",
                "ccs.survival.item.frontier.logcabinfoundationkit",
                "Log Cabin Foundation Kit");

            CCS_ShelterDefinition leanTo = EnsureShelterDefinition(
                "CCS_ShelterDefinition_LeanTo",
                "ccs.survival.shelter.frontier.leanto",
                "Lean-To",
                1,
                true,
                leanToKit,
                new (string, int)[]
                {
                    (WoodItemId, 3),
                    (SaplingItemId, 2),
                    (CordageItemId, 2)
                });
            CCS_ShelterDefinition tarp = EnsureShelterDefinition(
                "CCS_ShelterDefinition_Tarp",
                "ccs.survival.shelter.frontier.tarp",
                "Primitive Tarp Shelter",
                1,
                true,
                tarpKit,
                new (string, int)[]
                {
                    (WoodItemId, 2),
                    (CordageItemId, 2),
                    (CanvasItemId, 1)
                });
            CCS_ShelterDefinition trapper = EnsureShelterDefinition(
                "CCS_ShelterDefinition_TrapperShelter",
                "ccs.survival.shelter.frontier.trapper",
                "Trapper Shelter",
                1,
                true,
                trapperKit,
                new (string, int)[]
                {
                    (WoodItemId, 3),
                    (HideItemId, 2),
                    (CordageItemId, 2)
                });
            CCS_ShelterDefinition shack = EnsureShelterDefinition(
                "CCS_ShelterDefinition_SmallFrontierShack",
                "ccs.survival.shelter.frontier.shack",
                "Small Frontier Shack",
                2,
                false,
                null,
                System.Array.Empty<(string, int)>());
            CCS_ShelterDefinition logCabin = EnsureShelterDefinition(
                "CCS_ShelterDefinition_LogCabinFoundation",
                "ccs.survival.shelter.frontier.logcabinfoundation",
                "Log Cabin Foundation",
                2,
                false,
                logCabinKit,
                new (string, int)[] { ("ccs.survival.item.resource.largewood", 8) });

            CCS_CampDefinition campDefinition = EnsureCampDefinition(leanTo, tarp, trapper, shack, logCabin);
            AssignCampDefinitionToBootstrapHost(campDefinition);

            CCS_CraftingRecipeDefinition leanToRecipe = EnsureRecipe(
                "CCS_FrontierLeanToRecipe",
                "ccs.survival.recipe.frontier.leanto",
                "Craft Lean-To Kit",
                new[] { (wood, 3), (sapling, 2), (cordage, 2) },
                new[] { (leanToKit, 1) });
            CCS_CraftingRecipeDefinition tarpRecipe = EnsureRecipe(
                "CCS_FrontierTarpShelterRecipe",
                "ccs.survival.recipe.frontier.tarpshelter",
                "Craft Tarp Shelter Kit",
                new[] { (wood, 2), (cordage, 2), (canvas, 1) },
                new[] { (tarpKit, 1) });
            CCS_CraftingRecipeDefinition trapperRecipe = EnsureRecipe(
                "CCS_FrontierTrapperShelterRecipe",
                "ccs.survival.recipe.frontier.trappershelter",
                "Craft Trapper Shelter Kit",
                new[] { (wood, 3), (hide, 2), (cordage, 2) },
                new[] { (trapperKit, 1) });
            CCS_CraftingRecipeDefinition logCabinRecipe = EnsureRecipe(
                "CCS_FrontierLogCabinFoundationRecipe",
                "ccs.survival.recipe.frontier.logcabinfoundation",
                "Craft Log Cabin Foundation Kit",
                new[] { (largeWood, 8) },
                new[] { (logCabinKit, 1) });

            MergeRecipesIntoProgression(leanToRecipe, tarpRecipe, trapperRecipe, logCabinRecipe);
            EnsureGeneralStoreShelterCatalog(cordage, canvas, hide, wood, sapling);
            EnsurePlaytestShelterSteps();
            EnsureBuildShelterPlaytestStep();
            BumpProfileVersions();

            AssetDatabase.SaveAssets();
            Debug.Log($"{LogPrefix} Frontier shelter bootstrap setup complete (1.4.0).");
            EditorApplication.Exit(0);
        }

        private static void UpdateProjectVersion()
        {
            CCS_SurvivalBootstrapVersionUtility.EnsureBundleVersionAtLeast(
                CCS_SurvivalBootstrapVersionUtility.CurrentMilestoneVersion);
        }

        private static void EnsureFolders()
        {
            EnsureFolder("Assets/CCS/Survival/Profiles/Camp");
            EnsureFolder(FrontierStructuresRoot);
            EnsureFolder(FrontierItemsRoot);
            EnsureFolder(FrontierRecipesRoot);
        }

        private static CCS_ItemDefinition EnsureLargeWoodItem()
        {
            string path = "Assets/CCS/Survival/Content/Items/Resources/Frontier/CCS_Item_LargeWood.asset";
            CCS_ItemDefinition item = AssetDatabase.LoadAssetAtPath<CCS_ItemDefinition>(path);
            if (item == null)
            {
                item = ScriptableObject.CreateInstance<CCS_ItemDefinition>();
                AssetDatabase.CreateAsset(item, path);
            }

            SerializedObject serialized = new SerializedObject(item);
            serialized.FindProperty("itemId").stringValue = "ccs.survival.item.resource.largewood";
            serialized.FindProperty("displayName").stringValue = "Large Wood";
            serialized.FindProperty("description").stringValue =
                "Heavy timber placeholder for future log-cabin construction.";
            serialized.FindProperty("category").enumValueIndex = (int)CCS_ItemCategory.Material;
            serialized.FindProperty("gameplayKind").enumValueIndex = (int)CCS_ItemGameplayKind.Generic;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(item);
            return item;
        }

        private static CCS_ItemDefinition EnsureShelterKitItem(string assetName, string itemId, string displayName)
        {
            string path = $"{FrontierItemsRoot}/{assetName}.asset";
            CCS_ItemDefinition item = AssetDatabase.LoadAssetAtPath<CCS_ItemDefinition>(path);
            if (item == null)
            {
                item = ScriptableObject.CreateInstance<CCS_ItemDefinition>();
                AssetDatabase.CreateAsset(item, path);
            }

            SerializedObject serialized = new SerializedObject(item);
            serialized.FindProperty("itemId").stringValue = itemId;
            serialized.FindProperty("displayName").stringValue = displayName;
            serialized.FindProperty("description").stringValue =
                "Frontier shelter placement kit. Use to preview and place shelter on level ground.";
            serialized.FindProperty("category").enumValueIndex = (int)CCS_ItemCategory.Generic;
            serialized.FindProperty("gameplayKind").enumValueIndex = (int)CCS_ItemGameplayKind.Placeable;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(item);
            return item;
        }

        private static CCS_ShelterDefinition EnsureShelterDefinition(
            string assetName,
            string shelterId,
            string displayName,
            int tier,
            bool isFunctional,
            CCS_ItemDefinition kitItem,
            (string itemId, int quantity)[] costs)
        {
            string path = $"{FrontierStructuresRoot}/{assetName}.asset";
            CCS_ShelterDefinition definition = AssetDatabase.LoadAssetAtPath<CCS_ShelterDefinition>(path);
            if (definition == null)
            {
                definition = ScriptableObject.CreateInstance<CCS_ShelterDefinition>();
                AssetDatabase.CreateAsset(definition, path);
            }

            SerializedObject serialized = new SerializedObject(definition);
            serialized.FindProperty("shelterDefinitionId").stringValue = shelterId;
            serialized.FindProperty("displayName").stringValue = displayName;
            serialized.FindProperty("shelterTier").intValue = tier;
            serialized.FindProperty("isFunctional").boolValue = isFunctional;
            serialized.FindProperty("grantedCampTier").enumValueIndex = (int)CCS_CampTier.TemporaryCamp;
            serialized.FindProperty("placeableKitItem").objectReferenceValue = kitItem;
            serialized.FindProperty("warmthBonus").floatValue = 0.35f;
            serialized.FindProperty("sleepBonus").floatValue = 0.25f;
            serialized.FindProperty("weatherProtectionPercent").floatValue = 0.55f;
            serialized.FindProperty("enablesCampOwnershipMarker").boolValue = true;
            serialized.FindProperty("supportsFutureRespawnHook").boolValue = true;

            SerializedProperty craftCosts = serialized.FindProperty("craftCosts");
            craftCosts.arraySize = costs.Length;
            for (int index = 0; index < costs.Length; index++)
            {
                SerializedProperty entry = craftCosts.GetArrayElementAtIndex(index);
                entry.FindPropertyRelative("itemDefinitionId").stringValue = costs[index].itemId;
                entry.FindPropertyRelative("quantity").intValue = costs[index].quantity;
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(definition);
            return definition;
        }

        private static CCS_CampDefinition EnsureCampDefinition(params CCS_ShelterDefinition[] shelters)
        {
            CCS_CampDefinition definition = AssetDatabase.LoadAssetAtPath<CCS_CampDefinition>(CampProfilePath);
            if (definition == null)
            {
                definition = ScriptableObject.CreateInstance<CCS_CampDefinition>();
                AssetDatabase.CreateAsset(definition, CampProfilePath);
            }

            SerializedObject serialized = new SerializedObject(definition);
            serialized.FindProperty("profileVersion").stringValue = "1.4.0";
            serialized.FindProperty("profileDescription").stringValue =
                "Frontier camp tracking for shelter, campfire, and bedroll (1.4.0).";
            serialized.FindProperty("enableCampTracking").boolValue = true;
            serialized.FindProperty("campDetectionRadius").floatValue = 12f;
            serialized.FindProperty("campfirePieceId").stringValue = "ccs.survival.building.campfire.test";
            SerializedProperty shelterList = serialized.FindProperty("shelterDefinitions");
            shelterList.arraySize = shelters.Length;
            for (int index = 0; index < shelters.Length; index++)
            {
                shelterList.GetArrayElementAtIndex(index).objectReferenceValue = shelters[index];
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(definition);
            return definition;
        }

        private static void AssignCampDefinitionToBootstrapHost(CCS_CampDefinition campDefinition)
        {
            GameObject prefabRoot = AssetDatabase.LoadAssetAtPath<GameObject>(BootstrapRootPrefabPath);
            if (prefabRoot == null)
            {
                return;
            }

            CCS_SurvivalGameplayServiceHost host = prefabRoot.GetComponent<CCS_SurvivalGameplayServiceHost>();
            if (host == null)
            {
                return;
            }

            SerializedObject serialized = new SerializedObject(host);
            serialized.FindProperty("campDefinition").objectReferenceValue = campDefinition;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(host);
            EditorUtility.SetDirty(prefabRoot);
        }

        private static CCS_CraftingRecipeDefinition EnsureRecipe(
            string assetName,
            string recipeId,
            string displayName,
            (CCS_ItemDefinition item, int quantity)[] ingredients,
            (CCS_ItemDefinition item, int quantity)[] results)
        {
            string path = $"{FrontierRecipesRoot}/{assetName}.asset";
            CCS_CraftingRecipeDefinition recipe = AssetDatabase.LoadAssetAtPath<CCS_CraftingRecipeDefinition>(path);
            if (recipe == null)
            {
                recipe = ScriptableObject.CreateInstance<CCS_CraftingRecipeDefinition>();
                AssetDatabase.CreateAsset(recipe, path);
            }

            SerializedObject serialized = new SerializedObject(recipe);
            serialized.FindProperty("recipeId").stringValue = recipeId;
            serialized.FindProperty("displayName").stringValue = displayName;
            serialized.FindProperty("description").stringValue = "Frontier shelter hand recipe (1.4.0).";
            serialized.FindProperty("requiredStationType").enumValueIndex = (int)CCS_CraftingStationType.Hand;
            serialized.FindProperty("craftTimeSeconds").floatValue = 0f;
            serialized.FindProperty("isUnlockedByDefault").boolValue = true;

            SerializedProperty ingredientList = serialized.FindProperty("ingredients");
            ingredientList.arraySize = ingredients.Length;
            for (int index = 0; index < ingredients.Length; index++)
            {
                SerializedProperty entry = ingredientList.GetArrayElementAtIndex(index);
                entry.FindPropertyRelative("itemDefinition").objectReferenceValue = ingredients[index].item;
                entry.FindPropertyRelative("quantity").intValue = ingredients[index].quantity;
            }

            SerializedProperty resultList = serialized.FindProperty("results");
            resultList.arraySize = results.Length;
            for (int index = 0; index < results.Length; index++)
            {
                SerializedProperty entry = resultList.GetArrayElementAtIndex(index);
                entry.FindPropertyRelative("itemDefinition").objectReferenceValue = results[index].item;
                entry.FindPropertyRelative("quantity").intValue = results[index].quantity;
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(recipe);
            return recipe;
        }

        private static void MergeRecipesIntoProgression(params CCS_CraftingRecipeDefinition[] recipes)
        {
            CCS_CraftingProgressionProfile profile =
                AssetDatabase.LoadAssetAtPath<CCS_CraftingProgressionProfile>(CraftingProgressionPath);
            if (profile == null)
            {
                return;
            }

            SerializedObject serialized = new SerializedObject(profile);
            SerializedProperty progressionRecipes = serialized.FindProperty("progressionRecipes");
            for (int recipeIndex = 0; recipeIndex < recipes.Length; recipeIndex++)
            {
                CCS_CraftingRecipeDefinition recipe = recipes[recipeIndex];
                if (recipe == null)
                {
                    continue;
                }

                bool exists = false;
                for (int index = 0; index < progressionRecipes.arraySize; index++)
                {
                    SerializedProperty entry = progressionRecipes.GetArrayElementAtIndex(index);
                    if (entry.FindPropertyRelative("recipeDefinition").objectReferenceValue == recipe)
                    {
                        exists = true;
                        break;
                    }
                }

                if (exists)
                {
                    continue;
                }

                int newIndex = progressionRecipes.arraySize;
                progressionRecipes.InsertArrayElementAtIndex(newIndex);
                SerializedProperty newEntry = progressionRecipes.GetArrayElementAtIndex(newIndex);
                newEntry.FindPropertyRelative("recipeDefinition").objectReferenceValue = recipe;
                newEntry.FindPropertyRelative("unlockTier").intValue = 1;
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
        }

        private static void EnsureGeneralStoreShelterCatalog(
            CCS_ItemDefinition cordage,
            CCS_ItemDefinition canvas,
            CCS_ItemDefinition hide,
            CCS_ItemDefinition wood,
            CCS_ItemDefinition sapling)
        {
            CCS_VendorDefinition vendor = AssetDatabase.LoadAssetAtPath<CCS_VendorDefinition>(GeneralStoreVendorPath);
            if (vendor == null)
            {
                return;
            }

            SerializedObject serialized = new SerializedObject(vendor);
            serialized.FindProperty("description").stringValue =
                "Frontier general store (1.4.0). Sells cordage and canvas; buys hide and shelter materials.";
            SerializedProperty items = serialized.FindProperty("vendorInventory").FindPropertyRelative("items");
            MergeVendorRow(items, cordage, allowBuy: true, allowSell: false);
            MergeVendorRow(items, canvas, allowBuy: true, allowSell: false);
            MergeVendorRow(items, hide, allowBuy: false, allowSell: true);
            MergeVendorRow(items, wood, allowBuy: false, allowSell: true);
            MergeVendorRow(items, sapling, allowBuy: false, allowSell: true);
            NormalizeVendorBuyPrices(items);
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(vendor);
        }

        private static void MergeVendorRow(
            SerializedProperty items,
            CCS_ItemDefinition item,
            bool allowBuy,
            bool allowSell)
        {
            if (item == null)
            {
                return;
            }

            for (int index = 0; index < items.arraySize; index++)
            {
                SerializedProperty entry = items.GetArrayElementAtIndex(index);
                if (entry.FindPropertyRelative("itemDefinition").objectReferenceValue != item)
                {
                    continue;
                }

                entry.FindPropertyRelative("allowBuy").boolValue = allowBuy;
                entry.FindPropertyRelative("allowSell").boolValue = allowSell;
                if (allowBuy)
                {
                    SerializedProperty buyPrice = entry.FindPropertyRelative("buyPriceOverride");
                    if (buyPrice.intValue < 1)
                    {
                        buyPrice.intValue = 8;
                    }
                }

                if (allowSell)
                {
                    SerializedProperty sellPrice = entry.FindPropertyRelative("sellPriceOverride");
                    if (sellPrice.intValue < 1)
                    {
                        sellPrice.intValue = 4;
                    }
                }

                return;
            }

            int newIndex = items.arraySize;
            items.InsertArrayElementAtIndex(newIndex);
            SerializedProperty newEntry = items.GetArrayElementAtIndex(newIndex);
            newEntry.FindPropertyRelative("itemDefinition").objectReferenceValue = item;
            newEntry.FindPropertyRelative("stockQuantity").intValue = -1;
            newEntry.FindPropertyRelative("allowBuy").boolValue = allowBuy;
            newEntry.FindPropertyRelative("allowSell").boolValue = allowSell;
            newEntry.FindPropertyRelative("buyPriceOverride").intValue = allowBuy ? 8 : -1;
            newEntry.FindPropertyRelative("sellPriceOverride").intValue = allowSell ? 4 : -1;
        }

        private static void NormalizeVendorBuyPrices(SerializedProperty items)
        {
            for (int index = 0; index < items.arraySize; index++)
            {
                SerializedProperty entry = items.GetArrayElementAtIndex(index);
                if (!entry.FindPropertyRelative("allowBuy").boolValue)
                {
                    continue;
                }

                SerializedProperty buyPrice = entry.FindPropertyRelative("buyPriceOverride");
                if (buyPrice.intValue < 1)
                {
                    buyPrice.intValue = 8;
                }
            }
        }

        private static void EnsurePlaytestShelterSteps()
        {
            CCS_PlaytestProfile profile = AssetDatabase.LoadAssetAtPath<CCS_PlaytestProfile>(DefaultPlaytestProfilePath);
            if (profile == null)
            {
                return;
            }

            SerializedObject serializedProfile = new SerializedObject(profile);
            SerializedProperty stepList = serializedProfile.FindProperty("stepDefinitions");
            RemoveShelterSteps(stepList);

            int insertIndex = FindStepIndex(stepList, CookingCurrencyStepId);
            if (insertIndex < 0)
            {
                insertIndex = stepList.arraySize;
            }
            else
            {
                insertIndex += 1;
            }

            InsertStep(stepList, ref insertIndex, "ccs.survival.playtest.shelter.hatchet", "Buy hatchet", CCS_PlaytestStepType.BuyHatchetForShelter, "Press Ctrl+Shift+H to grant bone hatchet.", BoneHatchetItemId);
            InsertStep(stepList, ref insertIndex, "ccs.survival.playtest.shelter.wood", "Gather wood", CCS_PlaytestStepType.GatherWoodForShelter, "Chop test tree with hatchet.", WoodItemId);
            InsertStep(stepList, ref insertIndex, "ccs.survival.playtest.shelter.cordage", "Acquire cordage", CCS_PlaytestStepType.AcquireCordageForShelter, "Press Ctrl+Shift+C to grant cordage.", CordageItemId);
            InsertStep(stepList, ref insertIndex, "ccs.survival.playtest.shelter.craft.leanto", "Craft Lean-To kit", CCS_PlaytestStepType.CraftLeanToShelter, "Hand-craft Lean-To kit from wood, sapling, and cordage.", "ccs.survival.item.frontier.leantokit");
            InsertStep(stepList, ref insertIndex, "ccs.survival.playtest.shelter.place.leanto", "Place Lean-To", CCS_PlaytestStepType.PlaceLeanToShelter, "Equip Lean-To kit and use twice to preview and place.", "ccs.survival.item.frontier.leantokit");
            InsertStep(stepList, ref insertIndex, "ccs.survival.playtest.shelter.place.campfire", "Place campfire", CCS_PlaytestStepType.PlaceCampfireForCamp, "Place test campfire within camp radius.");
            InsertStep(stepList, ref insertIndex, "ccs.survival.playtest.shelter.place.bedroll", "Place bedroll", CCS_PlaytestStepType.PlaceBedrollForCamp, "Place bedroll within camp radius.");
            InsertStep(stepList, ref insertIndex, "ccs.survival.playtest.shelter.verify.camp", "Verify TemporaryCamp", CCS_PlaytestStepType.VerifyTemporaryCampTier, "Camp tier should be TemporaryCamp when all three are nearby.");
            InsertStep(stepList, ref insertIndex, "ccs.survival.playtest.shelter.sleep", "Sleep in camp", CCS_PlaytestStepType.SleepInFrontierCamp, "Sleep at bedroll inside camp.");
            InsertStep(stepList, ref insertIndex, "ccs.survival.playtest.shelter.verify.save", "Verify camp save/load", CCS_PlaytestStepType.VerifyCampPersistenceAfterLoad, "Save and load; camp tier and shelter should restore.");

            serializedProfile.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
        }

        private static void BumpProfileVersions()
        {
            string economyPath = "Assets/CCS/Survival/Profiles/Economy/CCS_DefaultEconomyProfile.asset";
            string vendorPath = "Assets/CCS/Survival/Profiles/Economy/CCS_DefaultVendorProfile.asset";
            BumpVersion(economyPath, "1.4.0");
            BumpVersion(vendorPath, "1.4.0");

            CCS_PlaytestProfile playtest = AssetDatabase.LoadAssetAtPath<CCS_PlaytestProfile>(DefaultPlaytestProfilePath);
            if (playtest != null)
            {
                SerializedObject serialized = new SerializedObject(playtest);
                serialized.FindProperty("profileVersion").stringValue = "1.4.0";
                serialized.FindProperty("profileDescription").stringValue =
                    "Frontier starter progression with shelter camp loop playtest checklist (1.4.0).";
                serialized.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(playtest);
            }
        }

        private static void BumpVersion(string path, string version)
        {
            ScriptableObject asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
            if (asset == null)
            {
                return;
            }

            SerializedObject serialized = new SerializedObject(asset);
            serialized.FindProperty("profileVersion").stringValue = version;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(asset);
        }

        private static CCS_ItemDefinition LoadItem(string assetName, string expectedItemId)
        {
            string[] searchRoots =
            {
                "Assets/CCS/Survival/Content/Items/Resources/Primitive",
                "Assets/CCS/Survival/Content/Items/Resources/Frontier",
                FrontierItemsRoot
            };

            for (int index = 0; index < searchRoots.Length; index++)
            {
                string path = $"{searchRoots[index]}/{assetName}.asset";
                CCS_ItemDefinition item = AssetDatabase.LoadAssetAtPath<CCS_ItemDefinition>(path);
                if (item != null)
                {
                    return item;
                }
            }

            Debug.LogError($"{LogPrefix} Missing item {assetName} ({expectedItemId}).");
            EditorApplication.Exit(1);
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

        private static void EnsureBuildShelterPlaytestStep()
        {
            CCS_PlaytestProfile profile = AssetDatabase.LoadAssetAtPath<CCS_PlaytestProfile>(DefaultPlaytestProfilePath);
            if (profile == null)
            {
                return;
            }

            SerializedObject serializedProfile = new SerializedObject(profile);
            SerializedProperty stepList = serializedProfile.FindProperty("stepDefinitions");
            if (HasStepType(stepList, CCS_PlaytestStepType.BuildShelter))
            {
                return;
            }

            int insertIndex = FindStepIndexByType(stepList, CCS_PlaytestStepType.EatFood);
            if (insertIndex < 0)
            {
                insertIndex = stepList.arraySize;
            }
            else
            {
                insertIndex += 1;
            }

            InsertStep(
                stepList,
                ref insertIndex,
                "ccs.survival.playtest.build.shelter",
                "Build shelter",
                CCS_PlaytestStepType.BuildShelter,
                "Place foundation, wall, and roof pieces to reach minimum shelter coverage.");
            serializedProfile.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
        }

        private static bool HasStepType(SerializedProperty stepList, CCS_PlaytestStepType stepType)
        {
            for (int index = 0; index < stepList.arraySize; index++)
            {
                if (stepList.GetArrayElementAtIndex(index).FindPropertyRelative("stepType").enumValueIndex == (int)stepType)
                {
                    return true;
                }
            }

            return false;
        }

        private static int FindStepIndexByType(SerializedProperty stepList, CCS_PlaytestStepType stepType)
        {
            for (int index = 0; index < stepList.arraySize; index++)
            {
                if (stepList.GetArrayElementAtIndex(index).FindPropertyRelative("stepType").enumValueIndex == (int)stepType)
                {
                    return index;
                }
            }

            return -1;
        }

        private static void RemoveShelterSteps(SerializedProperty stepList)
        {
            for (int index = stepList.arraySize - 1; index >= 0; index--)
            {
                SerializedProperty step = stepList.GetArrayElementAtIndex(index);
                string stepId = step.FindPropertyRelative("stepId").stringValue;
                if (stepId != null && stepId.Contains("ccs.survival.playtest.shelter"))
                {
                    stepList.DeleteArrayElementAtIndex(index);
                }
            }
        }

        private static int FindStepIndex(SerializedProperty stepList, string stepId)
        {
            for (int index = 0; index < stepList.arraySize; index++)
            {
                if (stepList.GetArrayElementAtIndex(index).FindPropertyRelative("stepId").stringValue == stepId)
                {
                    return index;
                }
            }

            return -1;
        }

        private static void InsertStep(
            SerializedProperty stepList,
            ref int insertIndex,
            string stepId,
            string displayName,
            CCS_PlaytestStepType stepType,
            string instructionText,
            string targetItemId = "")
        {
            stepList.InsertArrayElementAtIndex(insertIndex);
            SerializedProperty step = stepList.GetArrayElementAtIndex(insertIndex);
            step.FindPropertyRelative("stepId").stringValue = stepId;
            step.FindPropertyRelative("displayName").stringValue = displayName;
            step.FindPropertyRelative("stepType").enumValueIndex = (int)stepType;
            step.FindPropertyRelative("instructionText").stringValue = instructionText;
            step.FindPropertyRelative("targetItemId").stringValue = targetItemId ?? string.Empty;
            step.FindPropertyRelative("targetObjectId").stringValue = string.Empty;
            step.FindPropertyRelative("requiredCount").intValue = 1;
            insertIndex += 1;
        }
    }
}
