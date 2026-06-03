// =============================================================================
// SCRIPT: CCS_FarmingContentIds
// CATEGORY: Modules / Farming / Runtime / Data
// PURPOSE: Stable content ids for farming bootstrap and validation.
// PLACEMENT: Shared by bootstrap setup, validators, and playtest harness.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-06-02
// NOTES: Western-specific ids for Survival content assets. Milestone 2.2.0.
// =============================================================================

namespace CCS.Modules.Farming
{
    public static class CCS_FarmingContentIds
    {
        public const string DefaultProfileId = "ccs.survival.profile.farming.default";
        public const string CampOwnerId = "ccs.survival.camp.player";

        public const string FarmPlotStructureId = "ccs.survival.farming.structure.farmplot";
        public const string FarmPlotKitItemId = "ccs.survival.farming.item.farmplotkit";

        public const string CornCropId = "ccs.survival.farming.crop.corn";
        public const string BeanCropId = "ccs.survival.farming.crop.beans";
        public const string PotatoCropId = "ccs.survival.farming.crop.potatoes";
        public const string WheatCropId = "ccs.survival.farming.crop.wheat";

        public const string CornSeedItemId = "ccs.survival.farming.item.cornseed";
        public const string BeanSeedItemId = "ccs.survival.farming.item.beanseed";
        public const string PotatoSeedItemId = "ccs.survival.farming.item.potatoseed";
        public const string WheatSeedItemId = "ccs.survival.farming.item.wheatseed";

        public const string CornHarvestItemId = "ccs.survival.farming.item.corn";
        public const string BeanHarvestItemId = "ccs.survival.farming.item.beans";
        public const string PotatoHarvestItemId = "ccs.survival.farming.item.potatoes";
        public const string WheatHarvestItemId = "ccs.survival.farming.item.wheat";

        public const string GeneralStoreVendorId = "ccs.survival.vendor.frontier.generalstore";
    }
}
