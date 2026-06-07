using UnityEngine;

// =============================================================================
// SCRIPT: CCS_NpcDialogueStubDebugHud
// CATEGORY: Modules / NPCs / Runtime / UI
// PURPOSE: Dev-only dialogue stub panel for NPC interaction placeholder copy.
// PLACEMENT: Updated by CCS_NpcDialogueStubService and interactables.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 4.9.0 — no final dialogue UI.
// =============================================================================

namespace CCS.Modules.NPCs
{
    public static class CCS_NpcDialogueStubDebugHud
    {
        private static string lastDisplayName = string.Empty;
        private static string lastRoleDisplayName = string.Empty;
        private static string lastSettlementDisplayName = string.Empty;
        private static string lastBusinessDisplayName = string.Empty;
        private static string lastResultType = string.Empty;
        private static string lastMessage = string.Empty;
        private static string[] lastDisplayLines = System.Array.Empty<string>();
        private static bool hasDisplay;

        public static void NotifyDialogueResult(CCS_NpcDialogueStubResult result)
        {
            if (result == null)
            {
                return;
            }

            lastDisplayName = result.DisplayName ?? string.Empty;
            lastRoleDisplayName = result.RoleDisplayName ?? string.Empty;
            lastSettlementDisplayName = result.SettlementDisplayName ?? string.Empty;
            lastBusinessDisplayName = result.BusinessDisplayName ?? string.Empty;
            lastResultType = result.ResultType.ToString();
            lastMessage = result.Message ?? string.Empty;
            lastDisplayLines = result.DisplayLines ?? System.Array.Empty<string>();
            hasDisplay = true;
        }

        public static void DrawIfVisible()
        {
            if (!hasDisplay)
            {
                return;
            }

            const float width = 460f;
            float height = 240f + (lastDisplayLines.Length * 18f);
            Rect area = new Rect(12f, Screen.height - height - 12f, width, height);
            GUILayout.BeginArea(area, GUI.skin.box);
            GUILayout.Label("NPC Dialogue Stub (Dev)");
            GUILayout.Label($"Name: {lastDisplayName}");
            GUILayout.Label($"Role: {lastRoleDisplayName}");
            GUILayout.Label($"Settlement: {lastSettlementDisplayName}");
            GUILayout.Label($"Business: {lastBusinessDisplayName}");
            GUILayout.Label($"Result: {lastResultType}");
            if (!string.IsNullOrWhiteSpace(lastMessage))
            {
                GUILayout.Label($"Message: {lastMessage}");
            }

            GUILayout.Label("Dialogue:");
            for (int index = 0; index < lastDisplayLines.Length; index++)
            {
                GUILayout.Label($"- {lastDisplayLines[index]}");
            }

            GUILayout.EndArea();
        }
    }
}
