// =============================================================================
// SCRIPT: CCS_BusinessActivatedEventArgs
// CATEGORY: Modules / Settlements / Runtime / Businesses
// PURPOSE: Event payload when a settlement business activates from simulation thresholds.
// PLACEMENT: Raised by CCS_BusinessService after world simulation evaluation.
// AUTHOR: James Schilz
// CREATED: 2026-06-04
// NOTES: Milestone 3.7.0 — frontier businesses foundation.
// =============================================================================

namespace CCS.Modules.Settlements
{
    public sealed class CCS_BusinessActivatedEventArgs
    {
        public CCS_BusinessSnapshot Snapshot { get; set; } = CCS_BusinessSnapshot.Empty;

        public CCS_BusinessType BusinessType { get; set; } = CCS_BusinessType.Unknown;

        public string BusinessId { get; set; } = string.Empty;
    }
}
