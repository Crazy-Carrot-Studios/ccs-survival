using System;
using CCS.Survival;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_ReputationProfile
// CATEGORY: Modules / Reputation / Runtime / Profiles
// PURPOSE: Profile catalog for reputation definitions and event hook configuration.
// PLACEMENT: Assets/CCS/Survival/Profiles/Reputation/
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Registered on CCS_SurvivalGameplayServiceHost. Milestone 2.7.0.
// =============================================================================

namespace CCS.Modules.Reputation
{
    [CreateAssetMenu(
        fileName = "CCS_ReputationProfile",
        menuName = "CCS/Survival/Reputation/Reputation Profile")]
    public sealed class CCS_ReputationProfile : CCS_SurvivalProfileBase
    {
        [SerializeField] private CCS_ReputationDefinition[] reputationDefinitions =
            Array.Empty<CCS_ReputationDefinition>();

        [SerializeField] private string defaultSettlementReputationDefinitionId =
            CCS_ReputationContentIds.FrontierTradingPostReputationDefinitionId;

        [SerializeField] private string defaultTradingPostSettlementId =
            CCS_ReputationContentIds.DefaultTradingPostSettlementId;

        [Header("Event Hooks")]
        [SerializeField] private bool enableGoodsSoldEvents = true;
        [SerializeField] private bool enableLoanRepaidEvents = true;
        [SerializeField] private bool enableUpkeepPaidEvents = true;
        [SerializeField] private bool enableFailedUpkeepEvents = true;
        [SerializeField] private bool enableSettlementDiscoveredEvents = true;

        [Header("Conservative Deltas")]
        [SerializeField] private int goodsSoldDelta = 2;
        [SerializeField] private int loanRepaidDelta = 3;
        [SerializeField] private int upkeepPaidDelta = 2;
        [SerializeField] private int failedUpkeepDelta = -1;
        [SerializeField] private int settlementDiscoveredDelta = 1;

        [SerializeField] private bool enableContractCompletedEvents = true;

        [SerializeField] private bool enableDebugLogging = true;

        [Header("Service Access")]
        [SerializeField] private CCS_ServiceAccessProfile serviceAccessProfile;

        [Header("Vendor Price Modifiers")]
        [SerializeField] private bool enableBuyPriceModifiers = true;

        [SerializeField] private bool enableSellPriceModifiers;

        [SerializeField] private float neutralBuyPriceModifier = 1f;

        [SerializeField] private float trustedBuyPriceModifier = 0.95f;

        [SerializeField] private float honoredBuyPriceModifier = 0.9f;

        [SerializeField] private float distrustedBuyPriceModifier = 1.1f;

        [SerializeField] private float hostileBuyPriceModifier = 1.25f;

        [SerializeField] private float neutralSellPriceModifier = 1f;

        [SerializeField] private float trustedSellPriceModifier = 1f;

        [SerializeField] private float honoredSellPriceModifier = 1f;

        [SerializeField] private float distrustedSellPriceModifier = 1f;

        [SerializeField] private float hostileSellPriceModifier = 1f;

        public CCS_ServiceAccessProfile ServiceAccessProfile => serviceAccessProfile;

        public bool EnableBuyPriceModifiers => enableBuyPriceModifiers;

        public bool EnableSellPriceModifiers => enableSellPriceModifiers;

        public float NeutralBuyPriceModifier => neutralBuyPriceModifier;

        public float TrustedBuyPriceModifier => trustedBuyPriceModifier;

        public float HonoredBuyPriceModifier => honoredBuyPriceModifier;

        public float DistrustedBuyPriceModifier => distrustedBuyPriceModifier;

        public float HostileBuyPriceModifier => hostileBuyPriceModifier;

        public float NeutralSellPriceModifier => neutralSellPriceModifier;

        public float TrustedSellPriceModifier => trustedSellPriceModifier;

        public float HonoredSellPriceModifier => honoredSellPriceModifier;

        public float DistrustedSellPriceModifier => distrustedSellPriceModifier;

        public float HostileSellPriceModifier => hostileSellPriceModifier;

        public CCS_ReputationDefinition[] ReputationDefinitions =>
            reputationDefinitions ?? Array.Empty<CCS_ReputationDefinition>();

        public string DefaultSettlementReputationDefinitionId =>
            defaultSettlementReputationDefinitionId ?? string.Empty;

        public string DefaultTradingPostSettlementId => defaultTradingPostSettlementId ?? string.Empty;

        public bool EnableGoodsSoldEvents => enableGoodsSoldEvents;

        public bool EnableLoanRepaidEvents => enableLoanRepaidEvents;

        public bool EnableUpkeepPaidEvents => enableUpkeepPaidEvents;

        public bool EnableFailedUpkeepEvents => enableFailedUpkeepEvents;

        public bool EnableSettlementDiscoveredEvents => enableSettlementDiscoveredEvents;

        public int GoodsSoldDelta => goodsSoldDelta;

        public int LoanRepaidDelta => loanRepaidDelta;

        public int UpkeepPaidDelta => upkeepPaidDelta;

        public int FailedUpkeepDelta => failedUpkeepDelta;

        public int SettlementDiscoveredDelta => settlementDiscoveredDelta;

        public bool EnableContractCompletedEvents => enableContractCompletedEvents;

        public bool EnableDebugLogging => enableDebugLogging;

        public bool TryGetDefinitionById(string reputationDefinitionId, out CCS_ReputationDefinition definition)
        {
            definition = null;
            if (string.IsNullOrWhiteSpace(reputationDefinitionId))
            {
                return false;
            }

            CCS_ReputationDefinition[] definitions = ReputationDefinitions;
            for (int index = 0; index < definitions.Length; index++)
            {
                CCS_ReputationDefinition candidate = definitions[index];
                if (candidate != null
                    && string.Equals(candidate.ReputationDefinitionId, reputationDefinitionId, StringComparison.OrdinalIgnoreCase))
                {
                    definition = candidate;
                    return true;
                }
            }

            return false;
        }

        public bool TryGetDefaultSettlementReputation(out CCS_ReputationDefinition definition)
        {
            return TryGetDefinitionById(DefaultSettlementReputationDefinitionId, out definition);
        }
    }
}
