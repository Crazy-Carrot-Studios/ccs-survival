using System;
using CCS.Modules.Regions;
using CCS.Modules.Settlements;
using CCS.Modules.WorldSimulation;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_DynamicContractRule
// CATEGORY: Modules / Contracts / Runtime / Dynamic
// PURPOSE: Profile rule describing when and how to generate a settlement contract.
// PLACEMENT: Serialized on CCS_DynamicContractProfile.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 5.3.0 — deterministic generation with cooldown and expiration.
// =============================================================================

namespace CCS.Modules.Contracts
{
    [Serializable]
    public sealed class CCS_DynamicContractRule
    {
        [SerializeField] private string ruleId = string.Empty;

        [SerializeField] private int generationSource = (int)CCS_DynamicContractGenerationSource.Unknown;

        [SerializeField] private int contractKind = (int)CCS_DynamicContractKind.LocalSupply;

        [SerializeField] private int supplyType = (int)CCS_SettlementSupplyType.Food;

        [SerializeField] private float supplyThresholdPercent = 25f;

        [SerializeField] private int eventType = (int)CCS_SettlementEventType.Unknown;

        [SerializeField] private int regionSpecialization = (int)CCS_RegionSpecializationType.Unknown;

        [SerializeField] private string displayName = "Generated Contract";

        [SerializeField] private int contractType = (int)CCS_ContractType.TradingPostSupply;

        [SerializeField] private string[] requiredItemIds = Array.Empty<string>();

        [SerializeField] private int[] requiredQuantities = Array.Empty<int>();

        [SerializeField] private int tradeDollars = 10;

        [SerializeField] private int reputationGain = 2;

        [SerializeField] private float prosperityGain = 1f;

        [SerializeField] private int rewardSupplyType = (int)CCS_SettlementSupplyType.Food;

        [SerializeField] private float rewardSupplyAmount = 1f;

        [SerializeField] private int cooldownDays = 3;

        [SerializeField] private int expirationDays = 7;

        [SerializeField] private string freightDestinationSettlementId = string.Empty;

        [SerializeField] private bool enabled = true;

        [SerializeField] private bool placeholderOnly;

        public string RuleId => ruleId ?? string.Empty;

        public CCS_DynamicContractGenerationSource GenerationSource =>
            Enum.IsDefined(typeof(CCS_DynamicContractGenerationSource), generationSource)
                ? (CCS_DynamicContractGenerationSource)generationSource
                : CCS_DynamicContractGenerationSource.Unknown;

        public CCS_DynamicContractKind ContractKind =>
            Enum.IsDefined(typeof(CCS_DynamicContractKind), contractKind)
                ? (CCS_DynamicContractKind)contractKind
                : CCS_DynamicContractKind.LocalSupply;

        public CCS_SettlementSupplyType SupplyType =>
            Enum.IsDefined(typeof(CCS_SettlementSupplyType), supplyType)
                ? (CCS_SettlementSupplyType)supplyType
                : CCS_SettlementSupplyType.Food;

        public float SupplyThresholdPercent => supplyThresholdPercent < 0f ? 0f : supplyThresholdPercent;

        public CCS_SettlementEventType EventType =>
            Enum.IsDefined(typeof(CCS_SettlementEventType), eventType)
                ? (CCS_SettlementEventType)eventType
                : CCS_SettlementEventType.Unknown;

        public CCS_RegionSpecializationType RegionSpecialization =>
            Enum.IsDefined(typeof(CCS_RegionSpecializationType), regionSpecialization)
                ? (CCS_RegionSpecializationType)regionSpecialization
                : CCS_RegionSpecializationType.Unknown;

        public string DisplayName => displayName ?? string.Empty;

        public CCS_ContractType ContractType =>
            Enum.IsDefined(typeof(CCS_ContractType), contractType)
                ? (CCS_ContractType)contractType
                : CCS_ContractType.TradingPostSupply;

        public string[] RequiredItemIds => requiredItemIds ?? Array.Empty<string>();

        public int[] RequiredQuantities => requiredQuantities ?? Array.Empty<int>();

        public int TradeDollars => tradeDollars < 0 ? 0 : tradeDollars;

        public int ReputationGain => reputationGain;

        public float ProsperityGain => prosperityGain < 0f ? 0f : prosperityGain;

        public CCS_SettlementSupplyType RewardSupplyType =>
            Enum.IsDefined(typeof(CCS_SettlementSupplyType), rewardSupplyType)
                ? (CCS_SettlementSupplyType)rewardSupplyType
                : CCS_SettlementSupplyType.Food;

        public float RewardSupplyAmount => rewardSupplyAmount < 0f ? 0f : rewardSupplyAmount;

        public int CooldownDays => cooldownDays < 1 ? 1 : cooldownDays;

        public int ExpirationDays => expirationDays < 1 ? 1 : expirationDays;

        public string FreightDestinationSettlementId => freightDestinationSettlementId ?? string.Empty;

        public bool Enabled => enabled;

        public bool PlaceholderOnly => placeholderOnly;
    }
}
