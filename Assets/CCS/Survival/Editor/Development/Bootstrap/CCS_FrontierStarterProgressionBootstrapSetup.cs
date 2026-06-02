using System.Collections.Generic;
using System.IO;
using CCS.Modules.Crafting;
using CCS.Modules.Inventory;
using CCS.Survival.Composition;
using CCS.Survival.Player.Loadout;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_FrontierStarterProgressionBootstrapSetup
// CATEGORY: Survival / Editor / Development / Bootstrap
// PURPOSE: Western frontier starter loadout, items, and primitive crafting for 1.2.6.
// PLACEMENT: Batch entry for milestone 1.2.6 frontier starter progression rework.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: Survival content only. Spear remains legacy/regression, not default progression.
// =============================================================================

namespace CCS.Survival.Editor.Development
{
    public static class CCS_FrontierStarterProgressionBootstrapSetup
    {
        private const string FrontierItemsRoot = "Assets/CCS/Survival/Content/Items/Frontier";
        private const string StarterItemsRoot = "Assets/CCS/Survival/Content/Items/Starter";
        private const string FishingItemsRoot = "Assets/CCS/Survival/Content/Items/Fishing";
        private const string ToolsFishingRoot = "Assets/CCS/Survival/Content/Items/Tools/Fishing";
        private const string ResourcesPrimitiveRoot = "Assets/CCS/Survival/Content/Items/Resources/Primitive";
        private const string ResourcesFrontierRoot = "Assets/CCS/Survival/Content/Items/Resources/Frontier";
        private const string FrontierRecipesRoot = "Assets/CCS/Survival/Profiles/Crafting/FrontierPrimitiveRecipes";
        private const string StarterProfilePath = "Assets/CCS/Survival/Profiles/StarterLoadout/CCS_DefaultStarterLoadoutProfile.asset";
        private const string InventoryProfilePath = "Assets/CCS/Survival/Profiles/Inventory/CCS_DefaultInventoryProfile.asset";
        private const string BootstrapPrefabPath = "Assets/CCS/Survival/Prefabs/PF_CCS_Survival_BootstrapRoot.prefab";
        private const string LogPrefix = "[CCS_FrontierStarterProgressionBootstrapSetup]";

