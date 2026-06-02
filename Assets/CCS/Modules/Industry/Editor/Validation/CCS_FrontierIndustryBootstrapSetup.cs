using System.IO;
using CCS.Modules.Crafting;
using CCS.Modules.Economy;
using CCS.Modules.Inventory;
using CCS.Modules.Industry;
using CCS.Modules.Playtesting;
using CCS.Modules.Shelter;
using CCS.Survival.Composition;
using UnityEditor;
using UnityEngine;

namespace CCS.Modules.Industry.Editor
{
    public static class CCS_FrontierIndustryBootstrapSetup
    {
        private const string LogPrefix = "[CCS_FrontierIndustryBootstrapSetup]";
        private const string IndustryProfilePath = "Assets/CCS/Survival/Profiles/Industry/CCS_DefaultIndustryProfile.asset";
        private const string IndustryContentRoot = "Assets/CCS/Survival/Content/Industry";
        private const string IndustryItemsRoot = "Assets/CCS/Survival/Content/Items/Industry";
        private const string FrontierItemsRoot = "Assets/CCS/Survival/Content/Items/Frontier";
        private const string FrontierStructuresRoot = "Assets/CCS/Survival/Content/Structures/Frontier";
        private const string CampProfilePath = "Assets/CCS/Survival/Profiles/Camp/CCS_DefaultCampDefinition.asset";
        private const string CampTierProfilePath = "Assets/CCS/Survival/Profiles/Camp/CCS_DefaultCampTierProfile.asset";
        private const string CraftingProfilePath = "Assets/CCS/Survival/Profiles/Crafting/CCS_DefaultCraftingProfile.asset";
        private const string GeneralStoreVendorPath = "Assets/CCS/Survival/Content/Vendors/CCS_Vendor_GeneralStore.asset";
        private const string BootstrapRootPrefabPath = "Assets/CCS/Survival/Prefabs/PF_CCS_Survival_BootstrapRoot.prefab";
        private const string DefaultPlaytestProfilePath = "Assets/CCS/Survival/Profiles/Playtesting/CCS_DefaultPlaytestProfile.asset";
        private const string WoodItemPath = "Assets/CCS/Survival/Content/Items/Resources/Primitive/CCS_Item_Wood.asset";
        private const string SaplingItemPath = "Assets/CCS/Survival/Content/Items/Resources/Frontier/CCS_Item_Sapling.asset";
        private const string CharcoalItemPath = "Assets/CCS/Survival/Content/Items/Progression/CCS_Item_Charcoal.asset";
        private const string WorkbenchKitPath = FrontierItemsRoot + "/CCS_Item_WorkbenchKit.asset";
        private const string HomesteadPersistenceStepId = "ccs.survival.playtest.homestead.verify.load";

