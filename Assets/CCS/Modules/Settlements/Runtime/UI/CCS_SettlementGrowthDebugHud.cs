using CCS.Modules.CharacterController;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_SettlementGrowthDebugHud
// CATEGORY: Modules / Settlements / Runtime / UI
// PURPOSE: Debug panel showing settlement prosperity, supply health, and growth stage.
// PLACEMENT: Auto-created at runtime; toggled with settlement growth playtest shortcut.
// AUTHOR: James Schilz
// CREATED: 2026-06-04
// NOTES: Milestone 3.2.0 settlement growth foundation.
// =============================================================================

namespace CCS.Modules.Settlements
{
    public sealed class CCS_SettlementGrowthDebugHud : MonoBehaviour
    {
        private const string DefaultSettlementId = CCS_SettlementGrowthContentIds.TradingPostSettlementId;

        private static bool s_showPanel = true;
        private static string s_settlementId = DefaultSettlementId;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureInstance()
        {
            if (Object.FindAnyObjectByType<CCS_SettlementGrowthDebugHud>() != null)
            {
                return;
            }

            GameObject host = new GameObject("CCS_SettlementGrowthDebugHud");
            host.AddComponent<CCS_SettlementGrowthDebugHud>();
            DontDestroyOnLoad(host);
        }

        public static void ShowSettlement(string settlementId)
        {
            s_settlementId = string.IsNullOrWhiteSpace(settlementId) ? DefaultSettlementId : settlementId;
            s_showPanel = true;
        }

        private void Update()
        {
            if (!s_showPanel)
            {
                return;
            }

            if (CCS_KeyboardInputUtility.WasKeyPressedThisFrame(KeyCode.Escape))
            {
                s_showPanel = false;
            }
        }

        private void OnGUI()
        {
            if (!s_showPanel)
            {
                return;
            }

            const float width = 420f;
            Rect panelRect = new Rect(16f, Screen.height - 220f, width, 200f);
            GUI.Box(panelRect, "Settlement Growth");

            GUILayout.BeginArea(new Rect(panelRect.x + 10f, panelRect.y + 24f, panelRect.width - 20f, panelRect.height - 34f));
            DrawSettlementGrowthPanel();
            GUILayout.EndArea();
        }

        private static void DrawSettlementGrowthPanel()
        {
            if (!CCS_SettlementRuntimeBridge.TryGetSettlementService(out CCS_SettlementService settlementService)
                || settlementService == null
                || !settlementService.IsInitialized)
            {
                GUILayout.Label("Settlement service unavailable.");
                return;
            }

            if (!settlementService.TryGetGrowthSnapshot(s_settlementId, out CCS_SettlementGrowthSnapshot snapshot)
                || !snapshot.IsValid)
            {
                GUILayout.Label($"Settlement: {s_settlementId}");
                GUILayout.Label("Growth snapshot unavailable.");
                return;
            }

            GUILayout.Label($"Settlement: {snapshot.SettlementId}");
            GUILayout.Label($"Prosperity: {snapshot.Prosperity:0.##}");
            GUILayout.Label($"Food supply health: {snapshot.FoodSupplyHealthPercent:0.##}%");
            GUILayout.Label($"Industrial supply health: {snapshot.IndustrialSupplyHealthPercent:0.##}%");
            GUILayout.Label($"Growth stage: {CCS_SettlementGrowthUtility.GetDisplayName(snapshot.CurrentGrowthStage)}");
            GUILayout.Label($"Previous stage: {CCS_SettlementGrowthUtility.GetDisplayName(snapshot.PreviousGrowthStage)}");
            GUILayout.Label($"Next stage: {CCS_SettlementGrowthUtility.GetDisplayName(snapshot.NextGrowthStage)}");
            GUILayout.Label($"Next stage progress: {snapshot.GrowthProgressPercent:0.##}%");
            GUILayout.Label($"Completed contracts: {snapshot.CompletedContractsCount}");
        }
    }
}
