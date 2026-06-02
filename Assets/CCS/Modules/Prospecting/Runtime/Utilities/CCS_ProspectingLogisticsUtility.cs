using CCS.Modules.Inventory;

// =============================================================================
// SCRIPT: CCS_ProspectingLogisticsUtility
// CATEGORY: Modules / Prospecting / Runtime / Utilities
// PURPOSE: Placeholder bulk-haul weight/value hints for mining resources and wagons.
// PLACEMENT: Referenced by validation and future encumbrance systems.
// AUTHOR: James Schilz
// CREATED: 2026-06-02
// NOTES: No hard inventory overhaul in 1.7.0 — constants only.
// =============================================================================

namespace CCS.Modules.Prospecting
{
    public static class CCS_ProspectingLogisticsUtility
    {
        public const float StandardResourceWeight = 1f;
        public const float DenseOreWeight = 3.5f;
        public const float DenseCoalWeight = 2.5f;
        public const float ClayWeight = 2f;

        public const float StandardTradeValue = 1f;
        public const float OreTradeValue = 4f;
        public const float CoalTradeValue = 3f;

        public static bool IsDenseMiningResource(string itemId)
        {
            if (string.IsNullOrWhiteSpace(itemId))
            {
                return false;
            }

            return itemId == CCS_ProspectingContentIds.IronOreItemId
                || itemId == CCS_ProspectingContentIds.CoalItemId
                || itemId == CCS_ProspectingContentIds.ScrapIronItemId;
        }

        public static float ResolveRecommendedItemWeight(CCS_ItemDefinition itemDefinition)
        {
            if (itemDefinition == null)
            {
                return StandardResourceWeight;
            }

            if (itemDefinition.ItemId == CCS_ProspectingContentIds.IronOreItemId
                || itemDefinition.ItemId == CCS_ProspectingContentIds.ScrapIronItemId)
            {
                return DenseOreWeight;
            }

            if (itemDefinition.ItemId == CCS_ProspectingContentIds.CoalItemId)
            {
                return DenseCoalWeight;
            }

            if (itemDefinition.ItemId == CCS_ProspectingContentIds.ClayItemId)
            {
                return ClayWeight;
            }

            return itemDefinition.Weight > 0f ? itemDefinition.Weight : StandardResourceWeight;
        }

        public static bool PrefersWagonCargoForBulkHaul(CCS_ItemDefinition itemDefinition)
        {
            return IsDenseMiningResource(itemDefinition?.ItemId);
        }
    }
}
