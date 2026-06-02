using System.IO;
using CCS.Modules.Economy;
using CCS.Modules.Gathering;
using CCS.Modules.Inventory;
using CCS.Modules.Playtesting;
using CCS.Modules.Resources;
using CCS.Modules.WorldResources;
using CCS.Survival;
using CCS.Survival.Editor.Development;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CCS.Modules.Prospecting.Editor
{
    public sealed class CCS_ProspectingFoundationValidationValidator : CCS_ISurvivalValidationValidator
    {
        private const string ValidatorContext = "ccs.survival.validation.frontier.prospecting";
        private const string GatheringProfilePath = "Assets/CCS/Survival/Profiles/Gathering/CCS_DefaultGatheringProfile.asset";
        private const string GeneralStoreVendorPath = "Assets/CCS/Survival/Content/Vendors/CCS_Vendor_GeneralStore.asset";
        private const string GunsmithVendorPath = "Assets/CCS/Survival/Content/Vendors/CCS_Vendor_FrontierGunsmith.asset";
        private const string IronOreProcessPath =
            "Assets/CCS/Survival/Content/Industry/Processes/CCS_IndustryProcess_IronOreToRefinedIron.asset";
        private const string BootstrapScenePath = "Assets/CCS/Survival/Scenes/SCN_CCS_Survival_Bootstrap.unity";
        private const string PlaytestProfilePath = "Assets/CCS/Survival/Profiles/Playtesting/CCS_DefaultPlaytestProfile.asset";

        public string ValidatorId => ValidatorContext;

        public void Validate(CCS_SurvivalValidationReport report)
        {
            ValidateBundleVersion(report);
            ValidateNoStaleDowngradeBootstraps(report);
            ValidateGatheringMiningNodes(report);
            ValidateWorldResourceDefinitions(report);
            ValidateToolItems(report);
            ValidateIndustryIronProcess(report);
            ValidateVendorsBuyMiningGoods(report);
            ValidateBootstrapSceneNodes(report);
            ValidatePlaytestMiningSteps(report);
            ValidateDuplicateResourceIds(report);
        }

        private static void ValidateBundleVersion(CCS_SurvivalValidationReport report)
        {
            CCS_SurvivalBootstrapVersionUtility.AddBundleVersionValidationIssue(
                report,
                ValidatorContext,
                remediationHint: "Run CCS_ProspectingFoundationBootstrapSetup.ExecuteBatch.");
        }

        private static void ValidateNoStaleDowngradeBootstraps(CCS_SurvivalValidationReport report)
        {
            CCS_SurvivalBootstrapVersionUtility.ValidateNoHardcodedBootstrapVersionWrites(report, ValidatorContext);
        }

        private static void ValidateGatheringMiningNodes(CCS_SurvivalValidationReport report)
        {
            CCS_GatheringProfile profile = AssetDatabase.LoadAssetAtPath<CCS_GatheringProfile>(GatheringProfilePath);
            ValidateNode(
                report,
                profile,
                CCS_GatheringNodeType.StoneOutcrop,
                CCS_ResourceSourceType.Natural,
                CCS_HarvestMethodType.Mine,
                CCS_ItemToolType.Pickaxe,
                CCS_ToolTier.Primitive,
                CCS_ProspectingContentIds.StoneItemId);
            ValidateNode(
                report,
                profile,
                CCS_GatheringNodeType.OreVein,
                CCS_ResourceSourceType.Mining,
                CCS_HarvestMethodType.Mine,
                CCS_ItemToolType.Pickaxe,
                CCS_ToolTier.Iron,
                CCS_ProspectingContentIds.IronOreItemId);
            ValidateNode(
                report,
                profile,
                CCS_GatheringNodeType.CoalVein,
                CCS_ResourceSourceType.Mining,
                CCS_HarvestMethodType.Mine,
                CCS_ItemToolType.Pickaxe,
                CCS_ToolTier.Iron,
                CCS_ProspectingContentIds.CoalItemId);
            ValidateNode(
                report,
                profile,
                CCS_GatheringNodeType.ClayDeposit,
                CCS_ResourceSourceType.Natural,
                CCS_HarvestMethodType.Dig,
                CCS_ItemToolType.Shovel,
                CCS_ToolTier.Primitive,
                CCS_ProspectingContentIds.ClayItemId);
            ValidateNode(
                report,
                profile,
                CCS_GatheringNodeType.SalvageMineDebris,
                CCS_ResourceSourceType.Salvage,
                CCS_HarvestMethodType.Salvage,
                CCS_ItemToolType.None,
                CCS_ToolTier.None,
                CCS_ProspectingContentIds.ScrapIronItemId);
        }

        private static void ValidateNode(
            CCS_SurvivalValidationReport report,
            CCS_GatheringProfile profile,
            CCS_GatheringNodeType nodeType,
            CCS_ResourceSourceType source,
            CCS_HarvestMethodType method,
            CCS_ItemToolType tool,
            CCS_ToolTier tier,
            string rewardItemId)
        {
            CCS_SurvivalValidationResult result = CCS_ProspectingValidationUtility.ValidateGatheringNodeSettings(
                profile,
                nodeType,
                source,
                method,
                tool,
                tier,
                rewardItemId);
            report.AddIssue(
                result.IsSuccess ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                result.Message);
        }

        private static void ValidateWorldResourceDefinitions(CCS_SurvivalValidationReport report)
        {
            ValidateWorldDef(report, "Assets/CCS/Survival/Profiles/WorldResources/Frontier/CCS_FrontierResource_OreVein.asset");
            ValidateWorldDef(report, "Assets/CCS/Survival/Profiles/WorldResources/Frontier/CCS_FrontierResource_CoalVein.asset");
            ValidateWorldDef(report, "Assets/CCS/Survival/Profiles/WorldResources/Frontier/CCS_FrontierResource_MineDebris.asset");
        }

        private static void ValidateWorldDef(CCS_SurvivalValidationReport report, string path)
        {
            CCS_ResourceDefinition definition = AssetDatabase.LoadAssetAtPath<CCS_ResourceDefinition>(path);
            CCS_SurvivalValidationResult result = CCS_ProspectingValidationUtility.ValidateResourceDefinition(definition);
            report.AddIssue(
                result.IsSuccess ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                result.Message);
        }

        private static void ValidateToolItems(CCS_SurvivalValidationReport report)
        {
            CCS_ItemDefinition primitivePick = FindItem(CCS_ProspectingContentIds.PrimitivePickItemId);
            CCS_ItemDefinition ironPick = FindItem(CCS_ProspectingContentIds.IronPickItemId);
            bool ok = primitivePick != null
                && primitivePick.ToolTier == CCS_ToolTier.Primitive
                && ironPick != null
                && ironPick.ToolTier == CCS_ToolTier.Iron;
            report.AddIssue(
                ok ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                ok ? "Primitive and iron picks validated for mining tiers." : "Mining pick tools missing or wrong tier.");
        }

        private static void ValidateIndustryIronProcess(CCS_SurvivalValidationReport report)
        {
            bool ok = AssetDatabase.LoadAssetAtPath<ScriptableObject>(IronOreProcessPath) != null;
            report.AddIssue(
                ok ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                ok
                    ? "Iron ore to refined iron industry process present."
                    : "Missing iron refining process asset.");
        }

        private static void ValidateVendorsBuyMiningGoods(CCS_SurvivalValidationReport report)
        {
            CCS_VendorDefinition generalStore =
                AssetDatabase.LoadAssetAtPath<CCS_VendorDefinition>(GeneralStoreVendorPath);
            CCS_VendorDefinition gunsmith = AssetDatabase.LoadAssetAtPath<CCS_VendorDefinition>(GunsmithVendorPath);
            bool generalBuysOre = VendorBuysItem(generalStore, CCS_ProspectingContentIds.IronOreItemId);
            bool generalBuysCoal = VendorBuysItem(generalStore, CCS_ProspectingContentIds.CoalItemId);
            bool gunsmithBuysRefined = VendorBuysItem(gunsmith, CCS_ProspectingContentIds.RefinedIronItemId);
            bool ok = generalBuysOre && generalBuysCoal && gunsmithBuysRefined;
            report.AddIssue(
                ok ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                ok
                    ? "Vendors buy frontier mining goods."
                    : "General store or gunsmith missing mining buy catalog entries.");
        }

        private static void ValidateBootstrapSceneNodes(CCS_SurvivalValidationReport report)
        {
            if (!File.Exists(BootstrapScenePath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    ValidatorContext,
                    "Bootstrap scene missing.");
                return;
            }

            Scene scene = EditorSceneManager.OpenScene(BootstrapScenePath, OpenSceneMode.Additive);
            bool hasOre = false;
            bool hasCoal = false;
            bool hasEntrance = false;
            if (scene.isLoaded)
            {
                CCS_GatheringNode[] nodes = Object.FindObjectsByType<CCS_GatheringNode>();
                for (int index = 0; index < nodes.Length; index++)
                {
                    if (nodes[index].NodeType == CCS_GatheringNodeType.OreVein)
                    {
                        hasOre = true;
                    }

                    if (nodes[index].NodeType == CCS_GatheringNodeType.CoalVein)
                    {
                        hasCoal = true;
                    }
                }

                hasEntrance = Object.FindAnyObjectByType<CCS_AbandonedMineEntranceMarker>() != null;
            }

            EditorSceneManager.CloseScene(scene, true);
            bool ok = hasOre && hasCoal && hasEntrance;
            report.AddIssue(
                ok ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                ok
                    ? "Bootstrap scene contains frontier mining test nodes."
                    : "Bootstrap scene missing mining test nodes. Run prospecting bootstrap.");
        }

        private static void ValidatePlaytestMiningSteps(CCS_SurvivalValidationReport report)
        {
            CCS_PlaytestProfile profile = AssetDatabase.LoadAssetAtPath<CCS_PlaytestProfile>(PlaytestProfilePath);
            bool ok = profile != null
                && ProfileHasStep(profile, CCS_PlaytestStepType.AcquirePickForMining)
                && ProfileHasStep(profile, CCS_PlaytestStepType.VerifyMiningCurrencyIncreased);
            report.AddIssue(
                ok ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                ok ? "Mining playtest steps present." : "Playtest profile missing mining steps.");
        }

        private static void ValidateDuplicateResourceIds(CCS_SurvivalValidationReport report)
        {
            string[] guids = AssetDatabase.FindAssets("t:CCS_ResourceDefinition", new[] { "Assets/CCS/Survival/Profiles/WorldResources" });
            var seen = new System.Collections.Generic.HashSet<string>(System.StringComparer.OrdinalIgnoreCase);
            bool duplicate = false;
            for (int index = 0; index < guids.Length; index++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[index]);
                CCS_ResourceDefinition definition = AssetDatabase.LoadAssetAtPath<CCS_ResourceDefinition>(path);
                if (definition == null || string.IsNullOrWhiteSpace(definition.ResourceId))
                {
                    continue;
                }

                if (!seen.Add(definition.ResourceId))
                {
                    duplicate = true;
                    break;
                }
            }

            report.AddIssue(
                duplicate ? CCS_SurvivalValidationIssueSeverity.Error : CCS_SurvivalValidationIssueSeverity.Info,
                ValidatorContext,
                duplicate ? "Duplicate world resource IDs detected." : "No duplicate world resource IDs.");
        }

        private static bool VendorBuysItem(CCS_VendorDefinition vendor, string itemId)
        {
            if (vendor?.VendorInventory?.Items == null || string.IsNullOrWhiteSpace(itemId))
            {
                return false;
            }

            CCS_VendorItemEntry[] entries = vendor.VendorInventory.Items;
            for (int index = 0; index < entries.Length; index++)
            {
                CCS_VendorItemEntry entry = entries[index];
                if (entry?.ItemDefinition != null
                    && entry.AllowBuy
                    && entry.ItemDefinition.ItemId == itemId)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool ProfileHasStep(CCS_PlaytestProfile profile, CCS_PlaytestStepType stepType)
        {
            for (int index = 0; index < profile.StepDefinitions.Count; index++)
            {
                if (profile.StepDefinitions[index].StepType == stepType)
                {
                    return true;
                }
            }

            return false;
        }

        private static CCS_ItemDefinition FindItem(string itemId)
        {
            string[] guids = AssetDatabase.FindAssets("t:CCS_ItemDefinition");
            for (int index = 0; index < guids.Length; index++)
            {
                CCS_ItemDefinition item = AssetDatabase.LoadAssetAtPath<CCS_ItemDefinition>(
                    AssetDatabase.GUIDToAssetPath(guids[index]));
                if (item != null && item.ItemId == itemId)
                {
                    return item;
                }
            }

            return null;
        }
    }
}
