using System;

// =============================================================================
// SCRIPT: CCS_BusinessSnapshot
// CATEGORY: Modules / Settlements / Runtime / Businesses
// PURPOSE: Query snapshot of active, inactive, and available businesses per settlement.
// PLACEMENT: Built by CCS_BusinessValidationUtility; exposed via CCS_BusinessService.
// AUTHOR: James Schilz
// CREATED: 2026-06-04
// NOTES: Milestone 3.7.0 — frontier businesses foundation.
// =============================================================================

namespace CCS.Modules.Settlements
{
    public sealed class CCS_BusinessSnapshot
    {
        public static readonly CCS_BusinessSnapshot Empty = new CCS_BusinessSnapshot();

        public string SettlementId { get; set; } = string.Empty;

        public CCS_BusinessInstance[] ActiveBusinesses { get; set; } = Array.Empty<CCS_BusinessInstance>();

        public CCS_BusinessInstance[] InactiveBusinesses { get; set; } = Array.Empty<CCS_BusinessInstance>();

        public CCS_BusinessInstance[] AvailableBusinesses { get; set; } = Array.Empty<CCS_BusinessInstance>();

        public bool IsValid => !string.IsNullOrWhiteSpace(SettlementId);
    }
}
