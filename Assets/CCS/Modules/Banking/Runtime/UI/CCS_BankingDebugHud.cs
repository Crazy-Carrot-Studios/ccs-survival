using CCS.Modules.CharacterController;
using CCS.Modules.Economy;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_BankingDebugHud
// CATEGORY: Modules / Banking / Runtime / UI
// PURPOSE: Temporary debug panel for bank deposit/withdraw and land office summary.
// PLACEMENT: Auto-created at runtime; activated by settlement bank/land office routes.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Dev hotkeys route through CCS_DevHotkeyUtility (Input System keyboard reads).
// =============================================================================

namespace CCS.Modules.Banking
{
    public sealed class CCS_BankingDebugHud : MonoBehaviour
    {
        public enum PanelMode
        {
            Hidden = 0,
            Bank = 1,
            LandOffice = 2
        }

        private const int DefaultTransactionAmount = 50;
        private const string LogPrefix = "[CCS_BankingDebugHud]";

        private static PanelMode s_panelMode = PanelMode.Hidden;
        private static string s_servicePointLabel = string.Empty;
        private static string s_lastTransactionSummary = "Last transaction: none";
        private static string s_lastLoanTransactionSummary = "Last loan: none";
        private static string s_loanSummary = "Loan: none";
        private static string s_landOfficeSummary = "Land office: inactive";
        private static string s_lastUpkeepSummary = "Last upkeep: none";
        private static string s_nearbyClaimId = string.Empty;

        public delegate bool UpkeepPayHandler(string targetId, out string summary);

        public delegate bool UpkeepSummaryHandler(
            string targetId,
            out string statusLabel,
            out int amountDue);

        private static UpkeepPayHandler s_upkeepPayHandler;
        private static UpkeepSummaryHandler s_upkeepSummaryHandler;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureInstance()
        {
            if (FindAnyObjectByType<CCS_BankingDebugHud>() != null)
            {
                return;
            }

            GameObject host = new GameObject("CCS_BankingDebugHud");
            host.AddComponent<CCS_BankingDebugHud>();
            DontDestroyOnLoad(host);
        }

        public static void NotifyBankActivated(string servicePointLabel)
        {
            s_panelMode = PanelMode.Bank;
            s_servicePointLabel = servicePointLabel ?? "Bank";
            EnsureOpenDefaultAccount();
            RefreshLoanSummary();
            RefreshLandOfficeSummary(Vector3.zero, includeNearbyClaim: false);
        }

        public static void NotifyLandOfficeActivated(string servicePointLabel)
        {
            s_panelMode = PanelMode.LandOffice;
            s_servicePointLabel = servicePointLabel ?? "Land Office";
            EnsureOpenDefaultAccount();
            RefreshLandOfficeSummary(ResolvePlayerPosition(), includeNearbyClaim: true);
        }

        public static void HidePanel()
        {
            s_panelMode = PanelMode.Hidden;
            s_servicePointLabel = string.Empty;
        }

        public static void NotifyTransactionResult(CCS_BankTransactionResult result)
        {
            if (result == null)
            {
                s_lastTransactionSummary = "Last transaction: null result";
                return;
            }

            s_lastTransactionSummary =
                $"Last: {result.ResultType} | amount {result.Amount} | wallet {result.WalletBalanceAfter} | "
                + $"bank {result.BankBalanceAfter} | {result.Message}";
            RefreshLoanSummary();
        }

        public static void NotifyLoanTransactionResult(CCS_LoanTransactionResult result)
        {
            if (result == null)
            {
                s_lastLoanTransactionSummary = "Last loan: null result";
                return;
            }

            s_lastLoanTransactionSummary =
                $"Last loan: {result.ResultType} | amount {result.Amount} | wallet {result.WalletBalanceAfter} | "
                + $"bank {result.BankBalanceAfter} | balance {result.LoanBalanceAfter} | "
                + $"state {result.LoanStateAfter} | {result.Message}";
            RefreshLoanSummary();
        }

