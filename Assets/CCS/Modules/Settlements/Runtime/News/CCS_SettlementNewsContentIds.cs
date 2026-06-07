// =============================================================================
// SCRIPT: CCS_SettlementNewsContentIds
// CATEGORY: Modules / Settlements / Runtime / News
// PURPOSE: Stable ids for settlement news bootstrap, validation, and playtest.
// PLACEMENT: Shared by bootstrap setup, validators, and playtest service.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 5.2.0 settlement news and rumors foundation.
// =============================================================================

namespace CCS.Modules.Settlements
{
    public static class CCS_SettlementNewsContentIds
    {
        public const string NewsProfilesRoot = "Assets/CCS/Survival/Profiles/Settlements/News";

        public const string DefaultNewsProfilePath =
            NewsProfilesRoot + "/CCS_DefaultSettlementNewsProfile.asset";

        public const string DefaultNewsProfileId = "ccs.survival.profile.settlementnews.default";

        public const string MarketDayNewsDefinitionId = "ccs.survival.news.marketday";

        public const string SupplyShipmentNewsDefinitionId = "ccs.survival.news.supplyshipment";

        public const string HarvestFestivalNewsDefinitionId = "ccs.survival.news.harvestfestival";

        public const string MiningShipmentNewsDefinitionId = "ccs.survival.news.miningshipment";

        public const string TimberDeliveryNewsDefinitionId = "ccs.survival.news.timberdelivery";
    }
}