        public static void ExecuteBatch()
        {
            EnsureFolders();

            CCS_ItemDefinition pocketKnife = UpdatePocketKnife();
            CCS_ItemDefinition bedroll = LoadItem($"{StarterItemsRoot}/CCS_Item_Bedroll.asset");
            CCS_ItemDefinition canteen = EnsureFrontierItem(
                "CCS_Item_Canteen",
                "ccs.survival.item.starter.canteen",
                "Canteen",
                "Worn leather canteen for frontier travel.",
                CCS_ItemCategory.Consumable,
                1,
                0.8f);
            CCS_ItemDefinition hardtack = EnsureFrontierItem(
                "CCS_Item_Hardtack",
                "ccs.survival.item.starter.hardtack",
                "Hardtack Ration",
                "Dry frontier travel ration.",
                CCS_ItemCategory.Consumable,
                10,
                0.15f);
            CCS_ItemDefinition dollars = UpdateTradeDollars();
            CCS_ItemDefinition tinderbox = EnsureFrontierItem(
                "CCS_Item_Tinderbox",
                "ccs.survival.item.starter.tinderbox",
                "Tinderbox",
                "Flint and steel tinderbox placeholder.",
                CCS_ItemCategory.Material,
                1,
                0.2f);

            CCS_ItemDefinition sapling = LoadResourceItem("CCS_Item_Sapling");
            CCS_ItemDefinition fiber = LoadResourceItem("CCS_Item_Fiber");
            CCS_ItemDefinition stick = LoadResourceItem("CCS_Item_Stick");
            CCS_ItemDefinition wood = LoadResourceItem("CCS_Item_Wood");
            CCS_ItemDefinition stone = LoadResourceItem("CCS_Item_Stone");
            CCS_ItemDefinition bone = LoadResourceItem("CCS_Item_Bone");
            CCS_ItemDefinition scrapIron = LoadResourceItem("CCS_Item_ScrapIron");
            CCS_ItemDefinition flint = LoadResourceItem("CCS_Item_Flint");

            CCS_ItemDefinition cordage = EnsureFrontierItem(
                "CCS_Item_Cordage",
                "ccs.survival.item.frontier.cordage",
                "Cordage",
                "Twisted plant-fiber cordage.",
                CCS_ItemCategory.Material,
                20,
                0.1f);
            CCS_ItemDefinition rawhideCord = EnsureFrontierItem(
                "CCS_Item_RawhideCord",
                "ccs.survival.item.frontier.rawhidecord",
                "Rawhide Cord",
                "Stripped rawhide cord for rigs and traps.",
                CCS_ItemCategory.Material,
                20,
                0.12f);
            CCS_ItemDefinition feather = EnsureFrontierItem(
                "CCS_Item_Feather",
                "ccs.survival.item.frontier.feather",
                "Feather",
                "Light feather fletching material.",
                CCS_ItemCategory.Material,
                50,
                0.02f);
            CCS_ItemDefinition animalFat = EnsureFrontierItem(
                "CCS_Item_AnimalFat",
                "ccs.survival.item.frontier.animalfat",
                "Animal Fat",
                "Rendered fat for torches and camp cooking.",
                CCS_ItemCategory.Material,
                20,
                0.2f);
            CCS_ItemDefinition tinderBundle = EnsureFrontierItem(
                "CCS_Item_TinderBundle",
                "ccs.survival.item.frontier.tinderbundle",
                "Tinder Bundle",
                "Dry fiber tinder for camp fires.",
                CCS_ItemCategory.Material,
                20,
                0.05f);

            CCS_ItemDefinition crudeHook = LoadItem($"{FishingItemsRoot}/CCS_Item_CrudeHook.asset");
            CCS_ItemDefinition fishingLine = LoadItem($"{FishingItemsRoot}/CCS_Item_FishingLine.asset");
            CCS_ItemDefinition fishingPole = LoadItem($"{ToolsFishingRoot}/CCS_Item_FishingPole.asset");
            CCS_ItemDefinition rawFish = LoadItem($"{FishingItemsRoot}/CCS_Item_RawFish.asset");

            CCS_ItemDefinition bow = EnsureBowItem();
            CCS_ItemDefinition arrow = EnsureArrowItem();
            CCS_ItemDefinition simpleTrap = EnsureFrontierItem(
                "CCS_Item_SimpleTrap",
                "ccs.survival.item.frontier.simpletrap",
                "Simple Trap",
                "Primitive snare trap frame placeholder.",
                CCS_ItemCategory.Material,
                5,
                1.5f);

            CCS_ItemDefinition campfireKit = LoadItem($"{StarterItemsRoot}/CCS_Item_CampfireKit.asset");
            if (campfireKit == null)
            {
                campfireKit = EnsureFrontierItem(
                    "CCS_Item_CampfireKit",
                    "ccs.survival.item.starter.campfirekit",
                    "Campfire Kit",
                    "Portable campfire setup placeholder.",
                    CCS_ItemCategory.Material,
                    5,
                    2f);
            }

            CCS_ItemDefinition primitiveTorch = LoadItem(
                "Assets/CCS/Survival/Content/Items/Progression/CCS_Item_PrimitiveTorch.asset");

            ValidateRequired(
                pocketKnife,
                bedroll,
                sapling,
                fiber,
                stick,
                wood,
                stone,
                bone,
                scrapIron,
                flint,
                crudeHook,
                fishingLine,
                fishingPole);

            CCS_CraftingRecipeDefinition tinderBundleRecipe = EnsureRecipe(
                "CCS_FrontierTinderBundleRecipe",
                "ccs.survival.recipe.frontier.tinderbundle",
                "Tinder Bundle",
                new[] { (fiber, 2) },
                new[] { (tinderBundle, 1) });

            CCS_CraftingRecipeDefinition cordageRecipe = EnsureRecipe(
                "CCS_FrontierCordageRecipe",
                "ccs.survival.recipe.frontier.cordage",
                "Cordage",
                new[] { (fiber, 2) },
                new[] { (cordage, 1) });

            CCS_CraftingRecipeDefinition fishingLineRecipe = EnsureRecipe(
                "CCS_FrontierFishingLineRecipe",
                "ccs.survival.recipe.frontier.fishingline",
                "Fishing Line",
                new[] { (fiber, 2) },
                new[] { (fishingLine, 1) });

            CCS_CraftingRecipeDefinition crudeHookBoneRecipe = EnsureRecipe(
                "CCS_FrontierCrudeHookBoneRecipe",
                "ccs.survival.recipe.frontier.crudehook.bone",
                "Crude Hook (Bone)",
                new[] { (bone, 1) },
                new[] { (crudeHook, 1) });

            CCS_CraftingRecipeDefinition crudeHookScrapRecipe = EnsureRecipe(
                "CCS_FrontierCrudeHookScrapRecipe",
                "ccs.survival.recipe.frontier.crudehook.scrap",
                "Crude Hook (Scrap)",
                new[] { (scrapIron, 1) },
                new[] { (crudeHook, 1) });

            CCS_CraftingRecipeDefinition fishingPoleRecipe = EnsureRecipe(
                "CCS_FrontierFishingPoleRecipe",
                "ccs.survival.recipe.frontier.fishingpole",
                "Fishing Pole",
                new[] { (sapling, 1), (fishingLine, 1), (crudeHook, 1) },
                new[] { (fishingPole, 1) });

            CCS_CraftingRecipeDefinition bowRecipe = EnsureRecipe(
                "CCS_FrontierBowRecipe",
                "ccs.survival.recipe.frontier.bow",
                "Frontier Bow",
                new[] { (sapling, 1), (cordage, 1) },
                new[] { (bow, 1) });

            CCS_CraftingRecipeDefinition arrowRecipe = EnsureRecipe(
                "CCS_FrontierArrowRecipe",
                "ccs.survival.recipe.frontier.arrow",
                "Frontier Arrows",
                new[] { (stick, 2), (flint, 1), (feather, 2) },
                new[] { (arrow, 4) });

            CCS_CraftingRecipeDefinition trapRecipe = EnsureRecipe(
                "CCS_FrontierSimpleTrapRecipe",
                "ccs.survival.recipe.frontier.simpletrap",
                "Simple Trap",
                new[] { (sapling, 2), (cordage, 1) },
                new[] { (simpleTrap, 1) });

            CCS_CraftingRecipeDefinition campfireRecipe = EnsureRecipe(
                "CCS_FrontierCampfireRecipe",
                "ccs.survival.recipe.frontier.campfire",
                "Campfire Kit",
                new[] { (wood, 2), (stone, 2), (tinderBundle, 1) },
                new[] { (campfireKit, 1) });

            CCS_CraftingRecipeDefinition torchRecipe = null;
            if (primitiveTorch != null)
            {
                torchRecipe = EnsureRecipe(
                    "CCS_FrontierTorchRecipe",
                    "ccs.survival.recipe.frontier.torch",
                    "Primitive Torch",
                    new[] { (stick, 1), (fiber, 1), (animalFat, 1) },
                    new[] { (primitiveTorch, 1) });
            }

            List<CCS_CraftingRecipeDefinition> frontierRecipes = new List<CCS_CraftingRecipeDefinition>
            {
                tinderBundleRecipe,
                cordageRecipe,
                fishingLineRecipe,
                crudeHookBoneRecipe,
                crudeHookScrapRecipe,
                fishingPoleRecipe,
                bowRecipe,
                arrowRecipe,
                trapRecipe,
                campfireRecipe
            };
            if (torchRecipe != null)
            {
                frontierRecipes.Add(torchRecipe);
            }

            EnsureStarterLoadoutProfile(
                pocketKnife,
                bedroll,
                canteen,
                hardtack,
                tinderbox,
                dollars,
                frontierRecipes.ToArray(),
                hardtackQuantity: 3,
                startingCurrency: 10);

            EnsureInventoryCatalog(
                pocketKnife,
                bedroll,
                canteen,
                hardtack,
                dollars,
                tinderbox,
                cordage,
                rawhideCord,
                feather,
                animalFat,
                tinderBundle,
                bow,
                arrow,
                simpleTrap,
                crudeHook,
                fishingLine,
                fishingPole,
                rawFish,
                sapling,
                fiber,
                stick,
                wood,
                stone,
                bone,
                scrapIron,
                flint);

            EnsureBootstrapHost();

            AssetDatabase.SaveAssets();
            Debug.Log($"{LogPrefix} Frontier starter progression bootstrap complete.");
            EditorApplication.Exit(0);
        }

