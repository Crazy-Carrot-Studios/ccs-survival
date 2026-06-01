using System.IO;
using CCS.Modules.Crafting;
using CCS.Modules.Equipment;
using CCS.Modules.Inventory;
using CCS.Modules.Sleep;
using CCS.Survival.Composition;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// =============================================================================
// SCRIPT: CCS_SleepBootstrapSetup
// CATEGORY: Modules / Sleep / Editor / Validation
// PURPOSE: Creates sleep profile, bedroll content, recipe, and bootstrap test area.
// PLACEMENT: Batch entry for 0.9.6 sleep and bedroll foundation milestone.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: No dreams, death, enemy interruption, or sleep UI in 0.9.6 foundation.
// =============================================================================

namespace CCS.Modules.Sleep.Editor
{
    public static class CCS_SleepBootstrapSetup
    {
        private const string ProfilesRoot = "Assets/CCS/Survival/Profiles/Sleep";
        private const string DefaultProfilePath = ProfilesRoot + "/CCS_DefaultSleepProfile.asset";
        private const string PrimitiveRecipesRoot = "Assets/CCS/Survival/Profiles/Crafting/PrimitiveRecipes";
        private const string BedrollRecipePath = PrimitiveRecipesRoot + "/CCS_Recipe_Bedroll.asset";
        private const string BedrollItemPath = "Assets/CCS/Survival/Content/Items/Starter/CCS_Item_Bedroll.asset";
        private const string FiberItemPath = "Assets/CCS/Survival/Content/Items/Resources/Primitive/CCS_Item_Fiber.asset";
        private const string HideItemPath = "Assets/CCS/Survival/Content/Items/Resources/Primitive/CCS_Item_Hide.asset";
        private const string BedrollEquipmentPath =
            "Assets/CCS/Survival/Content/Equipment/Primitive/CCS_Equipment_Bedroll.asset";
        private const string BootstrapPrefabPath = "Assets/CCS/Survival/Prefabs/PF_CCS_Survival_BootstrapRoot.prefab";
        private const string BootstrapScenePath = "Assets/CCS/Survival/Scenes/SCN_CCS_Survival_Bootstrap.unity";
        private const string TestAreaObjectName = "CCS_SleepTestArea";
        private const string TestRestPointObjectName = "CCS_TestBedrollRestPoint";
        private const string HarnessObjectName = "CCS_SleepTestHarness";
        private const string LogPrefix = "[CCS_SleepBootstrapSetup]";

        #region Public Methods

        public static void ExecuteBatch()
        {
            EnsureFolders();

            CCS_ItemDefinition hideItem = LoadRequiredItem(HideItemPath, "Hide");
            CCS_ItemDefinition fiberItem = EnsureFiberItem();
            CCS_ItemDefinition bedrollItem = EnsureBedrollItem();
            CCS_EquipmentItemDefinition bedrollEquipment = EnsureBedrollEquipment(bedrollItem);
            CCS_CraftingRecipeDefinition bedrollRecipe = EnsureBedrollRecipe(hideItem, fiberItem, bedrollItem);
            CCS_SleepProfile profile = EnsureDefaultProfile(bedrollItem, bedrollEquipment, bedrollRecipe);

            EnsureBootstrapPrefabProfile(profile);
            EnsureBootstrapTestArea(profile, bedrollItem);

            AssetDatabase.SaveAssets();
            Debug.Log($"{LogPrefix} Sleep bootstrap setup complete.");
            EditorApplication.Exit(0);
        }

        #endregion

        #region Private Methods

        private static void EnsureFolders()
        {
            EnsureFolder("Assets/CCS/Survival/Profiles");
            EnsureFolder(ProfilesRoot);
            EnsureFolder("Assets/CCS/Survival/Content/Items/Resources/Primitive");
            EnsureFolder("Assets/CCS/Survival/Content/Equipment/Primitive");
            EnsureFolder(PrimitiveRecipesRoot);
        }

