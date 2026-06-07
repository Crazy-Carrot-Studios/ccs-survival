// =============================================================================
// SCRIPT: CCS_SettlementEventContentIds
// CATEGORY: Modules / Settlements / Runtime / Events
// PURPOSE: Stable ids for settlement event bootstrap, validation, and playtest.
// PLACEMENT: Shared by bootstrap setup, validators, and playtest service.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 5.1.0 dynamic settlement events foundation.
// =============================================================================

namespace CCS.Modules.Settlements
{
    public static class CCS_SettlementEventContentIds
    {
        public const string EventProfilesRoot = "Assets/CCS/Survival/Profiles/Settlements/Events";

        public const string DefaultEventProfilePath =
            EventProfilesRoot + "/CCS_DefaultSettlementEventProfile.asset";

        public const string DefaultEventProfileId = "ccs.survival.profile.settlementevents.default";

        public const string TradingPostMarketDayEventId = "ccs.survival.event.tradingpost.marketday";

        public const string TradingPostSupplyShipmentEventId = "ccs.survival.event.tradingpost.supplyshipment";

        public const string BrokenCreekHarvestFestivalEventId = "ccs.survival.event.brokencreek.harvestfestival";

        public const string IronRidgeMiningShipmentEventId = "ccs.survival.event.ironridge.miningshipment";

        public const string PineRidgeTimberDeliveryEventId = "ccs.survival.event.pineridge.timberdelivery";

        public const string TradingPostMarketDayAnchorId = "ccs.survival.event.tradingpost.marketday.anchor";

        public const string TradingPostSupplyShipmentAnchorId = "ccs.survival.event.tradingpost.supplyshipment.anchor";

        public const string BrokenCreekHarvestFestivalAnchorId = "ccs.survival.event.brokencreek.harvestfestival.anchor";

        public const string IronRidgeMiningShipmentAnchorId = "ccs.survival.event.ironridge.miningshipment.anchor";

        public const string PineRidgeTimberDeliveryAnchorId = "ccs.survival.event.pineridge.timberdelivery.anchor";
    }
}