        public static void ExecuteBatch()
        {
            AssetDatabase.Refresh();
            EnsureFolders();

            CCS_ItemDefinition wood = LoadItem(WoodItemPath);
            CCS_ItemDefinition sapling = LoadItem(SaplingItemPath);
            CCS_ItemDefinition charcoal = LoadItem(CharcoalItemPath);
            if (wood == null || sapling == null || charcoal == null)
            {
                Debug.LogError($"{LogPrefix} Missing prerequisite resource items.");
                EditorApplication.Exit(1);
                return;
            }

            CCS_ItemDefinition lumber = EnsureMaterialItem("CCS_Item_Lumber", "ccs.survival.item.resource.lumber", "Lumber");
            CCS_ItemDefinition poles = EnsureMaterialItem("CCS_Item_Poles", "ccs.survival.item.resource.poles", "Poles");
            CCS_ItemDefinition ironOre = EnsureMaterialItem("CCS_Item_IronOre", "ccs.survival.item.resource.ironore", "Iron Ore");
            CCS_ItemDefinition refinedIron = EnsureMaterialItem(
                "CCS_Item_RefinedIron",
                "ccs.survival.item.resource.refinediron",
                "Refined Iron");
            CCS_ItemDefinition ironBar = EnsureMaterialItem("CCS_Item_IronBar", "ccs.survival.item.resource.ironbar", "Iron Bar");
            CCS_ItemDefinition ironHatchetHead = EnsureMaterialItem(
                "CCS_Item_IronHatchetHead",
                "ccs.survival.item.industry.ironhatchethead",
                "Iron Hatchet Head");
            CCS_ItemDefinition ironPickHead = EnsureMaterialItem(
                "CCS_Item_IronPickHead",
                "ccs.survival.item.industry.ironpickhead",
                "Iron Pick Head");
            CCS_ItemDefinition ironNails = EnsureMaterialItem(
                "CCS_Item_IronNails",
                "ccs.survival.item.resource.nails",
                "Iron Nails",
                "Assets/CCS/Survival/Content/Items/Resources/Frontier/CCS_Item_Nails.asset");
            CCS_ItemDefinition horseshoe = EnsureMaterialItem(
                "CCS_Item_Horseshoe",
                "ccs.survival.item.industry.horseshoe",
                "Horseshoe (Future)");

            CCS_ItemDefinition ironHatchet = EnsureIronTool(
                "CCS_Item_IronHatchet",
                "ccs.survival.item.tool.hatchet.iron",
                "Iron Hatchet",
                CCS_ItemToolType.Axe,
                CCS_ToolArchetype.Hatchet);
            CCS_ItemDefinition ironPick = EnsureIronTool(
                "CCS_Item_IronPick",
                "ccs.survival.item.tool.pick.iron",
                "Iron Pick",
                CCS_ItemToolType.Pickaxe,
                CCS_ToolArchetype.Pick);
            CCS_ItemDefinition ironKnife = EnsureIronTool(
                "CCS_Item_IronKnife",
                "ccs.survival.item.tool.knife.iron",
                "Iron Knife",
                CCS_ItemToolType.Knife,
                CCS_ToolArchetype.Knife);

            CCS_IndustryDefinition woodToLumber = EnsureProcess(
                "CCS_IndustryProcess_WoodToLumber",
                "ccs.survival.industry.process.wood.lumber",
                "Wood to Lumber",
                CCS_IndustryWorkstationRole.SawTable,
                wood,
                1,
                lumber,
                1);
            CCS_IndustryDefinition saplingToPoles = EnsureProcess(
                "CCS_IndustryProcess_SaplingToPoles",
                "ccs.survival.industry.process.sapling.poles",
                "Sapling to Poles",
                CCS_IndustryWorkstationRole.SawTable,
                sapling,
                1,
                poles,
                1);
            CCS_IndustryDefinition woodToCharcoal = EnsureProcess(
                "CCS_IndustryProcess_WoodToCharcoal",
                "ccs.survival.industry.process.wood.charcoal",
                "Wood to Charcoal",
                CCS_IndustryWorkstationRole.CharcoalKiln,
                wood,
                2,
                charcoal,
                1);
            CCS_IndustryDefinition oreToIron = EnsureProcess(
                "CCS_IndustryProcess_IronOreToRefinedIron",
                "ccs.survival.industry.process.ironore.refinediron",
                "Refine Iron Ore",
                CCS_IndustryWorkstationRole.PrimitiveForge,
                ironOre,
                2,
                refinedIron,
                1);

            CCS_IndustryDefinition coalPlaceholder = EnsureFutureProcess("CCS_IndustryProcess_CoalPlaceholder", "Coal (Future)");
            CCS_IndustryDefinition sulfurPlaceholder = EnsureFutureProcess("CCS_IndustryProcess_SulfurPlaceholder", "Sulfur (Future)");
            CCS_IndustryDefinition saltpeterPlaceholder = EnsureFutureProcess("CCS_IndustryProcess_SaltpeterPlaceholder", "Saltpeter (Future)");

            CCS_ItemDefinition sawTableKit = EnsureKit("CCS_Item_SawTableKit", "ccs.survival.item.frontier.sawtablekit", "Saw Table Kit");
            CCS_ItemDefinition kilnKit = EnsureKit("CCS_Item_CharcoalKilnKit", "ccs.survival.item.frontier.charcoalkilnkit", "Charcoal Kiln Kit");
            CCS_ItemDefinition forgeKit = EnsureKit("CCS_Item_PrimitiveForgeKit", "ccs.survival.item.frontier.primitiveforgekit", "Primitive Forge Kit");
            CCS_ItemDefinition workbenchKit = LoadItem(WorkbenchKitPath);

            CCS_WorkbenchDefinition frontierWorkbench = EnsureWorkbench(
                "CCS_WorkbenchDefinition_FrontierWorkbench",
                "ccs.survival.workbench.frontier",
                "Frontier Workbench",
                workbenchKit,
                CCS_IndustryWorkstationRole.FrontierWorkbench,
                CCS_CampStructureKind.WorkArea,
                PrimitiveType.Cube,
                new Vector3(1.6f, 0.9f, 1.2f));
            CCS_WorkbenchDefinition sawTable = EnsureWorkbench(
                "CCS_WorkbenchDefinition_SawTable",
                "ccs.survival.workbench.sawtable",
                "Saw Table",
                sawTableKit,
                CCS_IndustryWorkstationRole.SawTable,
                CCS_CampStructureKind.SawTable,
                PrimitiveType.Cube,
                new Vector3(1.8f, 0.7f, 1f));
            CCS_WorkbenchDefinition charcoalKiln = EnsureWorkbench(
                "CCS_WorkbenchDefinition_CharcoalKiln",
                "ccs.survival.workbench.charcoalkiln",
                "Charcoal Kiln",
                kilnKit,
                CCS_IndustryWorkstationRole.CharcoalKiln,
                CCS_CampStructureKind.CharcoalKiln,
                PrimitiveType.Cylinder,
                new Vector3(1.2f, 1.4f, 1.2f));
            CCS_WorkbenchDefinition primitiveForge = EnsureWorkbench(
                "CCS_WorkbenchDefinition_PrimitiveForge",
                "ccs.survival.workbench.primitiveforge",
                "Primitive Forge",
                forgeKit,
                CCS_IndustryWorkstationRole.PrimitiveForge,
                CCS_CampStructureKind.PrimitiveForge,
                PrimitiveType.Cube,
                new Vector3(1.4f, 1f, 1.4f));

            CCS_CraftingRecipeDefinition hatchetHeadRecipe = EnsureForgeRecipe(
                "CCS_CraftingRecipe_IronHatchetHead",
                "ccs.survival.crafting.forge.ironhatchethead",
                "Iron Hatchet Head",
                refinedIron,
                2,
                ironHatchetHead,
                1);
            CCS_CraftingRecipeDefinition pickHeadRecipe = EnsureForgeRecipe(
                "CCS_CraftingRecipe_IronPickHead",
                "ccs.survival.crafting.forge.ironpickhead",
                "Iron Pick Head",
                refinedIron,
                2,
                ironPickHead,
                1);
            CCS_CraftingRecipeDefinition nailsRecipe = EnsureForgeRecipe(
                "CCS_CraftingRecipe_IronNails",
                "ccs.survival.crafting.forge.ironnails",
                "Iron Nails",
                refinedIron,
                1,
                ironNails,
                4);
            CCS_CraftingRecipeDefinition horseshoeRecipe = EnsureForgeRecipe(
                "CCS_CraftingRecipe_Horseshoe",
                "ccs.survival.crafting.forge.horseshoe",
                "Horseshoe",
                refinedIron,
                1,
                horseshoe,
                1);

            CCS_BlacksmithRecipeDefinition blacksmithHatchet = EnsureBlacksmithRecipe(
                hatchetHeadRecipe,
                "ccs.survival.industry.blacksmith.ironhatchethead",
                CCS_BlacksmithRecipeCategory.Tool);
            CCS_BlacksmithRecipeDefinition blacksmithPick = EnsureBlacksmithRecipe(
                pickHeadRecipe,
                "ccs.survival.industry.blacksmith.ironpickhead",
                CCS_BlacksmithRecipeCategory.Tool);
            CCS_BlacksmithRecipeDefinition blacksmithNails = EnsureBlacksmithRecipe(
                nailsRecipe,
                "ccs.survival.industry.blacksmith.ironnails",
                CCS_BlacksmithRecipeCategory.Hardware);
            CCS_BlacksmithRecipeDefinition blacksmithHorseshoe = EnsureBlacksmithRecipe(
                horseshoeRecipe,
                "ccs.survival.industry.blacksmith.horseshoe",
                CCS_BlacksmithRecipeCategory.Utility);

            CCS_CampTierProfile tierProfile = EnsureCampTierProfile();
            CCS_CampDefinition campDefinition = EnsureCampDefinition(
                tierProfile,
                frontierWorkbench,
                sawTable,
                charcoalKiln,
                primitiveForge);
            CCS_IndustryProfile industryProfile = EnsureIndustryProfile(
                woodToLumber,
                saplingToPoles,
                woodToCharcoal,
                oreToIron,
                coalPlaceholder,
                sulfurPlaceholder,
                saltpeterPlaceholder,
                blacksmithHatchet,
                blacksmithPick,
                blacksmithNails,
                blacksmithHorseshoe);

            AssignIndustryProfileToBootstrapHost(industryProfile);
            EnsureCraftingProgressionRecipes(
                hatchetHeadRecipe,
                pickHeadRecipe,
                nailsRecipe,
                horseshoeRecipe);
            EnsureGeneralStoreIndustryCatalog(lumber, ironBar, charcoal, ironNails, sawTableKit, kilnKit, forgeKit);
            EnsurePlaytestIndustrySteps();
            BumpVersions();
            BumpEconomyProfileVersion();

            AssetDatabase.SaveAssets();
            Debug.Log($"{LogPrefix} Frontier industry bootstrap setup complete (1.5.0).");
            EditorApplication.Exit(0);
        }