        public static void NotifyUpkeepSummary(string summary)
        {
            s_lastUpkeepSummary = string.IsNullOrWhiteSpace(summary)
                ? "Last upkeep: none"
                : summary;
            RefreshLandOfficeSummary(ResolvePlayerPosition(), includeNearbyClaim: true);
        }

        public static void BindUpkeepHandlers(
            UpkeepSummaryHandler summaryHandler,
            UpkeepPayHandler payHandler)
        {
            s_upkeepSummaryHandler = summaryHandler;
            s_upkeepPayHandler = payHandler;
        }

        private void Update()
        {
            if (s_panelMode == PanelMode.Hidden)
            {
                return;
            }

            if (CCS_DevHotkeyUtility.WasCloseBankingDebugPanelPressed())
            {
                HidePanel();
                return;
            }

            if (CCS_DevHotkeyUtility.WasBankDepositPressed())
            {
                TryDeposit(DefaultTransactionAmount);
            }
            else if (CCS_DevHotkeyUtility.WasBankWithdrawPressed())
            {
                TryWithdraw(DefaultTransactionAmount);
            }

            if (s_panelMode == PanelMode.Bank)
            {
                RefreshLoanSummary();

                if (CCS_DevHotkeyUtility.WasBankBorrowLoanPressed())
                {
                    TryBorrowDefaultLoan();
                }
                else if (CCS_DevHotkeyUtility.WasBankRepayLoanPressed())
                {
                    TryRepayDefaultLoan();
                }
            }

            if (s_panelMode == PanelMode.LandOffice)
            {
                RefreshLandOfficeSummary(ResolvePlayerPosition(), includeNearbyClaim: true);

                if (CCS_DevHotkeyUtility.WasUpkeepPayPressed())
                {
                    TryPayNearbyUpkeep();
                }
            }
        }

        private void OnGUI()
        {
            if (s_panelMode == PanelMode.Hidden)
            {
                return;
            }

            const float width = 440f;
            float height = s_panelMode == PanelMode.LandOffice ? 420f : 380f;
            Rect panel = new Rect(20f, Screen.height - height - 20f, width, height);
            GUI.Box(panel, GUIContent.none);
            GUILayout.BeginArea(new Rect(panel.x + 10f, panel.y + 10f, panel.width - 20f, panel.height - 20f));

            string modeLabel = s_panelMode == PanelMode.LandOffice ? "Land Office" : "Bank";
            GUILayout.Label($"{modeLabel}: {s_servicePointLabel}");
            DrawBalances();
            GUILayout.Label(s_lastTransactionSummary, GUILayout.MaxHeight(48f));
            if (s_panelMode == PanelMode.Bank)
            {
                GUILayout.Label(s_loanSummary, GUILayout.MaxHeight(72f));
                GUILayout.Label(s_lastLoanTransactionSummary, GUILayout.MaxHeight(48f));
            }
            GUILayout.Space(4f);
            string hotkeys = s_panelMode == PanelMode.LandOffice
                ? "Hotkeys: Shift+D deposit | Shift+W withdraw | Shift+T pay upkeep | Esc close"
                : "Hotkeys: Shift+D deposit | Shift+W withdraw | Shift+L borrow | Shift+P repay | Esc close";
            GUILayout.Label(hotkeys);

            if (s_panelMode == PanelMode.LandOffice)
            {
                GUILayout.Space(6f);
                GUILayout.Label("Land Office Summary");
                GUILayout.Label(s_landOfficeSummary, GUILayout.MaxHeight(96f));
                GUILayout.Label(s_lastUpkeepSummary, GUILayout.MaxHeight(48f));

                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Pay Upkeep (nearby claim)"))
                {
                    TryPayNearbyUpkeep();
                }
                GUILayout.EndHorizontal();
            }

            GUILayout.BeginHorizontal();
            if (GUILayout.Button($"Deposit {DefaultTransactionAmount}"))
            {
                TryDeposit(DefaultTransactionAmount);
            }

            if (GUILayout.Button($"Withdraw {DefaultTransactionAmount}"))
            {
                TryWithdraw(DefaultTransactionAmount);
            }
            GUILayout.EndHorizontal();

            if (s_panelMode == PanelMode.Bank)
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Borrow Small Loan"))
                {
                    TryBorrowDefaultLoan();
                }

                if (GUILayout.Button("Repay Loan"))
                {
                    TryRepayDefaultLoan();
                }
                GUILayout.EndHorizontal();
            }

