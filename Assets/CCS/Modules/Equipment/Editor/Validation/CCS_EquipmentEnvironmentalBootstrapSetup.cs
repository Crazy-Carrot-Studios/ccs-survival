using System.IO;
using CCS.Modules.Equipment;
using CCS.Modules.Inventory;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_EquipmentEnvironmentalBootstrapSetup
// CATEGORY: Modules / Equipment / Editor / Validation
// PURPOSE: Creates test equipment assets with environmental survival modifiers.
// PLACEMENT: Batch entry for 0.7.4 environmental equipment milestone.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-31
// NOTES: Survival modifiers only. No armor, combat stats, or visuals.
// =============================================================================

namespace CCS.Modules.Equipment.Editor
{
    public static class CCS_EquipmentEnvironmentalBootstrapSetup
    {
        private const string InventoryTestItemsRoot = "Assets/CCS/Survival/Profiles/Inventory/TestItems";
        private const string EquipmentTestItemsRoot = "Assets/CCS/Survival/Profiles/Equipment/TestItems";
        private const string DefaultEquipmentProfilePath =
            "Assets/CCS/Survival/Profiles/Equipment/CCS_DefaultEquipmentProfile.asset";
        private const string LogPrefix = "[CCS_EquipmentEnvironmentalBootstrapSetup]";

        #region Public Methods

        public static void ExecuteBatch()
        {
            EnsureFolders();

            CCS_ItemDefinition warmHatItem = EnsureInventoryItem(
                InventoryTestItemsRoot + "/CCS_TestItem_WarmHat.asset",
                "ccs.survival.item.test.warmhat",
                "Warm Hat",
                "Test equipment with temperature resistance.");

            CCS_ItemDefinition heavyCoatItem = EnsureInventoryItem(
                InventoryTestItemsRoot + "/CCS_TestItem_HeavyCoat.asset",
                "ccs.survival.item.test.heavycoat",
                "Heavy Coat",
                "Test equipment with temperature and exposure resistance.");

            CCS_ItemDefinition waterproofBootsItem = EnsureInventoryItem(
                InventoryTestItemsRoot + "/CCS_TestItem_WaterproofBoots.asset",
                "ccs.survival.item.test.waterproofboots",
                "Waterproof Boots",
                "Test equipment with wetness resistance.");

            CCS_EquipmentItemDefinition warmHatEquipment = EnsureEquipmentDefinition(
                EquipmentTestItemsRoot + "/CCS_TestEquipment_WarmHat.asset",
                warmHatItem,
                CCS_EquipmentSlotType.Head,
                temperatureResistance: 1f,
                wetnessResistance: 0f,
                exposureResistance: 0f);

            CCS_EquipmentItemDefinition heavyCoatEquipment = EnsureEquipmentDefinition(
                EquipmentTestItemsRoot + "/CCS_TestEquipment_HeavyCoat.asset",
                heavyCoatItem,
                CCS_EquipmentSlotType.Chest,
                temperatureResistance: 2f,
                wetnessResistance: 0f,
                exposureResistance: 0.3f);

            CCS_EquipmentItemDefinition waterproofBootsEquipment = EnsureEquipmentDefinition(
                EquipmentTestItemsRoot + "/CCS_TestEquipment_WaterproofBoots.asset",
                waterproofBootsItem,
                CCS_EquipmentSlotType.Feet,
                temperatureResistance: 0f,
                wetnessResistance: 0.4f,
                exposureResistance: 0f);

            EnsureDefaultProfileReferences(
                warmHatEquipment,
                heavyCoatEquipment,
                waterproofBootsEquipment);

            AssetDatabase.SaveAssets();
            Debug.Log($"{LogPrefix} Equipment environmental bootstrap setup complete.");
            EditorApplication.Exit(0);
        }

        #endregion

        #region Private Methods

        private static void EnsureFolders()
        {
            EnsureFolder("Assets/CCS/Survival/Profiles/Inventory");
            EnsureFolder(InventoryTestItemsRoot);
            EnsureFolder("Assets/CCS/Survival/Profiles/Equipment");
            EnsureFolder(EquipmentTestItemsRoot);
        }

        private static CCS_ItemDefinition EnsureInventoryItem(
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
            serializedItem.FindProperty("maxStackSize").intValue = 1;
            serializedItem.FindProperty("isStackable").boolValue = false;
            serializedItem.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(itemDefinition);
            return itemDefinition;
        }

        private static CCS_EquipmentItemDefinition EnsureEquipmentDefinition(
            string assetPath,
            CCS_ItemDefinition itemDefinition,
            CCS_EquipmentSlotType allowedSlot,
            float temperatureResistance,
            float wetnessResistance,
            float exposureResistance)
        {
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
            serializedEquipment.FindProperty("modifiesInventoryCapacity").boolValue = false;
            serializedEquipment.FindProperty("modifiesCarryWeight").boolValue = false;
            serializedEquipment.FindProperty("temperatureResistance").floatValue = temperatureResistance;
            serializedEquipment.FindProperty("wetnessResistance").floatValue = wetnessResistance;
            serializedEquipment.FindProperty("exposureResistance").floatValue = exposureResistance;
            serializedEquipment.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(equipmentDefinition);
            return equipmentDefinition;
        }

        private static void EnsureDefaultProfileReferences(
            params CCS_EquipmentItemDefinition[] equipmentDefinitions)
        {
            CCS_EquipmentProfile profile =
                AssetDatabase.LoadAssetAtPath<CCS_EquipmentProfile>(DefaultEquipmentProfilePath);
            if (profile == null)
            {
                Debug.LogError($"{LogPrefix} Missing default equipment profile: {DefaultEquipmentProfilePath}");
                EditorApplication.Exit(1);
                return;
            }

            SerializedObject serializedProfile = new SerializedObject(profile);
            serializedProfile.FindProperty("profileVersion").stringValue = "0.7.4";
            SerializedProperty definitionsProperty =
                serializedProfile.FindProperty("saveRestoreEquipmentDefinitions");

            for (int index = 0; index < equipmentDefinitions.Length; index++)
            {
                AppendDefinitionReference(definitionsProperty, equipmentDefinitions[index]);
            }

            serializedProfile.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
        }

        private static void AppendDefinitionReference(
            SerializedProperty definitionsProperty,
            CCS_EquipmentItemDefinition equipmentDefinition)
        {
            if (equipmentDefinition == null)
            {
                return;
            }

            for (int index = 0; index < definitionsProperty.arraySize; index++)
            {
                SerializedProperty element = definitionsProperty.GetArrayElementAtIndex(index);
                if (element.objectReferenceValue == equipmentDefinition)
                {
                    return;
                }
            }

            int newIndex = definitionsProperty.arraySize;
            definitionsProperty.InsertArrayElementAtIndex(newIndex);
            definitionsProperty.GetArrayElementAtIndex(newIndex).objectReferenceValue = equipmentDefinition;
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
