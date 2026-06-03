using System.Collections.Generic;
using System.IO;
using CCS.Modules.Economy;
using CCS.Modules.Farming;
using CCS.Modules.Inventory;
using CCS.Modules.Land;
using CCS.Modules.Playtesting;
using CCS.Survival.Composition;
using CCS.Survival.Editor.Development;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_LandOwnershipFoundationBootstrapSetup
// CATEGORY: Modules / Land / Editor / Validation
// PURPOSE: Batch-creates land claim content, vendor catalog, playtest steps, and wiring.
// PLACEMENT: Invoked by build verification / milestone bootstrap pipeline.
// AUTHOR: James Schilz
// CREATED: 2026-06-02
// NOTES: Milestone 2.3.0 — homestead claim deed and frontier homestead claim.
// =============================================================================

namespace CCS.Modules.Land.Editor
{
    public static class CCS_LandOwnershipFoundationBootstrapSetup
    {
        private const string LogPrefix = "[CCS_LandOwnershipFoundationBootstrapSetup]";
        private const string LandMilestoneVersion = "2.3.0";
        private const string LandProfilePath = "Assets/CCS/Survival/Profiles/Land/CCS_DefaultLandClaimProfile.asset";
        private const string LandContentRoot = "Assets/CCS/Survival/Content/Land";
        private const string ClaimsContentRoot = LandContentRoot + "/Claims";
        private const string ItemsContentRoot = LandContentRoot + "/Items";
        private const string GeneralStoreVendorPath = "Assets/CCS/Survival/Content/Vendors/CCS_Vendor_GeneralStore.asset";
        private const string InventoryProfilePath = "Assets/CCS/Survival/Profiles/Inventory/CCS_DefaultInventoryProfile.asset";
        private const string BootstrapRootPrefabPath = "Assets/CCS/Survival/Prefabs/PF_CCS_Survival_BootstrapRoot.prefab";
        private const string DefaultPlaytestProfilePath = "Assets/CCS/Survival/Profiles/Playtesting/CCS_DefaultPlaytestProfile.asset";

        public static void ExecuteBatch()
        {
            AssetDatabase.Refresh();
            EnsureFolders();

            CCS_ItemDefinition homesteadDeed = EnsureHomesteadClaimDeedItem();
            CCS_LandClaimDefinition homesteadClaim = EnsureFrontierHomesteadClaim(homesteadDeed);
            CCS_LandClaimProfile landProfile = EnsureLandClaimProfile(homesteadClaim);

            EnsureGeneralStoreLandCatalog(homesteadDeed);
            EnsureInventorySaveRestore(homesteadDeed);
            AssignLandClaimProfileToBootstrapHost(landProfile);
            EnsurePlaytestLandSteps();
            BumpVersions();

            AssetDatabase.SaveAssets();
            Debug.Log($"{LogPrefix} Land ownership foundation bootstrap setup complete ({LandMilestoneVersion}).");
            EditorApplication.Exit(0);
        }

