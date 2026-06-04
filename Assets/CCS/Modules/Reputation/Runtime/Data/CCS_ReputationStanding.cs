// =============================================================================
// SCRIPT: CCS_ReputationStanding
// CATEGORY: Modules / Reputation / Runtime / Data
// PURPOSE: Runtime reputation standing for queries and debug display.
// PLACEMENT: Returned by CCS_ReputationService.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 2.7.0 reputation and service trust foundation.
// =============================================================================

namespace CCS.Modules.Reputation
{
    public sealed class CCS_ReputationStanding
    {
        public CCS_ReputationStanding(
            string reputationDefinitionId,
            CCS_ReputationScopeType scopeType,
            string targetId,
            int currentValue,
            int minValue,
            int maxValue,
            CCS_ReputationTier displayTier)
        {
            ReputationDefinitionId = reputationDefinitionId ?? string.Empty;
            ScopeType = scopeType;
            TargetId = targetId ?? string.Empty;
            CurrentValue = currentValue;
            MinValue = minValue;
            MaxValue = maxValue;
            DisplayTier = displayTier;
        }

        public string ReputationDefinitionId { get; }

        public CCS_ReputationScopeType ScopeType { get; }

        public string TargetId { get; }

        public int CurrentValue { get; }

        public int MinValue { get; }

        public int MaxValue { get; }

        public CCS_ReputationTier DisplayTier { get; }
    }
}
