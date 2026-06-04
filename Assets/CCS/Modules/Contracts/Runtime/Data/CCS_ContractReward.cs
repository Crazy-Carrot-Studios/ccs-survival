using System;
using CCS.Modules.WorldSimulation;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_ContractReward
// CATEGORY: Modules / Contracts / Runtime / Data
// PURPOSE: Reward payload for completed frontier contracts.
// PLACEMENT: Embedded in CCS_ContractDefinition.
// AUTHOR: James Schilz
// CREATED: 2026-06-04
// NOTES: Milestone 3.0.0 — conservative trade dollars, reputation, prosperity, supply.
// =============================================================================

namespace CCS.Modules.Contracts
{
    [Serializable]
    public sealed class CCS_ContractReward
    {
        [SerializeField] private int tradeDollars;

        [SerializeField] private int reputationGain = 2;

        [Tooltip("Optional reputation gain at the freight origin settlement.")]
        [SerializeField] private int originReputationGain;

        [SerializeField] private float prosperityGain = 1f;

        [SerializeField] private CCS_SettlementSupplyType supplyType = CCS_SettlementSupplyType.TradeGoods;

        [SerializeField] private float supplyAmount = 1f;

        public int TradeDollars => tradeDollars < 0 ? 0 : tradeDollars;

        public int ReputationGain => reputationGain;

        public int OriginReputationGain => originReputationGain;

        public float ProsperityGain => prosperityGain < 0f ? 0f : prosperityGain;

        public CCS_SettlementSupplyType SupplyType => supplyType;

        public float SupplyAmount => supplyAmount < 0f ? 0f : supplyAmount;
    }
}
