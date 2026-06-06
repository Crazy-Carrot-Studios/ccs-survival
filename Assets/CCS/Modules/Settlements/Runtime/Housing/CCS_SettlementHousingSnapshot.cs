using System;

// =============================================================================
// SCRIPT: CCS_SettlementHousingSnapshot
// CATEGORY: Modules / Settlements / Runtime / Housing
// PURPOSE: Query snapshot for housing HUD, playtest, and runtime bridges.
// PLACEMENT: Built by CCS_SettlementHousingValidationUtility from simulation state.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 4.4.0 — includes population capacity breakdown fields.
// =============================================================================

namespace CCS.Modules.Settlements
{
    public sealed class CCS_SettlementHousingSnapshot
    {
        public static readonly CCS_SettlementHousingSnapshot Empty = new CCS_SettlementHousingSnapshot();

        public string SettlementId { get; set; } = string.Empty;

        public int BasePopulationCapacity { get; set; }

        public int HousingCapacityContribution { get; set; }

        public int TotalPopulationCapacity { get; set; }

        public int ActiveHousingCount { get; set; }

        public string[] ActiveHousingNames { get; set; } = Array.Empty<string>();

        public CCS_SettlementHousingEntry[] HousingEntries { get; set; } = Array.Empty<CCS_SettlementHousingEntry>();

        public bool IsValid => !string.IsNullOrWhiteSpace(SettlementId);
    }

    public sealed class CCS_SettlementHousingEntry
    {
        public string HousingId { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public int CapacityContribution { get; set; }

        public CCS_SettlementHousingStatus Status { get; set; } = CCS_SettlementHousingStatus.Unknown;

        public bool IsActive { get; set; }
    }
}
