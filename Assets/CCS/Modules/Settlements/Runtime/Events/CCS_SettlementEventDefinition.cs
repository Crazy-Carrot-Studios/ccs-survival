using System;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_SettlementEventDefinition
// CATEGORY: Modules / Settlements / Runtime / Events
// PURPOSE: Profile entry describing one settlement event and its simulation rules.
// PLACEMENT: Serialized on CCS_SettlementEventProfile.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 5.1.0 — active types only; placeholders reserved for future milestones.
// =============================================================================

namespace CCS.Modules.Settlements
{
    [Serializable]
    public sealed class CCS_SettlementEventDefinition
    {
        [SerializeField] private string eventId = string.Empty;

        [SerializeField] private int eventType = (int)CCS_SettlementEventType.Unknown;

        [SerializeField] private string displayName = string.Empty;

        [SerializeField] private string[] eligibleSettlementIds = Array.Empty<string>();

        [SerializeField] private int[] eligibleSettlementTypes = Array.Empty<int>();

        [SerializeField] private int minimumPopulation;

        [SerializeField] private float minimumProsperity;

        [SerializeField] private int minimumActiveBusinesses;

        [SerializeField] private int minimumTradeRouteUsage;

        [SerializeField] private int durationHours = 24;

        [SerializeField] private float prosperityBonus = 2f;

        [SerializeField] private float supplyBonus = 5f;

        [SerializeField] private float contractRewardMultiplier = 1.05f;

        [SerializeField] private float reputationGainMultiplier = 1.05f;

        [SerializeField] private string dialogueAppendLine = string.Empty;

        [SerializeField] private string preferredSocialAnchorId = string.Empty;

        [SerializeField] private string eventMarkerAnchorId = string.Empty;

        public string EventId => eventId ?? string.Empty;

        public CCS_SettlementEventType EventType =>
            Enum.IsDefined(typeof(CCS_SettlementEventType), eventType)
                ? (CCS_SettlementEventType)eventType
                : CCS_SettlementEventType.Unknown;

        public string DisplayName => displayName ?? string.Empty;

        public string[] EligibleSettlementIds => eligibleSettlementIds ?? Array.Empty<string>();

        public int[] EligibleSettlementTypes => eligibleSettlementTypes ?? Array.Empty<int>();

        public int MinimumPopulation => minimumPopulation < 0 ? 0 : minimumPopulation;

        public float MinimumProsperity => minimumProsperity < 0f ? 0f : minimumProsperity;

        public int MinimumActiveBusinesses => minimumActiveBusinesses < 0 ? 0 : minimumActiveBusinesses;

        public int MinimumTradeRouteUsage => minimumTradeRouteUsage < 0 ? 0 : minimumTradeRouteUsage;

        public int DurationHours => durationHours < 1 ? 1 : durationHours;

        public float ProsperityBonus => prosperityBonus < 0f ? 0f : prosperityBonus;

        public float SupplyBonus => supplyBonus < 0f ? 0f : supplyBonus;

        public float ContractRewardMultiplier => contractRewardMultiplier < 1f ? 1f : contractRewardMultiplier;

        public float ReputationGainMultiplier => reputationGainMultiplier < 1f ? 1f : reputationGainMultiplier;

        public string DialogueAppendLine => dialogueAppendLine ?? string.Empty;

        public string PreferredSocialAnchorId => preferredSocialAnchorId ?? string.Empty;

        public string EventMarkerAnchorId => eventMarkerAnchorId ?? string.Empty;
    }
}
