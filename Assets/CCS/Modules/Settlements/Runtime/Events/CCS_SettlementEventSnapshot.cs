using System;

// =============================================================================
// SCRIPT: CCS_SettlementEventSnapshot
// CATEGORY: Modules / Settlements / Runtime / Events
// PURPOSE: Runtime snapshot of active settlement event and temporary modifiers.
// PLACEMENT: Returned by CCS_SettlementEventService and runtime bridge.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 5.1.0 — metadata and small temporary modifiers only.
// =============================================================================

namespace CCS.Modules.Settlements
{
    public sealed class CCS_SettlementEventSnapshot
    {
        public static readonly CCS_SettlementEventSnapshot Empty = new CCS_SettlementEventSnapshot();

        public string ActiveEventId { get; set; } = string.Empty;

        public CCS_SettlementEventType EventType { get; set; } = CCS_SettlementEventType.Unknown;

        public string SettlementId { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public string EventMarkerAnchorId { get; set; } = string.Empty;

        public string PreferredSocialAnchorId { get; set; } = string.Empty;

        public string DialogueAppendLine { get; set; } = string.Empty;

        public int StartDayNumber { get; set; } = 1;

        public int StartHour { get; set; }

        public int DurationHours { get; set; } = 24;

        public float ProsperityBonus { get; set; }

        public float SupplyBonus { get; set; }

        public float ContractRewardMultiplier { get; set; } = 1f;

        public float ReputationGainMultiplier { get; set; } = 1f;

        public bool IsActive { get; set; }

        public bool IsValid =>
            IsActive
            && !string.IsNullOrWhiteSpace(ActiveEventId)
            && !string.IsNullOrWhiteSpace(SettlementId)
            && EventType != CCS_SettlementEventType.Unknown;

        public bool HasModifiers =>
            ProsperityBonus > 0f
            || SupplyBonus > 0f
            || ContractRewardMultiplier > 1f
            || ReputationGainMultiplier > 1f;
    }
}
