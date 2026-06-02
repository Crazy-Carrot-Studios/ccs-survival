using System.IO;
using CCS.Modules.Cooking;
using CCS.Modules.Economy;
using CCS.Modules.Inventory;
using CCS.Modules.Playtesting;
using CCS.Modules.Wildlife;
using CCS.Survival.Editor.Development;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_FrontierCookingBootstrapSetup
// CATEGORY: Modules / Cooking / Editor / Validation
// PURPOSE: Batch setup for milestone 1.3.4 frontier cooking and food preservation.
// PLACEMENT: ExecuteBatch entry for cooking expansion bootstrap.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: Smoke-only preservation recipes until salt is implemented.
// =============================================================================

namespace CCS.Modules.Cooking.Editor
{
    public static class CCS_FrontierCookingBootstrapSetup
    {
        private const string LogPrefix = "[CCS_FrontierCookingBootstrapSetup]";
        private const string DefaultCookingProfilePath = "Assets/CCS/Survival/Profiles/Cooking/CCS_DefaultCookingProfile.asset";
        private const string DefaultPlaytestProfilePath = "Assets/CCS/Survival/Profiles/Playtesting/CCS_DefaultPlaytestProfile.asset";
        private const string GeneralStoreVendorPath = "Assets/CCS/Survival/Content/Vendors/CCS_Vendor_GeneralStore.asset";
        private const string FoodItemsRoot = "Assets/CCS/Survival/Content/Items/Food";
        private const string WildlifeItemsRoot = "Assets/CCS/Survival/Content/Items/Resources/Wildlife";
        private const string FishingItemsRoot = "Assets/CCS/Survival/Content/Items/Fishing";
        private const string TurkeyHarvestPath = "Assets/CCS/Survival/Content/Wildlife/Harvest/CCS_HarvestDefinition_Turkey.asset";
        private const string TrappingCurrencyStepId = "ccs.survival.playtest.trapping.currency.verify";

        private const string RawFishItemId = "ccs.survival.item.resource.rawfish";
        private const string SmallFishItemId = "ccs.survival.item.resource.smallfish";
        private const string RawMeatItemId = "ccs.survival.item.resource.rawmeat";
        private const string RawRabbitMeatItemId = "ccs.survival.item.resource.rawrabbitmeat";
        private const string RawVenisonItemId = "ccs.survival.item.resource.rawvenison";
        private const string RawTurkeyMeatItemId = "ccs.survival.item.resource.rawturkeymeat";
        private const string CookedFishItemId = "ccs.survival.item.food.cookedfish";
        private const string CookedMeatItemId = "ccs.survival.item.food.cookedmeat";
        private const string CookedRabbitItemId = "ccs.survival.item.food.cookedrabbitmeat";
        private const string CookedVenisonItemId = "ccs.survival.item.food.cookedvenison";
        private const string CookedTurkeyItemId = "ccs.survival.item.food.cookedturkey";
        private const string JerkyItemId = "ccs.survival.item.food.jerky";
        private const string DriedFishItemId = "ccs.survival.item.food.driedfish";
        private const string HardtackItemId = "ccs.survival.item.starter.hardtack";