        private static void EnsureFolders()
        {
            EnsureFolder("Assets/CCS/Survival/Content/Items/Frontier");
            EnsureFolder(FrontierRecipesRoot);
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
            {
                return;
            }

            string parent = Path.GetDirectoryName(path)?.Replace('\\', '/') ?? "Assets";
            string name = Path.GetFileName(path);
            AssetDatabase.CreateFolder(parent, name);
        }

        private static CCS_ItemDefinition LoadItem(string path)
        {
            return AssetDatabase.LoadAssetAtPath<CCS_ItemDefinition>(path);
        }

        private static CCS_ItemDefinition LoadResourceItem(string assetName)
        {
            CCS_ItemDefinition item = LoadItem($"{ResourcesPrimitiveRoot}/{assetName}.asset");
            if (item != null)
            {
                return item;
            }

            return LoadItem($"{ResourcesFrontierRoot}/{assetName}.asset");
        }

        private static void ValidateRequired(params CCS_ItemDefinition[] items)
        {
            for (int index = 0; index < items.Length; index++)
            {
                if (items[index] == null)
                {
                    Debug.LogError($"{LogPrefix} Missing required item at index {index}. Run frontier resource and fishing bootstraps first.");
                    EditorApplication.Exit(1);
                }
            }
        }

        private static CCS_ItemDefinition UpdatePocketKnife()
        {
            CCS_ItemDefinition knife = LoadItem($"{StarterItemsRoot}/CCS_Item_Knife.asset");
            if (knife == null)
            {
                Debug.LogError($"{LogPrefix} Missing starter knife item.");
                EditorApplication.Exit(1);
                return null;
            }

            SerializedObject serialized = new SerializedObject(knife);
            serialized.FindProperty("itemId").stringValue = "ccs.survival.item.starter.knife";
            serialized.FindProperty("displayName").stringValue = "Pocket Knife";
            serialized.FindProperty("description").stringValue =
                "Frontier traveler's pocket knife for skinning, camp chores, and close defense.";
            serialized.FindProperty("category").intValue = (int)CCS_ItemCategory.Tool;
            serialized.FindProperty("gameplayKind").intValue = (int)CCS_ItemGameplayKind.ToolAndWeapon;
            serialized.FindProperty("hasToolIdentity").boolValue = true;
            serialized.FindProperty("toolType").intValue = (int)CCS_ItemToolType.Knife;
            serialized.FindProperty("toolArchetype").intValue = (int)CCS_ToolArchetype.Knife;
            serialized.FindProperty("hasWeaponIdentity").boolValue = true;
            serialized.FindProperty("weaponArchetype").intValue = (int)CCS_WeaponArchetype.Knife;
            serialized.FindProperty("weaponType").intValue = (int)CCS_WeaponType.Melee;
            serialized.FindProperty("damageType").intValue = (int)CCS_DamageType.Slash;
            serialized.FindProperty("rangeType").intValue = (int)CCS_RangeType.Melee;
            serialized.FindProperty("meleeDamage").floatValue = 8f;
            serialized.FindProperty("meleeRange").floatValue = 1.8f;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(knife);
            return knife;
        }

