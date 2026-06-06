using CCS.Modules.Settlements;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_NpcServiceRepresentativeDebugHud
// CATEGORY: Modules / NPCs / Runtime / UI
// PURPOSE: Dev-only HUD for representative interaction routing and fallback status.
// PLACEMENT: Updated by CCS_NpcServiceRepresentativeInteractable and service sync.
// AUTHOR: James Schilz
// CREATED: 2026-06-05
// NOTES: Milestone 4.3.0 — no final NPC UI.
// =============================================================================

namespace CCS.Modules.NPCs
{
    public static class CCS_NpcServiceRepresentativeDebugHud
    {
        private static string lastRepresentativeId = string.Empty;
        private static string lastDisplayName = string.Empty;
        private static string lastTitle = string.Empty;
        private static string lastBusinessId = string.Empty;
        private static string lastServicePointId = string.Empty;
        private static string lastSettlementId = string.Empty;
        private static string lastRouteType = string.Empty;
        private static string lastRouteResult = string.Empty;
        private static bool lastUsedFallback;
        private static bool hasDisplay;

        public static void NotifyRepresentativeSnapshot(CCS_NpcServiceRepresentativeSnapshot snapshot)
        {
            if (snapshot == null || !snapshot.IsValid)
            {
                return;
            }

            lastRepresentativeId = snapshot.RepresentativeId;
            lastDisplayName = snapshot.DisplayName;
            lastTitle = snapshot.DisplayTitle;
            lastBusinessId = snapshot.BusinessId;
            lastServicePointId = snapshot.ServicePointId;
            lastSettlementId = snapshot.SettlementId;
            lastRouteType = snapshot.RouteType.ToString();
            lastUsedFallback = false;
            lastRouteResult = snapshot.IsActive ? "Representative active." : "Representative inactive.";
            hasDisplay = true;
        }

        public static void NotifyRouteResult(
            string representativeId,
            string servicePointId,
            string businessId,
            string settlementId,
            CCS_SettlementServiceRouteType routeType,
            bool success,
            bool usedFallback,
            string message)
        {
            lastRepresentativeId = representativeId ?? string.Empty;
            lastServicePointId = servicePointId ?? string.Empty;
            lastBusinessId = businessId ?? string.Empty;
            lastSettlementId = settlementId ?? string.Empty;
            lastRouteType = routeType.ToString();
            lastRouteResult = message ?? string.Empty;
            lastUsedFallback = usedFallback;
            hasDisplay = true;
        }

        public static void NotifyFallback(string representativeId, string servicePointId, string message)
        {
            lastRepresentativeId = representativeId ?? string.Empty;
            lastServicePointId = servicePointId ?? string.Empty;
            lastRouteResult = message ?? string.Empty;
            lastUsedFallback = true;
            hasDisplay = true;
        }

        public static void DrawIfVisible()
        {
            if (!hasDisplay)
            {
                return;
            }

            const float width = 420f;
            Rect area = new Rect(12f, Screen.height - 220f, width, 200f);
            GUILayout.BeginArea(area, GUI.skin.box);
            GUILayout.Label("NPC Service Representative (Dev)");
            GUILayout.Label($"Name: {lastDisplayName}");
            GUILayout.Label($"Title: {lastTitle}");
            GUILayout.Label($"Business: {lastBusinessId}");
            GUILayout.Label($"Service Point: {lastServicePointId}");
            GUILayout.Label($"Settlement: {lastSettlementId}");
            GUILayout.Label($"Route: {lastRouteType}");
            GUILayout.Label($"Fallback: {(lastUsedFallback ? "Yes" : "No")}");
            GUILayout.Label($"Result: {lastRouteResult}");
            GUILayout.EndArea();
        }
    }
}
