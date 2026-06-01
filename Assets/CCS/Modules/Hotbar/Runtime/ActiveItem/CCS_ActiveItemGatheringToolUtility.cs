using CCS.Modules.Gathering;
using CCS.Modules.Inventory;
using CCS.Modules.WorldResources;

// =============================================================================
// SCRIPT: CCS_ActiveItemGatheringToolUtility
// CATEGORY: Modules / Hotbar / Runtime / ActiveItem
// PURPOSE: Maps gathering nodes and resources to required harvest tool metadata.
// PLACEMENT: Used by CCS_ActiveItemService for tool requirement validation.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: Reuses CCS_ItemGameplayUtility; does not hard-code item IDs.
// =============================================================================

namespace CCS.Modules.Hotbar
{
    public static class CCS_ActiveItemGatheringToolUtility
    {
        public static CCS_ItemToolType GetRequiredToolForGatheringNode(CCS_GatheringNodeType nodeType)
        {
            switch (nodeType)
            {
                case CCS_GatheringNodeType.SmallTree:
                case CCS_GatheringNodeType.Bush:
                    return CCS_ItemToolType.Axe;
                case CCS_GatheringNodeType.Rock:
                    return CCS_ItemToolType.Pickaxe;
                default:
                    return CCS_ItemToolType.None;
            }
        }

        public static bool ActiveToolMatchesGatheringNode(
            CCS_ItemDefinition activeToolDefinition,
            CCS_GatheringNodeType nodeType)
        {
            CCS_ItemToolType requiredTool = GetRequiredToolForGatheringNode(nodeType);
            if (requiredTool == CCS_ItemToolType.None)
            {
                return false;
            }

            return CCS_ItemGameplayUtility.ItemSatisfiesHarvestTool(activeToolDefinition, requiredTool);
        }

        public static bool ActiveToolMatchesHarvestableResource(
            CCS_ItemDefinition activeToolDefinition,
            CCS_ResourceDefinition resourceDefinition)
        {
            if (resourceDefinition == null)
            {
                return false;
            }

            CCS_RequiredToolType requiredTool = resourceDefinition.RequiredToolType;
            if (requiredTool == CCS_RequiredToolType.None)
            {
                return true;
            }

            CCS_ItemToolType requiredItemTool = (CCS_ItemToolType)(int)requiredTool;
            return CCS_ItemGameplayUtility.ItemSatisfiesHarvestTool(activeToolDefinition, requiredItemTool);
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
    }
}
