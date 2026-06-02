// =============================================================================
// SCRIPT: CCS_RanchingContentIds
// CATEGORY: Modules / Ranching / Runtime / Data
// PURPOSE: Stable content ids for ranching bootstrap and validation.
// PLACEMENT: Shared by bootstrap setup, validators, and playtest harness.
// AUTHOR: James Schilz
// CREATED: 2026-06-02
// NOTES: Western-specific ids for Survival content assets.
// =============================================================================

namespace CCS.Modules.Ranching
{
    public static class CCS_RanchingContentIds
    {
        public const string DefaultProfileId = "ccs.survival.profile.ranching.default";
        public const string CampOwnerId = "ccs.survival.camp.player";

        public const string ChickenLivestockId = "ccs.survival.livestock.chicken";
        public const string GoatLivestockId = "ccs.survival.livestock.goat";
        public const string CowLivestockId = "ccs.survival.livestock.cow";
        public const string PigLivestockId = "ccs.survival.livestock.pig";

        public const string ChickenItemId = "ccs.survival.item.livestock.chicken";
        public const string GoatItemId = "ccs.survival.item.livestock.goat";
        public const string CowItemId = "ccs.survival.item.livestock.cow";
        public const string PigItemId = "ccs.survival.item.livestock.pig";
        public const string FeedItemId = "ccs.survival.item.ranch.feed";
        public const string EggItemId = "ccs.survival.item.ranch.egg";
        public const string MilkItemId = "ccs.survival.item.ranch.milk";
        public const string RawPorkItemId = "ccs.survival.item.ranch.rawpork";
        public const string RawBeefItemId = "ccs.survival.item.ranch.rawbeef";

        public const string ChickenCoopStructureId = "ccs.survival.ranch.structure.chickencoop";
        public const string AnimalPenStructureId = "ccs.survival.ranch.structure.animalpen";
        public const string FeedTroughStructureId = "ccs.survival.ranch.structure.feedtrough";
        public const string WaterTroughStructureId = "ccs.survival.ranch.structure.watertrough";

        public const string ChickenCoopKitItemId = "ccs.survival.item.ranch.chickencoopkit";
        public const string AnimalPenKitItemId = "ccs.survival.item.ranch.animalpenkit";
        public const string FeedTroughKitItemId = "ccs.survival.item.ranch.feedtroughkit";
        public const string WaterTroughKitItemId = "ccs.survival.item.ranch.watertroughkit";

        public const string GeneralStoreVendorId = "ccs.survival.vendor.frontier.generalstore";
        public const string StableVendorId = "ccs.survival.vendor.frontier.stable";
    }
}