        private static void EnsureFolders()
        {
            EnsureFolder("Assets/CCS/Survival/Profiles/Land");
            EnsureFolder(LandContentRoot);
            EnsureFolder(ClaimsContentRoot);
            EnsureFolder(ItemsContentRoot);
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

        private static CCS_ItemDefinition EnsureHomesteadClaimDeedItem()
        {
            string path = $"{ItemsContentRoot}/CCS_Item_HomesteadClaimDeed.asset";
            CCS_ItemDefinition item = LoadOrCreateItem(path);
            SerializedObject serialized = new SerializedObject(item);
            serialized.FindProperty("itemId").stringValue = CCS_LandContentIds.HomesteadClaimDeedItemId;
            serialized.FindProperty("displayName").stringValue = "Homestead Claim Deed";
            serialized.FindProperty("description").stringValue =
                "Frontier homestead land claim deed. Use to preview claim radius and confirm legal frontier presence.";
            serialized.FindProperty("category").enumValueIndex = (int)CCS_ItemCategory.Generic;
            serialized.FindProperty("gameplayKind").enumValueIndex = (int)CCS_ItemGameplayKind.Placeable;
            serialized.FindProperty("maxStackSize").intValue = 3;
            serialized.FindProperty("buyValue").intValue = 250;
            serialized.FindProperty("sellValue").intValue = 50;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(item);
            return item;
        }

        private static CCS_LandClaimDefinition EnsureFrontierHomesteadClaim(CCS_ItemDefinition deedItem)
        {
            string path = $"{ClaimsContentRoot}/CCS_LandClaim_FrontierHomestead.asset";
            CCS_LandClaimDefinition definition = AssetDatabase.LoadAssetAtPath<CCS_LandClaimDefinition>(path);
            if (definition == null)
            {
                definition = ScriptableObject.CreateInstance<CCS_LandClaimDefinition>();
                AssetDatabase.CreateAsset(definition, path);
            }

            SerializedObject serialized = new SerializedObject(definition);
            serialized.FindProperty("claimDefinitionId").stringValue = CCS_LandContentIds.FrontierHomesteadClaimId;
            serialized.FindProperty("displayName").stringValue = "Frontier Homestead Claim";
            serialized.FindProperty("claimRadius").floatValue = 14f;
            serialized.FindProperty("maxStructuresPlaceholder").intValue = 32;
            serialized.FindProperty("registrationCost").intValue = 250;
            serialized.FindProperty("optionalRegionId").stringValue = string.Empty;
            serialized.FindProperty("claimDeedItem").objectReferenceValue = deedItem;
            serialized.FindProperty("placementForwardDistance").floatValue = 2f;
            serialized.FindProperty("placementMaxGroundRayDistance").floatValue = 12f;
            serialized.FindProperty("placementMaxSlopeAngle").floatValue = 30f;
            serialized.FindProperty("minimumClaimSeparation").floatValue = 4f;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(definition);
            return definition;
        }

        private static CCS_LandClaimProfile EnsureLandClaimProfile(CCS_LandClaimDefinition homesteadClaim)
        {
            CCS_LandClaimProfile profile = AssetDatabase.LoadAssetAtPath<CCS_LandClaimProfile>(LandProfilePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<CCS_LandClaimProfile>();
                AssetDatabase.CreateAsset(profile, LandProfilePath);
            }

            SerializedObject serialized = new SerializedObject(profile);
            serialized.FindProperty("profileId").stringValue = CCS_LandContentIds.DefaultProfileId;
            serialized.FindProperty("profileDisplayName").stringValue = "Default Land Claim Profile";
            serialized.FindProperty("profileDescription").stringValue =
                "Frontier homestead land claim profile for milestone 2.3.0.";
            SerializedProperty claimDefinitions = serialized.FindProperty("claimDefinitions");
            claimDefinitions.arraySize = 1;
            claimDefinitions.GetArrayElementAtIndex(0).objectReferenceValue = homesteadClaim;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
            return profile;
        }

        private static void EnsureGeneralStoreLandCatalog(CCS_ItemDefinition homesteadDeed)
        {
            CCS_VendorDefinition vendor = AssetDatabase.LoadAssetAtPath<CCS_VendorDefinition>(GeneralStoreVendorPath);
            if (vendor == null)
            {
                Debug.LogError($"{LogPrefix} Missing general store vendor.");
                EditorApplication.Exit(1);
                return;
            }

            SerializedObject serialized = new SerializedObject(vendor);
            serialized.FindProperty("description").stringValue =
                $"Frontier general store with homestead claim deeds ({LandMilestoneVersion}).";

            SerializedProperty catalogItems = serialized.FindProperty("vendorInventory").FindPropertyRelative("items");
            MergeVendorCatalogEntry(catalogItems, homesteadDeed, true, false, 250, 50);
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(vendor);
        }

        private static void MergeVendorCatalogEntry(
            SerializedProperty catalogItems,
            CCS_ItemDefinition item,
            bool allowBuy,
            bool allowSell,
            int buyOverride,
            int sellOverride)
        {
            if (item == null)
            {
                return;
            }

            for (int index = 0; index < catalogItems.arraySize; index++)
            {
                SerializedProperty entry = catalogItems.GetArrayElementAtIndex(index);
                if (entry.FindPropertyRelative("itemDefinition").objectReferenceValue != item)
                {
                    continue;
                }

                entry.FindPropertyRelative("stockQuantity").intValue = -1;
                entry.FindPropertyRelative("allowBuy").boolValue = allowBuy;
                entry.FindPropertyRelative("allowSell").boolValue = allowSell;
                if (allowBuy)
                {
                    entry.FindPropertyRelative("buyPriceOverride").intValue = buyOverride;
                }

                if (allowSell)
                {
                    entry.FindPropertyRelative("sellPriceOverride").intValue = sellOverride;
                }

                return;
            }

            int newIndex = catalogItems.arraySize;
            catalogItems.InsertArrayElementAtIndex(newIndex);
            SerializedProperty newEntry = catalogItems.GetArrayElementAtIndex(newIndex);
            newEntry.FindPropertyRelative("itemDefinition").objectReferenceValue = item;
            newEntry.FindPropertyRelative("stockQuantity").intValue = -1;
            newEntry.FindPropertyRelative("allowBuy").boolValue = allowBuy;
            newEntry.FindPropertyRelative("allowSell").boolValue = allowSell;
            newEntry.FindPropertyRelative("buyPriceOverride").intValue = allowBuy ? buyOverride : -1;
            newEntry.FindPropertyRelative("sellPriceOverride").intValue = allowSell ? sellOverride : -1;
        }

        private static CCS_ItemDefinition LoadOrCreateItem(string path)
        {
            CCS_ItemDefinition item = AssetDatabase.LoadAssetAtPath<CCS_ItemDefinition>(path);
            if (item == null)
            {
                item = ScriptableObject.CreateInstance<CCS_ItemDefinition>();
                AssetDatabase.CreateAsset(item, path);
            }

            return item;
        }

        private static void EnsureInventorySaveRestore(params CCS_ItemDefinition[] landItems)
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

            for (int index = 0; index < landItems.Length; index++)
            {
                CCS_ItemDefinition item = landItems[index];
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

        private static void AssignLandClaimProfileToBootstrapHost(CCS_LandClaimProfile profile)
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
            serialized.FindProperty("landClaimProfile").objectReferenceValue = profile;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(host);
            PrefabUtility.SavePrefabAsset(prefabRoot);
        }

        private static void EnsurePlaytestLandSteps()
        {
            CCS_PlaytestProfile profile =
                AssetDatabase.LoadAssetAtPath<CCS_PlaytestProfile>(DefaultPlaytestProfilePath);
            if (profile == null)
            {
                return;
            }

            InsertStep(
                profile,
                "ccs.survival.playtest.land.buy.deed",
                "Buy homestead claim deed",
                CCS_PlaytestStepType.BuyHomesteadClaimDeed,
                CCS_LandContentIds.HomesteadClaimDeedItemId);
            InsertStep(
                profile,
                "ccs.survival.playtest.land.place.claim",
                "Place land claim",
                CCS_PlaytestStepType.PlaceLandClaim,
                CCS_LandContentIds.HomesteadClaimDeedItemId);
            InsertStep(
                profile,
                "ccs.survival.playtest.land.place.structure",
                "Place farm plot inside claim",
                CCS_PlaytestStepType.PlaceStructureInsideClaim,
                CCS_FarmingContentIds.FarmPlotKitItemId);
            InsertStep(
                profile,
                "ccs.survival.playtest.land.verify.association",
                "Verify structure associated with claim",
                CCS_PlaytestStepType.VerifyStructureAssociatedWithClaim,
                string.Empty);
            InsertStep(
                profile,
                "ccs.survival.playtest.land.save",
                "Save land claim state",
                CCS_PlaytestStepType.SaveLandClaimState,
                string.Empty);
            InsertStep(
                profile,
                "ccs.survival.playtest.land.verify.load",
                "Verify land claim restored after load",
                CCS_PlaytestStepType.VerifyLandClaimAfterLoad,
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
                $"Land ownership playtest: {displayName}. Ctrl+Shift+L shortcuts available.";
            step.FindPropertyRelative("targetItemId").stringValue = targetItemId ?? string.Empty;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void BumpVersions()
        {
            CCS_SurvivalBootstrapVersionUtility.EnsureBundleVersionAtLeast(LandMilestoneVersion);
        }
    }
}
