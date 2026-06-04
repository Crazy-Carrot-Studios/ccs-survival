// =============================================================================
// SCRIPT: CCS_BusinessPresenceContentIds
// CATEGORY: Modules / Settlements / Runtime / Data
// PURPOSE: Stable ids and paths for business presence bootstrap and validation.
// PLACEMENT: Shared by editor bootstrap, validators, and playtest harness.
// AUTHOR: James Schilz
// CREATED: 2026-06-04
// NOTES: Milestone 3.8.0 — visible business presence foundation.
// =============================================================================

namespace CCS.Modules.Settlements
{
    public static class CCS_BusinessPresenceContentIds
    {
        public const string PresenceProfilesRoot = "Assets/CCS/Survival/Profiles/Settlements/BusinessPresence";
        public const string DefaultPresenceProfilePath =
            PresenceProfilesRoot + "/CCS_DefaultBusinessPresenceProfile.asset";
        public const string DefaultPresenceProfileId = "ccs.survival.profile.businesspresence.default";

        public const string TradingPostGeneralStoreAnchorId =
            "ccs.survival.businesspresence.testtradingpost.generalstore";
        public const string TradingPostStableAnchorId =
            "ccs.survival.businesspresence.testtradingpost.stable";
        public const string TradingPostGunsmithAnchorId =
            "ccs.survival.businesspresence.testtradingpost.gunsmith";
        public const string TradingPostBankAnchorId =
            "ccs.survival.businesspresence.testtradingpost.bank";
        public const string TradingPostContractOfficeAnchorId =
            "ccs.survival.businesspresence.testtradingpost.contractoffice";
        public const string BrokenCreekFarmSupplyAnchorId =
            "ccs.survival.businesspresence.brokencreek.farmsupply";
        public const string IronRidgeMiningSupplierAnchorId =
            "ccs.survival.businesspresence.ironridge.miningsupplier";
        public const string PineRidgeLumberYardAnchorId =
            "ccs.survival.businesspresence.pineridge.lumberyard";
    }
}
