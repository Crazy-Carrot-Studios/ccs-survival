// =============================================================================
// SCRIPT: CCS_SettlementPopulationCategory
// CATEGORY: Modules / Settlements / Runtime / Population
// PURPOSE: Workforce category bands for frontier settlement population simulation.
// PLACEMENT: Referenced by population state, utility, and snapshots.
// AUTHOR: James Schilz
// CREATED: 2026-06-04
// NOTES: Milestone 3.6.0 population foundation — generic categories only.
// =============================================================================

namespace CCS.Modules.Settlements
{
    public enum CCS_SettlementPopulationCategory
    {
        Unknown = 0,
        Farmers = 1,
        Ranchers = 2,
        Miners = 3,
        LumberWorkers = 4,
        Merchants = 5,
        Laborers = 6
    }
}