        private static void EnsureFolders()
        {
            EnsureFolder(IndustryContentRoot);
            EnsureFolder(IndustryContentRoot + "/Processes");
            EnsureFolder(IndustryContentRoot + "/Blacksmith");
            EnsureFolder(IndustryItemsRoot);
            EnsureFolder("Assets/CCS/Survival/Profiles/Industry");
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

        private static CCS_ItemDefinition LoadItem(string path)
        {
            return AssetDatabase.LoadAssetAtPath<CCS_ItemDefinition>(path);
        }

        private static CCS_ItemDefinition EnsureMaterialItem(
            string assetName,
            string itemId,
            string displayName,
            string existingPath = null)
        {
            string path = existingPath ?? $"{IndustryItemsRoot}/{assetName}.asset";
            CCS_ItemDefinition item = AssetDatabase.LoadAssetAtPath<CCS_ItemDefinition>(path);
            if (item == null)
            {
                item = ScriptableObject.CreateInstance<CCS_ItemDefinition>();
                AssetDatabase.CreateAsset(item, path);
            }

            SerializedObject serialized = new SerializedObject(item);
            serialized.FindProperty("itemId").stringValue = itemId;
            serialized.FindProperty("displayName").stringValue = displayName;
            serialized.FindProperty("category").enumValueIndex = (int)CCS_ItemCategory.Material;
            serialized.FindProperty("gameplayKind").enumValueIndex = (int)CCS_ItemGameplayKind.Generic;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(item);
            return item;
        }

        private static CCS_ItemDefinition EnsureIronTool(
            string assetName,
            string itemId,
            string displayName,
            CCS_ItemToolType toolType,
            CCS_ToolArchetype toolArchetype)
        {
            CCS_ItemDefinition item = EnsureMaterialItem(assetName, itemId, displayName);
            SerializedObject serialized = new SerializedObject(item);
            serialized.FindProperty("category").enumValueIndex = (int)CCS_ItemCategory.Tool;
            serialized.FindProperty("gameplayKind").enumValueIndex = (int)CCS_ItemGameplayKind.Tool;
            serialized.FindProperty("hasToolIdentity").boolValue = true;
            serialized.FindProperty("toolType").enumValueIndex = (int)toolType;
            serialized.FindProperty("toolArchetype").enumValueIndex = (int)toolArchetype;
            serialized.FindProperty("toolTier").enumValueIndex = 4;
            serialized.FindProperty("hasEconomyValues").boolValue = true;
            serialized.FindProperty("buyValue").intValue = 45;
            serialized.FindProperty("sellValue").intValue = 12;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(item);
            return item;
        }

        private static CCS_ItemDefinition EnsureKit(string assetName, string itemId, string displayName)
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
                "Frontier industry placement kit. Use to preview and place on level ground.";
            serialized.FindProperty("category").enumValueIndex = (int)CCS_ItemCategory.Generic;
            serialized.FindProperty("gameplayKind").enumValueIndex = (int)CCS_ItemGameplayKind.Placeable;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(item);
            return item;
        }

