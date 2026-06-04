// =============================================================================
// SCRIPT: CCS_BusinessContentIds
// CATEGORY: Modules / Settlements / Runtime / Data
// PURPOSE: Stable ids and paths for business foundation bootstrap and validation.
// PLACEMENT: Shared by editor bootstrap, validators, and playtest harness.
// AUTHOR: James Schilz
// CREATED: 2026-06-04
// NOTES: Milestone 3.7.0 — frontier businesses foundation.
// =============================================================================

namespace CCS.Modules.Settlements
{
    public static class CCS_BusinessContentIds
    {
        public const string BusinessProfilesRoot = "Assets/CCS/Survival/Profiles/Settlements/Businesses";
        public const string DefaultBusinessProfilePath =
            BusinessProfilesRoot + "/CCS_DefaultBusinessProfile.asset";
        public const string DefaultBusinessProfileId = "ccs.survival.profile.business.default";
    }
}