            GUILayout.EndArea();
        }

        private static void DrawBalances()
        {
            int walletBalance = 0;
            int bankBalance = 0;
            if (CCS_EconomyRuntimeBridge.TryGetCurrencyService(out CCS_CurrencyService currencyService)
                && currencyService.IsInitialized)
            {
                walletBalance = currencyService.GetBalance(CCS_BankingContentIds.TradeDollarsCurrencyId);
            }

            if (CCS_BankingRuntimeBridge.TryGetBankingService(out CCS_BankingService bankingService)
                && bankingService.IsInitialized)
            {
                bankBalance = bankingService.GetDefaultAccountBalance(CCS_BankingContentIds.DefaultPlayerOwnerId);
            }

            GUILayout.Label($"Wallet Trade Dollars: {walletBalance}");
            GUILayout.Label($"Bank Balance: {bankBalance}");
        }

        private static void EnsureOpenDefaultAccount()
        {
            if (!CCS_BankingRuntimeBridge.TryGetBankingService(out CCS_BankingService bankingService)
                || !bankingService.IsInitialized
                || bankingService.ActiveProfile == null)
            {
                return;
            }

            CCS_BankTransactionResult result = bankingService.TryOpenAccount(
                CCS_BankingContentIds.DefaultPlayerOwnerId,
                bankingService.ActiveProfile.DefaultAccountDefinitionId);
            NotifyTransactionResult(result);
        }

        private static void TryDeposit(int amount)
        {
            if (!CCS_BankingRuntimeBridge.TryGetBankingService(out CCS_BankingService bankingService)
                || !bankingService.IsInitialized
                || bankingService.ActiveProfile == null)
            {
                s_lastTransactionSummary = "Deposit failed: banking service unavailable.";
                return;
            }

            CCS_BankTransactionResult result = bankingService.TryDeposit(
                CCS_BankingContentIds.DefaultPlayerOwnerId,
                bankingService.ActiveProfile.DefaultAccountDefinitionId,
                amount);
            NotifyTransactionResult(result);
        }

        private static void TryWithdraw(int amount)
        {
            if (!CCS_BankingRuntimeBridge.TryGetBankingService(out CCS_BankingService bankingService)
                || !bankingService.IsInitialized
                || bankingService.ActiveProfile == null)
            {
                s_lastTransactionSummary = "Withdraw failed: banking service unavailable.";
                return;
            }

            CCS_BankTransactionResult result = bankingService.TryWithdraw(
                CCS_BankingContentIds.DefaultPlayerOwnerId,
                bankingService.ActiveProfile.DefaultAccountDefinitionId,
                amount);
            NotifyTransactionResult(result);
        }

        private static void TryBorrowDefaultLoan()
        {
            if (!CCS_BankingRuntimeBridge.TryGetBankingService(out CCS_BankingService bankingService)
                || !bankingService.IsInitialized
                || bankingService.ActiveLoanProfile == null)
            {
                s_lastLoanTransactionSummary = "Borrow failed: loan profile unavailable.";
                return;
            }

            CCS_LoanTransactionResult result = bankingService.TryOpenLoan(
                CCS_BankingContentIds.DefaultPlayerOwnerId,
                bankingService.ActiveLoanProfile.DefaultLoanDefinitionId);
            NotifyLoanTransactionResult(result);
        }

