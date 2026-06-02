using System.Collections.Generic;
using System.IO;
using CCS.Modules.Crafting;
using CCS.Modules.Equipment;
using CCS.Modules.Inventory;
using CCS.Survival.Player.Loadout;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// =============================================================================
// SCRIPT: CCS_PrimitiveToolWeaponBootstrapSetup
// CATEGORY: Survival / Editor / Development / Bootstrap
// PURPOSE: Creates tool/weapon classifications, bone resources, recipes, and equipment wiring.
// PLACEMENT: Batch entry for 0.9.2 primitive tool and weapon foundation milestone.
// AUTHOR: James Schilz
// CREATED: 2026-05-31
// NOTES: Harnesses remain disabled by default. No combat or wildlife content.
// =============================================================================

namespace CCS.Survival.Editor.Development
{
    public static class CCS_PrimitiveToolWeaponBootstrapSetup
    {
        private const string ResourcesRoot = "Assets/CCS/Survival/Content/Items/Resources/Primitive";
        private const string ToolsPrimitiveRoot = "Assets/CCS/Survival/Content/Items/Tools/Primitive";
        private const string ToolsBoneRoot = "Assets/CCS/Survival/Content/Items/Tools/Bone";
        private const string EquipmentRoot = "Assets/CCS/Survival/Content/Equipment/Primitive";
        private const string StarterItemsRoot = "Assets/CCS/Survival/Content/Items/Starter";
        private const string BoneRecipesRoot = "Assets/CCS/Survival/Profiles/Crafting/BoneRecipes";
        private const string InventoryProfilePath = "Assets/CCS/Survival/Profiles/Inventory/CCS_DefaultInventoryProfile.asset";
        private const string EquipmentProfilePath = "Assets/CCS/Survival/Profiles/Equipment/CCS_DefaultEquipmentProfile.asset";
        private const string StarterProfilePath = "Assets/CCS/Survival/Profiles/StarterLoadout/CCS_DefaultStarterLoadoutProfile.asset";
        private const string BootstrapScenePath = "Assets/CCS/Survival/Scenes/SCN_CCS_Survival_Bootstrap.unity";
        private const string LogPrefix = "[CCS_PrimitiveToolWeaponBootstrapSetup]";

        #region Public Methods

