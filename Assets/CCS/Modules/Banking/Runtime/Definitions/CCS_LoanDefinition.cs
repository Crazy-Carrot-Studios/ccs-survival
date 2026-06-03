using UnityEngine;

// =============================================================================
// SCRIPT: CCS_LoanDefinition
// CATEGORY: Modules / Banking / Runtime / Definitions
// PURPOSE: ScriptableObject definition for a bank loan product.
// PLACEMENT: Assets/CCS/Survival/Content/Banking/Loans/
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Generic loan template; western naming lives in Survival content assets.
// =============================================================================

namespace CCS.Modules.Banking
{
    [CreateAssetMenu(
        fileName = "CCS_LoanDefinition",
        menuName = "CCS/Survival/Banking/Loan Definition")]
    public sealed class CCS_LoanDefinition : ScriptableObject
    {
        [Header("Identity")]
        [Tooltip("Stable reverse-DNS loan definition id.")]
        [SerializeField] private string loanDefinitionId = string.Empty;

        [Tooltip("Player-facing loan label.")]
        [SerializeField] private string displayName = string.Empty;

        [Tooltip("Currency id issued to the wallet on borrow.")]
        [SerializeField] private string currencyId = string.Empty;

        [Header("Amounts")]
        [Tooltip("Principal issued to the player wallet on borrow.")]
        [SerializeField] private int principalAmount;

        [Tooltip("Full repayment amount due (no compound interest in 2.6.0).")]
        [SerializeField] private int repaymentAmount;

        [Tooltip("Repayment interval placeholder for future due scheduling.")]
        [SerializeField] private int repaymentIntervalDaysPlaceholder;

        [Header("Rules")]
        [Tooltip("Maximum concurrent active loans of this definition per owner.")]
        [SerializeField] private int maxActiveLoans = 1;

        [Tooltip("When false, borrow attempts fail safely.")]
        [SerializeField] private bool enabled = true;

        [Tooltip("Repayment may debit the default bank savings account first.")]
        [SerializeField] private bool autoRepayFromBank = true;

        [Tooltip("Repayment may debit the player wallet when bank funds are insufficient.")]
        [SerializeField] private bool autoRepayFromWallet = true;

        [Tooltip("Future collateral type placeholder (not enforced in 2.6.0).")]
        [SerializeField] private string futureCollateralTypePlaceholder = string.Empty;

        public string LoanDefinitionId => loanDefinitionId ?? string.Empty;

        public string DisplayName => displayName ?? string.Empty;

        public string CurrencyId => currencyId ?? string.Empty;

        public int PrincipalAmount => principalAmount;

        public int RepaymentAmount => repaymentAmount;

        public int RepaymentIntervalDaysPlaceholder => repaymentIntervalDaysPlaceholder;

        public int MaxActiveLoans => maxActiveLoans;

        public bool Enabled => enabled;

        public bool AutoRepayFromBank => autoRepayFromBank;

        public bool AutoRepayFromWallet => autoRepayFromWallet;

        public string FutureCollateralTypePlaceholder => futureCollateralTypePlaceholder ?? string.Empty;
    }
}
