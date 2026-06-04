// =============================================================================
// SCRIPT: CCS_ServiceAccessResultType
// CATEGORY: Modules / Reputation / Runtime / Data
// PURPOSE: Result codes for settlement service access evaluation.
// PLACEMENT: Used by CCS_ServiceAccessResult and settlement routing.
// AUTHOR: James Schilz
// CREATED: 2026-06-04
// NOTES: Milestone 2.8.0 service access and price modifier foundation.
// =============================================================================

namespace CCS.Modules.Reputation
{
    public enum CCS_ServiceAccessResultType
    {
        Allowed = 0,
        DeniedReputation = 1,
        DeniedUnavailable = 2,
        DeniedDisabled = 3,
        MissingRequirement = 4
    }
}
