using System.Collections.Generic;
using System.IO;
using CCS.Modules.Building;
using CCS.Modules.Cooking;
using CCS.Modules.Inventory;
using CCS.Modules.Wildlife;
using CCS.Survival.Composition;
using CCS.Survival.Player;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// =============================================================================
// SCRIPT: CCS_CookingBootstrapSetup
// CATEGORY: Modules / Cooking / Editor / Validation
// PURPOSE: Creates default profile, food items, campfire content, and bootstrap scene wiring.
// PLACEMENT: Batch entry for 0.9.4 campfire and cooking foundation milestone.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: 1.0.0 adds fuel-backed rabbit and venison recipes with CCS_CookingStation wiring.
// =============================================================================

namespace CCS.Modules.Cooking.Editor
{
    public static class CCS_CookingBootstrapSetup
    {
        private const string ProfilesRoot = "Assets/CCS/Survival/Profiles/Cooking";
        private const string DefaultProfilePath = ProfilesRoot + "/CCS_DefaultCookingProfile.asset";
        private const string DefaultBuildingProfilePath = "Assets/CCS/Survival/Profiles/Building/CCS_DefaultBuildingProfile.asset";
        private const string CookingDefinitionsRoot = "Assets/CCS/Survival/Content/Cooking/Definitions";
        private const string FoodItemsRoot = "Assets/CCS/Survival/Content/Items/Food";
        private const string BuildingDefinitionsRoot = "Assets/CCS/Survival/Content/Building/Definitions";
        private const string RawMeatItemPath = "Assets/CCS/Survival/Content/Items/Resources/Wildlife/CCS_Item_RawMeat.asset";
        private const string RawRabbitMeatItemPath = "Assets/CCS/Survival/Content/Items/Resources/Wildlife/CCS_Item_RawRabbitMeat.asset";
        private const string RawVenisonItemPath = "Assets/CCS/Survival/Content/Items/Resources/Wildlife/CCS_Item_RawVenison.asset";
        private const string CookedRabbitMeatItemPath = "Assets/CCS/Survival/Content/Items/Food/CCS_Item_CookedRabbitMeat.asset";
        private const string CookedVenisonItemPath = "Assets/CCS/Survival/Content/Items/Food/CCS_Item_CookedVenison.asset";
        private const string StickItemPath = "Assets/CCS/Survival/Content/Items/Resources/Primitive/CCS_Item_Stick.asset";
        private const string WoodItemPath = "Assets/CCS/Survival/Content/Items/Resources/Primitive/CCS_Item_Wood.asset";
        private const string InventoryProfilePath = "Assets/CCS/Survival/Profiles/Inventory/CCS_DefaultInventoryProfile.asset";
        private const string RabbitWildlifeDefinitionPath = "Assets/CCS/Survival/Content/Wildlife/Definitions/CCS_TestRabbit.asset";
        private const string DeerWildlifeDefinitionPath = "Assets/CCS/Survival/Content/Wildlife/Definitions/CCS_TestDeerCarcass.asset";
        private const string BasicFoodItemPath = "Assets/CCS/Survival/Content/Items/Starter/CCS_Item_BasicFood.asset";
        private const string CampfireKitItemPath = "Assets/CCS/Survival/Content/Items/Starter/CCS_Item_CampfireKit.asset";
        private const string BootstrapScenePath = "Assets/CCS/Survival/Scenes/SCN_CCS_Survival_Bootstrap.unity";
        private const string BootstrapPrefabPath = "Assets/CCS/Survival/Prefabs/PF_CCS_Survival_BootstrapRoot.prefab";
        private const string PlayerPrefabPath = "Assets/CCS/Survival/Prefabs/Player/PF_CCS_Player.prefab";
        private const string TestAreaObjectName = "CCS_CampfireTestArea";
        private const string TestCampfireObjectName = "CCS_TestCampfire";
        private const string PlacementPointObjectName = "CCS_CampfirePlacementPoint";
        private const string LogPrefix = "[CCS_CookingBootstrapSetup]";

        #region Public Methods

