using System;
using CCS.Modules.Regions;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_ContractDefinition
// CATEGORY: Modules / Contracts / Runtime / Definitions
// PURPOSE: ScriptableObject frontier contract/job definition.
// PLACEMENT: Assets/CCS/Survival/Content/Contracts/
// AUTHOR: James Schilz
// CREATED: 2026-06-04
// NOTES: Milestone 3.0.0 frontier contracts foundation.
// =============================================================================

namespace CCS.Modules.Contracts
{
    [CreateAssetMenu(
        fileName = "CCS_ContractDefinition",
        menuName = "CCS/Survival/Contracts/Contract Definition")]
    public sealed class CCS_ContractDefinition : ScriptableObject
    {
        [SerializeField] private string contractId = string.Empty;

        [SerializeField] private string displayName = "Frontier Contract";

        [SerializeField] private CCS_ContractType contractType = CCS_ContractType.TradingPostSupply;

        [Tooltip("Regional economic category favored by this contract.")]
        [SerializeField] private CCS_RegionSpecializationType regionSpecialization = CCS_RegionSpecializationType.Unknown;

        [Tooltip("Optional settlement restriction for accepting this contract.")]
        [SerializeField] private string settlementId = string.Empty;

        [SerializeField] private CCS_ContractRequirement[] requirements = Array.Empty<CCS_ContractRequirement>();

        [SerializeField] private CCS_ContractReward reward = new CCS_ContractReward();

        [SerializeField] private bool enabled = true;

        [Header("Freight Delivery")]
        [Tooltip("Origin settlement where freight contracts are accepted.")]
        [SerializeField] private string freightSourceSettlementId = string.Empty;

        [Tooltip("Destination settlement contract board where freight is delivered.")]
        [SerializeField] private string freightDestinationSettlementId = string.Empty;

        [Tooltip("Optional linked trade route id for usage tracking.")]
        [SerializeField] private string linkedTradeRouteId = string.Empty;

        [Tooltip("When true, wagon cargo is checked before player inventory.")]
        [SerializeField] private bool preferWagonCargo = true;

        [Tooltip("When true, player inventory may satisfy freight if wagon cargo is insufficient.")]
        [SerializeField] private bool allowPlayerInventoryFallback;

        public string ContractId => contractId ?? string.Empty;

        public string DisplayName => displayName ?? string.Empty;

        public CCS_ContractType ContractType => contractType;

        public CCS_RegionSpecializationType RegionSpecialization => ResolveRegionSpecialization();

        public string SettlementId => settlementId ?? string.Empty;

        public CCS_ContractRequirement[] Requirements => requirements ?? Array.Empty<CCS_ContractRequirement>();

        public CCS_ContractReward Reward => reward;

        public bool Enabled => enabled;

        public bool IsFreightContract => contractType == CCS_ContractType.FreightDelivery;

        public string FreightSourceSettlementId => freightSourceSettlementId ?? string.Empty;

        public string FreightDestinationSettlementId => freightDestinationSettlementId ?? string.Empty;

        public string LinkedTradeRouteId => linkedTradeRouteId ?? string.Empty;

        public bool PreferWagonCargo => preferWagonCargo;

        public bool AllowPlayerInventoryFallback => allowPlayerInventoryFallback;

        public bool MatchesSettlement(string resolvedSettlementId)
        {
            if (IsFreightContract)
            {
                return string.Equals(FreightSourceSettlementId, resolvedSettlementId, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(
                        FreightDestinationSettlementId,
                        resolvedSettlementId,
                        StringComparison.OrdinalIgnoreCase);
            }

            return string.IsNullOrWhiteSpace(SettlementId)
                || string.Equals(SettlementId, resolvedSettlementId, StringComparison.OrdinalIgnoreCase);
        }

        public bool CanAcceptAtSettlement(string settlementId)
        {
            if (!IsFreightContract)
            {
                return MatchesSettlement(settlementId);
            }

            return string.Equals(FreightSourceSettlementId, settlementId, StringComparison.OrdinalIgnoreCase);
        }

        public bool CanCompleteAtSettlement(string settlementId)
        {
            if (!IsFreightContract)
            {
                return true;
            }

            return string.Equals(FreightDestinationSettlementId, settlementId, StringComparison.OrdinalIgnoreCase);
        }

        public CCS_RegionSpecializationType ResolveRegionSpecialization()
        {
            if (regionSpecialization != CCS_RegionSpecializationType.Unknown)
            {
                return regionSpecialization;
            }

            CCS_ContractRequirement[] contractRequirements = Requirements;
            for (int index = 0; index < contractRequirements.Length; index++)
            {
                CCS_ContractRequirement requirement = contractRequirements[index];
                if (requirement == null || string.IsNullOrWhiteSpace(requirement.ItemId))
                {
                    continue;
                }

                if (CCS_RegionEconomyUtility.TryResolveSpecializationForItem(
                        requirement.ItemId,
                        out CCS_RegionSpecializationType resolved))
                {
                    return resolved;
                }
            }

            return CCS_RegionSpecializationType.Unknown;
        }

        public void ApplyRuntimeInit(CCS_ContractRuntimeInitData initData)
        {
            if (initData == null)
            {
                return;
            }

            contractId = initData.ContractId ?? string.Empty;
            displayName = initData.DisplayName ?? string.Empty;
            contractType = initData.ContractType;
            regionSpecialization = initData.RegionSpecialization;
            settlementId = initData.SettlementId ?? string.Empty;
            requirements = initData.Requirements ?? Array.Empty<CCS_ContractRequirement>();
            reward = initData.Reward ?? new CCS_ContractReward();
            enabled = initData.Enabled;
            freightSourceSettlementId = initData.FreightSourceSettlementId ?? string.Empty;
            freightDestinationSettlementId = initData.FreightDestinationSettlementId ?? string.Empty;
            linkedTradeRouteId = initData.LinkedTradeRouteId ?? string.Empty;
            preferWagonCargo = initData.PreferWagonCargo;
            allowPlayerInventoryFallback = initData.AllowPlayerInventoryFallback;
        }
    }
}
