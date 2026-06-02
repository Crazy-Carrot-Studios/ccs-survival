// =============================================================================
// SCRIPT: CCS_ProspectingContentIds
// CATEGORY: Modules / Prospecting / Runtime / Data
// PURPOSE: Stable IDs for prospecting and mining frontier content (1.7.0).
// PLACEMENT: Referenced by bootstrap, validation, and playtest harness.
// AUTHOR: James Schilz
// CREATED: 2026-06-02
// =============================================================================

namespace CCS.Modules.Prospecting
{
    public static class CCS_ProspectingContentIds
    {
        public const string IronOreItemId = "ccs.survival.item.resource.ironore";
        public const string CoalItemId = "ccs.survival.item.resource.coal";
        public const string StoneItemId = "ccs.survival.item.resource.stone";
        public const string FlintItemId = "ccs.survival.item.resource.flint";
        public const string ClayItemId = "ccs.survival.item.resource.clay";
        public const string ScrapIronItemId = "ccs.survival.item.resource.scrapiron";
        public const string NailsItemId = "ccs.survival.item.resource.nails";
        public const string RefinedIronItemId = "ccs.survival.item.resource.refinediron";
        public const string PrimitivePickItemId = "ccs.survival.item.tool.pick.primitive";
        public const string IronPickItemId = "ccs.survival.item.tool.pick.iron";

        public const string OreVeinResourceId = "ccs.survival.resource.frontier.orevein";
        public const string CoalVeinResourceId = "ccs.survival.resource.frontier.coalvein";
        public const string StoneOutcropResourceId = "ccs.survival.resource.frontier.stoneoutcrop";
        public const string ClayDepositResourceId = "ccs.survival.resource.frontier.claydeposit";
        public const string MineDebrisResourceId = "ccs.survival.resource.frontier.minedebris";
        public const string AbandonedMineEntranceResourceId = "ccs.survival.resource.frontier.abandonedmine";

        public const string ProspectingTestAreaName = "CCS_FrontierProspectingTestArea";
        public const string TestStoneOutcropObjectName = "CCS_TestFrontierStoneOutcrop_Mining";
        public const string TestOreVeinObjectName = "CCS_TestFrontierOreVein";
        public const string TestCoalVeinObjectName = "CCS_TestFrontierCoalVein";
        public const string TestClayDepositObjectName = "CCS_TestFrontierClayDeposit";
        public const string TestMineDebrisObjectName = "CCS_TestFrontierMineDebris";
        public const string TestProspectingSpotObjectName = "CCS_TestFrontierProspectingSpot";
        public const string TestAbandonedMineEntranceObjectName = "CCS_TestFrontierAbandonedMineEntrance";

        public const string StoneOutcropSaveNodeId = "ccs.survival.gathering.node.frontier.stoneoutcrop";
        public const string OreVeinSaveNodeId = "ccs.survival.gathering.node.frontier.orevein";
        public const string CoalVeinSaveNodeId = "ccs.survival.gathering.node.frontier.coalvein";
        public const string ClayDepositSaveNodeId = "ccs.survival.gathering.node.frontier.claydeposit";
        public const string MineDebrisSaveNodeId = "ccs.survival.gathering.node.frontier.minedebris";
    }
}
