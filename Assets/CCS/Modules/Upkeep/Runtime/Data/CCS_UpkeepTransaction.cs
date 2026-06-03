using System;

// =============================================================================
// SCRIPT: CCS_UpkeepTransaction
// CATEGORY: Modules / Upkeep / Runtime / Data
// PURPOSE: Placeholder transaction record for upkeep and tax payments.
// PLACEMENT: Stored in CCS_UpkeepService transaction history ring buffer.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 2.5.0 tax and upkeep foundation.
// =============================================================================

namespace CCS.Modules.Upkeep
{
    [Serializable]
    public sealed class CCS_UpkeepTransaction
    {
        public string entryId = string.Empty;
        public string targetId = string.Empty;
        public string upkeepDefinitionId = string.Empty;
        public string currencyId = string.Empty;
        public int amount;
        public int paymentSource;
        public int resultState;
        public string reason = string.Empty;
        public string timestampUtc = string.Empty;
        public string summaryPlaceholder = string.Empty;

        public CCS_UpkeepTransaction()
        {
        }

        public CCS_UpkeepTransaction(
            string entryId,
            string targetId,
            string upkeepDefinitionId,
            string currencyId,
            int amount,
            CCS_UpkeepPaymentSource paymentSource,
            CCS_UpkeepState resultState,
            string reason,
            string timestampUtc,
            string summaryPlaceholder)
        {
            this.entryId = entryId ?? string.Empty;
            this.targetId = targetId ?? string.Empty;
            this.upkeepDefinitionId = upkeepDefinitionId ?? string.Empty;
            this.currencyId = currencyId ?? string.Empty;
            this.amount = amount;
            this.paymentSource = (int)paymentSource;
            this.resultState = (int)resultState;
            this.reason = reason ?? string.Empty;
            this.timestampUtc = timestampUtc ?? string.Empty;
            this.summaryPlaceholder = summaryPlaceholder ?? string.Empty;
        }
    }
}
