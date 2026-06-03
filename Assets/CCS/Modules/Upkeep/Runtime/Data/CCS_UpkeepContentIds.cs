// =============================================================================
// SCRIPT: CCS_UpkeepContentIds
// CATEGORY: Modules / Upkeep / Runtime / Data
// PURPOSE: Stable content ids for upkeep bootstrap, validation, and playtest harness.
// PLACEMENT: Shared by bootstrap setup, validators, and debug HUD.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 2.5.0 frontier homestead claim tax foundation.
// =============================================================================

namespace CCS.Modules.Upkeep
{
    public static class CCS_UpkeepContentIds
    {
        public const string DefaultPlayerOwnerId = "ccs.survival.land.player";
        public const string TradeDollarsCurrencyId = "ccs.survival.currency.tradedollars";
        public const string FrontierHomesteadClaimTaxDefinitionId = "ccs.survival.upkeep.land.frontierhomesteadtax";
        public const string DefaultUpkeepProfilePath = "Assets/CCS/Survival/Profiles/Upkeep/CCS_DefaultUpkeepProfile.asset";
        public const string FrontierHomesteadClaimTaxDefinitionPath =
            "Assets/CCS/Survival/Content/Upkeep/CCS_Upkeep_FrontierHomesteadClaimTax.asset";
    }
}