        private static CCS_ItemDefinition EnsureFiberItem()
        {
            CCS_ItemDefinition itemDefinition = AssetDatabase.LoadAssetAtPath<CCS_ItemDefinition>(FiberItemPath);
            if (itemDefinition == null)
            {
                itemDefinition = ScriptableObject.CreateInstance<CCS_ItemDefinition>();
                AssetDatabase.CreateAsset(itemDefinition, FiberItemPath);
            }

            SerializedObject serializedItem = new SerializedObject(itemDefinition);
            serializedItem.FindProperty("itemId").stringValue = "ccs.survival.item.resource.fiber";
            serializedItem.FindProperty("displayName").stringValue = "Fiber";
            serializedItem.FindProperty("description").stringValue =
                "Plant fiber used for primitive crafting such as bedrolls.";
            serializedItem.FindProperty("category").enumValueIndex = (int)CCS_ItemCategory.Resource;
            serializedItem.FindProperty("maxStackSize").intValue = 50;
            serializedItem.FindProperty("isStackable").boolValue = true;
            serializedItem.FindProperty("weight").floatValue = 0.05f;
            serializedItem.FindProperty("hasToolIdentity").boolValue = false;
            serializedItem.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(itemDefinition);
            return itemDefinition;
        }

        private static CCS_ItemDefinition EnsureBedrollItem()
        {
            CCS_ItemDefinition itemDefinition = AssetDatabase.LoadAssetAtPath<CCS_ItemDefinition>(BedrollItemPath);
            if (itemDefinition == null)
            {
                itemDefinition = ScriptableObject.CreateInstance<CCS_ItemDefinition>();
                AssetDatabase.CreateAsset(itemDefinition, BedrollItemPath);
            }

            SerializedObject serializedItem = new SerializedObject(itemDefinition);
            serializedItem.FindProperty("itemId").stringValue = "ccs.survival.item.starter.bedroll";
            serializedItem.FindProperty("displayName").stringValue = "Bedroll";
            serializedItem.FindProperty("description").stringValue =
                "Portable bedroll required for resting and sleep.";
            serializedItem.FindProperty("category").enumValueIndex = (int)CCS_ItemCategory.Consumable;
            serializedItem.FindProperty("maxStackSize").intValue = 1;
            serializedItem.FindProperty("isStackable").boolValue = false;
            serializedItem.FindProperty("weight").floatValue = 2f;
            serializedItem.FindProperty("hasToolIdentity").boolValue = false;
            serializedItem.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(itemDefinition);
            return itemDefinition;
        }

        private static CCS_EquipmentItemDefinition EnsureBedrollEquipment(CCS_ItemDefinition bedrollItem)
        {
            CCS_EquipmentItemDefinition equipmentDefinition =
                AssetDatabase.LoadAssetAtPath<CCS_EquipmentItemDefinition>(BedrollEquipmentPath);
            if (equipmentDefinition == null)
            {
                equipmentDefinition = ScriptableObject.CreateInstance<CCS_EquipmentItemDefinition>();
                AssetDatabase.CreateAsset(equipmentDefinition, BedrollEquipmentPath);
            }

            SerializedObject serializedEquipment = new SerializedObject(equipmentDefinition);
            serializedEquipment.FindProperty("itemDefinition").objectReferenceValue = bedrollItem;
            serializedEquipment.FindProperty("allowedSlot").enumValueIndex = (int)CCS_EquipmentSlotType.Bedroll;
            serializedEquipment.FindProperty("durabilityEnabled").boolValue = false;
            serializedEquipment.FindProperty("modifiesInventoryCapacity").boolValue = false;
            serializedEquipment.FindProperty("modifiesCarryWeight").boolValue = false;
            serializedEquipment.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(equipmentDefinition);
            return equipmentDefinition;
        }

