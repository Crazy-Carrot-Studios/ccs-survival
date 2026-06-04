// =============================================================================
// SCRIPT: CCS_BusinessInstance
// CATEGORY: Modules / Settlements / Runtime / Businesses
// PURPOSE: Runtime view of a catalogued business at a settlement.
// PLACEMENT: Built by CCS_BusinessValidationUtility for services and playtest.
// AUTHOR: James Schilz
// CREATED: 2026-06-04
// NOTES: Milestone 3.7.0 — distinguishes active, inactive, and threshold-available.
// =============================================================================

namespace CCS.Modules.Settlements
{
    public sealed class CCS_BusinessInstance
    {
        public string SettlementId { get; set; } = string.Empty;

        public CCS_BusinessType BusinessType { get; set; } = CCS_BusinessType.Unknown;

        public string BusinessId { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public bool IsActive { get; set; }

        public bool MeetsActivationThresholds { get; set; }

        public bool IsAvailable => MeetsActivationThresholds && !IsActive;

        public bool IsInactive => !IsActive && !MeetsActivationThresholds;
    }
}
