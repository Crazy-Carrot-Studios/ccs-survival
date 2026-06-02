using CCS.Modules.Inventory;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_VendorDebugHud
// CATEGORY: Modules / Economy / Runtime / UI
// PURPOSE: Temporary debug panel for vendor buy/sell until final UI exists.
// PLACEMENT: Bootstrap harness or vendor test object.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// =============================================================================

namespace CCS.Modules.Economy
{
    public sealed class CCS_VendorDebugHud : MonoBehaviour
    {
        private static CCS_VendorDefinition s_activeVendorDefinition;
        private static bool s_showPanel;

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

        public static void NotifyVendorActivated(CCS_VendorDefinition vendorDefinition)
        {
            s_activeVendorDefinition = vendorDefinition;
            s_showPanel = vendorDefinition != null;
        }

        public static void HidePanel()
        {
            s_showPanel = false;
            s_activeVendorDefinition = null;
            if (CCS_EconomyRuntimeBridge.TryGetVendorService(out CCS_VendorService vendorService))
            {
                vendorService.ClearActiveVendor();
            }
        }

        private void OnGUI()
        {
            if (!s_showPanel || s_activeVendorDefinition == null)
            {
                return;
            }

            const float width = 360f;
            const float height = 280f;
            Rect panel = new Rect(20f, Screen.height - height - 20f, width, height);
            GUI.Box(panel, GUIContent.none);
            GUILayout.BeginArea(new Rect(panel.x + 10f, panel.y + 10f, panel.width - 20f, panel.height - 20f));

            GUILayout.Label($"Vendor: {s_activeVendorDefinition.DisplayName}");
            DrawCurrencyBalance();

            if (GUILayout.Button("Sell 1x first sellable item"))
            {
                TrySellFirstSellable();
            }

            if (GUILayout.Button("Buy 1x first buyable item"))
            {
                TryBuyFirstBuyable();
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
                GUILayout.Label("Currency: unavailable");
                return;
            }

            string currencyId = s_activeVendorDefinition.CurrencyDefinition.CurrencyId;
            int balance = currencyService.GetBalance(currencyId);
            GUILayout.Label(
                $"{s_activeVendorDefinition.CurrencyDefinition.DisplayName}: {balance}");
        }

        private static void TrySellFirstSellable()
        {
            if (!CCS_EconomyRuntimeBridge.TryGetVendorService(out CCS_VendorService vendorService)
                || !CCS_EconomyRuntimeBridge.TryGetInventoryService(out CCS_PlayerInventoryService inventoryService))
            {
                return;
            }

            vendorService.SetActiveVendor(s_activeVendorDefinition);
            CCS_VendorItemEntry[] entries = s_activeVendorDefinition.VendorInventory.Items;
            for (int index = 0; index < entries.Length; index++)
            {
                CCS_VendorItemEntry entry = entries[index];
                if (entry == null || !entry.AllowSell || entry.ItemDefinition == null)
                {
                    continue;
                }

                if (inventoryService.GetQuantity(entry.ItemDefinition) > 0)
                {
                    vendorService.TrySellActiveVendorItem(entry.ItemDefinition, 1);
                    return;
                }
            }
        }

        private static void TryBuyFirstBuyable()
        {
            if (!CCS_EconomyRuntimeBridge.TryGetVendorService(out CCS_VendorService vendorService))
            {
                return;
            }

            vendorService.SetActiveVendor(s_activeVendorDefinition);
            CCS_VendorItemEntry[] entries = s_activeVendorDefinition.VendorInventory.Items;
            for (int index = 0; index < entries.Length; index++)
            {
                CCS_VendorItemEntry entry = entries[index];
                if (entry != null && entry.AllowBuy && entry.ItemDefinition != null)
                {
                    vendorService.TryBuyActiveVendorItem(entry.ItemDefinition, 1);
                    return;
                }
            }
        }
    }
}
