using CCS.Modules.CharacterController;
using CCS.Modules.Inventory;
using CCS.Modules.Reputation;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_VendorDebugHud
// CATEGORY: Modules / Economy / Runtime / UI
// PURPOSE: Temporary debug panel for vendor buy/sell until final UI exists.
// PLACEMENT: Bootstrap harness or vendor test object.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: Milestone 2.8.0 shows reputation price modifier and settlement standing.
// =============================================================================

namespace CCS.Modules.Economy
{
    public sealed class CCS_VendorDebugHud : MonoBehaviour
    {
        private const string RawFishItemId = "ccs.survival.item.resource.rawfish";
        private const string CordageItemId = "ccs.survival.item.frontier.cordage";
        private const string BoneHatchetItemId = "ccs.survival.item.tool.hatchet.bone";

        private static CCS_VendorDefinition s_activeVendorDefinition;
        private static string s_activeSettlementId = string.Empty;
        private static bool s_showPanel;
        private static string s_lastTransactionSummary = "Last transaction: none";
        private static string s_reputationSummary = "Reputation: unavailable";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureInstance()
        {
            if (FindAnyObjectByType<CCS_VendorDebugHud>() != null)
            {
                return;
            }

            GameObject host = new GameObject("CCS_VendorDebugHud");
            host.AddComponent<CCS_VendorDebugHud>();
            DontDestroyOnLoad(host);
        }

        public static void NotifyVendorActivated(CCS_VendorDefinition vendorDefinition, string settlementId = "")
        {
            s_activeVendorDefinition = vendorDefinition;
            s_activeSettlementId = settlementId ?? string.Empty;
            s_showPanel = vendorDefinition != null;
            RefreshReputationSummary();
            if (vendorDefinition != null
                && CCS_EconomyRuntimeBridge.TryGetVendorService(out CCS_VendorService vendorService))
            {
                vendorService.SetActiveVendor(vendorDefinition, s_activeSettlementId);
            }
        }

        public static void NotifyTransactionResult(CCS_VendorTransactionResult result)
        {
            if (result == null)
            {
                s_lastTransactionSummary = "Last transaction: null result";
                return;
            }

            string itemLabel = string.IsNullOrEmpty(result.ItemId) ? "n/a" : result.ItemId;
            string action = result.WasSell ? "Sell" : "Buy";
            string priceDetail = result.IsSuccess
                ? $" | base {result.BaseUnitPrice} | mod {result.ReputationPriceModifier:0.00} | final {result.FinalUnitPrice}"
                : string.Empty;
            s_lastTransactionSummary =
                $"Last: {result.ResultType} | {action} {result.Quantity}x {itemLabel}{priceDetail} | "
                + $"currency {result.CurrencyDelta:+#;-#;0} | balance {result.CurrencyBalanceAfter} | {result.Message}";
            RefreshReputationSummary();
        }

        public static void HidePanel()
        {
            s_showPanel = false;
            s_activeVendorDefinition = null;
            s_activeSettlementId = string.Empty;
            s_reputationSummary = "Reputation: unavailable";
            if (CCS_EconomyRuntimeBridge.TryGetVendorService(out CCS_VendorService vendorService))
            {
                vendorService.ClearActiveVendor();
            }
        }

        private void Update()
        {
            if (!s_showPanel || s_activeVendorDefinition == null)
            {
                return;
            }

            if (CCS_DevHotkeyUtility.WasCloseVendorDebugPanelPressed())
            {
                HidePanel();
                return;
            }

            if (CCS_DevHotkeyUtility.WasShiftPressed(KeyCode.V))
            {
                TryTransactionByItemId(RawFishItemId, isSell: true);
            }
            else if (CCS_DevHotkeyUtility.WasUnmodifiedPressed(KeyCode.V))
            {
                TryTransactionByItemId(CordageItemId, isSell: false);
            }
            else if (CCS_DevHotkeyUtility.WasUnmodifiedPressed(KeyCode.H))
            {
                TryTransactionByItemId(BoneHatchetItemId, isSell: false);
            }
        }

