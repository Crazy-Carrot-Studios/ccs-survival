// =============================================================================
// SCRIPT: CCS_PopulationPresenceSnapshot
// CATEGORY: Modules / Settlements / Runtime / PopulationPresence
// PURPOSE: Query snapshot of population presence actor counts for a settlement.
// PLACEMENT: Built by CCS_PopulationPresenceValidationUtility.
// AUTHOR: James Schilz
// CREATED: 2026-06-04
// NOTES: Milestone 4.0.0 — derived from CCS_SettlementPopulationSnapshot.
// =============================================================================

namespace CCS.Modules.Settlements
{
    public sealed class CCS_PopulationPresenceSnapshot
    {
        public static readonly CCS_PopulationPresenceSnapshot Empty = new CCS_PopulationPresenceSnapshot();

        public string SettlementId { get; set; } = string.Empty;

        public CCS_PopulationPresenceEntry[] Entries { get; set; } =
            System.Array.Empty<CCS_PopulationPresenceEntry>();

        public bool IsValid => !string.IsNullOrWhiteSpace(SettlementId);
    }

    public sealed class CCS_PopulationPresenceEntry
    {
        public string AnchorId { get; set; } = string.Empty;

        public CCS_SettlementPopulationCategory WorkforceCategory { get; set; }

        public int SourcePopulationCount { get; set; }

        public int VisibleActorCount { get; set; }
    }
}