        public static void ExecuteBatch()
        {
            EnsureFolders();

            CCS_ItemDefinition bone = EnsureResourceItem(
                "CCS_Item_Bone",
                "ccs.survival.item.resource.bone",
                "Bone",
                "Primitive bone material placeholder for future wildlife drops.");

            CCS_ItemDefinition sinew = EnsureResourceItem(
                "CCS_Item_Sinew",
                "ccs.survival.item.resource.sinew",
                "Sinew",
                "Primitive sinew material placeholder for future crafting.");

            CCS_ItemDefinition hide = EnsureResourceItem(
                "CCS_Item_Hide",
                "ccs.survival.item.resource.hide",
                "Hide",
                "Primitive hide material placeholder for future wildlife drops.");

            CCS_ItemDefinition branch = AssetDatabase.LoadAssetAtPath<CCS_ItemDefinition>(
                $"{StarterItemsRoot}/CCS_Item_Branch.asset");

            CCS_ItemDefinition knife = EnsureToolItem(
                $"{StarterItemsRoot}/CCS_Item_Knife",
                "ccs.survival.item.starter.knife",
                "Knife",
                "Primitive cutting tool and melee weapon foundation.",
                CCS_ItemCategory.Tool,
                1,
                0.5f,
                CCS_ItemGameplayKind.ToolAndWeapon,
                CCS_ToolArchetype.Knife,
                CCS_ToolTier.Primitive,
                CCS_ItemToolType.Knife,
                true,
                CCS_WeaponArchetype.Knife,
                CCS_WeaponType.Melee,
                CCS_DamageType.Slash,
                CCS_RangeType.Melee);

            EnsureToolItem(
                $"{ToolsPrimitiveRoot}/CCS_Item_Hatchet",
                "ccs.survival.item.tool.hatchet.primitive",
                "Hatchet",
                "Primitive hatchet for future wood harvesting.",
                CCS_ItemCategory.Tool,
                1,
                1.2f,
                CCS_ItemGameplayKind.Tool,
                CCS_ToolArchetype.Hatchet,
                CCS_ToolTier.Primitive,
                CCS_ItemToolType.Axe,
                false,
                CCS_WeaponArchetype.None,
                CCS_WeaponType.None,
                CCS_DamageType.None,
                CCS_RangeType.None);

            EnsureToolItem(
                $"{ToolsPrimitiveRoot}/CCS_Item_Pick",
                "ccs.survival.item.tool.pick.primitive",
                "Pick",
                "Primitive pick for future stone harvesting.",
                CCS_ItemCategory.Tool,
                1,
                1.4f,
                CCS_ItemGameplayKind.Tool,
                CCS_ToolArchetype.Pick,
                CCS_ToolTier.Primitive,
                CCS_ItemToolType.Pickaxe,
                false,
                CCS_WeaponArchetype.None,
                CCS_WeaponType.None,
                CCS_DamageType.None,
                CCS_RangeType.None);

            EnsureToolItem(
                $"{ToolsPrimitiveRoot}/CCS_Item_Shovel",
                "ccs.survival.item.tool.shovel.primitive",
                "Shovel",
                "Primitive shovel for future digging.",
                CCS_ItemCategory.Tool,
                1,
                1.3f,
                CCS_ItemGameplayKind.Tool,
                CCS_ToolArchetype.Shovel,
                CCS_ToolTier.Primitive,
                CCS_ItemToolType.Shovel,
                false,
                CCS_WeaponArchetype.None,
                CCS_WeaponType.None,
                CCS_DamageType.None,
                CCS_RangeType.None);

            CCS_ItemDefinition boneKnife = EnsureToolItem(
                $"{ToolsBoneRoot}/CCS_Item_BoneKnife",
                "ccs.survival.item.tool.knife.bone",
                "Bone Knife",
                "Bone-tier knife tool foundation.",
                CCS_ItemCategory.Tool,
                1,
                0.6f,
                CCS_ItemGameplayKind.Tool,
                CCS_ToolArchetype.Knife,
                CCS_ToolTier.Bone,
                CCS_ItemToolType.Knife,
                false,
                CCS_WeaponArchetype.None,
                CCS_WeaponType.None,
                CCS_DamageType.None,
                CCS_RangeType.None);

            CCS_ItemDefinition boneHatchet = EnsureToolItem(
                $"{ToolsBoneRoot}/CCS_Item_BoneHatchet",
                "ccs.survival.item.tool.hatchet.bone",
                "Bone Hatchet",
                "Bone-tier hatchet tool foundation.",
                CCS_ItemCategory.Tool,
                1,
                1.3f,
                CCS_ItemGameplayKind.Tool,
                CCS_ToolArchetype.Hatchet,
                CCS_ToolTier.Bone,
                CCS_ItemToolType.Axe,
                false,
                CCS_WeaponArchetype.None,
                CCS_WeaponType.None,
                CCS_DamageType.None,
                CCS_RangeType.None);

            CCS_ItemDefinition bonePick = EnsureToolItem(
                $"{ToolsBoneRoot}/CCS_Item_BonePick",
                "ccs.survival.item.tool.pick.bone",
                "Bone Pick",
                "Bone-tier pick tool foundation.",
                CCS_ItemCategory.Tool,
                1,
                1.5f,
                CCS_ItemGameplayKind.Tool,
                CCS_ToolArchetype.Pick,
                CCS_ToolTier.Bone,
                CCS_ItemToolType.Pickaxe,
                false,
                CCS_WeaponArchetype.None,
                CCS_WeaponType.None,
                CCS_DamageType.None,
                CCS_RangeType.None);

            CCS_ItemDefinition boneShovel = EnsureToolItem(
                $"{ToolsBoneRoot}/CCS_Item_BoneShovel",
                "ccs.survival.item.tool.shovel.bone",
                "Bone Shovel",
                "Bone-tier shovel tool foundation.",
                CCS_ItemCategory.Tool,
                1,
                1.4f,
                CCS_ItemGameplayKind.Tool,
                CCS_ToolArchetype.Shovel,
                CCS_ToolTier.Bone,
                CCS_ItemToolType.Shovel,
                false,
                CCS_WeaponArchetype.None,
                CCS_WeaponType.None,
                CCS_DamageType.None,
                CCS_RangeType.None);

            EnsureWeaponItem(
                $"{StarterItemsRoot}/CCS_Item_Spear",
                "ccs.survival.item.starter.spear",
                "Spear",
                "Primitive spear weapon placeholder.",
                CCS_ItemCategory.Tool,
                1,
                1.5f,
                CCS_WeaponArchetype.Spear,
                CCS_WeaponType.Melee,
                CCS_DamageType.Pierce,
                CCS_RangeType.Melee);

            CCS_EquipmentItemDefinition knifeEquipment = EnsureEquipmentDefinition(
                "CCS_Equipment_Knife",
                knife,
                CCS_EquipmentSlotType.MainHand);

            CCS_EquipmentItemDefinition boneHatchetEquipment = EnsureEquipmentDefinition(
                "CCS_Equipment_BoneHatchet",
                boneHatchet,
                CCS_EquipmentSlotType.Tool);

            CCS_EquipmentItemDefinition bonePickEquipment = EnsureEquipmentDefinition(
                "CCS_Equipment_BonePick",
                bonePick,
                CCS_EquipmentSlotType.Tool);

            EnsureEquipmentDefinition("CCS_Equipment_BoneKnife", boneKnife, CCS_EquipmentSlotType.Tool);
            EnsureEquipmentDefinition("CCS_Equipment_BoneShovel", boneShovel, CCS_EquipmentSlotType.Tool);

            if (branch == null)
            {
                Debug.LogError($"{LogPrefix} Missing branch item required for bone recipes.");
                EditorApplication.Exit(1);
                return;
            }

            CCS_CraftingRecipeDefinition boneKnifeRecipe = EnsureBoneRecipe(
                "CCS_BoneKnifeRecipe",
                "ccs.survival.recipe.bone.knife",
                "Bone Knife",
                bone,
                1,
                branch,
                1,
                boneKnife,
                1);

            CCS_CraftingRecipeDefinition boneHatchetRecipe = EnsureBoneRecipe(
                "CCS_BoneHatchetRecipe",
                "ccs.survival.recipe.bone.hatchet",
                "Bone Hatchet",
                bone,
                1,
                branch,
                2,
                boneHatchet,
                1);

            CCS_CraftingRecipeDefinition bonePickRecipe = EnsureBoneRecipe(
                "CCS_BonePickRecipe",
                "ccs.survival.recipe.bone.pick",
                "Bone Pick",
                bone,
                1,
                branch,
                2,
                bonePick,
                1);

            CCS_CraftingRecipeDefinition boneShovelRecipe = EnsureBoneRecipe(
                "CCS_BoneShovelRecipe",
                "ccs.survival.recipe.bone.shovel",
                "Bone Shovel",
                bone,
                1,
                branch,
                2,
                boneShovel,
                1);

            CCS_CraftingRecipeDefinition[] boneRecipes =
            {
                boneKnifeRecipe,
                boneHatchetRecipe,
                bonePickRecipe,
                boneShovelRecipe
            };

            EnsureStarterProfileBoneRecipes(boneRecipes);
            EnsureInventorySaveRestoreCatalog(
                bone,
                sinew,
                hide,
                boneKnife,
                boneHatchet,
                bonePick,
                boneShovel,
                AssetDatabase.LoadAssetAtPath<CCS_ItemDefinition>($"{ToolsPrimitiveRoot}/CCS_Item_Hatchet.asset"),
                AssetDatabase.LoadAssetAtPath<CCS_ItemDefinition>($"{ToolsPrimitiveRoot}/CCS_Item_Pick.asset"),
                AssetDatabase.LoadAssetAtPath<CCS_ItemDefinition>($"{ToolsPrimitiveRoot}/CCS_Item_Shovel.asset"));

            EnsureEquipmentSaveRestoreCatalog(
                knifeEquipment,
                boneHatchetEquipment,
                bonePickEquipment);

            EnsurePrimitiveToolEquipHarness(
                knifeEquipment,
                boneHatchetEquipment,
                bonePickEquipment);

            AssetDatabase.SaveAssets();
            Debug.Log($"{LogPrefix} Primitive tool and weapon bootstrap setup complete.");
            EditorApplication.Exit(0);
        }

