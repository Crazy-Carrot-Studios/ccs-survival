using UnityEngine;

// =============================================================================
// SCRIPT: CCS_SettlementDebugMessageHud
// CATEGORY: Modules / Settlements / Runtime / UI
// PURPOSE: Temporary placeholder panel for non-vendor settlement services.
// PLACEMENT: Auto-created at runtime; not final UI.
// AUTHOR: James Schilz
// CREATED: 2026-06-02
// NOTES: Milestone 1.8.0 blacksmith and future town service placeholders.
// =============================================================================

namespace CCS.Modules.Settlements
{
    public sealed class CCS_SettlementDebugMessageHud : MonoBehaviour
    {
        private static bool s_showPanel;
        private static string s_serviceTitle = "Settlement Service";
        private static string s_message = string.Empty;
        private static string s_accessResultType = string.Empty;
        private static string s_missingRequirementMessage = string.Empty;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureInstance()
        {
            if (FindAnyObjectByType<CCS_SettlementDebugMessageHud>() != null)
            {
                return;
            }

            GameObject host = new GameObject("CCS_SettlementDebugMessageHud");
            host.AddComponent<CCS_SettlementDebugMessageHud>();
            DontDestroyOnLoad(host);
        }

        public static void ShowMessage(string serviceTitle, string message)
        {
            s_serviceTitle = string.IsNullOrWhiteSpace(serviceTitle) ? "Settlement Service" : serviceTitle;
            s_message = message ?? string.Empty;
            s_accessResultType = string.Empty;
            s_missingRequirementMessage = string.Empty;
            s_showPanel = true;
        }

        public static void ShowServiceAccessResult(
            string serviceTitle,
            string accessResultType,
            string message,
            string missingRequirementMessage)
        {
            s_serviceTitle = string.IsNullOrWhiteSpace(serviceTitle) ? "Settlement Service" : serviceTitle;
            s_accessResultType = accessResultType ?? string.Empty;
            s_message = message ?? string.Empty;
            s_missingRequirementMessage = missingRequirementMessage ?? string.Empty;
            s_showPanel = true;
        }

        public static void HidePanel()
        {
            s_showPanel = false;
        }

        private void Update()
        {
            if (!s_showPanel)
            {
                return;
            }

            if (CCS.Modules.CharacterController.CCS_DevHotkeyUtility.WasCloseVendorDebugPanelPressed())
            {
                HidePanel();
            }
        }

        private void OnGUI()
        {
            if (!s_showPanel)
            {
                return;
            }

            const float width = 360f;
            const float height = 180f;
            Rect panel = new Rect(20f, 20f, width, height);
            GUI.Box(panel, GUIContent.none);
            GUILayout.BeginArea(new Rect(panel.x + 10f, panel.y + 10f, panel.width - 20f, panel.height - 20f));
            GUILayout.Label(s_serviceTitle);
            if (!string.IsNullOrWhiteSpace(s_accessResultType))
            {
                GUILayout.Label($"Access: {s_accessResultType}");
            }

            GUILayout.Label(s_message, GUI.skin.box);
            if (!string.IsNullOrWhiteSpace(s_missingRequirementMessage))
            {
                GUILayout.Label($"Missing: {s_missingRequirementMessage}");
            }
            GUILayout.Label("Esc close");
            if (GUILayout.Button("Close"))
            {
                HidePanel();
            }

            GUILayout.EndArea();
        }
    }
}
