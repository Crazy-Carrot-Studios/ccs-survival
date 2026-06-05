// =============================================================================
// SCRIPT: CCS_SettlementVisualGrowthState
// CATEGORY: Modules / Settlements / Runtime / VisualGrowth
// PURPOSE: Optional per-anchor runtime visibility override for dev harness.
// PLACEMENT: Not persisted by default; growth simulation drives visuals.
// AUTHOR: James Schilz
// CREATED: 2026-06-04
// NOTES: Milestone 3.9.0 — prefer deriving from settlement growth snapshots.
// =============================================================================

namespace CCS.Modules.Settlements
{
    public sealed class CCS_SettlementVisualGrowthState
    {
        public string AnchorId { get; set; } = string.Empty;

        public string SettlementId { get; set; } = string.Empty;

        public bool HasVisibilityOverride { get; set; }

        public bool VisibilityOverrideActive { get; set; }
    }
}