        #endregion

        #region Private Methods

        private static void EnsureFolders()
        {
            EnsureFolder("Assets/CCS/Survival/Content/Items/Resources");
            EnsureFolder(ResourcesRoot);
            EnsureFolder("Assets/CCS/Survival/Content/Items/Tools");
            EnsureFolder(ToolsPrimitiveRoot);
            EnsureFolder(ToolsBoneRoot);
            EnsureFolder("Assets/CCS/Survival/Content/Equipment");
            EnsureFolder(EquipmentRoot);
            EnsureFolder("Assets/CCS/Survival/Profiles/Crafting");
            EnsureFolder(BoneRecipesRoot);
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

        private static CCS_ItemDefinition EnsureResourceItem(
            string assetName,
            string itemId,
            string displayName,
            string description)
        {
            string assetPath = $"{ResourcesRoot}/{assetName}.asset";
            return EnsureGenericItem(
                assetPath,
                itemId,
                displayName,
                description,
                CCS_ItemCategory.Material,
                50,
                0.2f,
                CCS_ItemGameplayKind.Generic);
        }

        private static CCS_ItemDefinition EnsureToolItem(
            string assetPathWithoutExtension,
            string itemId,
            string displayName,
            string description,
            CCS_ItemCategory category,
            int maxStackSize,
            float weight,
            CCS_ItemGameplayKind gameplayKind,
            CCS_ToolArchetype toolArchetype,
            CCS_ToolTier toolTier,
            CCS_ItemToolType toolType,
            bool hasWeaponIdentity,
            CCS_WeaponArchetype weaponArchetype,
            CCS_WeaponType weaponType,
            CCS_DamageType damageType,
            CCS_RangeType rangeType)
        {
            string assetPath = assetPathWithoutExtension + ".asset";
            CCS_ItemDefinition itemDefinition = EnsureGenericItem(
                assetPath,
                itemId,
                displayName,
                description,
                category,
                maxStackSize,
                weight,
                gameplayKind);

            SerializedObject serializedItem = new SerializedObject(itemDefinition);
            serializedItem.FindProperty("hasToolIdentity").boolValue = true;
            serializedItem.FindProperty("toolType").enumValueIndex = (int)toolType;
            serializedItem.FindProperty("toolArchetype").enumValueIndex = (int)toolArchetype;
            serializedItem.FindProperty("toolTier").enumValueIndex = (int)toolTier;
            serializedItem.FindProperty("hasWeaponIdentity").boolValue = hasWeaponIdentity;
            serializedItem.FindProperty("weaponArchetype").enumValueIndex = (int)weaponArchetype;
            serializedItem.FindProperty("weaponType").enumValueIndex = (int)weaponType;
            serializedItem.FindProperty("damageType").enumValueIndex = (int)damageType;
            serializedItem.FindProperty("rangeType").enumValueIndex = (int)rangeType;
            serializedItem.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(itemDefinition);
            return itemDefinition;
        }

        private static void EnsureWeaponItem(
            string assetPathWithoutExtension,
            string itemId,
            string displayName,
            string description,
            CCS_ItemCategory category,
            int maxStackSize,
            float weight,
            CCS_WeaponArchetype weaponArchetype,
            CCS_WeaponType weaponType,
            CCS_DamageType damageType,
            CCS_RangeType rangeType)
        {
            string assetPath = assetPathWithoutExtension + ".asset";
            CCS_ItemDefinition itemDefinition = EnsureGenericItem(
                assetPath,
                itemId,
                displayName,
                description,
                category,
                maxStackSize,
                weight,
                CCS_ItemGameplayKind.Weapon);

            SerializedObject serializedItem = new SerializedObject(itemDefinition);
            serializedItem.FindProperty("hasToolIdentity").boolValue = false;
            serializedItem.FindProperty("hasWeaponIdentity").boolValue = true;
            serializedItem.FindProperty("weaponArchetype").enumValueIndex = (int)weaponArchetype;
            serializedItem.FindProperty("weaponType").enumValueIndex = (int)weaponType;
            serializedItem.FindProperty("damageType").enumValueIndex = (int)damageType;
            serializedItem.FindProperty("rangeType").enumValueIndex = (int)rangeType;
            serializedItem.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(itemDefinition);
        }

        private static CCS_ItemDefinition EnsureGenericItem(
            string assetPath,
            string itemId,
            string displayName,
            string description,
            CCS_ItemCategory category,
            int maxStackSize,
            float weight,
            CCS_ItemGameplayKind gameplayKind)
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
            serializedItem.FindProperty("category").enumValueIndex = (int)category;
            serializedItem.FindProperty("maxStackSize").intValue = maxStackSize;
            serializedItem.FindProperty("isStackable").boolValue = maxStackSize > 1;
            serializedItem.FindProperty("weight").floatValue = weight;
            serializedItem.FindProperty("gameplayKind").enumValueIndex = (int)gameplayKind;
            serializedItem.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(itemDefinition);
            return itemDefinition;
        }

