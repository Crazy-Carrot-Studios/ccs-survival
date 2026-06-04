// =============================================================================
// SCRIPT: CCS_ReputationScopeType
// CATEGORY: Modules / Reputation / Runtime / Data
// PURPOSE: Reputation scope identifiers for settlements, regions, services, and future systems.
// PLACEMENT: Used by CCS_ReputationService and CCS_ReputationDefinition.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 2.7.0 — Settlement scope active; FutureFaction placeholder only.
// =============================================================================

namespace CCS.Modules.Reputation
{
    public enum CCS_ReputationScopeType
    {
        Settlement = 0,
        Region = 1,
        Service = 2,
        FutureFaction = 3,
        Global = 4
    }
}
