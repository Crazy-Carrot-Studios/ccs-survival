using UnityEngine;

// =============================================================================
// SCRIPT: CCS_SettlementSnapshot
// CATEGORY: Modules / Settlements / Runtime / Data
// PURPOSE: Runtime discovery record for a settlement (map placeholder data).
// PLACEMENT: Stored by CCS_SettlementService and save payloads.
// AUTHOR: James Schilz
// CREATED: 2026-06-02
// NOTES: No map UI yet; supports future discovery tracking.
// =============================================================================

namespace CCS.Modules.Settlements
{
    public sealed class CCS_SettlementSnapshot
    {
        public static readonly CCS_SettlementSnapshot Empty = new CCS_SettlementSnapshot();

        public string SettlementId { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public CCS_SettlementType SettlementType { get; set; }

        public bool Discovered { get; set; }

        public Vector3 Position { get; set; }

        public bool IsValid =>
            !string.IsNullOrWhiteSpace(SettlementId)
            && !string.IsNullOrWhiteSpace(DisplayName);
    }
}
