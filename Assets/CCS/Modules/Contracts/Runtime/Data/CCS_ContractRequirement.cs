using System;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_ContractRequirement
// CATEGORY: Modules / Contracts / Runtime / Data
// PURPOSE: Item delivery requirement for contract completion.
// PLACEMENT: Embedded in CCS_ContractDefinition.
// AUTHOR: James Schilz
// CREATED: 2026-06-04
// NOTES: Milestone 3.0.0 frontier contracts foundation.
// =============================================================================

namespace CCS.Modules.Contracts
{
    [Serializable]
    public sealed class CCS_ContractRequirement
    {
        [SerializeField] private string itemId = string.Empty;

        [SerializeField] private int quantity = 1;

        [Tooltip("Optional settlement restriction for this requirement. Empty allows any settlement.")]
        [SerializeField] private string settlementIdRestriction = string.Empty;

        public string ItemId => itemId ?? string.Empty;

        public int Quantity => quantity < 1 ? 1 : quantity;

        public string SettlementIdRestriction => settlementIdRestriction ?? string.Empty;

        public bool MatchesSettlement(string settlementId)
        {
            return string.IsNullOrWhiteSpace(SettlementIdRestriction)
                || string.Equals(SettlementIdRestriction, settlementId, StringComparison.OrdinalIgnoreCase);
        }

        public void ApplyRuntimeInit(string resolvedItemId, int resolvedQuantity, string settlementIdRestriction = "")
        {
            itemId = resolvedItemId ?? string.Empty;
            quantity = resolvedQuantity < 1 ? 1 : resolvedQuantity;
            this.settlementIdRestriction = settlementIdRestriction ?? string.Empty;
        }
    }
}