        private static CCS_IndustryDefinition EnsureProcess(
            string assetName,
            string processId,
            string displayName,
            string workstationRole,
            CCS_ItemDefinition input,
            int inputQty,
            CCS_ItemDefinition output,
            int outputQty)
        {
            string path = $"{IndustryContentRoot}/Processes/{assetName}.asset";
            CCS_IndustryDefinition definition = AssetDatabase.LoadAssetAtPath<CCS_IndustryDefinition>(path);
            if (definition == null)
            {
                definition = ScriptableObject.CreateInstance<CCS_IndustryDefinition>();
                AssetDatabase.CreateAsset(definition, path);
            }

            SerializedObject serialized = new SerializedObject(definition);
            serialized.FindProperty("processId").stringValue = processId;
            serialized.FindProperty("displayName").stringValue = displayName;
            serialized.FindProperty("requiredWorkstationRoleId").stringValue = workstationRole;
            serialized.FindProperty("processTimeSeconds").floatValue = 0f;
            serialized.FindProperty("isFuturePlaceholder").boolValue = false;

            SetProcessStack(serialized.FindProperty("inputs"), input, inputQty);
            SetProcessStack(serialized.FindProperty("outputs"), output, outputQty);
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(definition);
            return definition;
        }

        private static CCS_IndustryDefinition EnsureFutureProcess(string assetName, string displayName)
        {
            string path = $"{IndustryContentRoot}/Processes/{assetName}.asset";
            CCS_IndustryDefinition definition = AssetDatabase.LoadAssetAtPath<CCS_IndustryDefinition>(path);
            if (definition == null)
            {
                definition = ScriptableObject.CreateInstance<CCS_IndustryDefinition>();
                AssetDatabase.CreateAsset(definition, path);
            }

            SerializedObject serialized = new SerializedObject(definition);
            serialized.FindProperty("processId").stringValue = $"ccs.survival.industry.process.{assetName.ToLowerInvariant()}";
            serialized.FindProperty("displayName").stringValue = displayName;
            serialized.FindProperty("isFuturePlaceholder").boolValue = true;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(definition);
            return definition;
        }