        public static void ExecuteBatch()
        {
            UpdateProjectVersion();

            CCS_ItemDefinition rawFish = LoadItem("Assets/CCS/Survival/Content/Items/Fishing/CCS_Item_RawFish.asset");
            CCS_ItemDefinition smallFish = LoadItem("Assets/CCS/Survival/Content/Items/Fishing/CCS_Item_SmallFish.asset");
            CCS_ItemDefinition rawMeat = LoadItem(WildlifeItemsRoot + "/CCS_Item_RawMeat.asset");
            CCS_ItemDefinition rawRabbit = LoadItem(WildlifeItemsRoot + "/CCS_Item_RawRabbitMeat.asset");
            CCS_ItemDefinition rawVenison = LoadItem(WildlifeItemsRoot + "/CCS_Item_RawVenison.asset");
            CCS_ItemDefinition rawTurkey = EnsureRawTurkeyMeatItem();
            CCS_ItemDefinition cookedFish = EnsureCookedFishItem();
            CCS_ItemDefinition cookedMeat = LoadItem(FoodItemsRoot + "/CCS_Item_CookedMeat.asset");
            CCS_ItemDefinition cookedRabbit = LoadItem(FoodItemsRoot + "/CCS_Item_CookedRabbitMeat.asset");
            CCS_ItemDefinition cookedVenison = LoadItem(FoodItemsRoot + "/CCS_Item_CookedVenison.asset");
            CCS_ItemDefinition cookedTurkey = EnsureCookedTurkeyItem();
            CCS_ItemDefinition jerky = EnsureJerkyItem();
            CCS_ItemDefinition driedFish = EnsureDriedFishItem();
            CCS_ItemDefinition hardtack = LoadItem("Assets/CCS/Survival/Content/Items/Frontier/CCS_Item_Hardtack.asset");
            CCS_ItemDefinition stick = LoadItem("Assets/CCS/Survival/Content/Items/Resources/Primitive/CCS_Item_Stick.asset");
            CCS_ItemDefinition wood = LoadItem("Assets/CCS/Survival/Content/Items/Resources/Primitive/CCS_Item_Wood.asset");

            ApplyEconomyValues(rawFish, sellValue: 2, buyValue: 0);
            ApplyEconomyValues(smallFish, sellValue: 2, buyValue: 0);
            ApplyEconomyValues(rawMeat, sellValue: 2, buyValue: 0);
            ApplyEconomyValues(rawRabbit, sellValue: 0, buyValue: 0);
            ApplyEconomyValues(rawVenison, sellValue: 0, buyValue: 0);
            ApplyEconomyValues(rawTurkey, sellValue: 0, buyValue: 0);
            ApplyEconomyValues(cookedFish, sellValue: 6, buyValue: 0);
            ApplyEconomyValues(cookedMeat, sellValue: 5, buyValue: 0);
            ApplyEconomyValues(cookedRabbit, sellValue: 5, buyValue: 0);
            ApplyEconomyValues(cookedVenison, sellValue: 8, buyValue: 0);
            ApplyEconomyValues(cookedTurkey, sellValue: 7, buyValue: 0);
            ApplyEconomyValues(jerky, sellValue: 8, buyValue: 3);
            ApplyEconomyValues(driedFish, sellValue: 7, buyValue: 3);
            ApplyEconomyValues(hardtack, sellValue: 1, buyValue: 2);

            EnsureTurkeyHarvestRawTurkeyMeat(rawTurkey);
            EnsureCookingProfile(
                rawFish,
                smallFish,
                rawMeat,
                rawRabbit,
                rawVenison,
                rawTurkey,
                cookedFish,
                cookedMeat,
                cookedRabbit,
                cookedVenison,
                cookedTurkey,
                jerky,
                driedFish,
                hardtack,
                stick,
                wood);
            EnsureGeneralStoreFoodCatalog(
                hardtack,
                jerky,
                driedFish,
                cookedFish,
                cookedMeat,
                cookedRabbit,
                cookedVenison,
                cookedTurkey,
                rawMeat,
                rawFish);
            EnsurePlaytestCookingSteps();
            UpdatePlaytestProfileVersion();
            BumpEconomyProfileVersions();

            AssetDatabase.SaveAssets();
            Debug.Log($"{LogPrefix} Frontier cooking bootstrap setup complete.");
            EditorApplication.Exit(0);
        }

        private static void UpdateProjectVersion()
        {
            CCS_SurvivalBootstrapVersionUtility.EnsureBundleVersionAtLeast(
                CCS_SurvivalBootstrapVersionUtility.CurrentMilestoneVersion);
        }