        public static void ExecuteBatch()
        {
            EnsureFolders();

            CCS_ItemDefinition rawMeatItem = LoadRequiredItem(RawMeatItemPath, "Raw Meat");
            CCS_ItemDefinition rawRabbitMeatItem = EnsureRawRabbitMeatItem();
            CCS_ItemDefinition rawVenisonItem = EnsureRawVenisonItem();
            CCS_ItemDefinition cookedRabbitMeatItem = EnsureCookedRabbitMeatItem();
            CCS_ItemDefinition cookedVenisonItem = EnsureCookedVenisonItem();
            CCS_ItemDefinition cookedMeatItem = EnsureCookedMeatItem();
            CCS_ItemDefinition stickItem = LoadRequiredItem(StickItemPath, "Stick");
            CCS_ItemDefinition woodItem = LoadRequiredItem(WoodItemPath, "Wood");
            CCS_ItemDefinition basicFoodItem = LoadRequiredItem(BasicFoodItemPath, "Basic Food");
            CCS_ItemDefinition campfireKitItem = LoadRequiredItem(CampfireKitItemPath, "Campfire Kit");

            EnsureInventoryCatalog(
                rawMeatItem,
                rawRabbitMeatItem,
                rawVenisonItem,
                cookedMeatItem,
                cookedRabbitMeatItem,
                cookedVenisonItem,
                stickItem,
                woodItem);
            UpdateWildlifeHarvestDrops(rawRabbitMeatItem, rawVenisonItem);

            CCS_CampfireDefinition campfireDefinition = EnsureCampfireDefinition();
            CCS_BuildingPieceDefinition campfireBuildingPiece = EnsureCampfireBuildingPiece(campfireKitItem);
            EnsureBuildingProfileIncludesCampfire(campfireBuildingPiece);

            CCS_CookingProfile profile = EnsureDefaultProfile(
                campfireDefinition,
                campfireBuildingPiece,
                rawMeatItem,
                cookedMeatItem,
                rawRabbitMeatItem,
                rawVenisonItem,
                cookedRabbitMeatItem,
                cookedVenisonItem,
                stickItem,
                woodItem,
                basicFoodItem);

            EnsureBootstrapPrefabProfile(profile);
            EnsurePlayerPrefabDrivers();
            EnsureBootstrapTestArea(profile, campfireDefinition);

            AssetDatabase.SaveAssets();
            Debug.Log($"{LogPrefix} Cooking bootstrap setup complete.");
            EditorApplication.Exit(0);
        }

        #endregion

        #region Private Methods

        private static void EnsureFolders()
        {
            EnsureFolder("Assets/CCS/Survival/Profiles");
            EnsureFolder(ProfilesRoot);
            EnsureFolder("Assets/CCS/Survival/Content/Cooking");
            EnsureFolder(CookingDefinitionsRoot);
            EnsureFolder(FoodItemsRoot);
        }

