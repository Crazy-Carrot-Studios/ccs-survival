using System;
using CCS.Modules.NPCs;
using CCS.Modules.Settlements;

// =============================================================================
// SCRIPT: CCS_SettlementSimulationState
// CATEGORY: Modules / WorldSimulation / Runtime / Data
// PURPOSE: Runtime settlement supply, demand, production, and prosperity state.
// PLACEMENT: Stored by CCS_WorldSimulationService and save payloads.
// AUTHOR: James Schilz
// CREATED: 2026-06-02
// NOTES: Milestone 5.1.0 — active settlement event persisted with world simulation save/load.
// =============================================================================

namespace CCS.Modules.WorldSimulation
{
    [Serializable]
    public sealed class CCS_SettlementSimulationState
    {
        public string settlementId = string.Empty;
        public string regionId = string.Empty;
        public CCS_BusinessState[] businessStates = Array.Empty<CCS_BusinessState>();
        public CCS_NpcIdentityState[] npcIdentityStates = Array.Empty<CCS_NpcIdentityState>();
        public CCS_NpcServiceRepresentativeState[] npcServiceRepresentativeStates =
            Array.Empty<CCS_NpcServiceRepresentativeState>();
        public CCS_SettlementHousingState[] housingStates = Array.Empty<CCS_SettlementHousingState>();
        public CCS_NpcMovementState[] npcMovementStates = Array.Empty<CCS_NpcMovementState>();
        public CCS_NpcScheduleState[] npcScheduleStates = Array.Empty<CCS_NpcScheduleState>();
        public CCS_NpcActivityState[] npcActivityStates = Array.Empty<CCS_NpcActivityState>();
        public CCS_NpcAffiliationState[] npcAffiliationStates = Array.Empty<CCS_NpcAffiliationState>();
        public CCS_NpcSocialState[] npcSocialStates = Array.Empty<CCS_NpcSocialState>();

        public CCS_SettlementEventState activeSettlementEvent = new CCS_SettlementEventState();
        public int population;
        public int populationCapacity;
        public float populationGrowthRate;
        public float populationStability;
        public int farmerCount;
        public int rancherCount;
        public int minerCount;
        public int lumberWorkerCount;
        public int merchantCount;
        public int laborerCount;
        public float prosperity;
        public bool isDiscovered;
        public CCS_SettlementSupplyEntry[] supplies = Array.Empty<CCS_SettlementSupplyEntry>();
        public CCS_SettlementDemandEntry[] demands = Array.Empty<CCS_SettlementDemandEntry>();
        public CCS_SettlementProductionEntry[] productions = Array.Empty<CCS_SettlementProductionEntry>();
        public int currentGrowthStage;
        public int previousGrowthStage;
        public float growthProgressPercent;
        public int completedContractsCount;
    }
}
