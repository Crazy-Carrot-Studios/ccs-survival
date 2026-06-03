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
        private static string s_landOfficeSummary = "Land office: inactive";

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

            if (s_panelMode == PanelMode.LandOffice)
            {
                RefreshLandOfficeSummary(ResolvePlayerPosition(), includeNearbyClaim: true);
            }
        }

        private void OnGUI()
        {
            if (s_panelMode == PanelMode.Hidden)
            {
                return;
            }

            const float width = 440f;
            float height = s_panelMode == PanelMode.LandOffice ? 360f : 300f;
            Rect panel = new Rect(20f, Screen.height - height - 20f, width, height);
            GUI.Box(panel, GUIContent.none);
            GUILayout.BeginArea(new Rect(panel.x + 10f, panel.y + 10f, panel.width - 20f, panel.height - 20f));

            string modeLabel = s_panelMode == PanelMode.LandOffice ? "Land Office" : "Bank";
            GUILayout.Label($"{modeLabel}: {s_servicePointLabel}");
            DrawBalances();
            GUILayout.Label(s_lastTransactionSummary, GUILayout.MaxHeight(48f));
            GUILayout.Space(4f);
            GUILayout.Label("Hotkeys: Shift+D deposit | Shift+W withdraw | Esc close");

            if (s_panelMode == PanelMode.LandOffice)
            {
                GUILayout.Space(6f);
                GUILayout.Label("Land Office Summary");
                GUILayout.Label(s_landOfficeSummary, GUILayout.MaxHeight(72f));
                GUILayout.Label("Future: deed registry, tax ledger, mortgage filings (placeholders).");
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

        private static void RefreshLandOfficeSummary(Vector3 worldPosition, bool includeNearbyClaim)
        {
            if (!CCS_BankingRuntimeBridge.TryGetBankingService(out CCS_BankingService bankingService)
                || !bankingService.IsInitialized)
            {
                s_landOfficeSummary = "Land office unavailable: banking service missing.";
                return;
            }

            int claimCount = bankingService.GetOwnedLandClaimCount();
            string nearbyClaimId = includeNearbyClaim
                ? bankingService.TryResolveNearbyLandClaimId(worldPosition)
                : string.Empty;
            string nearbyLabel = string.IsNullOrWhiteSpace(nearbyClaimId)
                ? "none nearby"
                : nearbyClaimId;
            s_landOfficeSummary =
                $"Owned claims: {claimCount}\nNearby claim id: {nearbyLabel}\nDeed registry: placeholder\nTax ledger: placeholder";
        }

        private static Vector3 ResolvePlayerPosition()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            return player != null ? player.transform.position : Vector3.zero;
        }
    }
}