        private static void SetProcessStack(SerializedProperty listProperty, CCS_ItemDefinition item, int quantity)
        {
            listProperty.arraySize = 1;
            SerializedProperty stack = listProperty.GetArrayElementAtIndex(0);
            stack.FindPropertyRelative("itemDefinition").objectReferenceValue = item;
            stack.FindPropertyRelative("quantity").intValue = quantity;
        }

        private static CCS_WorkbenchDefinition EnsureWorkbench(
            string assetName,
            string definitionId,
            string displayName,
            CCS_ItemDefinition kit,
            string industryRole,
            CCS_CampStructureKind campKind,
            PrimitiveType primitive,
            Vector3 scale)
        {
            string path = $"{FrontierStructuresRoot}/{assetName}.asset";
            CCS_WorkbenchDefinition definition = AssetDatabase.LoadAssetAtPath<CCS_WorkbenchDefinition>(path);
            if (definition == null)
            {
                definition = ScriptableObject.CreateInstance<CCS_WorkbenchDefinition>();
                AssetDatabase.CreateAsset(definition, path);
            }

            SerializedObject serialized = new SerializedObject(definition);
            serialized.FindProperty("workbenchDefinitionId").stringValue = definitionId;
            serialized.FindProperty("displayName").stringValue = displayName;
            serialized.FindProperty("placeableKitItem").objectReferenceValue = kit;
            serialized.FindProperty("industryWorkstationRoleId").stringValue = industryRole;
            serialized.FindProperty("contributesToCampTier").boolValue = true;
            serialized.FindProperty("campStructureKind").enumValueIndex = (int)campKind;
            serialized.FindProperty("placementPrimitive").enumValueIndex = (int)primitive;
            serialized.FindProperty("placedLocalScale").vector3Value = scale;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(definition);
            return definition;
        }

        private static CCS_CraftingRecipeDefinition EnsureForgeRecipe(
            string assetName,
            string recipeId,
            string displayName,
            CCS_ItemDefinition input,
            int inputQty,
            CCS_ItemDefinition output,
            int outputQty)
        {
            string path = $"Assets/CCS/Survival/Profiles/Crafting/IndustryRecipes/{assetName}.asset";
            EnsureFolder("Assets/CCS/Survival/Profiles/Crafting/IndustryRecipes");
            CCS_CraftingRecipeDefinition recipe = AssetDatabase.LoadAssetAtPath<CCS_CraftingRecipeDefinition>(path);
            if (recipe == null)
            {
                recipe = ScriptableObject.CreateInstance<CCS_CraftingRecipeDefinition>();
                AssetDatabase.CreateAsset(recipe, path);
            }

            SerializedObject serialized = new SerializedObject(recipe);
            serialized.FindProperty("recipeId").stringValue = recipeId;
            serialized.FindProperty("displayName").stringValue = displayName;
            serialized.FindProperty("requiredStationType").enumValueIndex = (int)CCS_CraftingStationType.Forge;
            serialized.FindProperty("craftTimeSeconds").floatValue = 0f;
            serialized.FindProperty("isUnlockedByDefault").boolValue = true;

            SerializedProperty ingredients = serialized.FindProperty("ingredients");
            ingredients.arraySize = 1;
            ingredients.GetArrayElementAtIndex(0).FindPropertyRelative("itemDefinition").objectReferenceValue = input;
            ingredients.GetArrayElementAtIndex(0).FindPropertyRelative("quantity").intValue = inputQty;

            SerializedProperty results = serialized.FindProperty("results");
            results.arraySize = 1;
            results.GetArrayElementAtIndex(0).FindPropertyRelative("itemDefinition").objectReferenceValue = output;
            results.GetArrayElementAtIndex(0).FindPropertyRelative("quantity").intValue = outputQty;

            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(recipe);
            return recipe;
        }

        private static CCS_BlacksmithRecipeDefinition EnsureBlacksmithRecipe(
            CCS_CraftingRecipeDefinition craftingRecipe,
            string blacksmithId,
            CCS_BlacksmithRecipeCategory category)
        {
            string path = $"{IndustryContentRoot}/Blacksmith/{craftingRecipe.name}.asset";
            CCS_BlacksmithRecipeDefinition definition =
                AssetDatabase.LoadAssetAtPath<CCS_BlacksmithRecipeDefinition>(path);
            if (definition == null)
            {
                definition = ScriptableObject.CreateInstance<CCS_BlacksmithRecipeDefinition>();
                AssetDatabase.CreateAsset(definition, path);
            }

            SerializedObject serialized = new SerializedObject(definition);
            serialized.FindProperty("blacksmithRecipeId").stringValue = blacksmithId;
            serialized.FindProperty("category").enumValueIndex = (int)category;
            serialized.FindProperty("craftingRecipe").objectReferenceValue = craftingRecipe;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(definition);
            return definition;
        }