        private void OnGUI()
        {
            if (!s_showPanel || s_activeVendorDefinition == null)
            {
                return;
            }

            const float width = 420f;
            const float height = 360f;
            Rect panel = new Rect(20f, Screen.height - height - 20f, width, height);
            GUI.Box(panel, GUIContent.none);
            GUILayout.BeginArea(new Rect(panel.x + 10f, panel.y + 10f, panel.width - 20f, panel.height - 20f));

            GUILayout.Label($"Vendor: {s_activeVendorDefinition.DisplayName}");
            if (!string.IsNullOrWhiteSpace(s_activeSettlementId))
            {
                GUILayout.Label($"Settlement: {s_activeSettlementId}");
            }

            DrawCurrencyBalance();
            DrawPriceModifierPreview();
            GUILayout.Label(s_reputationSummary);
            GUILayout.Label(s_lastTransactionSummary, GUILayout.MaxHeight(56f));
            GUILayout.Space(4f);
            GUILayout.Label("Hotkeys: Shift+V sell fish | V buy cordage | H buy hatchet | Esc close");

            if (GUILayout.Button("Sell 1x Raw Fish"))
            {
                TryTransactionByItemId(RawFishItemId, isSell: true);
            }

            if (GUILayout.Button("Buy 1x Cordage"))
            {
                TryTransactionByItemId(CordageItemId, isSell: false);
            }

            if (GUILayout.Button("Buy 1x Bone Hatchet"))
            {
                TryTransactionByItemId(BoneHatchetItemId, isSell: false);
            }

            if (GUILayout.Button("Close vendor debug"))
            {
                HidePanel();
            }

            GUILayout.EndArea();
        }

        private static void DrawCurrencyBalance()
        {
            if (!CCS_EconomyRuntimeBridge.TryGetCurrencyService(out CCS_CurrencyService currencyService)
                || s_activeVendorDefinition.CurrencyDefinition == null)
            {
                GUILayout.Label("Trade Dollars: unavailable");
                return;
            }

            string currencyId = s_activeVendorDefinition.CurrencyDefinition.CurrencyId;
            int balance = currencyService.GetBalance(currencyId);
            GUILayout.Label(
                $"{s_activeVendorDefinition.CurrencyDefinition.DisplayName}: {balance}");
        }

        private static void DrawPriceModifierPreview()
        {
            if (!CCS_EconomyRuntimeBridge.TryGetVendorService(out CCS_VendorService vendorService))
            {
                return;
            }

            float buyModifier = vendorService.ResolveBuyPriceModifier();
            GUILayout.Label($"Buy price modifier: {buyModifier:0.00}");
        }

        private static void RefreshReputationSummary()
        {
            if (string.IsNullOrWhiteSpace(s_activeSettlementId)
                || !CCS_ReputationRuntimeBridge.TryGetReputationService(out CCS_ReputationService reputationService))
            {
                s_reputationSummary = "Reputation: unavailable (fallback modifier 1.00)";
                return;
            }

            if (reputationService.TryGetSettlementStanding(s_activeSettlementId, out CCS_ReputationStanding standing)
                && standing != null)
            {
                s_reputationSummary =
                    $"Reputation: {standing.DisplayTier} ({standing.CurrentValue})";
                return;
            }

            s_reputationSummary = "Reputation: neutral fallback (0)";
        }

        private static void TryTransactionByItemId(string itemId, bool isSell)
        {
            if (string.IsNullOrWhiteSpace(itemId)
                || !CCS_EconomyRuntimeBridge.TryGetVendorService(out CCS_VendorService vendorService)
                || !CCS_EconomyRuntimeBridge.TryGetInventoryService(out CCS_PlayerInventoryService inventoryService))
            {
                return;
            }

            vendorService.SetActiveVendor(s_activeVendorDefinition, s_activeSettlementId);
            CCS_ItemDefinition item = FindItemDefinition(inventoryService, itemId);
            if (item == null)
            {
                s_lastTransactionSummary = $"Last: item not found ({itemId})";
                return;
            }

            CCS_VendorTransactionResult result = isSell
                ? vendorService.TrySellActiveVendorItem(item, 1)
                : vendorService.TryBuyActiveVendorItem(item, 1);
            NotifyTransactionResult(result);
        }

        private static CCS_ItemDefinition FindItemDefinition(
            CCS_PlayerInventoryService inventoryService,
            string itemId)
        {
            if (inventoryService?.ActiveProfile?.SaveRestoreItemDefinitions == null)
            {
                return null;
            }

            CCS_ItemDefinition[] definitions = inventoryService.ActiveProfile.SaveRestoreItemDefinitions;
            for (int index = 0; index < definitions.Length; index++)
            {
                CCS_ItemDefinition definition = definitions[index];
                if (definition != null && definition.ItemId == itemId)
                {
                    return definition;
                }
            }

            return null;
        }
    }
}
