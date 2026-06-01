// =============================================================================
// SCRIPT: CCS_ItemGameplayUtility
// CATEGORY: Modules / Inventory / Runtime / Utilities
// PURPOSE: Shared helpers for item, tool, and weapon gameplay classification checks.
// PLACEMENT: Used by harvesting, equipment, crafting, and validation systems.
// AUTHOR: James Schilz
// CREATED: 2026-05-31
// NOTES: No combat execution or durability logic in 0.9.2 foundation.
// =============================================================================

namespace CCS.Modules.Inventory
{
    public static class CCS_ItemGameplayUtility
    {
        #region Public Methods

        public static bool IsToolItem(CCS_ItemDefinition itemDefinition)
        {
            if (itemDefinition == null)
            {
                return false;
            }

            return itemDefinition.GameplayKind == CCS_ItemGameplayKind.Tool
                || itemDefinition.GameplayKind == CCS_ItemGameplayKind.ToolAndWeapon
                || itemDefinition.HasToolIdentity;
        }

        public static bool IsWeaponItem(CCS_ItemDefinition itemDefinition)
        {
            if (itemDefinition == null)
            {
                return false;
            }

            return itemDefinition.GameplayKind == CCS_ItemGameplayKind.Weapon
                || itemDefinition.GameplayKind == CCS_ItemGameplayKind.ToolAndWeapon
                || itemDefinition.HasWeaponIdentity;
        }

        public static CCS_ItemToolType ResolveHarvestToolType(CCS_ItemDefinition itemDefinition)
        {
            if (itemDefinition == null || !itemDefinition.HasToolIdentity)
            {
                return CCS_ItemToolType.None;
            }

            if (itemDefinition.ToolType != CCS_ItemToolType.None)
            {
                return itemDefinition.ToolType;
            }

            return ResolveHarvestToolTypeFromArchetype(itemDefinition.ToolArchetype);
        }

        public static CCS_ItemToolType ResolveHarvestToolTypeFromArchetype(CCS_ToolArchetype toolArchetype)
        {
            switch (toolArchetype)
            {
                case CCS_ToolArchetype.Knife:
                    return CCS_ItemToolType.Knife;
                case CCS_ToolArchetype.Hatchet:
                    return CCS_ItemToolType.Axe;
                case CCS_ToolArchetype.Pick:
                    return CCS_ItemToolType.Pickaxe;
                case CCS_ToolArchetype.Shovel:
                    return CCS_ItemToolType.Shovel;
                default:
                    return CCS_ItemToolType.None;
            }
        }

        public static bool ItemSatisfiesHarvestTool(
            CCS_ItemDefinition itemDefinition,
            CCS_ItemToolType requiredTool)
        {
            if (itemDefinition == null || requiredTool == CCS_ItemToolType.None)
            {
                return requiredTool == CCS_ItemToolType.None;
            }

            return ResolveHarvestToolType(itemDefinition) == requiredTool;
        }

        #endregion
    }
}
