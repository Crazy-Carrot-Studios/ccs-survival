// =============================================================================
// SCRIPT: CCS_LandContentIds
// CATEGORY: Modules / Land / Runtime / Data
// PURPOSE: Stable content ids for land ownership bootstrap and validation.
// PLACEMENT: Shared by bootstrap setup, validators, and playtest harness.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-06-02
// NOTES: Western-specific ids for Survival content assets. Milestone 2.3.0.
// =============================================================================

namespace CCS.Modules.Land
{
    public static class CCS_LandContentIds
    {
        public const string DefaultProfileId = "ccs.survival.profile.land.default";
        public const string PlayerOwnerId = "ccs.survival.land.player";
        public const string DefaultCampOwnerId = "ccs.survival.camp.player";

        public const string FrontierHomesteadClaimId = "ccs.survival.land.claim.frontierhomestead";
        public const string HomesteadClaimDeedItemId = "ccs.survival.land.item.homesteadclaimdeed";

        public const string GeneralStoreVendorId = "ccs.survival.vendor.frontier.generalstore";
    }
}
