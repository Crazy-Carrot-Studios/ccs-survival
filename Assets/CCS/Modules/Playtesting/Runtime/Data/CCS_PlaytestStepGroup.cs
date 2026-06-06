// =============================================================================
// SCRIPT: CCS_PlaytestStepGroup
// CATEGORY: Modules / Playtesting / Runtime / Data
// PURPOSE: Checklist grouping labels for the manual playtest HUD.
// PLACEMENT: Used by CCS_PlaytestStepGroupingUtility and CCS_PlaytestHud.
// AUTHOR: James Schilz
// CREATED: 2026-06-02
// NOTES: Milestone 1.7.2 playtest harness organization.
// =============================================================================

namespace CCS.Modules.Playtesting
{
    public enum CCS_PlaytestStepGroup
    {
        CoreSpawnMovement = 0,
        InventoryEquipment = 1,
        GatheringCrafting = 2,
        Fishing = 3,
        Economy = 4,
        Hunting = 5,
        Trapping = 6,
        Cooking = 7,
        ShelterHomestead = 8,
        Industry = 9,
        HorseWagon = 10,
        Firearms = 11,
        Prospecting = 12,
        Settlement = 13,
        Region = 14,
        WorldSimulation = 15,
        Ranching = 16,
        Farming = 17,
        LandOwnership = 18,
        Banking = 19,
        Upkeep = 20,
        Reputation = 21,
        Contracts = 22,
        RegionalEconomy = 23,
        SettlementGrowth = 24,
        MultiSettlement = 25,
        TradeRoutesFreight = 26,
        RouteRiskFreight = 27,
        Population = 28,
        Businesses = 29,
        BusinessPresence = 30,
        SettlementVisualGrowth = 31,
        PopulationPresence = 32,
        NpcIdentity = 33,
        NpcServiceRepresentatives = 34,
        SettlementHousing = 35
    }
}
