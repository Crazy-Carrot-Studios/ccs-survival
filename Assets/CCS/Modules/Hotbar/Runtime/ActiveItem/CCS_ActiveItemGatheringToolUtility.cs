using CCS.Modules.Gathering;
using CCS.Modules.Inventory;
using CCS.Modules.Resources;
using CCS.Modules.WorldResources;

// =============================================================================
// SCRIPT: CCS_ActiveItemGatheringToolUtility
// CATEGORY: Modules / Hotbar / Runtime / ActiveItem
// PURPOSE: Maps gathering nodes and resources to harvest method and tool metadata.
// PLACEMENT: Used by CCS_ActiveItemService for tool requirement validation.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: Uses profile metadata when available. Fish returns not-implemented for routing.
// =============================================================================

namespace CCS.Modules.Hotbar
{
    public static class CCS_ActiveItemGatheringToolUtility
    {
        public static bool IsHarvestMethodImplementedForActiveUse(CCS_HarvestMethodType harvestMethod)
        {
            return CCS_HarvestMethodToolRulesUtility.IsHarvestMethodImplementedForGatheringRouting(harvestMethod);
        }

        public static bool ActiveToolMatchesGatheringNode(
            CCS_ItemDefinition activeToolDefinition,
            CCS_GatheringNodeType nodeType,
            CCS_GatheringProfile gatheringProfile = null)
        {
            if (gatheringProfile != null
                && gatheringProfile.TryGetNodeRewardSettings(nodeType, out CCS_GatheringNodeRewardSettings settings))
            {
                return ActiveToolMatchesHarvestMetadata(
                    activeToolDefinition,
                    settings.harvestMethod,
                    settings.requiredToolType,
                    settings.minimumToolTier);
            }

            return ActiveToolMatchesLegacyGatheringNode(activeToolDefinition, nodeType);
        }

        public static bool ActiveToolMatchesHarvestableResource(
            CCS_ItemDefinition activeToolDefinition,
            CCS_ResourceDefinition resourceDefinition)
        {
            if (resourceDefinition == null)
            {
                return false;
            }

            if (!IsHarvestMethodImplementedForActiveUse(resourceDefinition.HarvestMethod))
            {
                return false;
            }

            CCS_ItemToolType explicitTool = (CCS_ItemToolType)(int)resourceDefinition.RequiredToolType;
            return ActiveToolMatchesHarvestMetadata(
                activeToolDefinition,
                resourceDefinition.HarvestMethod,
                explicitTool,
                resourceDefinition.MinimumToolTier);
        }

        public static bool ActiveToolMatchesHarvestMetadata(
            CCS_ItemDefinition activeToolDefinition,
            CCS_HarvestMethodType harvestMethod,
            CCS_ItemToolType explicitRequiredTool,
            CCS_ToolTier minimumToolTier = CCS_ToolTier.None)
        {
            if (!IsHarvestMethodImplementedForActiveUse(harvestMethod))
            {
                return false;
            }

            if (!CCS_HarvestMethodToolRulesUtility.ToolSatisfiesHarvestMethod(
                    activeToolDefinition,
                    harvestMethod,
                    explicitRequiredTool))
            {
                return false;
            }

            return CCS_ItemGameplayUtility.ToolMeetsMinimumTier(activeToolDefinition, minimumToolTier);
        }

        public static CCS_RequiredToolType ResolveEquippedToolType(CCS_ItemDefinition activeToolDefinition)
        {
            CCS_ItemToolType itemToolType = CCS_ItemGameplayUtility.ResolveHarvestToolType(activeToolDefinition);
            if (itemToolType == CCS_ItemToolType.None)
            {
                return CCS_RequiredToolType.None;
            }

            return (CCS_RequiredToolType)(int)itemToolType;
        }

        private static bool ActiveToolMatchesLegacyGatheringNode(
            CCS_ItemDefinition activeToolDefinition,
            CCS_GatheringNodeType nodeType)
        {
            CCS_HarvestMethodType harvestMethod;
            CCS_ItemToolType requiredTool;

            switch (nodeType)
            {
                case CCS_GatheringNodeType.SmallTree:
                case CCS_GatheringNodeType.Tree:
                    harvestMethod = CCS_HarvestMethodType.Chop;
                    requiredTool = CCS_ItemToolType.Axe;
                    break;
                case CCS_GatheringNodeType.Rock:
                case CCS_GatheringNodeType.StoneOutcrop:
                case CCS_GatheringNodeType.OreVein:
                case CCS_GatheringNodeType.CoalVein:
                    harvestMethod = CCS_HarvestMethodType.Mine;
                    requiredTool = CCS_ItemToolType.Pickaxe;
                    break;
                case CCS_GatheringNodeType.Bush:
                case CCS_GatheringNodeType.FiberPlant:
                case CCS_GatheringNodeType.WaterSource:
                    harvestMethod = CCS_HarvestMethodType.Gather;
                    requiredTool = CCS_ItemToolType.None;
                    break;
                case CCS_GatheringNodeType.DeadfallLog:
                    harvestMethod = CCS_HarvestMethodType.Chop;
                    requiredTool = CCS_ItemToolType.Axe;
                    break;
                case CCS_GatheringNodeType.ClayDeposit:
                    harvestMethod = CCS_HarvestMethodType.Dig;
                    requiredTool = CCS_ItemToolType.Shovel;
                    break;
                case CCS_GatheringNodeType.SalvageAbandonedWagon:
                case CCS_GatheringNodeType.SalvageCampRemains:
                case CCS_GatheringNodeType.SalvageHomesteadRuins:
                case CCS_GatheringNodeType.SalvageMineDebris:
                    harvestMethod = CCS_HarvestMethodType.Salvage;
                    requiredTool = CCS_ItemToolType.None;
                    break;
                default:
                    return false;
            }

            return ActiveToolMatchesHarvestMetadata(activeToolDefinition, harvestMethod, requiredTool);
        }
    }
}