        private static CCS_ItemDefinition UpdateTradeDollars()
        {
            CCS_ItemDefinition coin = LoadItem($"{StarterItemsRoot}/CCS_Item_Coin.asset");
            if (coin == null)
            {
                coin = ScriptableObject.CreateInstance<CCS_ItemDefinition>();
                AssetDatabase.CreateAsset(coin, $"{StarterItemsRoot}/CCS_Item_Coin.asset");
            }

            SerializedObject serialized = new SerializedObject(coin);
            serialized.FindProperty("itemId").stringValue = "ccs.survival.item.starter.dollars";
            serialized.FindProperty("displayName").stringValue = "Trade Dollars";
            serialized.FindProperty("description").stringValue =
                "Frontier trade coin placeholder for future economy systems.";
            serialized.FindProperty("category").intValue = (int)CCS_ItemCategory.Generic;
            serialized.FindProperty("maxStackSize").intValue = 999;
            serialized.FindProperty("isStackable").boolValue = true;
            serialized.FindProperty("weight").floatValue = 0.01f;
            serialized.FindProperty("gameplayKind").intValue = (int)CCS_ItemGameplayKind.Generic;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(coin);
            return coin;
        }

        private static CCS_ItemDefinition EnsureFrontierItem(
            string assetName,
            string itemId,
            string displayName,
            string description,
            CCS_ItemCategory category,
            int maxStack,
            float weight)
        {
            string path = $"{FrontierItemsRoot}/{assetName}.asset";
            CCS_ItemDefinition item = LoadItem(path);
            if (item == null)
            {
                item = ScriptableObject.CreateInstance<CCS_ItemDefinition>();
                AssetDatabase.CreateAsset(item, path);
            }

            SerializedObject serialized = new SerializedObject(item);
            serialized.FindProperty("itemId").stringValue = itemId;
            serialized.FindProperty("displayName").stringValue = displayName;
            serialized.FindProperty("description").stringValue = description;
            serialized.FindProperty("category").intValue = (int)category;
            serialized.FindProperty("maxStackSize").intValue = maxStack;
            serialized.FindProperty("isStackable").boolValue = maxStack > 1;
            serialized.FindProperty("weight").floatValue = weight;
            serialized.FindProperty("gameplayKind").intValue = (int)CCS_ItemGameplayKind.Generic;
            serialized.FindProperty("hasToolIdentity").boolValue = false;
            serialized.FindProperty("hasWeaponIdentity").boolValue = false;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(item);
            return item;
        }