        private static CCS_CookingProfile EnsureDefaultProfile(
            CCS_CampfireDefinition campfireDefinition,
            CCS_BuildingPieceDefinition campfireBuildingPiece,
            CCS_ItemDefinition rawMeatItem,
            CCS_ItemDefinition cookedMeatItem,
            CCS_ItemDefinition rawRabbitMeatItem,
            CCS_ItemDefinition rawVenisonItem,
            CCS_ItemDefinition cookedRabbitMeatItem,
            CCS_ItemDefinition cookedVenisonItem,
            CCS_ItemDefinition stickItem,
            CCS_ItemDefinition woodItem,
            CCS_ItemDefinition basicFoodItem)
        {
            CCS_CookingProfile profile = AssetDatabase.LoadAssetAtPath<CCS_CookingProfile>(DefaultProfilePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<CCS_CookingProfile>();
                AssetDatabase.CreateAsset(profile, DefaultProfilePath);
            }

            SerializedObject serializedProfile = new SerializedObject(profile);
            serializedProfile.FindProperty("profileDisplayName").stringValue = "Default Cooking";
            serializedProfile.FindProperty("profileId").stringValue = "ccs.survival.profile.cooking.default";
            serializedProfile.FindProperty("profileDescription").stringValue =
                "Campfire cooking with fuel, rabbit and venison recipes for 1.0.0 foundation.";
            serializedProfile.FindProperty("profileVersion").stringValue = "1.0.0";
            serializedProfile.FindProperty("defaultInteractDistance").floatValue = 3f;
            serializedProfile.FindProperty("defaultCookDurationSeconds").floatValue = 5f;
            serializedProfile.FindProperty("defaultFuelBurnDurationSeconds").floatValue = 30f;
            serializedProfile.FindProperty("enableCooking").boolValue = true;
            serializedProfile.FindProperty("autoLightCampfiresOnPlacement").boolValue = true;
            serializedProfile.FindProperty("defaultCampfireDefinition").objectReferenceValue = campfireDefinition;
            serializedProfile.FindProperty("campfireBuildingPiece").objectReferenceValue = campfireBuildingPiece;
            serializedProfile.FindProperty("rawMeatItemDefinition").objectReferenceValue = rawMeatItem;
            serializedProfile.FindProperty("cookedMeatItemDefinition").objectReferenceValue = cookedMeatItem;

            SerializedProperty catalog = serializedProfile.FindProperty("recipeItemCatalog");
            catalog.arraySize = 8;
            catalog.GetArrayElementAtIndex(0).objectReferenceValue = rawRabbitMeatItem;
            catalog.GetArrayElementAtIndex(1).objectReferenceValue = rawVenisonItem;
            catalog.GetArrayElementAtIndex(2).objectReferenceValue = cookedRabbitMeatItem;
            catalog.GetArrayElementAtIndex(3).objectReferenceValue = cookedVenisonItem;
            catalog.GetArrayElementAtIndex(4).objectReferenceValue = rawMeatItem;
            catalog.GetArrayElementAtIndex(5).objectReferenceValue = cookedMeatItem;
            catalog.GetArrayElementAtIndex(6).objectReferenceValue = stickItem;
            catalog.GetArrayElementAtIndex(7).objectReferenceValue = woodItem;

            ApplyCookingRecipes(serializedProfile.FindProperty("recipes"), stickItem, woodItem);

            SerializedProperty consumableDefinitions =
                serializedProfile.FindProperty("consumableFoodDefinitions");
            consumableDefinitions.arraySize = 6;
            SetConsumableDefinition(consumableDefinitions.GetArrayElementAtIndex(0), cookedRabbitMeatItem, 35f, "Cooked Rabbit Meat");
            SetConsumableDefinition(consumableDefinitions.GetArrayElementAtIndex(1), cookedVenisonItem, 50f, "Cooked Venison");
            SetConsumableDefinition(consumableDefinitions.GetArrayElementAtIndex(2), cookedMeatItem, 40f, "Cooked Meat");
            SetConsumableDefinition(consumableDefinitions.GetArrayElementAtIndex(3), rawRabbitMeatItem, 8f, "Raw Rabbit Meat");
            SetConsumableDefinition(consumableDefinitions.GetArrayElementAtIndex(4), rawVenisonItem, 12f, "Raw Venison");
            SetConsumableDefinition(consumableDefinitions.GetArrayElementAtIndex(5), basicFoodItem, 15f, "Basic Food");

            serializedProfile.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
            profile.BuildRecipeLookup();
            return profile;
        }

        private static void ApplyCookingRecipes(SerializedProperty recipesProperty, CCS_ItemDefinition stickItem, CCS_ItemDefinition woodItem)
        {
            recipesProperty.arraySize = 2;

            ApplyRecipe(
                recipesProperty.GetArrayElementAtIndex(0),
                "ccs.survival.cooking.recipe.cookrabbit",
                "Cook Rabbit Meat",
                "ccs.survival.item.resource.rawrabbitmeat",
                "ccs.survival.item.food.cookedrabbitmeat",
                5f,
                stickItem,
                woodItem);

            ApplyRecipe(
                recipesProperty.GetArrayElementAtIndex(1),
                "ccs.survival.cooking.recipe.cookvenison",
                "Cook Venison",
                "ccs.survival.item.resource.rawvenison",
                "ccs.survival.item.food.cookedvenison",
                7f,
                stickItem,
                woodItem);
        }