        private static CCS_ItemDefinition EnsureRawTurkeyMeatItem()
        {
            return EnsureWildlifeMeatItem(
                WildlifeItemsRoot + "/CCS_Item_RawTurkeyMeat.asset",
                RawTurkeyMeatItemId,
                "Raw Turkey Meat",
                "Uncooked turkey meat from hunting. Cook on a campfire before eating.");
        }

        private static CCS_ItemDefinition EnsureCookedFishItem()
        {
            return EnsureCookedFoodItem(
                FoodItemsRoot + "/CCS_Item_CookedFish.asset",
                CookedFishItemId,
                "Cooked Fish",
                "Cooked frontier fish that restores more hunger than raw fish.");
        }

        private static CCS_ItemDefinition EnsureCookedTurkeyItem()
        {
            return EnsureCookedFoodItem(
                FoodItemsRoot + "/CCS_Item_CookedTurkey.asset",
                CookedTurkeyItemId,
                "Cooked Turkey",
                "Cooked turkey meat that restores meaningful hunger.");
        }

        private static CCS_ItemDefinition EnsureJerkyItem()
        {
            return EnsureCookedFoodItem(
                FoodItemsRoot + "/CCS_Item_Jerky.asset",
                JerkyItemId,
                "Jerky",
                "Smoke-dried meat trail ration. Lower hunger restore than a hot meal, better trade value than raw meat.");
        }

        private static CCS_ItemDefinition EnsureDriedFishItem()
        {
            return EnsureCookedFoodItem(
                FoodItemsRoot + "/CCS_Item_DriedFish.asset",
                DriedFishItemId,
                "Dried Fish",
                "Smoke-dried fish trail ration. Salt curing planned for a future milestone.");
        }

        private static CCS_ItemDefinition EnsureWildlifeMeatItem(
            string assetPath,
            string itemId,
            string displayName,
            string description)
        {
            CCS_ItemDefinition item = LoadOrCreateItem(assetPath);
            SerializedObject serialized = new SerializedObject(item);
            serialized.FindProperty("itemId").stringValue = itemId;
            serialized.FindProperty("displayName").stringValue = displayName;
            serialized.FindProperty("description").stringValue = description;
            serialized.FindProperty("category").enumValueIndex = (int)CCS_ItemCategory.Resource;
            serialized.FindProperty("maxStackSize").intValue = 20;
            serialized.FindProperty("isStackable").boolValue = true;
            serialized.FindProperty("weight").floatValue = 0.35f;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(item);
            return item;
        }

        private static CCS_ItemDefinition EnsureCookedFoodItem(
            string assetPath,
            string itemId,
            string displayName,
            string description)
        {
            CCS_ItemDefinition item = LoadOrCreateItem(assetPath);
            SerializedObject serialized = new SerializedObject(item);
            serialized.FindProperty("itemId").stringValue = itemId;
            serialized.FindProperty("displayName").stringValue = displayName;
            serialized.FindProperty("description").stringValue = description;
            serialized.FindProperty("category").enumValueIndex = (int)CCS_ItemCategory.Consumable;
            serialized.FindProperty("maxStackSize").intValue = 20;
            serialized.FindProperty("isStackable").boolValue = true;
            serialized.FindProperty("weight").floatValue = 0.3f;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(item);
            return item;
        }

        private static void ApplyEconomyValues(CCS_ItemDefinition item, int sellValue, int buyValue)
        {
            if (item == null)
            {
                return;
            }

            SerializedObject serialized = new SerializedObject(item);
            serialized.FindProperty("hasEconomyValues").boolValue = true;
            serialized.FindProperty("sellValue").intValue = sellValue;
            serialized.FindProperty("buyValue").intValue = buyValue;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(item);
        }

