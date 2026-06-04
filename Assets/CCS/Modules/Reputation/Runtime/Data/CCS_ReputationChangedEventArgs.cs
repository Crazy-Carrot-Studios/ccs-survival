// =============================================================================
// SCRIPT: CCS_ReputationChangedEventArgs
// CATEGORY: Modules / Reputation / Runtime / Data
// PURPOSE: Event payload when a reputation standing changes.
// PLACEMENT: Raised by CCS_ReputationService and forwarded by CCS_SettlementService.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 2.7.0 reputation and service trust foundation.
// =============================================================================

namespace CCS.Modules.Reputation
{
    public sealed class CCS_ReputationChangedEventArgs
    {
        public CCS_ReputationChangedEventArgs(
            string reputationDefinitionId,
            CCS_ReputationScopeType scopeType,
            string targetId,
            int previousValue,
            int newValue,
            CCS_ReputationTier previousTier,
            CCS_ReputationTier newTier,
            CCS_ReputationEventType eventType,
            string message)
        {
            ReputationDefinitionId = reputationDefinitionId ?? string.Empty;
            ScopeType = scopeType;
            TargetId = targetId ?? string.Empty;
            PreviousValue = previousValue;
            NewValue = newValue;
            PreviousTier = previousTier;
            NewTier = newTier;
            EventType = eventType;
            Message = message ?? string.Empty;
        }

        public string ReputationDefinitionId { get; }

        public CCS_ReputationScopeType ScopeType { get; }

        public string TargetId { get; }

        public int PreviousValue { get; }

        public int NewValue { get; }

        public CCS_ReputationTier PreviousTier { get; }

        public CCS_ReputationTier NewTier { get; }

        public CCS_ReputationEventType EventType { get; }

        public string Message { get; }
    }
}