        private static void TryRepayDefaultLoan()
        {
            if (!CCS_BankingRuntimeBridge.TryGetBankingService(out CCS_BankingService bankingService)
                || !bankingService.IsInitialized
                || bankingService.ActiveLoanProfile == null)
            {
                s_lastLoanTransactionSummary = "Repay failed: loan profile unavailable.";
                return;
            }

            CCS_LoanTransactionResult result = bankingService.TryRepayLoan(
                CCS_BankingContentIds.DefaultPlayerOwnerId,
                bankingService.ActiveLoanProfile.DefaultLoanDefinitionId);
            NotifyLoanTransactionResult(result);
        }

        private static void RefreshLoanSummary()
        {
            if (!CCS_BankingRuntimeBridge.TryGetBankingService(out CCS_BankingService bankingService)
                || !bankingService.IsInitialized
                || bankingService.ActiveLoanProfile == null)
            {
                s_loanSummary = "Loan: profile unavailable";
                return;
            }

            string loanDefinitionId = bankingService.ActiveLoanProfile.DefaultLoanDefinitionId;
            CCS_LoanSnapshot activeLoan = bankingService.GetActiveLoan(
                CCS_BankingContentIds.DefaultPlayerOwnerId,
                loanDefinitionId);
            if (activeLoan == null)
            {
                s_loanSummary = $"Loan id: {loanDefinitionId}\nState: none";
                return;
            }

            s_loanSummary =
                $"Loan id: {activeLoan.loanDefinitionId}\n"
                + $"Principal: {activeLoan.principalAmount}\n"
                + $"Repayment: {activeLoan.repaymentAmount}\n"
                + $"Balance: {activeLoan.balance}\n"
                + $"State: {(CCS_LoanState)activeLoan.loanState}";
        }

        private static void TryPayNearbyUpkeep()
        {
            if (string.IsNullOrWhiteSpace(s_nearbyClaimId))
            {
                s_lastUpkeepSummary = "Pay upkeep failed: no nearby land claim selected.";
                return;
            }

            if (!s_upkeepPayHandler(
                    s_nearbyClaimId,
                    out string summary))
            {
                s_lastUpkeepSummary = string.IsNullOrWhiteSpace(summary)
                    ? "Pay upkeep failed: no nearby land claim selected."
                    : summary;
                return;
            }

            s_lastUpkeepSummary = summary;
        }

        private static void RefreshLandOfficeSummary(Vector3 worldPosition, bool includeNearbyClaim)
        {
            if (!CCS_BankingRuntimeBridge.TryGetBankingService(out CCS_BankingService bankingService)
                || !bankingService.IsInitialized)
            {
                s_landOfficeSummary = "Land office unavailable: banking service missing.";
                s_nearbyClaimId = string.Empty;
                return;
            }

            int claimCount = bankingService.GetOwnedLandClaimCount();
            s_nearbyClaimId = includeNearbyClaim
                ? bankingService.TryResolveNearbyLandClaimId(worldPosition)
                : string.Empty;
            string nearbyLabel = string.IsNullOrWhiteSpace(s_nearbyClaimId)
                ? "none nearby"
                : s_nearbyClaimId;

            string upkeepStatus = "no entry";
            int amountDue = 0;
            if (!string.IsNullOrWhiteSpace(s_nearbyClaimId)
                && s_upkeepSummaryHandler != null
                && s_upkeepSummaryHandler(s_nearbyClaimId, out string statusLabel, out amountDue))
            {
                upkeepStatus = statusLabel;
            }

            s_landOfficeSummary =
                $"Owned claims: {claimCount}\nNearby claim id: {nearbyLabel}\n"
                + $"Upkeep status: {upkeepStatus}\nAmount due: {amountDue}";
        }

        private static Vector3 ResolvePlayerPosition()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            return player != null ? player.transform.position : Vector3.zero;
        }
    }
}
