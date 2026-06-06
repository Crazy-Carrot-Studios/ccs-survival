// =============================================================================
// SCRIPT: CCS_SettlementHousingType
// CATEGORY: Modules / Settlements / Runtime / Housing
// PURPOSE: Generic settlement-owned housing archetypes for population capacity.
// PLACEMENT: Referenced by CCS_SettlementHousingDefinition and validation utilities.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 4.4.0 — no player housing or final building art.
// =============================================================================

namespace CCS.Modules.Settlements
{
    public enum CCS_SettlementHousingType
    {
        Unknown = 0,
        WorkerCabin = 1,
        Farmhouse = 2,
        BoardingHouse = 3,
        MiningBarracks = 4
    }
}
