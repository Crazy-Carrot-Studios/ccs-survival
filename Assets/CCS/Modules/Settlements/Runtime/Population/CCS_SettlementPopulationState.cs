using System;

// =============================================================================
// SCRIPT: CCS_SettlementPopulationState
// CATEGORY: Modules / Settlements / Runtime / Population
// PURPOSE: Serializable runtime population metrics stored on settlement simulation state.
// PLACEMENT: Fields mirrored on CCS_SettlementSimulationState for save/load.
// AUTHOR: James Schilz
// CREATED: 2026-06-04
// NOTES: Milestone 3.6.0 — total population also stored on simulationState.population.
// =============================================================================

namespace CCS.Modules.Settlements
{
    [Serializable]
    public sealed class CCS_SettlementPopulationState
    {
        public int totalPopulation;
        public int populationCapacity;
        public float populationGrowthRate;
        public float populationStability;
        public int farmerCount;
        public int rancherCount;
        public int minerCount;
        public int lumberWorkerCount;
        public int merchantCount;
        public int laborerCount;

        public int GetWorkforceCount(CCS_SettlementPopulationCategory category)
        {
            switch (category)
            {
                case CCS_SettlementPopulationCategory.Farmers:
                    return farmerCount < 0 ? 0 : farmerCount;
                case CCS_SettlementPopulationCategory.Ranchers:
                    return rancherCount < 0 ? 0 : rancherCount;
                case CCS_SettlementPopulationCategory.Miners:
                    return minerCount < 0 ? 0 : minerCount;
                case CCS_SettlementPopulationCategory.LumberWorkers:
                    return lumberWorkerCount < 0 ? 0 : lumberWorkerCount;
                case CCS_SettlementPopulationCategory.Merchants:
                    return merchantCount < 0 ? 0 : merchantCount;
                case CCS_SettlementPopulationCategory.Laborers:
                    return laborerCount < 0 ? 0 : laborerCount;
                default:
                    return 0;
            }
        }

        public void SetWorkforceCount(CCS_SettlementPopulationCategory category, int count)
        {
            int safeCount = count < 0 ? 0 : count;
            switch (category)
            {
                case CCS_SettlementPopulationCategory.Farmers:
                    farmerCount = safeCount;
                    break;
                case CCS_SettlementPopulationCategory.Ranchers:
                    rancherCount = safeCount;
                    break;
                case CCS_SettlementPopulationCategory.Miners:
                    minerCount = safeCount;
                    break;
                case CCS_SettlementPopulationCategory.LumberWorkers:
                    lumberWorkerCount = safeCount;
                    break;
                case CCS_SettlementPopulationCategory.Merchants:
                    merchantCount = safeCount;
                    break;
                case CCS_SettlementPopulationCategory.Laborers:
                    laborerCount = safeCount;
                    break;
            }
        }

        public int SumWorkforceCounts()
        {
            return GetWorkforceCount(CCS_SettlementPopulationCategory.Farmers)
                + GetWorkforceCount(CCS_SettlementPopulationCategory.Ranchers)
                + GetWorkforceCount(CCS_SettlementPopulationCategory.Miners)
                + GetWorkforceCount(CCS_SettlementPopulationCategory.LumberWorkers)
                + GetWorkforceCount(CCS_SettlementPopulationCategory.Merchants)
                + GetWorkforceCount(CCS_SettlementPopulationCategory.Laborers);
        }
    }
}