        private static void ApplyRecipe(
            SerializedProperty recipeProperty,
            string recipeId,
            string displayName,
            string rawItemId,
            string cookedItemId,
            float cookDuration,
            CCS_ItemDefinition stickItem,
            CCS_ItemDefinition woodItem)
        {
            recipeProperty.FindPropertyRelative("recipeId").stringValue = recipeId;
            recipeProperty.FindPropertyRelative("displayName").stringValue = displayName;
            recipeProperty.FindPropertyRelative("rawItemDefinitionId").stringValue = rawItemId;
            recipeProperty.FindPropertyRelative("cookedItemDefinitionId").stringValue = cookedItemId;
            recipeProperty.FindPropertyRelative("rawAmount").intValue = 1;
            recipeProperty.FindPropertyRelative("cookedAmount").intValue = 1;
            recipeProperty.FindPropertyRelative("cookDurationSeconds").floatValue = cookDuration;
            recipeProperty.FindPropertyRelative("requiredFuelAmount").intValue = 1;

            SerializedProperty fuelIds = recipeProperty.FindPropertyRelative("acceptedFuelItemIds");
            fuelIds.arraySize = 2;
            fuelIds.GetArrayElementAtIndex(0).stringValue = stickItem.ItemId;
            fuelIds.GetArrayElementAtIndex(1).stringValue = woodItem.ItemId;
        }

        private static void SetConsumableDefinition(
            SerializedProperty entry,
            CCS_ItemDefinition itemDefinition,
            float hungerRestoreAmount,
            string notificationDisplayName)
        {
            entry.FindPropertyRelative("itemDefinition").objectReferenceValue = itemDefinition;
            entry.FindPropertyRelative("hungerRestoreAmount").floatValue = hungerRestoreAmount;
            entry.FindPropertyRelative("consumeCooldownSeconds").floatValue = 0f;
            entry.FindPropertyRelative("notificationDisplayName").stringValue = notificationDisplayName;
        }

        private static CCS_CampfireDefinition EnsureCampfireDefinition()
        {
            string assetPath = CookingDefinitionsRoot + "/CCS_TestCampfireDefinition.asset";
            CCS_CampfireDefinition definition = AssetDatabase.LoadAssetAtPath<CCS_CampfireDefinition>(assetPath);
            if (definition == null)
            {
                definition = ScriptableObject.CreateInstance<CCS_CampfireDefinition>();
                AssetDatabase.CreateAsset(definition, assetPath);
            }

            SerializedObject serializedDefinition = new SerializedObject(definition);
            serializedDefinition.FindProperty("campfireId").stringValue = "ccs.survival.cooking.campfire.test";
            serializedDefinition.FindProperty("displayName").stringValue = "Test Campfire";
            serializedDefinition.FindProperty("cookTimeSeconds").floatValue = 5f;
            serializedDefinition.FindProperty("isLitOnPlacement").boolValue = false;
            serializedDefinition.FindProperty("maxQueueCount").intValue = 1;
            serializedDefinition.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(definition);
            return definition;
        }

        private static CCS_ItemDefinition EnsureWildlifeMeatItem(
            string assetPath,
            string itemId,
            string displayName,
            string description)
        {
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
            serializedItem.FindProperty("category").enumValueIndex = (int)CCS_ItemCategory.Resource;
            serializedItem.FindProperty("maxStackSize").intValue = 20;
            serializedItem.FindProperty("isStackable").boolValue = true;
            serializedItem.FindProperty("weight").floatValue = 0.35f;
            serializedItem.FindProperty("hasToolIdentity").boolValue = false;
            serializedItem.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(itemDefinition);
            return itemDefinition;
        }

        private static CCS_ItemDefinition EnsureCookedFoodItem(
            string assetPath,
            string itemId,
            string displayName,
            string description)
        {
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
            serializedItem.FindProperty("category").enumValueIndex = (int)CCS_ItemCategory.Consumable;
            serializedItem.FindProperty("maxStackSize").intValue = 20;
            serializedItem.FindProperty("isStackable").boolValue = true;
            serializedItem.FindProperty("weight").floatValue = 0.3f;
            serializedItem.FindProperty("hasToolIdentity").boolValue = false;
            serializedItem.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(itemDefinition);
            return itemDefinition;
        }

        private static CCS_ItemDefinition EnsureRawRabbitMeatItem()
        {
            return EnsureWildlifeMeatItem(
                RawRabbitMeatItemPath,
                "ccs.survival.item.resource.rawrabbitmeat",
                "Raw Rabbit Meat",
                "Uncooked rabbit meat. Cook on a campfire before eating.");
        }

        private static CCS_ItemDefinition EnsureRawVenisonItem()
        {
            return EnsureWildlifeMeatItem(
                RawVenisonItemPath,
                "ccs.survival.item.resource.rawvenison",
                "Raw Venison",
                "Uncooked deer meat. Cook on a campfire before eating.");
        }

