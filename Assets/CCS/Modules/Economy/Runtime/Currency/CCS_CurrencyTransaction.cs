using System;

// =============================================================================
// SCRIPT: CCS_CurrencyTransaction
// CATEGORY: Modules / Economy / Runtime / Currency
// PURPOSE: Placeholder transaction record for currency wallet history.
// PLACEMENT: Stored in CCS_CurrencyService transaction history ring buffer.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// =============================================================================

namespace CCS.Modules.Economy
{
    [Serializable]
    public sealed class CCS_CurrencyTransaction
    {
        public string currencyId = string.Empty;
        public int deltaAmount;
        public int balanceAfter;
        public string reason = string.Empty;
        public string timestampUtc = string.Empty;

        public CCS_CurrencyTransaction()
        {
        }

        public CCS_CurrencyTransaction(
            string currencyId,
            int deltaAmount,
            int balanceAfter,
            string reason,
            string timestampUtc)
        {
            this.currencyId = currencyId ?? string.Empty;
            this.deltaAmount = deltaAmount;
            this.balanceAfter = balanceAfter;
            this.reason = reason ?? string.Empty;
            this.timestampUtc = timestampUtc ?? string.Empty;
        }
    }
}
