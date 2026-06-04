// =============================================================================
// SCRIPT: CCS_ReputationTier
// CATEGORY: Modules / Reputation / Runtime / Data
// PURPOSE: Display tiers for reputation standing values.
// PLACEMENT: Used by CCS_ReputationService and CCS_ReputationSnapshot.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 2.7.0 default range -100 to +100.
// =============================================================================

namespace CCS.Modules.Reputation
{
    public enum CCS_ReputationTier
    {
        Hostile = 0,
        Distrusted = 1,
        Neutral = 2,
        Trusted = 3,
        Honored = 4
    }
}
