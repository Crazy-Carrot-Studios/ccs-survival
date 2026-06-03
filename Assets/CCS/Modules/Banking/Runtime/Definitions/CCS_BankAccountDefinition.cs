using UnityEngine;

// =============================================================================
// SCRIPT: CCS_BankAccountDefinition
// CATEGORY: Modules / Banking / Runtime / Definitions
// PURPOSE: ScriptableObject definition for a stored-currency bank account type.
// PLACEMENT: Assets/CCS/Survival/Content/Banking/Accounts/
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Generic account template; western naming lives in Survival content assets.
// =============================================================================

namespace CCS.Modules.Banking
{
    [CreateAssetMenu(
        fileName = "CCS_BankAccountDefinition",
        menuName = "CCS/Survival/Banking/Bank Account Definition")]
    public sealed class CCS_BankAccountDefinition : ScriptableObject
    {
        [Header("Identity")]
        [Tooltip("Stable reverse-DNS bank account definition id.")]
        [SerializeField] private string accountDefinitionId = string.Empty;

        [Tooltip("Player-facing account label.")]
        [SerializeField] private string displayName = string.Empty;

        [Tooltip("Currency id stored by this account.")]
        [SerializeField] private string currencyId = string.Empty;

        [Header("Rules")]
        [Tooltip("Minimum deposit amount allowed (future enforcement placeholder).")]
        [SerializeField] private int minimumDepositAmount = 1;

        [Tooltip("Minimum withdraw amount allowed (future enforcement placeholder).")]
        [SerializeField] private int minimumWithdrawAmount = 1;

        public string AccountDefinitionId => accountDefinitionId ?? string.Empty;

        public string DisplayName => displayName ?? string.Empty;

        public string CurrencyId => currencyId ?? string.Empty;

        public int MinimumDepositAmount => minimumDepositAmount;

        public int MinimumWithdrawAmount => minimumWithdrawAmount;
    }
}
