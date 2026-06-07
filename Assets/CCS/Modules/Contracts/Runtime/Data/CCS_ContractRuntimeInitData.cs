using System;
using CCS.Modules.Regions;
using CCS.Modules.WorldSimulation;

// =============================================================================
// SCRIPT: CCS_ContractRuntimeInitData
// CATEGORY: Modules / Contracts / Runtime / Data
// PURPOSE: Runtime payload for creating temporary contract definitions.
// PLACEMENT: Built by CCS_DynamicContractValidationUtility and applied to ScriptableObject instances.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 5.3.0 — keeps dynamic contracts on the existing completion path.
// =============================================================================

namespace CCS.Modules.Contracts
{
    public sealed class CCS_ContractRuntimeInitData
    {
        public string ContractId = string.Empty;

        public string DisplayName = "Generated Contract";

        public CCS_ContractType ContractType = CCS_ContractType.TradingPostSupply;

        public CCS_RegionSpecializationType RegionSpecialization = CCS_RegionSpecializationType.Unknown;

        public string SettlementId = string.Empty;

        public CCS_ContractRequirement[] Requirements = Array.Empty<CCS_ContractRequirement>();

        public CCS_ContractReward Reward = new CCS_ContractReward();

        public string FreightSourceSettlementId = string.Empty;

        public string FreightDestinationSettlementId = string.Empty;

        public string LinkedTradeRouteId = string.Empty;

        public bool PreferWagonCargo = true;

        public bool AllowPlayerInventoryFallback;

        public bool Enabled = true;
    }
}