        private static CCS_CampTierProfile EnsureCampTierProfile()
        {
            CCS_CampTierProfile profile = AssetDatabase.LoadAssetAtPath<CCS_CampTierProfile>(CampTierProfilePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<CCS_CampTierProfile>();
                AssetDatabase.CreateAsset(profile, CampTierProfilePath);
            }

            SerializedObject serialized = new SerializedObject(profile);
            serialized.FindProperty("profileVersion").stringValue = "1.5.0";
            serialized.FindProperty("profileDescription").stringValue =
                "Frontier camp tiers through IndustrialHomestead (1.5.0).";
            SerializedProperty tiers = serialized.FindProperty("tierDefinitions");
            tiers.arraySize = 4;
            ConfigureTier(tiers.GetArrayElementAtIndex(0), CCS_CampTier.TemporaryCamp, "Temporary Camp", CCS_CampTier.None,
                new[] { CCS_CampStructureKind.Shelter, CCS_CampStructureKind.Campfire, CCS_CampStructureKind.Bedroll });
            ConfigureTier(tiers.GetArrayElementAtIndex(1), CCS_CampTier.FrontierCamp, "Frontier Camp", CCS_CampTier.TemporaryCamp,
                new[] { CCS_CampStructureKind.Storage });
            ConfigureTier(tiers.GetArrayElementAtIndex(2), CCS_CampTier.FrontierHomestead, "Frontier Homestead", CCS_CampTier.FrontierCamp,
                new[] { CCS_CampStructureKind.WorkArea });
            ConfigureTier(tiers.GetArrayElementAtIndex(3), CCS_CampTier.IndustrialHomestead, "Industrial Homestead", CCS_CampTier.FrontierHomestead,
                new[] { CCS_CampStructureKind.PrimitiveForge });
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
            return profile;
        }

        private static void ConfigureTier(
            SerializedProperty tierProperty,
            CCS_CampTier tier,
            string displayName,
            CCS_CampTier prerequisite,
            CCS_CampStructureKind[] requirements)
        {
            tierProperty.FindPropertyRelative("campTier").enumValueIndex = (int)tier;
            tierProperty.FindPropertyRelative("displayName").stringValue = displayName;
            tierProperty.FindPropertyRelative("prerequisiteTier").enumValueIndex = (int)prerequisite;
            SerializedProperty requirementList = tierProperty.FindPropertyRelative("requirements");
            requirementList.arraySize = requirements.Length;
            for (int index = 0; index < requirements.Length; index++)
            {
                requirementList.GetArrayElementAtIndex(index).FindPropertyRelative("structureKind").enumValueIndex =
                    (int)requirements[index];
            }
        }