        private static CCS_ItemDefinition EnsureCookedRabbitMeatItem()
        {
            return EnsureCookedFoodItem(
                CookedRabbitMeatItemPath,
                "ccs.survival.item.food.cookedrabbitmeat",
                "Cooked Rabbit Meat",
                "Cooked rabbit meat that restores more hunger than raw rabbit meat.");
        }

        private static CCS_ItemDefinition EnsureCookedVenisonItem()
        {
            return EnsureCookedFoodItem(
                CookedVenisonItemPath,
                "ccs.survival.item.food.cookedvenison",
                "Cooked Venison",
                "Cooked venison that restores more hunger than raw venison.");
        }

        private static CCS_ItemDefinition EnsureCookedMeatItem()
        {
            string assetPath = FoodItemsRoot + "/CCS_Item_CookedMeat.asset";
            CCS_ItemDefinition itemDefinition = AssetDatabase.LoadAssetAtPath<CCS_ItemDefinition>(assetPath);
            if (itemDefinition == null)
            {
                itemDefinition = ScriptableObject.CreateInstance<CCS_ItemDefinition>();
                AssetDatabase.CreateAsset(itemDefinition, assetPath);
            }

            SerializedObject serializedItem = new SerializedObject(itemDefinition);
            serializedItem.FindProperty("itemId").stringValue = "ccs.survival.item.food.cookedmeat";
            serializedItem.FindProperty("displayName").stringValue = "Cooked Meat";
            serializedItem.FindProperty("description").stringValue =
                "Cooked meat food item that restores hunger when consumed.";
            serializedItem.FindProperty("category").enumValueIndex = (int)CCS_ItemCategory.Consumable;
            serializedItem.FindProperty("maxStackSize").intValue = 20;
            serializedItem.FindProperty("isStackable").boolValue = true;
            serializedItem.FindProperty("weight").floatValue = 0.3f;
            serializedItem.FindProperty("hasToolIdentity").boolValue = false;
            serializedItem.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(itemDefinition);
            return itemDefinition;
        }

        private static CCS_BuildingPieceDefinition EnsureCampfireBuildingPiece(CCS_ItemDefinition campfireKitItem)
        {
            string assetPath = BuildingDefinitionsRoot + "/CCS_TestCampfire.asset";
            CCS_BuildingPieceDefinition definition =
                AssetDatabase.LoadAssetAtPath<CCS_BuildingPieceDefinition>(assetPath);

            if (definition == null)
            {
                definition = ScriptableObject.CreateInstance<CCS_BuildingPieceDefinition>();
                AssetDatabase.CreateAsset(definition, assetPath);
            }

            SerializedObject serializedDefinition = new SerializedObject(definition);
            serializedDefinition.FindProperty("pieceId").stringValue = "ccs.survival.building.campfire.test";
            serializedDefinition.FindProperty("displayName").stringValue = "Campfire";
            serializedDefinition.FindProperty("description").stringValue =
                "Camp structure placed by consuming one campfire kit.";
            serializedDefinition.FindProperty("buildingPieceType").enumValueIndex =
                (int)CCS_BuildingPieceType.CampStructure;
            serializedDefinition.FindProperty("allowsFreePlacement").boolValue = true;
            serializedDefinition.FindProperty("requiresSnapPoint").boolValue = false;

            SerializedProperty costList = serializedDefinition.FindProperty("buildCostEntries");
            costList.arraySize = 1;
            SerializedProperty costEntry = costList.GetArrayElementAtIndex(0);
            costEntry.FindPropertyRelative("itemDefinition").objectReferenceValue = campfireKitItem;
            costEntry.FindPropertyRelative("quantity").intValue = 1;

            serializedDefinition.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(definition);
            return definition;
        }

