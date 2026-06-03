// =============================================================================
// SCRIPT: CCS_LandClaimStructureKind
// CATEGORY: Modules / Land / Runtime / Data
// PURPOSE: Stable structure kind ids for land claim association rules.
// PLACEMENT: Referenced by claim definitions and structure registration hooks.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-06-02
// NOTES: Milestone 2.3.0 — generic kinds for future modules.
// =============================================================================

namespace CCS.Modules.Land
{
    public static class CCS_LandClaimStructureKind
    {
        public const string Shelter = "shelter";
        public const string Campfire = "campfire";
        public const string Bedroll = "bedroll";
        public const string Storage = "storage";
        public const string Workbench = "workbench";
        public const string IndustryStation = "industry";
        public const string RanchStructure = "ranch";
        public const string FarmPlot = "farmplot";
    }
}