        private static CCS_EquipmentItemDefinition EnsureEquipmentDefinition(
            string assetName,
            CCS_ItemDefinition itemDefinition,
            CCS_EquipmentSlotType allowedSlot)
        {
            string assetPath = $"{EquipmentRoot}/{assetName}.asset";
            CCS_EquipmentItemDefinition equipmentDefinition =
                AssetDatabase.LoadAssetAtPath<CCS_EquipmentItemDefinition>(assetPath);
            if (equipmentDefinition == null)
            {
                equipmentDefinition = ScriptableObject.CreateInstance<CCS_EquipmentItemDefinition>();
                AssetDatabase.CreateAsset(equipmentDefinition, assetPath);
            }

            SerializedObject serializedEquipment = new SerializedObject(equipmentDefinition);
            serializedEquipment.FindProperty("itemDefinition").objectReferenceValue = itemDefinition;
            serializedEquipment.FindProperty("allowedSlot").enumValueIndex = (int)allowedSlot;
            serializedEquipment.FindProperty("durabilityEnabled").boolValue = false;
            serializedEquipment.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(equipmentDefinition);
            return equipmentDefinition;
        }

        private static CCS_CraftingRecipeDefinition EnsureBoneRecipe(
            string assetName,
            string recipeId,
            string displayName,
            CCS_ItemDefinition primaryIngredient,
            int primaryQuantity,
            CCS_ItemDefinition secondaryIngredient,
            int secondaryQuantity,
            CCS_ItemDefinition result,
            int resultQuantity)
        {
            string assetPath = $"{BoneRecipesRoot}/{assetName}.asset";
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
                "Bone and branch primitive equipment recipe for 0.9.2 foundation.";
            serializedRecipe.FindProperty("requiredStationType").enumValueIndex = (int)CCS_CraftingStationType.Hand;
            serializedRecipe.FindProperty("craftTimeSeconds").floatValue = 0f;
            serializedRecipe.FindProperty("isUnlockedByDefault").boolValue = true;

            SerializedProperty ingredients = serializedRecipe.FindProperty("ingredients");
            ingredients.ClearArray();
            AddIngredient(ingredients, 0, primaryIngredient, primaryQuantity);
            AddIngredient(ingredients, 1, secondaryIngredient, secondaryQuantity);

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

        private static void AddIngredient(
            SerializedProperty ingredients,
            int index,
            CCS_ItemDefinition itemDefinition,
            int quantity)
        {
            ingredients.InsertArrayElementAtIndex(index);
            SerializedProperty ingredientEntry = ingredients.GetArrayElementAtIndex(index);
            ingredientEntry.FindPropertyRelative("itemDefinition").objectReferenceValue = itemDefinition;
            ingredientEntry.FindPropertyRelative("quantity").intValue = quantity;
        }

        private static void EnsureStarterProfileBoneRecipes(CCS_CraftingRecipeDefinition[] boneRecipes)
        {
            CCS_StarterLoadoutProfile profile =
                AssetDatabase.LoadAssetAtPath<CCS_StarterLoadoutProfile>(StarterProfilePath);
            if (profile == null)
            {
                Debug.LogError($"{LogPrefix} Missing starter loadout profile: {StarterProfilePath}");
                EditorApplication.Exit(1);
                return;
            }

            SerializedObject serializedProfile = new SerializedObject(profile);
            serializedProfile.FindProperty("profileVersion").stringValue = "0.9.2";
            serializedProfile.FindProperty("profileDescription").stringValue =
                "Starter loadout and progression profile updated for 0.9.2 tool and weapon foundation.";

            SerializedProperty recipeList = serializedProfile.FindProperty("boneToolRecipes");
            recipeList.ClearArray();
            for (int index = 0; index < boneRecipes.Length; index++)
            {
                recipeList.InsertArrayElementAtIndex(index);
                recipeList.GetArrayElementAtIndex(index).objectReferenceValue = boneRecipes[index];
            }

            serializedProfile.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
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

        private static void EnsureEquipmentSaveRestoreCatalog(
            params CCS_EquipmentItemDefinition[] equipmentDefinitions)
        {
            CCS_EquipmentProfile equipmentProfile =
                AssetDatabase.LoadAssetAtPath<CCS_EquipmentProfile>(EquipmentProfilePath);
            if (equipmentProfile == null)
            {
                Debug.LogError($"{LogPrefix} Missing equipment profile: {EquipmentProfilePath}");
                EditorApplication.Exit(1);
                return;
            }

            List<CCS_EquipmentItemDefinition> mergedDefinitions = new List<CCS_EquipmentItemDefinition>();
            CCS_EquipmentItemDefinition[] existingDefinitions = equipmentProfile.SaveRestoreEquipmentDefinitions;
            for (int index = 0; index < existingDefinitions.Length; index++)
            {
                CCS_EquipmentItemDefinition existingDefinition = existingDefinitions[index];
                if (existingDefinition != null && !mergedDefinitions.Contains(existingDefinition))
                {
                    mergedDefinitions.Add(existingDefinition);
                }
            }

            for (int index = 0; index < equipmentDefinitions.Length; index++)
            {
                CCS_EquipmentItemDefinition equipmentDefinition = equipmentDefinitions[index];
                if (equipmentDefinition != null && !mergedDefinitions.Contains(equipmentDefinition))
                {
                    mergedDefinitions.Add(equipmentDefinition);
                }
            }

            SerializedObject serializedProfile = new SerializedObject(equipmentProfile);
            SerializedProperty restoreList = serializedProfile.FindProperty("saveRestoreEquipmentDefinitions");
            restoreList.ClearArray();
            for (int index = 0; index < mergedDefinitions.Count; index++)
            {
                restoreList.InsertArrayElementAtIndex(index);
                restoreList.GetArrayElementAtIndex(index).objectReferenceValue = mergedDefinitions[index];
            }

            serializedProfile.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(equipmentProfile);
        }

        private static void EnsurePrimitiveToolEquipHarness(
            CCS_EquipmentItemDefinition knifeEquipment,
            CCS_EquipmentItemDefinition boneHatchetEquipment,
            CCS_EquipmentItemDefinition bonePickEquipment)
        {
            Scene scene = EditorSceneManager.OpenScene(BootstrapScenePath, OpenSceneMode.Single);
            CCS_PrimitiveToolEquipTestHarness[] harnesses =
                Object.FindObjectsByType<CCS_PrimitiveToolEquipTestHarness>();

            CCS_PrimitiveToolEquipTestHarness harness;
            if (harnesses.Length == 0)
            {
                GameObject harnessObject = new GameObject("CCS_PrimitiveToolEquipTestHarness");
                harness = harnessObject.AddComponent<CCS_PrimitiveToolEquipTestHarness>();
            }
            else
            {
                harness = harnesses[0];
            }

            SerializedObject serializedHarness = new SerializedObject(harness);
            serializedHarness.FindProperty("enableHarness").boolValue = false;
            serializedHarness.FindProperty("knifeEquipment").objectReferenceValue = knifeEquipment;
            serializedHarness.FindProperty("boneHatchetEquipment").objectReferenceValue = boneHatchetEquipment;
            serializedHarness.FindProperty("bonePickEquipment").objectReferenceValue = bonePickEquipment;
            serializedHarness.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        #endregion
    }
}
