// =============================================================================
// SCRIPT: CCS_SettlementHousingContentIds
// CATEGORY: Modules / Settlements / Runtime / Housing
// PURPOSE: Stable ids for settlement housing bootstrap, validation, and playtest.
// PLACEMENT: Shared by bootstrap setup, validators, and playtest service.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 4.4.0 settlement housing foundation.
// =============================================================================

namespace CCS.Modules.Settlements
{
    public static class CCS_SettlementHousingContentIds
    {
        public const string HousingProfilesRoot = "Assets/CCS/Survival/Profiles/Settlements/Housing";
        public const string DefaultHousingProfilePath =
            HousingProfilesRoot + "/CCS_DefaultSettlementHousingProfile.asset";
        public const string DefaultHousingProfileId = "ccs.survival.profile.settlementhousing.default";

        public const string TradingPostBoardingHouseId =
            "ccs.survival.settlement.housing.tradingpost.boardinghouse";
        public const string BrokenCreekFarmhouseId =
            "ccs.survival.settlement.housing.brokencreek.farmhouse";
        public const string PineRidgeWorkerCabinId =
            "ccs.survival.settlement.housing.pineridge.workercabin";
        public const string IronRidgeMiningBarracksId =
            "ccs.survival.settlement.housing.ironridge.miningbarracks";

        public const string TradingPostBoardingHouseAnchorId =
            "ccs.survival.settlement.housinganchor.tradingpost.boardinghouse";
        public const string BrokenCreekFarmhouseAnchorId =
            "ccs.survival.settlement.housinganchor.brokencreek.farmhouse";
        public const string PineRidgeWorkerCabinAnchorId =
            "ccs.survival.settlement.housinganchor.pineridge.workercabin";
        public const string IronRidgeMiningBarracksAnchorId =
            "ccs.survival.settlement.housinganchor.ironridge.miningbarracks";
    }
}
