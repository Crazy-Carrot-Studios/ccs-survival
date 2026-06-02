using System.Collections.Generic;
using System.IO;
using CCS.Modules.Crafting;
using CCS.Modules.Economy;
using CCS.Modules.Equipment;
using CCS.Modules.Industry;
using CCS.Modules.Inventory;
using CCS.Modules.Playtesting;
using CCS.Survival.Composition;
using CCS.Survival.Editor.Development;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CCS.Modules.Firearms.Editor
{
    public static class CCS_FirearmFoundationBootstrapSetup
    {
        private const string LogPrefix = "[CCS_FirearmFoundationBootstrapSetup]";
        private const string FirearmProfilePath = "Assets/CCS/Survival/Profiles/Firearms/CCS_DefaultFirearmProfile.asset";
        private const string FirearmContentRoot = "Assets/CCS/Survival/Content/Firearms";
        private const string AmmoContentRoot = FirearmContentRoot + "/Ammo";
        private const string EquipmentContentRoot = "Assets/CCS/Survival/Content/Equipment/Firearms";
        private const string EquipmentVisualRoot = "Assets/CCS/Survival/Content/Equipment/Visuals";
        private const string WeaponPrefabsRoot = "Assets/CCS/Survival/Prefabs/Weapons";
        private const string GunsmithVendorPath = "Assets/CCS/Survival/Content/Vendors/CCS_Vendor_FrontierGunsmith.asset";
        private const string StableVendorPath = "Assets/CCS/Survival/Content/Vendors/CCS_Vendor_FrontierStable.asset";
        private const string GeneralStoreVendorPath = "Assets/CCS/Survival/Content/Vendors/CCS_Vendor_GeneralStore.asset";
        private const string VendorProfilePath = "Assets/CCS/Survival/Profiles/Economy/CCS_DefaultVendorProfile.asset";
        private const string InventoryProfilePath = "Assets/CCS/Survival/Profiles/Inventory/CCS_DefaultInventoryProfile.asset";
        private const string EquipmentProfilePath = "Assets/CCS/Survival/Profiles/Equipment/CCS_DefaultEquipmentProfile.asset";
        private const string EquipmentVisualProfilePath = "Assets/CCS/Survival/Profiles/Equipment/CCS_DefaultEquipmentVisualProfile.asset";
        private const string IndustryProfilePath = "Assets/CCS/Survival/Profiles/Industry/CCS_DefaultIndustryProfile.asset";
        private const string IndustryRecipesRoot = "Assets/CCS/Survival/Profiles/Crafting/IndustryRecipes";
        private const string IndustryBlacksmithRoot = "Assets/CCS/Survival/Content/Industry/Blacksmith";
        private const string BootstrapRootPrefabPath = "Assets/CCS/Survival/Prefabs/PF_CCS_Survival_BootstrapRoot.prefab";
        private const string DefaultPlaytestProfilePath = "Assets/CCS/Survival/Profiles/Playtesting/CCS_DefaultPlaytestProfile.asset";
        private const string TradeDollarsPath = "Assets/CCS/Survival/Profiles/Economy/Currencies/CCS_Currency_TradeDollars.asset";
        private const string BootstrapScenePath = "Assets/CCS/Survival/Scenes/SCN_CCS_Survival_Bootstrap.unity";
        private const string LumberItemPath = "Assets/CCS/Survival/Content/Items/Industry/CCS_Item_Lumber.asset";
        private const string CharcoalItemPath = "Assets/CCS/Survival/Content/Items/Progression/CCS_Item_Charcoal.asset";
        private const string RefinedIronItemPath = "Assets/CCS/Survival/Content/Items/Industry/CCS_Item_RefinedIron.asset";
        private const string GenericToolVisualPrefabPath =
            "Assets/CCS/Survival/Prefabs/Equipment/Visuals/PF_CCS_Visual_GenericTool.prefab";
        private const string TestGunsmithObjectName = "CCS_TestFrontierGunsmith";

        private const int RevolverBuyValue = 1400;
        private const int RifleBuyValue = 3600;
        private const int ShotgunBuyValue = 2800;
        private const int AmmoBuyValue = 10;
        private const int AmmoSellValue = 2;
        private const int LumberBuyOverride = 12;
        private const int CharcoalBuyOverride = 10;
        private const int RefinedIronBuyOverride = 28;

        public static void ExecuteBatch()
        {
            AssetDatabase.Refresh();
            EnsureFolders();

            CCS_CurrencyDefinition tradeDollars = AssetDatabase.LoadAssetAtPath<CCS_CurrencyDefinition>(TradeDollarsPath);
            if (tradeDollars == null)
            {
                Debug.LogError($"{LogPrefix} Missing trade dollars currency.");
                EditorApplication.Exit(1);
                return;
            }

            CCS_ItemDefinition lumber = LoadItem(LumberItemPath);
            CCS_ItemDefinition charcoal = LoadItem(CharcoalItemPath);
            CCS_ItemDefinition refinedIron = LoadItem(RefinedIronItemPath);
            if (lumber == null || charcoal == null || refinedIron == null)
            {
                Debug.LogError($"{LogPrefix} Missing prerequisite industry items.");
                EditorApplication.Exit(1);
                return;
            }

            GameObject revolverPrefab = EnsureWeaponPrefab(
                CCS_FirearmContentIds.RevolverPrefabName,
                PrimitiveType.Cube,
                new Vector3(0.18f, 0.12f, 0.35f));
            GameObject riflePrefab = EnsureWeaponPrefab(
                CCS_FirearmContentIds.RiflePrefabName,
                PrimitiveType.Capsule,
                new Vector3(0.14f, 0.14f, 0.9f));
            GameObject shotgunPrefab = EnsureWeaponPrefab(
                CCS_FirearmContentIds.ShotgunPrefabName,
                PrimitiveType.Cube,
                new Vector3(0.2f, 0.14f, 0.55f));

            CCS_ItemDefinition revolverCartridgeItem = EnsureAmmoItem(
                "CCS_Item_RevolverCartridge",
                CCS_FirearmContentIds.RevolverCartridgeItemId,
                "Revolver Cartridge",
                "Frontier revolver ammunition.");
            CCS_ItemDefinition rifleCartridgeItem = EnsureAmmoItem(
                "CCS_Item_RifleCartridge",
                CCS_FirearmContentIds.RifleCartridgeItemId,
                "Rifle Cartridge",
                "Frontier rifle ammunition.");
            CCS_ItemDefinition shotgunShellItem = EnsureAmmoItem(
                "CCS_Item_ShotgunShell",
                CCS_FirearmContentIds.ShotgunShellItemId,
                "Shotgun Shell",
                "Frontier shotgun ammunition.");

            CCS_AmmoDefinition revolverAmmo = EnsureAmmoDefinition(
                "CCS_Ammo_RevolverCartridge",
                CCS_FirearmContentIds.RevolverCartridgeAmmoId,
                "Revolver Cartridge",
                revolverCartridgeItem);
            CCS_AmmoDefinition rifleAmmo = EnsureAmmoDefinition(
                "CCS_Ammo_RifleCartridge",
                CCS_FirearmContentIds.RifleCartridgeAmmoId,
                "Rifle Cartridge",
                rifleCartridgeItem);
            CCS_AmmoDefinition shotgunAmmo = EnsureAmmoDefinition(
                "CCS_Ammo_ShotgunShell",
                CCS_FirearmContentIds.ShotgunShellAmmoId,
                "Shotgun Shell",
                shotgunShellItem);

            CCS_ItemDefinition revolverItem = EnsureFirearmItem(
                "CCS_Item_FrontierRevolver",
                CCS_FirearmContentIds.FrontierRevolverItemId,
                "Frontier Revolver",
                "Six-shot frontier revolver for close-range hunting.",
                CCS_WeaponArchetype.Revolver,
                CCS_RangeType.ShortRanged,
                28f,
                25f,
                RevolverBuyValue);
            CCS_ItemDefinition rifleItem = EnsureFirearmItem(
                "CCS_Item_FrontierRifle",
                CCS_FirearmContentIds.FrontierRifleItemId,
                "Frontier Rifle",
                "Long-range frontier rifle for precision hunting.",
                CCS_WeaponArchetype.Rifle,
                CCS_RangeType.LongRanged,
                42f,
                55f,
                RifleBuyValue);
            CCS_ItemDefinition shotgunItem = EnsureFirearmItem(
                "CCS_Item_FrontierShotgun",
                CCS_FirearmContentIds.FrontierShotgunItemId,
                "Frontier Shotgun",
                "Double-barrel frontier shotgun for close burst hunting.",
                CCS_WeaponArchetype.Shotgun,
                CCS_RangeType.ShortRanged,
                55f,
                18f,
                ShotgunBuyValue);

            CCS_FirearmDefinition revolverDefinition = EnsureFirearmDefinition(
                "CCS_Firearm_FrontierRevolver",
                CCS_FirearmContentIds.FrontierRevolverFirearmId,
                "Frontier Revolver",
                revolverItem,
                revolverAmmo,
                6,
                28f,
                25f,
                revolverPrefab);
            CCS_FirearmDefinition rifleDefinition = EnsureFirearmDefinition(
                "CCS_Firearm_FrontierRifle",
                CCS_FirearmContentIds.FrontierRifleFirearmId,
                "Frontier Rifle",
                rifleItem,
                rifleAmmo,
                5,
                42f,
                55f,
                riflePrefab);
            CCS_FirearmDefinition shotgunDefinition = EnsureFirearmDefinition(
                "CCS_Firearm_FrontierShotgun",
                CCS_FirearmContentIds.FrontierShotgunFirearmId,
                "Frontier Shotgun",
                shotgunItem,
                shotgunAmmo,
                2,
                55f,
                18f,
                shotgunPrefab);

            CCS_FirearmProfile firearmProfile = EnsureFirearmProfile(
                revolverDefinition,
                rifleDefinition,
                shotgunDefinition,
                revolverAmmo,
                rifleAmmo,
                shotgunAmmo);

            CCS_EquipmentItemDefinition revolverEquipment = EnsureFirearmEquipment(
                "CCS_Equipment_FrontierRevolver",
                revolverItem);
            CCS_EquipmentItemDefinition rifleEquipment = EnsureFirearmEquipment(
                "CCS_Equipment_FrontierRifle",
                rifleItem);
            CCS_EquipmentItemDefinition shotgunEquipment = EnsureFirearmEquipment(
                "CCS_Equipment_FrontierShotgun",
                shotgunItem);
            EnsureEquipmentSaveRestore(revolverEquipment, rifleEquipment, shotgunEquipment);

            EnsureFirearmEquipmentVisual(
                "CCS_EquipmentVisual_FrontierRevolver",
                revolverItem,
                new Vector3(0f, 0f, 0.08f),
                new Vector3(10f, 0f, 90f),
                new Vector3(0.12f, 0.08f, 0.22f));
            EnsureFirearmEquipmentVisual(
                "CCS_EquipmentVisual_FrontierRifle",
                rifleItem,
                new Vector3(0f, 0f, 0.2f),
                new Vector3(10f, 0f, 90f),
                new Vector3(0.08f, 0.08f, 0.55f));
            EnsureFirearmEquipmentVisual(
                "CCS_EquipmentVisual_FrontierShotgun",
                shotgunItem,
                new Vector3(0f, 0f, 0.14f),
                new Vector3(10f, 0f, 90f),
                new Vector3(0.14f, 0.1f, 0.36f));

            CCS_VendorDefinition gunsmithVendor = EnsureGunsmithVendor(
                tradeDollars,
                lumber,
                charcoal,
                refinedIron,
                revolverItem,
                rifleItem,
                shotgunItem,
                revolverCartridgeItem,
                rifleCartridgeItem,
                shotgunShellItem);
            EnsureVendorProfileIncludesGunsmith(gunsmithVendor);
            RemoveFirearmCatalogEntriesFromVendor(StableVendorPath);
            RemoveFirearmCatalogEntriesFromVendor(GeneralStoreVendorPath);

            EnsureAmmoIndustryRecipes(charcoal, refinedIron, revolverCartridgeItem, rifleCartridgeItem, shotgunShellItem);
            EnsureInventorySaveRestore(
                revolverItem,
                rifleItem,
                shotgunItem,
                revolverCartridgeItem,
                rifleCartridgeItem,
                shotgunShellItem);
            AssignFirearmProfileToBootstrapHost(firearmProfile);
            EnsureBootstrapSceneGunsmith(gunsmithVendor);
            EnsurePlaytestFirearmSteps();
            BumpVersions();

            AssetDatabase.SaveAssets();
            Debug.Log($"{LogPrefix} Firearm foundation bootstrap setup complete (1.6.0).");
            EditorApplication.Exit(0);
        }

        private static void EnsureFolders()
        {
            EnsureFolder("Assets/CCS/Survival/Profiles/Firearms");
            EnsureFolder(FirearmContentRoot);
            EnsureFolder(AmmoContentRoot);
            EnsureFolder(EquipmentContentRoot);
            EnsureFolder(WeaponPrefabsRoot);
            EnsureFolder(EquipmentVisualRoot);
            EnsureFolder(IndustryRecipesRoot);
            EnsureFolder(IndustryBlacksmithRoot);
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
            return AssetDatabase.LoadAssetAtPath<CCS_ItemDefinition>(assetPath);
        }

        private static GameObject EnsureWeaponPrefab(string prefabName, PrimitiveType primitiveType, Vector3 scale)
        {
            string prefabPath = $"{WeaponPrefabsRoot}/{prefabName}.prefab";
            GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (existing != null)
            {
                return existing;
            }

            GameObject root = GameObject.CreatePrimitive(primitiveType);
            root.name = prefabName;
            root.transform.localScale = scale;

            Collider collider = root.GetComponent<Collider>();
            if (collider != null)
            {
                Object.DestroyImmediate(collider);
            }

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            Object.DestroyImmediate(root);
            return prefab;
        }

        private static CCS_ItemDefinition EnsureAmmoItem(
            string assetName,
            string itemId,
            string displayName,
            string description)
        {
            string assetPath = $"{AmmoContentRoot}/{assetName}.asset";
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
            serialized.FindProperty("category").enumValueIndex = (int)CCS_ItemCategory.Material;
            serialized.FindProperty("maxStackSize").intValue = 50;
            serialized.FindProperty("isStackable").boolValue = true;
            serialized.FindProperty("hasWeaponIdentity").boolValue = false;
            serialized.FindProperty("hasToolIdentity").boolValue = false;
            serialized.FindProperty("gameplayKind").enumValueIndex = (int)CCS_ItemGameplayKind.Generic;
            serialized.FindProperty("hasEconomyValues").boolValue = true;
            serialized.FindProperty("buyValue").intValue = AmmoBuyValue;
            serialized.FindProperty("sellValue").intValue = AmmoSellValue;
            serialized.FindProperty("vendorCategory").enumValueIndex = (int)CCS_ItemVendorCategory.Ammunition;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(item);
            return item;
        }

        private static CCS_AmmoDefinition EnsureAmmoDefinition(
            string assetName,
            string ammoId,
            string displayName,
            CCS_ItemDefinition inventoryItem)
        {
            string assetPath = $"{AmmoContentRoot}/{assetName}.asset";
            CCS_AmmoDefinition definition = AssetDatabase.LoadAssetAtPath<CCS_AmmoDefinition>(assetPath);
            if (definition == null)
            {
                definition = ScriptableObject.CreateInstance<CCS_AmmoDefinition>();
                AssetDatabase.CreateAsset(definition, assetPath);
            }

            SerializedObject serialized = new SerializedObject(definition);
            serialized.FindProperty("ammoId").stringValue = ammoId;
            serialized.FindProperty("displayName").stringValue = displayName;
            serialized.FindProperty("inventoryItemId").stringValue = inventoryItem.ItemId;
            serialized.FindProperty("inventoryItem").objectReferenceValue = inventoryItem;
            serialized.FindProperty("roundsPerInventoryUnit").intValue = 1;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(definition);
            return definition;
        }

        private static CCS_ItemDefinition EnsureFirearmItem(
            string assetName,
            string itemId,
            string displayName,
            string description,
            CCS_WeaponArchetype weaponArchetype,
            CCS_RangeType rangeType,
            float damage,
            float range,
            int buyValue)
        {
            string assetPath = $"{FirearmContentRoot}/{assetName}.asset";
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
            serialized.FindProperty("category").enumValueIndex = (int)CCS_ItemCategory.Generic;
            serialized.FindProperty("maxStackSize").intValue = 1;
            serialized.FindProperty("isStackable").boolValue = false;
            serialized.FindProperty("hasWeaponIdentity").boolValue = true;
            serialized.FindProperty("hasToolIdentity").boolValue = false;
            serialized.FindProperty("gameplayKind").enumValueIndex = (int)CCS_ItemGameplayKind.Weapon;
            serialized.FindProperty("weaponArchetype").enumValueIndex = (int)weaponArchetype;
            serialized.FindProperty("weaponType").enumValueIndex = (int)CCS_WeaponType.Ranged;
            serialized.FindProperty("damageType").enumValueIndex = (int)CCS_DamageType.Pierce;
            serialized.FindProperty("rangeType").enumValueIndex = (int)rangeType;
            serialized.FindProperty("meleeDamage").floatValue = damage;
            serialized.FindProperty("meleeRange").floatValue = range;
            serialized.FindProperty("hasEconomyValues").boolValue = true;
            serialized.FindProperty("buyValue").intValue = buyValue;
            serialized.FindProperty("sellValue").intValue = buyValue / 5;
            serialized.FindProperty("vendorCategory").enumValueIndex = (int)CCS_ItemVendorCategory.Weapons;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(item);
            return item;
        }

        private static CCS_FirearmDefinition EnsureFirearmDefinition(
            string assetName,
            string firearmId,
            string displayName,
            CCS_ItemDefinition inventoryItem,
            CCS_AmmoDefinition ammoDefinition,
            int magazineCapacity,
            float damage,
            float range,
            GameObject worldPrefab)
        {
            string assetPath = $"{FirearmContentRoot}/{assetName}.asset";
            CCS_FirearmDefinition definition = AssetDatabase.LoadAssetAtPath<CCS_FirearmDefinition>(assetPath);
            if (definition == null)
            {
                definition = ScriptableObject.CreateInstance<CCS_FirearmDefinition>();
                AssetDatabase.CreateAsset(definition, assetPath);
            }

            SerializedObject serialized = new SerializedObject(definition);
            serialized.FindProperty("firearmId").stringValue = firearmId;
            serialized.FindProperty("displayName").stringValue = displayName;
            serialized.FindProperty("inventoryItemId").stringValue = inventoryItem.ItemId;
            serialized.FindProperty("weaponArchetype").enumValueIndex = (int)inventoryItem.WeaponArchetype;
            serialized.FindProperty("ammoDefinition").objectReferenceValue = ammoDefinition;
            serialized.FindProperty("magazineCapacity").intValue = magazineCapacity;
            serialized.FindProperty("damage").floatValue = damage;
            serialized.FindProperty("range").floatValue = range;
            serialized.FindProperty("worldPrefab").objectReferenceValue = worldPrefab;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(definition);
            return definition;
        }

        private static CCS_FirearmProfile EnsureFirearmProfile(
            CCS_FirearmDefinition revolver,
            CCS_FirearmDefinition rifle,
            CCS_FirearmDefinition shotgun,
            CCS_AmmoDefinition revolverAmmo,
            CCS_AmmoDefinition rifleAmmo,
            CCS_AmmoDefinition shotgunAmmo)
        {
            CCS_FirearmProfile profile = AssetDatabase.LoadAssetAtPath<CCS_FirearmProfile>(FirearmProfilePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<CCS_FirearmProfile>();
                AssetDatabase.CreateAsset(profile, FirearmProfilePath);
            }

            SerializedObject serialized = new SerializedObject(profile);
            SerializedProperty firearmDefinitions = serialized.FindProperty("firearmDefinitions");
            firearmDefinitions.arraySize = 3;
            firearmDefinitions.GetArrayElementAtIndex(0).objectReferenceValue = revolver;
            firearmDefinitions.GetArrayElementAtIndex(1).objectReferenceValue = rifle;
            firearmDefinitions.GetArrayElementAtIndex(2).objectReferenceValue = shotgun;

            SerializedProperty ammoDefinitions = serialized.FindProperty("ammoDefinitions");
            ammoDefinitions.arraySize = 3;
            ammoDefinitions.GetArrayElementAtIndex(0).objectReferenceValue = revolverAmmo;
            ammoDefinitions.GetArrayElementAtIndex(1).objectReferenceValue = rifleAmmo;
            ammoDefinitions.GetArrayElementAtIndex(2).objectReferenceValue = shotgunAmmo;

            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
            return profile;
        }

        private static CCS_EquipmentItemDefinition EnsureFirearmEquipment(string assetName, CCS_ItemDefinition item)
        {
            string assetPath = $"{EquipmentContentRoot}/{assetName}.asset";
            CCS_EquipmentItemDefinition equipment =
                AssetDatabase.LoadAssetAtPath<CCS_EquipmentItemDefinition>(assetPath);
            if (equipment == null)
            {
                equipment = ScriptableObject.CreateInstance<CCS_EquipmentItemDefinition>();
                AssetDatabase.CreateAsset(equipment, assetPath);
            }

            SerializedObject serialized = new SerializedObject(equipment);
            serialized.FindProperty("itemDefinition").objectReferenceValue = item;
            serialized.FindProperty("allowedSlot").enumValueIndex = (int)CCS_EquipmentSlotType.MainHand;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(equipment);
            return equipment;
        }

        private static void EnsureEquipmentSaveRestore(params CCS_EquipmentItemDefinition[] equipmentDefinitions)
        {
            CCS_EquipmentProfile equipmentProfile =
                AssetDatabase.LoadAssetAtPath<CCS_EquipmentProfile>(EquipmentProfilePath);
            if (equipmentProfile == null)
            {
                return;
            }

            List<CCS_EquipmentItemDefinition> merged = new List<CCS_EquipmentItemDefinition>();
            CCS_EquipmentItemDefinition[] existing = equipmentProfile.SaveRestoreEquipmentDefinitions;
            for (int index = 0; index < existing.Length; index++)
            {
                if (existing[index] != null && !merged.Contains(existing[index]))
                {
                    merged.Add(existing[index]);
                }
            }

            for (int index = 0; index < equipmentDefinitions.Length; index++)
            {
                CCS_EquipmentItemDefinition equipment = equipmentDefinitions[index];
                if (equipment != null && !merged.Contains(equipment))
                {
                    merged.Add(equipment);
                }
            }

            SerializedObject serialized = new SerializedObject(equipmentProfile);
            SerializedProperty restoreList = serialized.FindProperty("saveRestoreEquipmentDefinitions");
            restoreList.arraySize = merged.Count;
            for (int index = 0; index < merged.Count; index++)
            {
                restoreList.GetArrayElementAtIndex(index).objectReferenceValue = merged[index];
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(equipmentProfile);
        }

        private static void EnsureFirearmEquipmentVisual(
            string assetName,
            CCS_ItemDefinition item,
            Vector3 localPosition,
            Vector3 localEuler,
            Vector3 localScale)
        {
            CCS_EquipmentVisualProfile profile =
                AssetDatabase.LoadAssetAtPath<CCS_EquipmentVisualProfile>(EquipmentVisualProfilePath);
            GameObject genericPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(GenericToolVisualPrefabPath);
            if (profile == null || item == null)
            {
                return;
            }

            string definitionPath = $"{EquipmentVisualRoot}/{assetName}.asset";
            CCS_EquipmentVisualDefinition definition =
                AssetDatabase.LoadAssetAtPath<CCS_EquipmentVisualDefinition>(definitionPath);
            if (definition == null)
            {
                definition = ScriptableObject.CreateInstance<CCS_EquipmentVisualDefinition>();
                AssetDatabase.CreateAsset(definition, definitionPath);
            }

            SerializedObject serializedDefinition = new SerializedObject(definition);
            serializedDefinition.FindProperty("itemId").stringValue = item.ItemId;
            serializedDefinition.FindProperty("visualPrefab").objectReferenceValue = genericPrefab;
            serializedDefinition.FindProperty("attachmentSocket").enumValueIndex =
                (int)CCS_EquipmentAttachmentSocketType.RightHand;
            serializedDefinition.FindProperty("localPositionOffset").vector3Value = localPosition;
            serializedDefinition.FindProperty("localEulerOffset").vector3Value = localEuler;
            serializedDefinition.FindProperty("localScale").vector3Value = localScale;
            serializedDefinition.FindProperty("hideWhenUnequipped").boolValue = true;
            serializedDefinition.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(definition);

            SerializedObject serializedProfile = new SerializedObject(profile);
            SerializedProperty definitions = serializedProfile.FindProperty("visualDefinitions");
            bool exists = false;
            for (int index = 0; index < definitions.arraySize; index++)
            {
                if (definitions.GetArrayElementAtIndex(index).objectReferenceValue == definition)
                {
                    exists = true;
                    break;
                }
            }

            if (!exists)
            {
                int newIndex = definitions.arraySize;
                definitions.InsertArrayElementAtIndex(newIndex);
                definitions.GetArrayElementAtIndex(newIndex).objectReferenceValue = definition;
            }

            serializedProfile.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
        }

        private static CCS_VendorDefinition EnsureGunsmithVendor(
            CCS_CurrencyDefinition currency,
            CCS_ItemDefinition lumber,
            CCS_ItemDefinition charcoal,
            CCS_ItemDefinition refinedIron,
            CCS_ItemDefinition revolverItem,
            CCS_ItemDefinition rifleItem,
            CCS_ItemDefinition shotgunItem,
            CCS_ItemDefinition revolverAmmoItem,
            CCS_ItemDefinition rifleAmmoItem,
            CCS_ItemDefinition shotgunAmmoItem)
        {
            CCS_VendorDefinition vendor = AssetDatabase.LoadAssetAtPath<CCS_VendorDefinition>(GunsmithVendorPath);
            if (vendor == null)
            {
                vendor = ScriptableObject.CreateInstance<CCS_VendorDefinition>();
                AssetDatabase.CreateAsset(vendor, GunsmithVendorPath);
            }

            SerializedObject serialized = new SerializedObject(vendor);
            serialized.FindProperty("vendorId").stringValue = CCS_FirearmContentIds.FrontierGunsmithVendorId;
            serialized.FindProperty("displayName").stringValue = "Frontier Gunsmith";
            serialized.FindProperty("description").stringValue =
                "Frontier gunsmith for firearms, ammunition, and metalworking supplies (1.6.0).";
            serialized.FindProperty("currencyDefinition").objectReferenceValue = currency;

            SerializedProperty catalogItems = serialized.FindProperty("vendorInventory").FindPropertyRelative("items");
            catalogItems.arraySize = 9;
            SetVendorCatalogEntry(catalogItems, 0, revolverItem, true, true, RevolverBuyValue);
            SetVendorCatalogEntry(catalogItems, 1, rifleItem, true, true, RifleBuyValue);
            SetVendorCatalogEntry(catalogItems, 2, shotgunItem, true, true, ShotgunBuyValue);
            SetVendorCatalogEntry(catalogItems, 3, revolverAmmoItem, true, true, AmmoBuyValue, AmmoSellValue);
            SetVendorCatalogEntry(catalogItems, 4, rifleAmmoItem, true, true, AmmoBuyValue, AmmoSellValue);
            SetVendorCatalogEntry(catalogItems, 5, shotgunAmmoItem, true, true, AmmoBuyValue, AmmoSellValue);
            SetVendorCatalogEntry(catalogItems, 6, lumber, true, true, LumberBuyOverride, LumberBuyOverride / 5);
            SetVendorCatalogEntry(catalogItems, 7, charcoal, true, true, CharcoalBuyOverride, CharcoalBuyOverride / 5);
            SetVendorCatalogEntry(
                catalogItems,
                8,
                refinedIron,
                true,
                true,
                RefinedIronBuyOverride,
                RefinedIronBuyOverride / 5);

            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(vendor);
            return vendor;
        }

        private static void SetVendorCatalogEntry(
            SerializedProperty catalogItems,
            int index,
            CCS_ItemDefinition item,
            bool allowBuy,
            bool allowSell,
            int buyOverride,
            int? sellOverride = null)
        {
            SerializedProperty entry = catalogItems.GetArrayElementAtIndex(index);
            entry.FindPropertyRelative("itemDefinition").objectReferenceValue = item;
            entry.FindPropertyRelative("stockQuantity").intValue = -1;
            entry.FindPropertyRelative("allowBuy").boolValue = allowBuy;
            entry.FindPropertyRelative("allowSell").boolValue = allowSell;
            entry.FindPropertyRelative("buyPriceOverride").intValue = buyOverride;
            entry.FindPropertyRelative("sellPriceOverride").intValue = sellOverride ?? buyOverride / 5;
        }

        private static void EnsureVendorProfileIncludesGunsmith(CCS_VendorDefinition gunsmithVendor)
        {
            CCS_VendorProfile vendorProfile = AssetDatabase.LoadAssetAtPath<CCS_VendorProfile>(VendorProfilePath);
            if (vendorProfile == null)
            {
                return;
            }

            List<CCS_VendorDefinition> merged = new List<CCS_VendorDefinition>();
            CCS_VendorDefinition[] existing = vendorProfile.VendorDefinitions;
            for (int index = 0; index < existing.Length; index++)
            {
                if (existing[index] != null && !merged.Contains(existing[index]))
                {
                    merged.Add(existing[index]);
                }
            }

            if (!merged.Contains(gunsmithVendor))
            {
                merged.Add(gunsmithVendor);
            }

            SerializedObject serialized = new SerializedObject(vendorProfile);
            SerializedProperty vendors = serialized.FindProperty("vendorDefinitions");
            vendors.arraySize = merged.Count;
            for (int index = 0; index < merged.Count; index++)
            {
                vendors.GetArrayElementAtIndex(index).objectReferenceValue = merged[index];
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(vendorProfile);
        }

        private static void RemoveFirearmCatalogEntriesFromVendor(string vendorPath)
        {
            CCS_VendorDefinition vendor = AssetDatabase.LoadAssetAtPath<CCS_VendorDefinition>(vendorPath);
            if (vendor == null)
            {
                return;
            }

            HashSet<string> firearmItemIds = new HashSet<string>
            {
                CCS_FirearmContentIds.FrontierRevolverItemId,
                CCS_FirearmContentIds.FrontierRifleItemId,
                CCS_FirearmContentIds.FrontierShotgunItemId,
                CCS_FirearmContentIds.RevolverCartridgeItemId,
                CCS_FirearmContentIds.RifleCartridgeItemId,
                CCS_FirearmContentIds.ShotgunShellItemId
            };

            SerializedObject serialized = new SerializedObject(vendor);
            SerializedProperty catalogItems = serialized.FindProperty("vendorInventory").FindPropertyRelative("items");
            for (int index = catalogItems.arraySize - 1; index >= 0; index--)
            {
                SerializedProperty entry = catalogItems.GetArrayElementAtIndex(index);
                CCS_ItemDefinition item =
                    entry.FindPropertyRelative("itemDefinition").objectReferenceValue as CCS_ItemDefinition;
                if (item != null && firearmItemIds.Contains(item.ItemId))
                {
                    catalogItems.DeleteArrayElementAtIndex(index);
                }
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(vendor);
        }

        private static void EnsureAmmoIndustryRecipes(
            CCS_ItemDefinition charcoal,
            CCS_ItemDefinition refinedIron,
            CCS_ItemDefinition revolverAmmoItem,
            CCS_ItemDefinition rifleAmmoItem,
            CCS_ItemDefinition shotgunAmmoItem)
        {
            CCS_BlacksmithRecipeDefinition revolverRecipe = EnsureAmmoForgeBlacksmithRecipe(
                "CCS_CraftingRecipe_AmmoRevolverCartridge",
                "ccs.survival.crafting.forge.ammo.revolver.cartridge",
                "Forge Revolver Cartridges",
                "ccs.survival.industry.blacksmith.ammo.revolver.cartridge",
                refinedIron,
                charcoal,
                revolverAmmoItem);
            CCS_BlacksmithRecipeDefinition rifleRecipe = EnsureAmmoForgeBlacksmithRecipe(
                "CCS_CraftingRecipe_AmmoRifleCartridge",
                "ccs.survival.crafting.forge.ammo.rifle.cartridge",
                "Forge Rifle Cartridges",
                "ccs.survival.industry.blacksmith.ammo.rifle.cartridge",
                refinedIron,
                charcoal,
                rifleAmmoItem);
            CCS_BlacksmithRecipeDefinition shotgunRecipe = EnsureAmmoForgeBlacksmithRecipe(
                "CCS_CraftingRecipe_AmmoShotgunShell",
                "ccs.survival.crafting.forge.ammo.shotgun.shell",
                "Forge Shotgun Shells",
                "ccs.survival.industry.blacksmith.ammo.shotgun.shell",
                refinedIron,
                charcoal,
                shotgunAmmoItem);

            MergeBlacksmithRecipesIntoIndustryProfile(revolverRecipe, rifleRecipe, shotgunRecipe);
        }

        private static CCS_BlacksmithRecipeDefinition EnsureAmmoForgeBlacksmithRecipe(
            string recipeAssetName,
            string recipeId,
            string displayName,
            string blacksmithId,
            CCS_ItemDefinition refinedIron,
            CCS_ItemDefinition charcoal,
            CCS_ItemDefinition outputAmmo)
        {
            string recipePath = $"{IndustryRecipesRoot}/{recipeAssetName}.asset";
            CCS_CraftingRecipeDefinition recipe = AssetDatabase.LoadAssetAtPath<CCS_CraftingRecipeDefinition>(recipePath);
            if (recipe == null)
            {
                recipe = ScriptableObject.CreateInstance<CCS_CraftingRecipeDefinition>();
                AssetDatabase.CreateAsset(recipe, recipePath);
            }

            SerializedObject serializedRecipe = new SerializedObject(recipe);
            serializedRecipe.FindProperty("recipeId").stringValue = recipeId;
            serializedRecipe.FindProperty("displayName").stringValue = displayName;
            serializedRecipe.FindProperty("requiredStationType").enumValueIndex = (int)CCS_CraftingStationType.Forge;
            serializedRecipe.FindProperty("craftTimeSeconds").floatValue = 0f;
            serializedRecipe.FindProperty("isUnlockedByDefault").boolValue = true;

            SerializedProperty ingredients = serializedRecipe.FindProperty("ingredients");
            ingredients.arraySize = 2;
            ingredients.GetArrayElementAtIndex(0).FindPropertyRelative("itemDefinition").objectReferenceValue = refinedIron;
            ingredients.GetArrayElementAtIndex(0).FindPropertyRelative("quantity").intValue = 1;
            ingredients.GetArrayElementAtIndex(1).FindPropertyRelative("itemDefinition").objectReferenceValue = charcoal;
            ingredients.GetArrayElementAtIndex(1).FindPropertyRelative("quantity").intValue = 1;

            SerializedProperty results = serializedRecipe.FindProperty("results");
            results.arraySize = 1;
            results.GetArrayElementAtIndex(0).FindPropertyRelative("itemDefinition").objectReferenceValue = outputAmmo;
            results.GetArrayElementAtIndex(0).FindPropertyRelative("quantity").intValue = 5;

            serializedRecipe.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(recipe);

            string blacksmithPath = $"{IndustryBlacksmithRoot}/{recipeAssetName}.asset";
            CCS_BlacksmithRecipeDefinition blacksmithRecipe =
                AssetDatabase.LoadAssetAtPath<CCS_BlacksmithRecipeDefinition>(blacksmithPath);
            if (blacksmithRecipe == null)
            {
                blacksmithRecipe = ScriptableObject.CreateInstance<CCS_BlacksmithRecipeDefinition>();
                AssetDatabase.CreateAsset(blacksmithRecipe, blacksmithPath);
            }

            SerializedObject serializedBlacksmith = new SerializedObject(blacksmithRecipe);
            serializedBlacksmith.FindProperty("blacksmithRecipeId").stringValue = blacksmithId;
            serializedBlacksmith.FindProperty("category").enumValueIndex = (int)CCS_BlacksmithRecipeCategory.Ammunition;
            serializedBlacksmith.FindProperty("craftingRecipe").objectReferenceValue = recipe;
            serializedBlacksmith.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(blacksmithRecipe);
            return blacksmithRecipe;
        }

        private static void MergeBlacksmithRecipesIntoIndustryProfile(params CCS_BlacksmithRecipeDefinition[] ammoRecipes)
        {
            CCS_IndustryProfile profile = AssetDatabase.LoadAssetAtPath<CCS_IndustryProfile>(IndustryProfilePath);
            if (profile == null)
            {
                return;
            }

            List<CCS_BlacksmithRecipeDefinition> merged = new List<CCS_BlacksmithRecipeDefinition>();
            IReadOnlyList<CCS_BlacksmithRecipeDefinition> existing = profile.BlacksmithRecipes;
            for (int index = 0; index < existing.Count; index++)
            {
                CCS_BlacksmithRecipeDefinition recipe = existing[index];
                if (recipe != null && !merged.Contains(recipe))
                {
                    merged.Add(recipe);
                }
            }

            for (int index = 0; index < ammoRecipes.Length; index++)
            {
                CCS_BlacksmithRecipeDefinition recipe = ammoRecipes[index];
                if (recipe != null && !merged.Contains(recipe))
                {
                    merged.Add(recipe);
                }
            }

            SerializedObject serialized = new SerializedObject(profile);
            SerializedProperty blacksmithList = serialized.FindProperty("blacksmithRecipes");
            blacksmithList.arraySize = merged.Count;
            for (int index = 0; index < merged.Count; index++)
            {
                blacksmithList.GetArrayElementAtIndex(index).objectReferenceValue = merged[index];
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
        }

        private static void EnsureInventorySaveRestore(params CCS_ItemDefinition[] items)
        {
            CCS_InventoryProfile inventoryProfile =
                AssetDatabase.LoadAssetAtPath<CCS_InventoryProfile>(InventoryProfilePath);
            if (inventoryProfile == null)
            {
                return;
            }

            List<CCS_ItemDefinition> merged = new List<CCS_ItemDefinition>();
            CCS_ItemDefinition[] existing = inventoryProfile.SaveRestoreItemDefinitions;
            for (int index = 0; index < existing.Length; index++)
            {
                if (existing[index] != null && !merged.Contains(existing[index]))
                {
                    merged.Add(existing[index]);
                }
            }

            for (int index = 0; index < items.Length; index++)
            {
                CCS_ItemDefinition item = items[index];
                if (item != null && !merged.Contains(item))
                {
                    merged.Add(item);
                }
            }

            SerializedObject serialized = new SerializedObject(inventoryProfile);
            SerializedProperty restoreList = serialized.FindProperty("saveRestoreItemDefinitions");
            restoreList.arraySize = merged.Count;
            for (int index = 0; index < merged.Count; index++)
            {
                restoreList.GetArrayElementAtIndex(index).objectReferenceValue = merged[index];
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(inventoryProfile);
        }

        private static void AssignFirearmProfileToBootstrapHost(CCS_FirearmProfile profile)
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
            serialized.FindProperty("firearmProfile").objectReferenceValue = profile;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(host);
            PrefabUtility.SavePrefabAsset(prefabRoot);
        }

        private static void EnsureBootstrapSceneGunsmith(CCS_VendorDefinition gunsmithVendor)
        {
            if (!File.Exists(BootstrapScenePath))
            {
                return;
            }

            Scene scene = EditorSceneManager.OpenScene(BootstrapScenePath);
            GameObject sceneRoot = GameObject.Find("CCS_SurvivalBootstrapSceneRoot");
            if (sceneRoot == null)
            {
                return;
            }

            Transform existing = sceneRoot.transform.Find(TestGunsmithObjectName);
            GameObject gunsmithObject = existing != null ? existing.gameObject : new GameObject(TestGunsmithObjectName);
            if (existing == null)
            {
                gunsmithObject.transform.SetParent(sceneRoot.transform, false);
                gunsmithObject.transform.localPosition = new Vector3(12f, 0f, 6f);
            }

            CCS_VendorInteractable interactable = gunsmithObject.GetComponent<CCS_VendorInteractable>();
            if (interactable == null)
            {
                interactable = gunsmithObject.AddComponent<CCS_VendorInteractable>();
            }

            SerializedObject serialized = new SerializedObject(interactable);
            serialized.FindProperty("vendorDefinition").objectReferenceValue = gunsmithVendor;
            serialized.FindProperty("interactionDisplayNameOverride").stringValue = "Frontier Gunsmith";
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(gunsmithObject);
            EditorSceneManager.SaveScene(scene);
        }

        private static void EnsurePlaytestFirearmSteps()
        {
            CCS_PlaytestProfile profile =
                AssetDatabase.LoadAssetAtPath<CCS_PlaytestProfile>(DefaultPlaytestProfilePath);
            if (profile == null)
            {
                return;
            }

            InsertStep(
                profile,
                "ccs.survival.playtest.firearm.earn.currency",
                "Earn firearm currency",
                CCS_PlaytestStepType.EarnCurrencyForFirearm,
                string.Empty);
            InsertStep(
                profile,
                "ccs.survival.playtest.firearm.buy.revolver",
                "Buy frontier revolver",
                CCS_PlaytestStepType.BuyRevolverFromGunsmith,
                CCS_FirearmContentIds.FrontierRevolverItemId);
            InsertStep(
                profile,
                "ccs.survival.playtest.firearm.buy.ammo",
                "Buy firearm ammo",
                CCS_PlaytestStepType.BuyFirearmAmmo,
                CCS_FirearmContentIds.RevolverCartridgeItemId);
            InsertStep(
                profile,
                "ccs.survival.playtest.firearm.equip",
                "Equip firearm",
                CCS_PlaytestStepType.EquipFirearm,
                CCS_FirearmContentIds.FrontierRevolverItemId);
            InsertStep(
                profile,
                "ccs.survival.playtest.firearm.reload",
                "Reload firearm",
                CCS_PlaytestStepType.ReloadFirearm,
                string.Empty);
            InsertStep(
                profile,
                "ccs.survival.playtest.firearm.shoot",
                "Shoot wildlife with firearm",
                CCS_PlaytestStepType.ShootWildlifeWithFirearm,
                string.Empty);
            InsertStep(
                profile,
                "ccs.survival.playtest.firearm.harvest.knife",
                "Harvest with knife after firearm",
                CCS_PlaytestStepType.HarvestWithKnifeAfterFirearm,
                string.Empty);
            InsertStep(
                profile,
                "ccs.survival.playtest.firearm.sell.hunt",
                "Sell hunting resources after firearm",
                CCS_PlaytestStepType.SellHuntingResourcesAfterFirearm,
                string.Empty);
            InsertStep(
                profile,
                "ccs.survival.playtest.firearm.save",
                "Save firearm state",
                CCS_PlaytestStepType.SaveFirearmState,
                string.Empty);
            InsertStep(
                profile,
                "ccs.survival.playtest.firearm.verify.load",
                "Verify firearm after load",
                CCS_PlaytestStepType.VerifyFirearmPersistenceAfterLoad,
                string.Empty);
            EditorUtility.SetDirty(profile);
        }

        private static void InsertStep(
            CCS_PlaytestProfile profile,
            string stepId,
            string displayName,
            CCS_PlaytestStepType stepType,
            string targetItemId)
        {
            SerializedObject serialized = new SerializedObject(profile);
            SerializedProperty steps = serialized.FindProperty("stepDefinitions");
            for (int index = 0; index < steps.arraySize; index++)
            {
                if (steps.GetArrayElementAtIndex(index).FindPropertyRelative("stepId").stringValue == stepId)
                {
                    return;
                }
            }

            steps.InsertArrayElementAtIndex(steps.arraySize);
            SerializedProperty step = steps.GetArrayElementAtIndex(steps.arraySize - 1);
            step.FindPropertyRelative("stepId").stringValue = stepId;
            step.FindPropertyRelative("displayName").stringValue = displayName;
            step.FindPropertyRelative("stepType").enumValueIndex = (int)stepType;
            step.FindPropertyRelative("instructionText").stringValue =
                $"Firearm playtest: {displayName}. Ctrl+Shift+G shortcuts available.";
            step.FindPropertyRelative("targetItemId").stringValue = targetItemId ?? string.Empty;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void BumpVersions()
        {
            CCS_SurvivalBootstrapVersionUtility.EnsureBundleVersionAtLeast(CCS_SurvivalBootstrapVersionUtility.CurrentMilestoneVersion);
        }
    }
}