        private static CCS_ItemDefinition EnsureBowItem()
        {
            CCS_ItemDefinition bow = EnsureFrontierItem(
                "CCS_Item_Bow",
                "ccs.survival.item.frontier.bow",
                "Frontier Bow",
                "Simple wood-and-cord bow. Ranged use not implemented in 1.2.6.",
                CCS_ItemCategory.Generic,
                1,
                1.2f);
            SerializedObject serialized = new SerializedObject(bow);
            serialized.FindProperty("hasWeaponIdentity").boolValue = false;
            serialized.FindProperty("hasToolIdentity").boolValue = false;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(bow);
            return bow;
        }

        private static CCS_ItemDefinition EnsureArrowItem()
        {
            return EnsureFrontierItem(
                "CCS_Item_Arrow",
                "ccs.survival.item.frontier.arrow",
                "Frontier Arrow",
                "Fletched arrow placeholder. Ranged ammunition not implemented in 1.2.6.",
                CCS_ItemCategory.Material,
                50,
                0.08f);
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
            serialized.FindProperty("description").stringValue =
                "Frontier hand-crafting recipe using practical resource sources (1.2.6).";
            serialized.FindProperty("requiredStationType").intValue = (int)CCS_CraftingStationType.Hand;
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

        private static void EnsureStarterLoadoutProfile(
            CCS_ItemDefinition pocketKnife,
            CCS_ItemDefinition bedroll,
            CCS_ItemDefinition canteen,
            CCS_ItemDefinition hardtack,
            CCS_ItemDefinition tinderbox,
            CCS_ItemDefinition dollars,
            CCS_CraftingRecipeDefinition[] frontierRecipes,
            int hardtackQuantity,
            int startingCurrency)
        {
            CCS_StarterLoadoutProfile profile = AssetDatabase.LoadAssetAtPath<CCS_StarterLoadoutProfile>(StarterProfilePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<CCS_StarterLoadoutProfile>();
                AssetDatabase.CreateAsset(profile, StarterProfilePath);
            }

            SerializedObject serialized = new SerializedObject(profile);
            serialized.FindProperty("profileDisplayName").stringValue = "Frontier Traveler Starter Loadout";
            serialized.FindProperty("profileId").stringValue = "ccs.survival.profile.starterloadout.frontier";
            serialized.FindProperty("profileDescription").stringValue =
                "Western frontier traveler starter supplies for milestone 1.2.6.";
            serialized.FindProperty("profileVersion").stringValue = "1.2.6";
            serialized.FindProperty("startingCurrencyAmount").intValue = startingCurrency;
            serialized.FindProperty("currencyItemDefinition").objectReferenceValue = dollars;
            serialized.FindProperty("applyWhenInventoryEmpty").boolValue = true;

            SerializedProperty startingItems = serialized.FindProperty("startingItems");
            startingItems.ClearArray();
            AddLoadoutEntry(startingItems, pocketKnife, 1);
            AddLoadoutEntry(startingItems, bedroll, 1);
            AddLoadoutEntry(startingItems, canteen, 1);
            AddLoadoutEntry(startingItems, hardtack, hardtackQuantity);
            AddLoadoutEntry(startingItems, tinderbox, 1);

            SerializedProperty recipeList = serialized.FindProperty("primitiveRecipes");
            recipeList.ClearArray();
            for (int index = 0; index < frontierRecipes.Length; index++)
            {
                recipeList.InsertArrayElementAtIndex(index);
                recipeList.GetArrayElementAtIndex(index).objectReferenceValue = frontierRecipes[index];
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
        }

        private static void AddLoadoutEntry(
            SerializedProperty startingItems,
            CCS_ItemDefinition itemDefinition,
            int quantity)
        {
            int index = startingItems.arraySize;
            startingItems.InsertArrayElementAtIndex(index);
            SerializedProperty entry = startingItems.GetArrayElementAtIndex(index);
            entry.FindPropertyRelative("itemDefinition").objectReferenceValue = itemDefinition;
            entry.FindPropertyRelative("quantity").intValue = quantity;
        }

        private static void EnsureInventoryCatalog(params CCS_ItemDefinition[] items)
        {
            CCS_InventoryProfile profile = AssetDatabase.LoadAssetAtPath<CCS_InventoryProfile>(InventoryProfilePath);
            if (profile == null)
            {
                Debug.LogError($"{LogPrefix} Missing inventory profile.");
                EditorApplication.Exit(1);
                return;
            }

            List<CCS_ItemDefinition> merged = new List<CCS_ItemDefinition>();
            CCS_ItemDefinition[] existing = profile.SaveRestoreItemDefinitions;
            for (int index = 0; index < existing.Length; index++)
            {
                if (existing[index] != null && !merged.Contains(existing[index]))
                {
                    merged.Add(existing[index]);
                }
            }

            for (int index = 0; index < items.Length; index++)
            {
                if (items[index] != null && !merged.Contains(items[index]))
                {
                    merged.Add(items[index]);
                }
            }

            SerializedObject serialized = new SerializedObject(profile);
            SerializedProperty catalog = serialized.FindProperty("saveRestoreItemDefinitions");
            catalog.ClearArray();
            for (int index = 0; index < merged.Count; index++)
            {
                catalog.InsertArrayElementAtIndex(index);
                catalog.GetArrayElementAtIndex(index).objectReferenceValue = merged[index];
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
        }

        private static void EnsureBootstrapHost()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(BootstrapPrefabPath);
            if (prefab == null)
            {
                return;
            }

            CCS_SurvivalGameplayServiceHost host = prefab.GetComponent<CCS_SurvivalGameplayServiceHost>();
            if (host == null)
            {
                return;
            }

            SerializedObject serialized = new SerializedObject(host);
            serialized.FindProperty("starterLoadoutProfile").objectReferenceValue =
                AssetDatabase.LoadAssetAtPath<CCS_StarterLoadoutProfile>(StarterProfilePath);
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(prefab);
        }
    }
}
