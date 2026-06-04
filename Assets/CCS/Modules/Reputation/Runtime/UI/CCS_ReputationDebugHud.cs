using CCS.Modules.CharacterController;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_ReputationDebugHud
// CATEGORY: Modules / Reputation / Runtime / UI
// PURPOSE: Dev-only summary panel for settlement reputation standings and last event.
// PLACEMENT: Auto-created at runtime; not final UI.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 2.7.0 reputation and service trust foundation.
// =============================================================================

namespace CCS.Modules.Reputation
{
    public sealed class CCS_ReputationDebugHud : MonoBehaviour
    {
        private static bool s_showPanel = true;
        private static string s_settlementSummary = "Settlement trust: unavailable";
        private static string s_lastEventSummary = "Last reputation event: none";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureInstance()
        {
            if (FindAnyObjectByType<CCS_ReputationDebugHud>() != null)
            {
                return;
            }

            GameObject host = new GameObject("CCS_ReputationDebugHud");
            host.AddComponent<CCS_ReputationDebugHud>();
            DontDestroyOnLoad(host);
        }

        public static void RefreshSummary()
        {
            if (!CCS_ReputationRuntimeBridge.TryGetReputationService(out CCS_ReputationService reputationService)
                || reputationService.ActiveProfile == null)
            {
                s_settlementSummary = "Settlement trust: service unavailable";
                s_lastEventSummary = "Last reputation event: none";
                return;
            }

            string settlementId = reputationService.ActiveProfile.DefaultTradingPostSettlementId;
            if (reputationService.TryGetSettlementStanding(settlementId, out CCS_ReputationStanding standing)
                && standing != null)
            {
                s_settlementSummary =
                    $"Settlement: {settlementId}\n"
                    + $"Value: {standing.CurrentValue}\n"
                    + $"Tier: {standing.DisplayTier}";
            }
            else
            {
                s_settlementSummary = $"Settlement: {settlementId}\nValue: 0\nTier: Neutral";
            }

            CCS_ReputationEvent lastEvent = reputationService.LastEvent;
            s_lastEventSummary = lastEvent == null
                ? "Last reputation event: none"
                : $"Last: {lastEvent.EventType} | delta {lastEvent.DeltaApplied} | "
                  + $"value {lastEvent.ValueAfter} | tier {lastEvent.TierAfter} | {lastEvent.SummaryPlaceholder}";
        }

        private void Update()
        {
            RefreshSummary();

            if (CCS_DevHotkeyUtility.WasCloseVendorDebugPanelPressed())
            {
                s_showPanel = !s_showPanel;
            }
        }

        private void OnGUI()
        {
            if (!s_showPanel)
            {
                return;
            }

            const float width = 420f;
            const float height = 160f;
            Rect panel = new Rect(Screen.width - width - 20f, Screen.height - height - 20f, width, height);
            GUI.Box(panel, GUIContent.none);
            GUILayout.BeginArea(new Rect(panel.x + 10f, panel.y + 10f, panel.width - 20f, panel.height - 20f));
            GUILayout.Label("Reputation Debug");
            GUILayout.Label(s_settlementSummary, GUILayout.MaxHeight(72f));
            GUILayout.Label(s_lastEventSummary, GUILayout.MaxHeight(48f));
            GUILayout.Label("Esc toggle panel");
            GUILayout.EndArea();
        }
    }
}
