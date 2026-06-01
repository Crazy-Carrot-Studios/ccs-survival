// =============================================================================
// SCRIPT: CCS_GatheringNodeType
// CATEGORY: Modules / Gathering / Runtime / Data
// PURPOSE: Identifies gathering node archetypes for reward and harvest metadata lookup.
// PLACEMENT: Serialized on CCS_GatheringNode and CCS_GatheringProfile reward tables.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-06-01
// NOTES: Legacy SmallTree/Rock/Bush remain for bootstrap tests. Frontier types use 10+ band.
// =============================================================================

namespace CCS.Modules.Gathering
{
    public enum CCS_GatheringNodeType
    {
        None = 0,

        // Legacy bootstrap verification (0.9.9)
        SmallTree = 1,
        Rock = 2,
        Bush = 3,

        // Practical frontier source archetypes (1.2.4+)
        Tree = 10,
        DeadfallLog = 11,
        FiberPlant = 12,
        StoneOutcrop = 13,
        ClayDeposit = 14,
        WaterSource = 15,
        OreVein = 16,
        CoalVein = 17,
        SalvageAbandonedWagon = 18,
        SalvageCampRemains = 19,
        SalvageHomesteadRuins = 20,
        SalvageMineDebris = 21
    }
}