        private static void EnsureBuildingProfileIncludesCampfire(CCS_BuildingPieceDefinition campfirePiece)
        {
            CCS_BuildingProfile buildingProfile =
                AssetDatabase.LoadAssetAtPath<CCS_BuildingProfile>(DefaultBuildingProfilePath);

            if (buildingProfile == null)
            {
                Debug.LogError($"{LogPrefix} Missing building profile: {DefaultBuildingProfilePath}");
                EditorApplication.Exit(1);
                return;
            }

            SerializedObject serializedProfile = new SerializedObject(buildingProfile);
            SerializedProperty startupDefinitions = serializedProfile.FindProperty("startupDefinitions");
            List<Object> definitions = new List<Object>();
            for (int index = 0; index < startupDefinitions.arraySize; index++)
            {
                Object existingDefinition = startupDefinitions.GetArrayElementAtIndex(index).objectReferenceValue;
                if (existingDefinition != null && existingDefinition != campfirePiece)
                {
                    definitions.Add(existingDefinition);
                }
            }

            definitions.Add(campfirePiece);
            startupDefinitions.arraySize = definitions.Count;
            for (int index = 0; index < definitions.Count; index++)
            {
                startupDefinitions.GetArrayElementAtIndex(index).objectReferenceValue = definitions[index];
            }

            serializedProfile.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(buildingProfile);
        }

        private static CCS_ItemDefinition LoadRequiredItem(string assetPath, string displayName)
        {
            CCS_ItemDefinition itemDefinition = AssetDatabase.LoadAssetAtPath<CCS_ItemDefinition>(assetPath);
            if (itemDefinition == null)
            {
                Debug.LogError($"{LogPrefix} Missing required item asset '{displayName}': {assetPath}");
                EditorApplication.Exit(1);
            }

            return itemDefinition;
        }

        private static void EnsureBootstrapPrefabProfile(CCS_CookingProfile profile)
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
            serializedHost.FindProperty("cookingProfile").objectReferenceValue = profile;
            serializedHost.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(prefabRoot);
        }

        private static void EnsurePlayerPrefabDrivers()
        {
            GameObject prefabRoot = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath);
            if (prefabRoot == null)
            {
                Debug.LogError($"{LogPrefix} Missing player prefab: {PlayerPrefabPath}");
                EditorApplication.Exit(1);
                return;
            }

            string prefabPath = AssetDatabase.GetAssetPath(prefabRoot);
            GameObject prefabContents = PrefabUtility.LoadPrefabContents(prefabPath);

            if (prefabContents.GetComponent<CCS_ConsumableFoodPlayerDriver>() == null)
            {
                prefabContents.AddComponent<CCS_ConsumableFoodPlayerDriver>();
            }

            if (prefabContents.GetComponent<CCS_CampfireBuildingPlayerDriver>() == null)
            {
                prefabContents.AddComponent<CCS_CampfireBuildingPlayerDriver>();
            }

