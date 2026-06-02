using System;

// =============================================================================
// SCRIPT: CCS_CurrencyBalance
// CATEGORY: Modules / Economy / Runtime / Currency
// PURPOSE: Serializable wallet balance entry for a single currency.
// PLACEMENT: Used by save data and currency service snapshots.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// =============================================================================

namespace CCS.Modules.Economy
{
    [Serializable]
    public sealed class CCS_CurrencyBalance
    {
        public string currencyId = string.Empty;
        public int amount;

        public CCS_CurrencyBalance()
        {
        }

        public CCS_CurrencyBalance(string currencyId, int amount)
        {
            this.currencyId = currencyId ?? string.Empty;
            this.amount = amount < 0 ? 0 : amount;
        }
    }
}
