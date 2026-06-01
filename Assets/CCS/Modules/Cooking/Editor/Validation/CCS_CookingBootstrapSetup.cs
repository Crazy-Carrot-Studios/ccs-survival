using System.Collections.Generic;
using System.IO;
using CCS.Modules.Building;
using CCS.Modules.Cooking;
using CCS.Modules.Inventory;
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
// NOTES: No fuel systems, cooking UI, or health restore in 0.9.4 foundation.
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
            CCS_ItemDefinition basicFoodItem = LoadRequiredItem(BasicFoodItemPath, "Basic Food");
            CCS_ItemDefinition campfireKitItem = LoadRequiredItem(CampfireKitItemPath, "Campfire Kit");
            CCS_ItemDefinition cookedMeatItem = EnsureCookedMeatItem();

            CCS_CampfireDefinition campfireDefinition = EnsureCampfireDefinition();
            CCS_BuildingPieceDefinition campfireBuildingPiece = EnsureCampfireBuildingPiece(campfireKitItem);
            EnsureBuildingProfileIncludesCampfire(campfireBuildingPiece);

            CCS_CookingProfile profile = EnsureDefaultProfile(
                campfireDefinition,
                campfireBuildingPiece,
                rawMeatItem,
                cookedMeatItem,
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
                "Default campfire cooking and consumable food rules for 0.9.4 foundation.";
            serializedProfile.FindProperty("profileVersion").stringValue = "0.9.4";
            serializedProfile.FindProperty("enableCooking").boolValue = true;
            serializedProfile.FindProperty("defaultCookTimeSeconds").floatValue = 5f;
            serializedProfile.FindProperty("autoLightCampfiresOnPlacement").boolValue = true;
            serializedProfile.FindProperty("defaultCampfireDefinition").objectReferenceValue = campfireDefinition;
            serializedProfile.FindProperty("campfireBuildingPiece").objectReferenceValue = campfireBuildingPiece;
            serializedProfile.FindProperty("rawMeatItemDefinition").objectReferenceValue = rawMeatItem;
            serializedProfile.FindProperty("cookedMeatItemDefinition").objectReferenceValue = cookedMeatItem;

            SerializedProperty consumableDefinitions =
                serializedProfile.FindProperty("consumableFoodDefinitions");
            consumableDefinitions.arraySize = 2;

            SetConsumableDefinition(consumableDefinitions.GetArrayElementAtIndex(0), basicFoodItem, 15f);
            SetConsumableDefinition(consumableDefinitions.GetArrayElementAtIndex(1), cookedMeatItem, 40f);

            serializedProfile.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
            return profile;
        }

        private static void SetConsumableDefinition(
            SerializedProperty entry,
            CCS_ItemDefinition itemDefinition,
            float hungerRestoreAmount)
        {
            entry.FindPropertyRelative("itemDefinition").objectReferenceValue = itemDefinition;
            entry.FindPropertyRelative("hungerRestoreAmount").floatValue = hungerRestoreAmount;
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

            CCS_CampfireInteractable interactable = campfireObject.GetComponent<CCS_CampfireInteractable>();
            if (interactable == null)
            {
                interactable = campfireObject.AddComponent<CCS_CampfireInteractable>();
            }

            SerializedObject serializedInteractable = new SerializedObject(interactable);
            serializedInteractable.FindProperty("campfireDefinition").objectReferenceValue = campfireDefinition;
            serializedInteractable.FindProperty("cookingProfile").objectReferenceValue = profile;
            serializedInteractable.FindProperty("interactionDistance").floatValue = 3f;
            serializedInteractable.FindProperty("assumeLitOnStart").boolValue = false;
            serializedInteractable.ApplyModifiedPropertiesWithoutUndo();
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