        private static CCS_CraftingRecipeDefinition EnsureBedrollRecipe(
            CCS_ItemDefinition hideItem,
            CCS_ItemDefinition fiberItem,
            CCS_ItemDefinition bedrollItem)
        {
            CCS_CraftingRecipeDefinition recipe =
                AssetDatabase.LoadAssetAtPath<CCS_CraftingRecipeDefinition>(BedrollRecipePath);
            if (recipe == null)
            {
                recipe = ScriptableObject.CreateInstance<CCS_CraftingRecipeDefinition>();
                AssetDatabase.CreateAsset(recipe, BedrollRecipePath);
            }

            SerializedObject serializedRecipe = new SerializedObject(recipe);
            serializedRecipe.FindProperty("recipeId").stringValue = "ccs.survival.recipe.primitive.bedroll";
            serializedRecipe.FindProperty("displayName").stringValue = "Bedroll";
            serializedRecipe.FindProperty("description").stringValue =
                "Primitive hand recipe for crafting a portable bedroll.";
            serializedRecipe.FindProperty("requiredStationType").enumValueIndex = (int)CCS_CraftingStationType.Hand;
            serializedRecipe.FindProperty("craftTimeSeconds").floatValue = 0f;
            serializedRecipe.FindProperty("isUnlockedByDefault").boolValue = true;

            SerializedProperty ingredients = serializedRecipe.FindProperty("ingredients");
            ingredients.arraySize = 2;
            SetIngredient(ingredients.GetArrayElementAtIndex(0), hideItem, 2);
            SetIngredient(ingredients.GetArrayElementAtIndex(1), fiberItem, 4);

            SerializedProperty results = serializedRecipe.FindProperty("results");
            results.arraySize = 1;
            SetResult(results.GetArrayElementAtIndex(0), bedrollItem, 1);

            serializedRecipe.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(recipe);
            return recipe;
        }

        private static CCS_SleepProfile EnsureDefaultProfile(
            CCS_ItemDefinition bedrollItem,
            CCS_EquipmentItemDefinition bedrollEquipment,
            CCS_CraftingRecipeDefinition bedrollRecipe)
        {
            CCS_SleepProfile profile = AssetDatabase.LoadAssetAtPath<CCS_SleepProfile>(DefaultProfilePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<CCS_SleepProfile>();
                AssetDatabase.CreateAsset(profile, DefaultProfilePath);
            }

            SerializedObject serializedProfile = new SerializedObject(profile);
            serializedProfile.FindProperty("profileDisplayName").stringValue = "Default Sleep";
            serializedProfile.FindProperty("profileId").stringValue = "ccs.survival.profile.sleep.default";
            serializedProfile.FindProperty("profileDescription").stringValue =
                "Default sleep and bedroll rules for 0.9.6 foundation.";
            serializedProfile.FindProperty("profileVersion").stringValue = "0.9.6";
            serializedProfile.FindProperty("defaultSleepHours").floatValue = 6f;
            serializedProfile.FindProperty("minimumSleepHours").floatValue = 1f;
            serializedProfile.FindProperty("maximumSleepHours").floatValue = 10f;
            serializedProfile.FindProperty("fatigueRestorePerHour").floatValue = 12f;
            serializedProfile.FindProperty("requireBedroll").boolValue = true;
            serializedProfile.FindProperty("requireShelter").boolValue = false;
            serializedProfile.FindProperty("unshelteredFatigueRestoreMultiplier").floatValue = 0.5f;
            serializedProfile.FindProperty("hungerDrainDuringSleepMultiplier").floatValue = 1f;
            serializedProfile.FindProperty("thirstDrainDuringSleepMultiplier").floatValue = 1f;
            serializedProfile.FindProperty("bedrollItemDefinition").objectReferenceValue = bedrollItem;
            serializedProfile.FindProperty("bedrollEquipmentDefinition").objectReferenceValue = bedrollEquipment;
            serializedProfile.FindProperty("bedrollRecipeDefinition").objectReferenceValue = bedrollRecipe;
            serializedProfile.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
            return profile;
        }

