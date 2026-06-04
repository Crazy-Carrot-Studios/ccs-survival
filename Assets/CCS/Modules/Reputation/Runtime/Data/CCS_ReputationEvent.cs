// =============================================================================
// SCRIPT: CCS_ReputationEvent
// CATEGORY: Modules / Reputation / Runtime / Data
// PURPOSE: Last reputation event record for debug HUD and validation placeholders.
// PLACEMENT: Recorded by CCS_ReputationService.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 2.7.0 reputation and service trust foundation.
// =============================================================================

namespace CCS.Modules.Reputation
{
    public sealed class CCS_ReputationEvent
    {
        public CCS_ReputationEvent(
            CCS_ReputationEventType eventType,
            CCS_ReputationScopeType scopeType,
            string targetId,
            int deltaApplied,
            int valueAfter,
            CCS_ReputationTier tierAfter,
            string timestampUtc,
            string summaryPlaceholder)
        {
            EventType = eventType;
            ScopeType = scopeType;
            TargetId = targetId ?? string.Empty;
            DeltaApplied = deltaApplied;
            ValueAfter = valueAfter;
            TierAfter = tierAfter;
            TimestampUtc = timestampUtc ?? string.Empty;
            SummaryPlaceholder = summaryPlaceholder ?? string.Empty;
        }

        public CCS_ReputationEventType EventType { get; }

        public CCS_ReputationScopeType ScopeType { get; }

        public string TargetId { get; }

        public int DeltaApplied { get; }

        public int ValueAfter { get; }

        public CCS_ReputationTier TierAfter { get; }

        public string TimestampUtc { get; }

        public string SummaryPlaceholder { get; }
    }
}
