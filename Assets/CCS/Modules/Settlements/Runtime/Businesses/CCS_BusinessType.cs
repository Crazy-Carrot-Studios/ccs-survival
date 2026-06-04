// =============================================================================
// SCRIPT: CCS_BusinessType
// CATEGORY: Modules / Settlements / Runtime / Businesses
// PURPOSE: Frontier settlement business archetypes for simulation activation.
// PLACEMENT: Used by business profiles, world simulation, and validation.
// AUTHOR: James Schilz
// CREATED: 2026-06-04
// NOTES: Milestone 3.7.0 — frontier businesses foundation.
// =============================================================================

namespace CCS.Modules.Settlements
{
    public enum CCS_BusinessType
    {
        Unknown = 0,
        GeneralStore = 1,
        Blacksmith = 2,
        Stable = 3,
        Gunsmith = 4,
        Bank = 5,
        FarmSupply = 6,
        MiningSupplier = 7,
        LumberYard = 8,
        ContractOffice = 9
    }
}