        private static void EnsureTurkeyHarvestRawTurkeyMeat(CCS_ItemDefinition rawTurkeyMeat)
        {
            CCS_WildlifeHarvestDefinition harvest =
                AssetDatabase.LoadAssetAtPath<CCS_WildlifeHarvestDefinition>(TurkeyHarvestPath);
            if (harvest == null || rawTurkeyMeat == null)
            {
                return;
            }

            SerializedObject serialized = new SerializedObject(harvest);
            SerializedProperty butcherDrops = serialized.FindProperty("butcherDrops");
            bool alreadyLinked = false;
            for (int index = 0; index < butcherDrops.arraySize; index++)
            {
                SerializedProperty drop = butcherDrops.GetArrayElementAtIndex(index);
                if (drop.FindPropertyRelative("itemDefinition").objectReferenceValue == rawTurkeyMeat)
                {
                    alreadyLinked = true;
                    break;
                }
            }

            if (!alreadyLinked)
            {
                int insertIndex = butcherDrops.arraySize;
                butcherDrops.InsertArrayElementAtIndex(insertIndex);
                SerializedProperty drop = butcherDrops.GetArrayElementAtIndex(insertIndex);
                drop.FindPropertyRelative("itemDefinition").objectReferenceValue = rawTurkeyMeat;
                drop.FindPropertyRelative("minQuantity").intValue = 1;
                drop.FindPropertyRelative("maxQuantity").intValue = 1;
                drop.FindPropertyRelative("harvestMethodType").enumValueIndex = 5;
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(harvest);
        }

        private static void EnsureCookingProfile(
            CCS_ItemDefinition rawFish,
            CCS_ItemDefinition smallFish,
            CCS_ItemDefinition rawMeat,
            CCS_ItemDefinition rawRabbit,
            CCS_ItemDefinition rawVenison,
            CCS_ItemDefinition rawTurkey,
            CCS_ItemDefinition cookedFish,
            CCS_ItemDefinition cookedMeat,
            CCS_ItemDefinition cookedRabbit,
            CCS_ItemDefinition cookedVenison,
            CCS_ItemDefinition cookedTurkey,
            CCS_ItemDefinition jerky,
            CCS_ItemDefinition driedFish,
            CCS_ItemDefinition hardtack,
            CCS_ItemDefinition stick,
            CCS_ItemDefinition wood)
        {
            CCS_CookingProfile profile = AssetDatabase.LoadAssetAtPath<CCS_CookingProfile>(DefaultCookingProfilePath);
            if (profile == null)
            {
                Debug.LogError($"{LogPrefix} Missing cooking profile.");
                EditorApplication.Exit(1);
                return;
            }

            SerializedObject serialized = new SerializedObject(profile);
            serialized.FindProperty("profileVersion").stringValue = "1.3.4";
            serialized.FindProperty("profileDescription").stringValue =
                "Frontier campfire cooking for fish, meat, and smoke preservation (1.3.4). Salt curing planned.";
            serialized.FindProperty("rawMeatItemDefinition").objectReferenceValue = rawMeat;
            serialized.FindProperty("cookedMeatItemDefinition").objectReferenceValue = cookedMeat;

            CCS_ItemDefinition[] catalog =
            {
                rawFish, smallFish, rawMeat, rawRabbit, rawVenison, rawTurkey,
                cookedFish, cookedMeat, cookedRabbit, cookedVenison, cookedTurkey,
                jerky, driedFish, hardtack, stick, wood
            };
            SerializedProperty catalogProperty = serialized.FindProperty("recipeItemCatalog");
            catalogProperty.arraySize = catalog.Length;
            for (int index = 0; index < catalog.Length; index++)
            {
                catalogProperty.GetArrayElementAtIndex(index).objectReferenceValue = catalog[index];
            }

            SerializedProperty recipes = serialized.FindProperty("recipes");
            recipes.arraySize = 8;
            ApplyRecipe(recipes.GetArrayElementAtIndex(0), "ccs.survival.cooking.recipe.cookfish", "Cook Fish", RawFishItemId, CookedFishItemId, 5f, stick, wood, 1);
            ApplyRecipe(recipes.GetArrayElementAtIndex(1), "ccs.survival.cooking.recipe.cooksmallfish", "Cook Small Fish", SmallFishItemId, CookedFishItemId, 4f, stick, wood, 1);
            ApplyRecipe(recipes.GetArrayElementAtIndex(2), "ccs.survival.cooking.recipe.cookmeat", "Cook Meat", RawMeatItemId, CookedMeatItemId, 6f, stick, wood, 1);
            ApplyRecipe(recipes.GetArrayElementAtIndex(3), "ccs.survival.cooking.recipe.cookrabbit", "Cook Rabbit Meat", RawRabbitMeatItemId, CookedRabbitItemId, 5f, stick, wood, 1);
            ApplyRecipe(recipes.GetArrayElementAtIndex(4), "ccs.survival.cooking.recipe.cookvenison", "Cook Venison", RawVenisonItemId, CookedVenisonItemId, 7f, stick, wood, 1);
            ApplyRecipe(recipes.GetArrayElementAtIndex(5), "ccs.survival.cooking.recipe.cookturkey", "Cook Turkey", RawTurkeyMeatItemId, CookedTurkeyItemId, 6f, stick, wood, 1);
            ApplyRecipe(recipes.GetArrayElementAtIndex(6), "ccs.survival.cooking.recipe.smokejerky", "Smoke Jerky", RawMeatItemId, JerkyItemId, 12f, stick, wood, 2);
            ApplyRecipe(recipes.GetArrayElementAtIndex(7), "ccs.survival.cooking.recipe.smokedriedfish", "Smoke Dried Fish", RawFishItemId, DriedFishItemId, 10f, stick, wood, 2);

            SerializedProperty consumables = serialized.FindProperty("consumableFoodDefinitions");
            consumables.arraySize = 14;
            SetConsumable(consumables.GetArrayElementAtIndex(0), cookedFish, 28f, "Cooked Fish");
            SetConsumable(consumables.GetArrayElementAtIndex(1), cookedMeat, 40f, "Cooked Meat");
            SetConsumable(consumables.GetArrayElementAtIndex(2), cookedRabbit, 35f, "Cooked Rabbit Meat");
            SetConsumable(consumables.GetArrayElementAtIndex(3), cookedVenison, 50f, "Cooked Venison");
            SetConsumable(consumables.GetArrayElementAtIndex(4), cookedTurkey, 45f, "Cooked Turkey");
            SetConsumable(consumables.GetArrayElementAtIndex(5), jerky, 18f, "Jerky");
            SetConsumable(consumables.GetArrayElementAtIndex(6), driedFish, 15f, "Dried Fish");
            SetConsumable(consumables.GetArrayElementAtIndex(7), hardtack, 12f, "Hardtack");
            SetConsumable(consumables.GetArrayElementAtIndex(8), rawFish, 5f, "Raw Fish");
            SetConsumable(consumables.GetArrayElementAtIndex(9), smallFish, 4f, "Small Fish");
            SetConsumable(consumables.GetArrayElementAtIndex(10), rawMeat, 6f, "Raw Meat");
            SetConsumable(consumables.GetArrayElementAtIndex(11), rawRabbit, 8f, "Raw Rabbit Meat");
            SetConsumable(consumables.GetArrayElementAtIndex(12), rawVenison, 12f, "Raw Venison");
            SetConsumable(consumables.GetArrayElementAtIndex(13), rawTurkey, 10f, "Raw Turkey Meat");

            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
            profile.BuildRecipeLookup();
        }

        private static void ApplyRecipe(
            SerializedProperty recipeProperty,
            string recipeId,
            string displayName,
            string rawItemId,
            string cookedItemId,
            float cookDuration,
            CCS_ItemDefinition stick,
            CCS_ItemDefinition wood,
            int requiredFuel)
        {
            recipeProperty.FindPropertyRelative("recipeId").stringValue = recipeId;
            recipeProperty.FindPropertyRelative("displayName").stringValue = displayName;
            recipeProperty.FindPropertyRelative("rawItemDefinitionId").stringValue = rawItemId;
            recipeProperty.FindPropertyRelative("cookedItemDefinitionId").stringValue = cookedItemId;
            recipeProperty.FindPropertyRelative("rawAmount").intValue = 1;
            recipeProperty.FindPropertyRelative("cookedAmount").intValue = 1;
            recipeProperty.FindPropertyRelative("cookDurationSeconds").floatValue = cookDuration;
            recipeProperty.FindPropertyRelative("requiredFuelAmount").intValue = requiredFuel;
            SerializedProperty fuelIds = recipeProperty.FindPropertyRelative("acceptedFuelItemIds");
            fuelIds.arraySize = 2;
            fuelIds.GetArrayElementAtIndex(0).stringValue = stick.ItemId;
            fuelIds.GetArrayElementAtIndex(1).stringValue = wood.ItemId;
        }

        private static void SetConsumable(
            SerializedProperty entry,
            CCS_ItemDefinition item,
            float hunger,
            string displayName)
        {
            entry.FindPropertyRelative("itemDefinition").objectReferenceValue = item;
            entry.FindPropertyRelative("hungerRestoreAmount").floatValue = hunger;
            entry.FindPropertyRelative("consumeCooldownSeconds").floatValue = 0f;
            entry.FindPropertyRelative("notificationDisplayName").stringValue = displayName;
        }

        private static void EnsureGeneralStoreFoodCatalog(
            CCS_ItemDefinition hardtack,
            CCS_ItemDefinition jerky,
            CCS_ItemDefinition driedFish,
            CCS_ItemDefinition cookedFish,
            CCS_ItemDefinition cookedMeat,
            CCS_ItemDefinition cookedRabbit,
            CCS_ItemDefinition cookedVenison,
            CCS_ItemDefinition cookedTurkey,
            CCS_ItemDefinition rawMeat,
            CCS_ItemDefinition rawFish)
        {
            CCS_VendorDefinition vendor = AssetDatabase.LoadAssetAtPath<CCS_VendorDefinition>(GeneralStoreVendorPath);
            if (vendor == null)
            {
                return;
            }

            VendorRow[] rows =
            {
                new VendorRow(hardtack, true, false),
                new VendorRow(jerky, true, true),
                new VendorRow(driedFish, true, true),
                new VendorRow(cookedFish, false, true),
                new VendorRow(cookedMeat, false, true),
                new VendorRow(cookedRabbit, false, true),
                new VendorRow(cookedVenison, false, true),
                new VendorRow(cookedTurkey, false, true),
                new VendorRow(rawMeat, false, true),
                new VendorRow(rawFish, false, true)
            };

            SerializedObject serialized = new SerializedObject(vendor);
            serialized.FindProperty("description").stringValue =
                "Frontier general store (1.3.4). Sells trail rations; buys fish, meat, and cooked goods.";
            SerializedProperty items = serialized.FindProperty("vendorInventory").FindPropertyRelative("items");
            MergeVendorRows(items, rows);
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(vendor);
        }

        private static void MergeVendorRows(SerializedProperty items, VendorRow[] rows)
        {
            for (int rowIndex = 0; rowIndex < rows.Length; rowIndex++)
            {
                VendorRow row = rows[rowIndex];
                if (row.Item == null)
                {
                    continue;
                }

                int existingIndex = FindVendorEntryIndex(items, row.Item);
                if (existingIndex >= 0)
                {
                    SerializedProperty existingEntry = items.GetArrayElementAtIndex(existingIndex);
                    existingEntry.FindPropertyRelative("allowBuy").boolValue = row.AllowBuy;
                    existingEntry.FindPropertyRelative("allowSell").boolValue = row.AllowSell;
                    continue;
                }

                int newIndex = items.arraySize;
                items.InsertArrayElementAtIndex(newIndex);
                SerializedProperty newEntry = items.GetArrayElementAtIndex(newIndex);
                newEntry.FindPropertyRelative("itemDefinition").objectReferenceValue = row.Item;
                newEntry.FindPropertyRelative("stockQuantity").intValue = -1;
                newEntry.FindPropertyRelative("allowBuy").boolValue = row.AllowBuy;
                newEntry.FindPropertyRelative("allowSell").boolValue = row.AllowSell;
                newEntry.FindPropertyRelative("buyPriceOverride").intValue = -1;
                newEntry.FindPropertyRelative("sellPriceOverride").intValue = -1;
            }
        }

        private static int FindVendorEntryIndex(SerializedProperty items, CCS_ItemDefinition item)
        {
            for (int index = 0; index < items.arraySize; index++)
            {
                if (items.GetArrayElementAtIndex(index).FindPropertyRelative("itemDefinition").objectReferenceValue == item)
                {
                    return index;
                }
            }

            return -1;
        }

        private static void EnsurePlaytestCookingSteps()
        {
            CCS_PlaytestProfile profile = AssetDatabase.LoadAssetAtPath<CCS_PlaytestProfile>(DefaultPlaytestProfilePath);
            if (profile == null)
            {
                return;
            }

            SerializedObject serializedProfile = new SerializedObject(profile);
            SerializedProperty stepList = serializedProfile.FindProperty("stepDefinitions");
            RemoveCookingExpansionSteps(stepList);

            int insertIndex = FindStepIndex(stepList, TrappingCurrencyStepId);
            if (insertIndex < 0)
            {
                insertIndex = stepList.arraySize;
            }
            else
            {
                insertIndex += 1;
            }

            InsertStep(stepList, insertIndex++, "ccs.survival.playtest.cooking.raw.obtain", "Obtain raw food", CCS_PlaytestStepType.ObtainRawFoodForCooking, "Press Ctrl+Alt+M for raw meat or Ctrl+V for raw fish.", RawMeatItemId);
            InsertStep(stepList, insertIndex++, "ccs.survival.playtest.cooking.cook", "Cook at campfire", CCS_PlaytestStepType.CookFood, "Interact with CCS_TestCampfire and cook raw meat or fish.");
            InsertStep(stepList, insertIndex++, "ccs.survival.playtest.cooking.verify", "Verify cooked food", CCS_PlaytestStepType.VerifyCookedFoodInInventory, "Confirm cooked fish or meat is in inventory after cooking.", CookedMeatItemId);
            InsertStep(stepList, insertIndex++, "ccs.survival.playtest.cooking.eat", "Consume cooked food", CCS_PlaytestStepType.EatFood, "Press F to consume cooked food from inventory.");
            InsertStep(stepList, insertIndex++, "ccs.survival.playtest.cooking.preserve", "Preserve food at campfire", CCS_PlaytestStepType.PreserveFoodAtCampfire, "Smoke raw meat into jerky or raw fish into dried fish (longer cook time).", JerkyItemId);
            InsertStep(stepList, insertIndex++, "ccs.survival.playtest.cooking.sell", "Sell preserved food", CCS_PlaytestStepType.SellPreservedFoodAtVendor, "Sell jerky at general store (Shift+V on this step).", JerkyItemId);
            InsertStep(stepList, insertIndex, "ccs.survival.playtest.cooking.currency.verify", "Verify cooking currency increased", CCS_PlaytestStepType.VerifyCookingCurrencyIncreased, "Confirm Trade Dollars increased after selling preserved food.");

            serializedProfile.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
        }

        private static void RemoveCookingExpansionSteps(SerializedProperty stepList)
        {
            for (int index = stepList.arraySize - 1; index >= 0; index--)
            {
                string stepId = stepList.GetArrayElementAtIndex(index).FindPropertyRelative("stepId").stringValue;
                if (!string.IsNullOrEmpty(stepId) && stepId.StartsWith("ccs.survival.playtest.cooking."))
                {
                    stepList.DeleteArrayElementAtIndex(index);
                }
            }
        }

        private static void UpdatePlaytestProfileVersion()
        {
            CCS_PlaytestProfile profile = AssetDatabase.LoadAssetAtPath<CCS_PlaytestProfile>(DefaultPlaytestProfilePath);
            if (profile == null)
            {
                return;
            }

            SerializedObject serialized = new SerializedObject(profile);
            serialized.FindProperty("profileVersion").stringValue = "1.3.4";
            serialized.FindProperty("profileDescription").stringValue =
                "Frontier starter progression with fishing, hunting, trapping, and cooking playtest checklist (1.3.4).";
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
        }

        private static void BumpEconomyProfileVersions()
        {
            BumpProfileVersion("Assets/CCS/Survival/Profiles/Economy/CCS_DefaultEconomyProfile.asset");
            BumpProfileVersion("Assets/CCS/Survival/Profiles/Economy/CCS_DefaultVendorProfile.asset");
        }

        private static void BumpProfileVersion(string path)
        {
            ScriptableObject profile = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
            if (profile == null)
            {
                return;
            }

            SerializedObject serialized = new SerializedObject(profile);
            serialized.FindProperty("profileVersion").stringValue = "1.3.4";
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
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
            int index,
            string stepId,
            string displayName,
            CCS_PlaytestStepType stepType,
            string instructionText,
            string targetItemId = "")
        {
            stepList.InsertArrayElementAtIndex(index);
            SerializedProperty stepProperty = stepList.GetArrayElementAtIndex(index);
            stepProperty.FindPropertyRelative("stepId").stringValue = stepId;
            stepProperty.FindPropertyRelative("displayName").stringValue = displayName;
            stepProperty.FindPropertyRelative("stepType").enumValueIndex = (int)stepType;
            stepProperty.FindPropertyRelative("instructionText").stringValue = instructionText;
            stepProperty.FindPropertyRelative("targetItemId").stringValue = targetItemId ?? string.Empty;
            stepProperty.FindPropertyRelative("targetObjectId").stringValue = string.Empty;
            stepProperty.FindPropertyRelative("requiredCount").intValue = 1;
        }

        private static CCS_ItemDefinition LoadOrCreateItem(string path)
        {
            CCS_ItemDefinition item = AssetDatabase.LoadAssetAtPath<CCS_ItemDefinition>(path);
            if (item != null)
            {
                return item;
            }

            string directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory) && !AssetDatabase.IsValidFolder(directory))
            {
                string[] parts = directory.Replace('\\', '/').Split('/');
                string current = parts[0];
                for (int index = 1; index < parts.Length; index++)
                {
                    string next = current + "/" + parts[index];
                    if (!AssetDatabase.IsValidFolder(next))
                    {
                        AssetDatabase.CreateFolder(current, parts[index]);
                    }

                    current = next;
                }
            }

            item = ScriptableObject.CreateInstance<CCS_ItemDefinition>();
            AssetDatabase.CreateAsset(item, path);
            return item;
        }

        private static CCS_ItemDefinition LoadItem(string path)
        {
            CCS_ItemDefinition item = AssetDatabase.LoadAssetAtPath<CCS_ItemDefinition>(path);
            if (item == null)
            {
                Debug.LogError($"{LogPrefix} Missing item: {path}");
                EditorApplication.Exit(1);
            }

            return item;
        }

        private readonly struct VendorRow
        {
            public VendorRow(CCS_ItemDefinition item, bool allowBuy, bool allowSell)
            {
                Item = item;
                AllowBuy = allowBuy;
                AllowSell = allowSell;
            }

            public CCS_ItemDefinition Item { get; }

            public bool AllowBuy { get; }

            public bool AllowSell { get; }
        }
    }
}
