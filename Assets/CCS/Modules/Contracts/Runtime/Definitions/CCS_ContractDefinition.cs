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

        public string ContractId => contractId ?? string.Empty;

        public string DisplayName => displayName ?? string.Empty;

        public CCS_ContractType ContractType => contractType;

        public CCS_RegionSpecializationType RegionSpecialization => ResolveRegionSpecialization();

        public string SettlementId => settlementId ?? string.Empty;

        public CCS_ContractRequirement[] Requirements => requirements ?? Array.Empty<CCS_ContractRequirement>();

        public CCS_ContractReward Reward => reward;

        public bool Enabled => enabled;

        public bool MatchesSettlement(string resolvedSettlementId)
        {
            return string.IsNullOrWhiteSpace(SettlementId)
                || string.Equals(SettlementId, resolvedSettlementId, StringComparison.OrdinalIgnoreCase);
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
    }
}