        private static CCS_CampDefinition EnsureCampDefinition(
            CCS_CampTierProfile tierProfile,
            params CCS_WorkbenchDefinition[] workbenches)
        {
            CCS_CampDefinition definition = AssetDatabase.LoadAssetAtPath<CCS_CampDefinition>(CampProfilePath);
            if (definition == null)
            {
                definition = ScriptableObject.CreateInstance<CCS_CampDefinition>();
                AssetDatabase.CreateAsset(definition, CampProfilePath);
            }

            SerializedObject serialized = new SerializedObject(definition);
            serialized.FindProperty("profileVersion").stringValue = "1.5.0";
            serialized.FindProperty("profileDescription").stringValue =
                "Frontier camp with industry workstations and IndustrialHomestead tier (1.5.0).";
            serialized.FindProperty("campTierProfile").objectReferenceValue = tierProfile;
            SerializedProperty workbenchList = serialized.FindProperty("workbenchDefinitions");
            workbenchList.arraySize = workbenches.Length;
            for (int index = 0; index < workbenches.Length; index++)
            {
                workbenchList.GetArrayElementAtIndex(index).objectReferenceValue = workbenches[index];
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(definition);
            return definition;
        }

        private static CCS_IndustryProfile EnsureIndustryProfile(
            CCS_IndustryDefinition woodToLumber,
            CCS_IndustryDefinition saplingToPoles,
            CCS_IndustryDefinition woodToCharcoal,
            CCS_IndustryDefinition oreToIron,
            CCS_IndustryDefinition coalPlaceholder,
            CCS_IndustryDefinition sulfurPlaceholder,
            CCS_IndustryDefinition saltpeterPlaceholder,
            params CCS_BlacksmithRecipeDefinition[] blacksmithRecipes)
        {
            CCS_IndustryProfile profile = AssetDatabase.LoadAssetAtPath<CCS_IndustryProfile>(IndustryProfilePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<CCS_IndustryProfile>();
                AssetDatabase.CreateAsset(profile, IndustryProfilePath);
            }

            SerializedObject serialized = new SerializedObject(profile);
            serialized.FindProperty("profileVersion").stringValue = "1.5.0";
            serialized.FindProperty("profileDescription").stringValue =
                "Frontier industry processing and primitive forge catalog (1.5.0).";
            SerializedProperty processes = serialized.FindProperty("processDefinitions");
            processes.arraySize = 4;
            processes.GetArrayElementAtIndex(0).objectReferenceValue = woodToLumber;
            processes.GetArrayElementAtIndex(1).objectReferenceValue = saplingToPoles;
            processes.GetArrayElementAtIndex(2).objectReferenceValue = woodToCharcoal;
            processes.GetArrayElementAtIndex(3).objectReferenceValue = oreToIron;
            serialized.FindProperty("coalPlaceholder").objectReferenceValue = coalPlaceholder;
            serialized.FindProperty("sulfurPlaceholder").objectReferenceValue = sulfurPlaceholder;
            serialized.FindProperty("saltpeterPlaceholder").objectReferenceValue = saltpeterPlaceholder;

            SerializedProperty blacksmithList = serialized.FindProperty("blacksmithRecipes");
            blacksmithList.arraySize = blacksmithRecipes.Length;
            for (int index = 0; index < blacksmithRecipes.Length; index++)
            {
                blacksmithList.GetArrayElementAtIndex(index).objectReferenceValue = blacksmithRecipes[index];
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
            return profile;
        }

        private static void AssignIndustryProfileToBootstrapHost(CCS_IndustryProfile profile)
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
            serialized.FindProperty("industryProfile").objectReferenceValue = profile;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(host);
            PrefabUtility.SavePrefabAsset(prefabRoot);
        }

        private static void EnsureCraftingProgressionRecipes(params CCS_CraftingRecipeDefinition[] recipes)
        {
            const string progressionProfilePath =
                "Assets/CCS/Survival/Profiles/Crafting/CCS_DefaultCraftingProgressionProfile.asset";
            CCS_CraftingProgressionProfile profile =
                AssetDatabase.LoadAssetAtPath<CCS_CraftingProgressionProfile>(progressionProfilePath);
            if (profile == null)
            {
                return;
            }

            SerializedObject serialized = new SerializedObject(profile);
            SerializedProperty recipeList = serialized.FindProperty("progressionRecipes");
            int start = recipeList.arraySize;
            recipeList.arraySize = start + recipes.Length;
            for (int index = 0; index < recipes.Length; index++)
            {
                SerializedProperty entry = recipeList.GetArrayElementAtIndex(start + index);
                entry.FindPropertyRelative("recipeDefinition").objectReferenceValue = recipes[index];
                entry.FindPropertyRelative("unlockTier").intValue = 1;
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
        }

        private static void EnsureGeneralStoreIndustryCatalog(
            CCS_ItemDefinition lumber,
            CCS_ItemDefinition ironBar,
            CCS_ItemDefinition charcoal,
            CCS_ItemDefinition nails,
            CCS_ItemDefinition sawTableKit,
            CCS_ItemDefinition kilnKit,
            CCS_ItemDefinition forgeKit)
        {
            CCS_VendorDefinition vendor = AssetDatabase.LoadAssetAtPath<CCS_VendorDefinition>(GeneralStoreVendorPath);
            if (vendor == null)
            {
                return;
            }

            SerializedObject serialized = new SerializedObject(vendor);
            serialized.FindProperty("description").stringValue =
                "Frontier general store (1.5.0). Homestead and industry trade catalog.";
            SerializedProperty items = serialized.FindProperty("vendorInventory").FindPropertyRelative("items");
            MergeVendorRow(items, lumber, true, false, 8);
            MergeVendorRow(items, ironBar, true, false, 22);
            MergeVendorRow(items, charcoal, true, false, 6);
            MergeVendorRow(items, nails, true, false, 5);
            MergeVendorRow(items, sawTableKit, true, false, 16);
            MergeVendorRow(items, kilnKit, true, false, 18);
            MergeVendorRow(items, forgeKit, true, false, 28);
            MergeVendorRow(items, lumber, false, true, 0, 4);
            MergeVendorRow(items, charcoal, false, true, 0, 2);
            MergeVendorRow(items, ironBar, false, true, 0, 10);
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(vendor);
        }

        private static void MergeVendorRow(
            SerializedProperty items,
            CCS_ItemDefinition item,
            bool allowBuy,
            bool allowSell,
            int buyPrice,
            int sellPrice = 4)
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
                entry.FindPropertyRelative("buyPriceOverride").intValue = allowBuy ? buyPrice : -1;
                entry.FindPropertyRelative("sellPriceOverride").intValue = allowSell ? sellPrice : -1;
                return;
            }

            int newIndex = items.arraySize;
            items.InsertArrayElementAtIndex(newIndex);
            SerializedProperty newEntry = items.GetArrayElementAtIndex(newIndex);
            newEntry.FindPropertyRelative("itemDefinition").objectReferenceValue = item;
            newEntry.FindPropertyRelative("stockQuantity").intValue = -1;
            newEntry.FindPropertyRelative("allowBuy").boolValue = allowBuy;
            newEntry.FindPropertyRelative("allowSell").boolValue = allowSell;
            newEntry.FindPropertyRelative("buyPriceOverride").intValue = allowBuy ? buyPrice : -1;
            newEntry.FindPropertyRelative("sellPriceOverride").intValue = allowSell ? sellPrice : -1;
        }