        private static void EnsureBootstrapPrefabProfile(CCS_SleepProfile profile)
        {
            GameObject prefabRoot = AssetDatabase.LoadAssetAtPath<GameObject>(BootstrapPrefabPath);
            if (prefabRoot == null)
            {
                Debug.LogError($"{LogPrefix} Missing bootstrap prefab: {BootstrapPrefabPath}");
                EditorApplication.Exit(1);
                return;
            }

            CCS_SurvivalGameplayServiceHost serviceHost = prefabRoot.GetComponent<CCS_SurvivalGameplayServiceHost>();
            if (serviceHost == null)
            {
                Debug.LogError($"{LogPrefix} Bootstrap prefab is missing CCS_SurvivalGameplayServiceHost.");
                EditorApplication.Exit(1);
                return;
            }

            SerializedObject serializedHost = new SerializedObject(serviceHost);
            serializedHost.FindProperty("sleepProfile").objectReferenceValue = profile;
            serializedHost.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(prefabRoot);
        }

        private static void EnsureBootstrapTestArea(CCS_SleepProfile profile, CCS_ItemDefinition bedrollItem)
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
                testAreaObject.transform.localPosition = new Vector3(-6f, 0f, 8f);
                testArea = testAreaObject.transform;
            }

            EnsureRestPoint(testArea, profile, new Vector3(0f, 0.1f, 0f));
            EnsureSleepHarness(testArea, bedrollItem);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static void EnsureRestPoint(Transform parent, CCS_SleepProfile profile, Vector3 localPosition)
        {
            Transform existing = parent.Find(TestRestPointObjectName);
            GameObject restObject = existing != null ? existing.gameObject : new GameObject(TestRestPointObjectName);
            if (existing == null)
            {
                restObject.transform.SetParent(parent, false);
            }

            restObject.transform.localPosition = localPosition;

            CCS_BedrollSleepInteractable interactable = restObject.GetComponent<CCS_BedrollSleepInteractable>();
            if (interactable == null)
            {
                interactable = restObject.AddComponent<CCS_BedrollSleepInteractable>();
            }

            SerializedObject serializedInteractable = new SerializedObject(interactable);
            serializedInteractable.FindProperty("sleepProfile").objectReferenceValue = profile;
            serializedInteractable.FindProperty("sleepHours").floatValue = 0f;
            serializedInteractable.FindProperty("interactionDistance").floatValue = 3f;
            serializedInteractable.FindProperty("updateShelterSubjectPosition").boolValue = true;
            serializedInteractable.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void EnsureSleepHarness(Transform parent, CCS_ItemDefinition bedrollItem)
        {
            Transform existing = parent.Find(HarnessObjectName);
            GameObject harnessObject = existing != null ? existing.gameObject : new GameObject(HarnessObjectName);
            if (existing == null)
            {
                harnessObject.transform.SetParent(parent, false);
            }

            CCS_SleepTestHarness harness = harnessObject.GetComponent<CCS_SleepTestHarness>();
            if (harness == null)
            {
                harness = harnessObject.AddComponent<CCS_SleepTestHarness>();
            }

            SerializedObject serializedHarness = new SerializedObject(harness);
            serializedHarness.FindProperty("enableHarness").boolValue = false;
            serializedHarness.FindProperty("seedBedrollItem").objectReferenceValue = bedrollItem;
            serializedHarness.FindProperty("seedBedrollQuantity").intValue = 1;
            serializedHarness.FindProperty("seedFatigueAmount").floatValue = 60f;
            serializedHarness.FindProperty("startupDelaySeconds").floatValue = 2f;
            serializedHarness.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetIngredient(SerializedProperty entry, CCS_ItemDefinition itemDefinition, int quantity)
        {
            entry.FindPropertyRelative("itemDefinition").objectReferenceValue = itemDefinition;
            entry.FindPropertyRelative("quantity").intValue = quantity;
        }

        private static void SetResult(SerializedProperty entry, CCS_ItemDefinition itemDefinition, int quantity)
        {
            entry.FindPropertyRelative("itemDefinition").objectReferenceValue = itemDefinition;
            entry.FindPropertyRelative("quantity").intValue = quantity;
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
