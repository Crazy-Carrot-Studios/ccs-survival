using CCS.Modules.Inventory;

// =============================================================================
// SCRIPT: CCS_HarvestMethodToolRulesUtility
// CATEGORY: Modules / Resources / Runtime / Utilities
// PURPOSE: Maps harvest methods to compatible tool categories for validation and routing.
// PLACEMENT: Used by gathering/world resource validation and active item routing.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: Definition-level requiredTool overrides these defaults when explicitly set.
// =============================================================================

namespace CCS.Modules.Resources
{
    public static class CCS_HarvestMethodToolRulesUtility
    {
        public static bool IsHarvestMethodImplemented(CCS_HarvestMethodType harvestMethod)
        {
            return harvestMethod != CCS_HarvestMethodType.None;
        }

        public static bool IsHarvestMethodImplementedForGatheringRouting(CCS_HarvestMethodType harvestMethod)
        {
            return harvestMethod != CCS_HarvestMethodType.Fish
                && harvestMethod != CCS_HarvestMethodType.None;
        }

        public static CCS_ItemToolType GetDefaultRequiredTool(CCS_HarvestMethodType harvestMethod)
        {
            switch (harvestMethod)
            {
                case CCS_HarvestMethodType.Gather:
                case CCS_HarvestMethodType.Collect:
                case CCS_HarvestMethodType.Salvage:
                    return CCS_ItemToolType.None;
                case CCS_HarvestMethodType.Chop:
                    return CCS_ItemToolType.Axe;
                case CCS_HarvestMethodType.Mine:
                    return CCS_ItemToolType.Pickaxe;
                case CCS_HarvestMethodType.Skin:
                case CCS_HarvestMethodType.Butcher:
                    return CCS_ItemToolType.Knife;
                case CCS_HarvestMethodType.Dig:
                    return CCS_ItemToolType.Shovel;
                case CCS_HarvestMethodType.Fish:
                    return CCS_ItemToolType.FishingPole;
                case CCS_HarvestMethodType.Other:
                case CCS_HarvestMethodType.None:
                default:
                    return CCS_ItemToolType.None;
            }
        }

        public static bool ToolSatisfiesHarvestMethod(
            CCS_ItemDefinition toolDefinition,
            CCS_HarvestMethodType harvestMethod,
            CCS_ItemToolType explicitRequiredTool)
        {
            if (harvestMethod == CCS_HarvestMethodType.None)
            {
                return false;
            }

            CCS_ItemToolType requiredTool = explicitRequiredTool != CCS_ItemToolType.None
                ? explicitRequiredTool
                : GetDefaultRequiredTool(harvestMethod);

            if (requiredTool == CCS_ItemToolType.None)
            {
                return true;
            }

            return CCS_ItemGameplayUtility.ItemSatisfiesHarvestTool(toolDefinition, requiredTool);
        }

        public static bool HarvestMethodRequiresMiningTool(CCS_HarvestMethodType harvestMethod)
        {
            return harvestMethod == CCS_HarvestMethodType.Mine;
        }

        public static bool HarvestMethodRequiresAxeTool(CCS_HarvestMethodType harvestMethod)
        {
            return harvestMethod == CCS_HarvestMethodType.Chop;
        }

        public static bool HarvestMethodAllowsNoTool(CCS_HarvestMethodType harvestMethod)
        {
            CCS_ItemToolType defaultTool = GetDefaultRequiredTool(harvestMethod);
            return defaultTool == CCS_ItemToolType.None
                && harvestMethod != CCS_HarvestMethodType.Fish
                && harvestMethod != CCS_HarvestMethodType.None;
        }
    }
}