        private static void BumpEconomyProfileVersion()
        {
            const string economyProfilePath = "Assets/CCS/Survival/Profiles/Economy/CCS_DefaultEconomyProfile.asset";
            CCS_EconomyProfile profile = AssetDatabase.LoadAssetAtPath<CCS_EconomyProfile>(economyProfilePath);
            if (profile == null)
            {
                return;
            }

            SerializedObject serialized = new SerializedObject(profile);
            serialized.FindProperty("profileVersion").stringValue = "1.5.0";
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
        }

        private static void EnsurePlaytestIndustrySteps()
        {
            CCS_PlaytestProfile profile = AssetDatabase.LoadAssetAtPath<CCS_PlaytestProfile>(DefaultPlaytestProfilePath);
            if (profile == null)
            {
                return;
            }

            InsertStep(profile, "ccs.survival.playtest.industry.gather.wood", "Gather wood for industry", CCS_PlaytestStepType.GatherWoodForIndustry, "ccs.survival.item.resource.wood");
            InsertStep(profile, "ccs.survival.playtest.industry.lumber", "Produce lumber", CCS_PlaytestStepType.ProduceLumberAtSawTable, string.Empty);
            InsertStep(profile, "ccs.survival.playtest.industry.charcoal", "Produce charcoal", CCS_PlaytestStepType.ProduceCharcoalAtKiln, string.Empty);
            InsertStep(profile, "ccs.survival.playtest.industry.refine.iron", "Refine iron ore", CCS_PlaytestStepType.RefineIronAtPrimitiveForge, string.Empty);
            InsertStep(profile, "ccs.survival.playtest.industry.hatchet.head", "Craft iron hatchet head", CCS_PlaytestStepType.CraftIronHatchetHeadAtForge, string.Empty);
            InsertStep(profile, "ccs.survival.playtest.industry.upgrade.tool", "Upgrade to iron tool", CCS_PlaytestStepType.UpgradeToIronTool, "ccs.survival.item.tool.hatchet.iron");
            InsertStep(profile, "ccs.survival.playtest.industry.verify.tier", "Verify IndustrialHomestead", CCS_PlaytestStepType.VerifyIndustrialHomesteadTier, string.Empty);
            InsertStep(profile, "ccs.survival.playtest.industry.save", "Save industry camp", CCS_PlaytestStepType.SaveIndustryCampState, string.Empty);
            InsertStep(profile, "ccs.survival.playtest.industry.verify.load", "Verify industry load", CCS_PlaytestStepType.VerifyIndustryCampPersistenceAfterLoad, string.Empty);
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

            int insertIndex = steps.arraySize;
            for (int index = 0; index < steps.arraySize; index++)
            {
                if (steps.GetArrayElementAtIndex(index).FindPropertyRelative("stepId").stringValue == HomesteadPersistenceStepId)
                {
                    insertIndex = index + 1;
                    break;
                }
            }

            steps.InsertArrayElementAtIndex(insertIndex);
            SerializedProperty step = steps.GetArrayElementAtIndex(insertIndex);
            step.FindPropertyRelative("stepId").stringValue = stepId;
            step.FindPropertyRelative("displayName").stringValue = displayName;
            step.FindPropertyRelative("stepType").enumValueIndex = (int)stepType;
            step.FindPropertyRelative("instructionText").stringValue =
                $"Industry playtest: {displayName}. Shortcuts available via playtest HUD.";
            step.FindPropertyRelative("targetItemId").stringValue = targetItemId ?? string.Empty;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void BumpVersions()
        {
            string projectSettingsPath = "ProjectSettings/ProjectSettings.asset";
            string text = File.ReadAllText(projectSettingsPath);
            text = System.Text.RegularExpressions.Regex.Replace(
                text,
                @"bundleVersion: [0-9]+\.[0-9]+\.[0-9]+",
                "bundleVersion: 1.5.0");
            File.WriteAllText(projectSettingsPath, text);
        }
    }
}
