// =============================================================================
// SCRIPT: CCS_PopulationPresenceState
// CATEGORY: Modules / Settlements / Runtime / PopulationPresence
// PURPOSE: Optional per-anchor runtime override for dev harness visibility.
// PLACEMENT: Not persisted by default; population simulation drives actors.
// AUTHOR: James Schilz
// CREATED: 2026-06-04
// NOTES: Milestone 4.0.0 — prefer deriving from settlement population snapshots.
// =============================================================================

namespace CCS.Modules.Settlements
{
    public sealed class CCS_PopulationPresenceState
    {
        public string AnchorId { get; set; } = string.Empty;

        public string SettlementId { get; set; } = string.Empty;

        public int SourcePopulationCount { get; set; }

        public int VisibleActorCount { get; set; }
    }
}
