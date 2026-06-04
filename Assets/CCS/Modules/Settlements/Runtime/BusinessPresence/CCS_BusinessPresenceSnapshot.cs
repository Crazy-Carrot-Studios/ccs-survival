using System;

// =============================================================================
// SCRIPT: CCS_BusinessPresenceSnapshot
// CATEGORY: Modules / Settlements / Runtime / BusinessPresence
// PURPOSE: Query snapshot of visible business presence states for a settlement.
// PLACEMENT: Built by CCS_BusinessPresenceValidationUtility.
// AUTHOR: James Schilz
// CREATED: 2026-06-04
// NOTES: Milestone 3.8.0 — visible business presence foundation.
// =============================================================================

namespace CCS.Modules.Settlements
{
    public sealed class CCS_BusinessPresenceSnapshot
    {
        public static readonly CCS_BusinessPresenceSnapshot Empty = new CCS_BusinessPresenceSnapshot();

        public string SettlementId { get; set; } = string.Empty;

        public CCS_BusinessPresenceEntry[] Entries { get; set; } = Array.Empty<CCS_BusinessPresenceEntry>();

        public bool IsValid => !string.IsNullOrWhiteSpace(SettlementId);
    }

    public sealed class CCS_BusinessPresenceEntry
    {
        public string AnchorId { get; set; } = string.Empty;

        public string BusinessId { get; set; } = string.Empty;

        public CCS_BusinessType BusinessType { get; set; } = CCS_BusinessType.Unknown;

        public string DisplayName { get; set; } = string.Empty;

        public CCS_BusinessPresenceStatus Status { get; set; } = CCS_BusinessPresenceStatus.Locked;
    }
}