            PrefabUtility.SaveAsPrefabAsset(prefabContents, prefabPath);
            PrefabUtility.UnloadPrefabContents(prefabContents);
        }

        private static void EnsureBootstrapTestArea(
            CCS_CookingProfile profile,
            CCS_CampfireDefinition campfireDefinition)
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
                testAreaObject.transform.localPosition = new Vector3(6f, 0f, 4f);
                testArea = testAreaObject.transform;
            }

            EnsurePlacementPoint(testArea, new Vector3(1.5f, 0f, 0f));
            EnsureTestCampfire(testArea, profile, campfireDefinition, new Vector3(-1f, 0.15f, 0f));

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static void EnsurePlacementPoint(Transform parent, Vector3 localPosition)
        {
            Transform existing = parent.Find(PlacementPointObjectName);
            if (existing == null)
            {
                GameObject placementPoint = new GameObject(PlacementPointObjectName);
                placementPoint.transform.SetParent(parent, false);
                existing = placementPoint.transform;
            }

            existing.localPosition = localPosition;
        }

        private static void EnsureTestCampfire(
            Transform parent,
            CCS_CookingProfile profile,
            CCS_CampfireDefinition campfireDefinition,
            Vector3 localPosition)
        {
            Transform existing = parent.Find(TestCampfireObjectName);
            GameObject campfireObject;

            if (existing != null)
            {
                campfireObject = existing.gameObject;
            }
            else
            {
                campfireObject = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                campfireObject.name = TestCampfireObjectName;
                campfireObject.transform.SetParent(parent, false);
            }

            campfireObject.transform.localPosition = localPosition;
            campfireObject.transform.localScale = new Vector3(0.8f, 0.25f, 0.8f);

            Collider collider = campfireObject.GetComponent<Collider>();
            if (collider != null)
            {
                collider.isTrigger = false;
                collider.enabled = true;
            }

            EnsureCampfireVisualChildren(campfireObject.transform);

            CCS_CookingStation cookingStation = campfireObject.GetComponent<CCS_CookingStation>();
            if (cookingStation == null)
            {
                cookingStation = campfireObject.AddComponent<CCS_CookingStation>();
            }

            cookingStation.ConfigureFromProfile(profile, startActive: true);

            CCS_CookingInteractable cookingInteractable = campfireObject.GetComponent<CCS_CookingInteractable>();
            if (cookingInteractable == null)
            {
                cookingInteractable = campfireObject.AddComponent<CCS_CookingInteractable>();
            }

            cookingInteractable.ConfigureRuntime(profile, cookingStation, startActive: true);
            EditorUtility.SetDirty(campfireObject);
        }

        private static void EnsureCampfireVisualChildren(Transform campfireRoot)
        {
            EnsureChildPrimitive(
                campfireRoot,
                "CCS_CampfireLogs",
                PrimitiveType.Cylinder,
                new Vector3(0.35f, 0.08f, 0f),
                new Vector3(0.2f, 0.08f, 0.2f),
                new Vector3(0f, 0f, 35f));
            EnsureChildPrimitive(
                campfireRoot,
                "CCS_CampfireFlame",
                PrimitiveType.Sphere,
                new Vector3(0f, 0.35f, 0f),
                new Vector3(0.25f, 0.35f, 0.25f),
                Vector3.zero);
        }

        private static void EnsureChildPrimitive(
            Transform parent,
            string objectName,
            PrimitiveType primitiveType,
            Vector3 localPosition,
            Vector3 localScale,
            Vector3 localEulerAngles)
        {
            Transform existing = parent.Find(objectName);
            GameObject childObject = existing != null ? existing.gameObject : GameObject.CreatePrimitive(primitiveType);
            childObject.name = objectName;
            childObject.transform.SetParent(parent, false);
            childObject.transform.localPosition = localPosition;
            childObject.transform.localScale = localScale;
            childObject.transform.localEulerAngles = localEulerAngles;

            Collider childCollider = childObject.GetComponent<Collider>();
            if (childCollider != null)
            {
                Object.DestroyImmediate(childCollider);
            }
        }

        private static void EnsureInventoryCatalog(params CCS_ItemDefinition[] items)
        {
            CCS_InventoryProfile inventoryProfile = AssetDatabase.LoadAssetAtPath<CCS_InventoryProfile>(InventoryProfilePath);
            if (inventoryProfile == null)
            {
                Debug.LogError($"{LogPrefix} Missing inventory profile: {InventoryProfilePath}");
                EditorApplication.Exit(1);
                return;
            }

            SerializedObject serializedProfile = new SerializedObject(inventoryProfile);
            SerializedProperty catalog = serializedProfile.FindProperty("saveRestoreItemDefinitions");
            for (int itemIndex = 0; itemIndex < items.Length; itemIndex++)
            {
                AddCatalogEntryIfMissing(catalog, items[itemIndex]);
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

        private static void UpdateWildlifeHarvestDrops(CCS_ItemDefinition rabbitMeat, CCS_ItemDefinition venisonMeat)
        {
            UpdateWildlifeDefinitionFirstMeatDrop(RabbitWildlifeDefinitionPath, rabbitMeat);
            UpdateWildlifeDefinitionFirstMeatDrop(DeerWildlifeDefinitionPath, venisonMeat);
        }

        private static void UpdateWildlifeDefinitionFirstMeatDrop(string definitionPath, CCS_ItemDefinition meatItem)
        {
            CCS_WildlifeDefinition wildlifeDefinition =
                AssetDatabase.LoadAssetAtPath<CCS_WildlifeDefinition>(definitionPath);
            if (wildlifeDefinition == null || meatItem == null)
            {
                return;
            }

            SerializedObject serializedDefinition = new SerializedObject(wildlifeDefinition);
            SerializedProperty harvestDrops = serializedDefinition.FindProperty("harvestDrops");
            if (harvestDrops.arraySize == 0)
            {
                harvestDrops.InsertArrayElementAtIndex(0);
            }

            harvestDrops.GetArrayElementAtIndex(0).FindPropertyRelative("itemDefinition").objectReferenceValue = meatItem;
            serializedDefinition.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(wildlifeDefinition);
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
