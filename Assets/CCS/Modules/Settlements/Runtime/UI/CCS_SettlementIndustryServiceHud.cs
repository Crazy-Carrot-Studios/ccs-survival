using System.Text;
using CCS.Modules.Industry;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_SettlementIndustryServiceHud
// CATEGORY: Modules / Settlements / Runtime / UI
// PURPOSE: Debug summary panel for blacksmith / industry service routing.
// PLACEMENT: Auto-created at runtime; not final town UI.
// AUTHOR: James Schilz
// CREATED: 2026-06-02
// NOTES: Shows forge and workstation availability; does not auto-craft.
// =============================================================================

namespace CCS.Modules.Settlements
{
    public sealed class CCS_SettlementIndustryServiceHud : MonoBehaviour
    {
        private static bool s_showPanel;
        private static string s_serviceTitle = "Industry Services";
        private static string s_summary = string.Empty;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureInstance()
        {
            if (FindAnyObjectByType<CCS_SettlementIndustryServiceHud>() != null)
            {
                return;
            }

            GameObject host = new GameObject("CCS_SettlementIndustryServiceHud");
            host.AddComponent<CCS_SettlementIndustryServiceHud>();
            DontDestroyOnLoad(host);
        }

        public static void ShowIndustrySummary(string serviceTitle, CCS_IndustryProfile profile)
        {
            s_serviceTitle = string.IsNullOrWhiteSpace(serviceTitle) ? "Industry Services" : serviceTitle;
            s_summary = BuildSummary(profile);
            s_showPanel = true;
        }

        public static void HidePanel()
        {
            s_showPanel = false;
        }

        private static string BuildSummary(CCS_IndustryProfile profile)
        {
            StringBuilder builder = new StringBuilder();
            if (profile == null)
            {
                builder.AppendLine("Industry profile unavailable.");
                return builder.ToString();
            }

            builder.AppendLine($"Profile: {profile.ProfileDisplayName}");
            builder.AppendLine("Primitive Forge / industry profile: available");
            builder.AppendLine();
            builder.AppendLine("Refine / forge services (camp workstation required):");
            AppendProcesses(builder, profile);
            builder.AppendLine();
            builder.AppendLine("Blacksmith forge recipes:");
            AppendBlacksmithRecipes(builder, profile);
            builder.AppendLine();
            builder.AppendLine("Future settlement services:");
            builder.AppendLine("- Buy / sell refined iron");
            builder.AppendLine("- Forge service");
            builder.AppendLine("- Repair service");
            builder.AppendLine("- Tool upgrade service");
            builder.AppendLine();
            builder.AppendLine("No auto-craft from settlement. Place Primitive Forge at camp.");
            return builder.ToString();
        }

        private static void AppendProcesses(StringBuilder builder, CCS_IndustryProfile profile)
        {
            bool wroteEntry = false;
            for (int index = 0; index < profile.ProcessDefinitions.Count; index++)
            {
                CCS_IndustryDefinition definition = profile.ProcessDefinitions[index];
                if (definition == null || definition.IsFuturePlaceholder)
                {
                    continue;
                }

                builder.AppendLine($"- {definition.DisplayName} ({FormatRoleLabel(definition.RequiredWorkstationRoleId)})");
                wroteEntry = true;
            }

            if (!wroteEntry)
            {
                builder.AppendLine("- None configured.");
            }
        }

        private static void AppendBlacksmithRecipes(StringBuilder builder, CCS_IndustryProfile profile)
        {
            bool wroteEntry = false;
            for (int index = 0; index < profile.BlacksmithRecipes.Count; index++)
            {
                CCS_BlacksmithRecipeDefinition recipe = profile.BlacksmithRecipes[index];
                if (recipe == null || recipe.Category == CCS_BlacksmithRecipeCategory.FutureWeapon)
                {
                    continue;
                }

                string recipeLabel = !string.IsNullOrWhiteSpace(recipe.BlacksmithRecipeId)
                    ? recipe.BlacksmithRecipeId
                    : "Forge recipe";
                builder.AppendLine($"- {recipeLabel} ({recipe.Category})");
                wroteEntry = true;
            }

            if (!wroteEntry)
            {
                builder.AppendLine("- None configured.");
            }
        }

        private static string FormatRoleLabel(string roleId)
        {
            if (string.Equals(roleId, CCS_IndustryWorkstationRole.PrimitiveForge, System.StringComparison.OrdinalIgnoreCase))
            {
                return "Primitive Forge";
            }

            if (string.Equals(roleId, CCS_IndustryWorkstationRole.SawTable, System.StringComparison.OrdinalIgnoreCase))
            {
                return "Saw Table";
            }

            if (string.Equals(roleId, CCS_IndustryWorkstationRole.CharcoalKiln, System.StringComparison.OrdinalIgnoreCase))
            {
                return "Charcoal Kiln";
            }

            return string.IsNullOrWhiteSpace(roleId) ? "Workstation" : roleId;
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

            const float width = 420f;
            const float height = 360f;
            Rect panel = new Rect(20f, 20f, width, height);
            GUI.Box(panel, GUIContent.none);
            GUILayout.BeginArea(new Rect(panel.x + 10f, panel.y + 10f, panel.width - 20f, panel.height - 20f));
            GUILayout.Label(s_serviceTitle);
            GUILayout.Label(s_summary, GUI.skin.box);
            GUILayout.Label("Esc close");
            if (GUILayout.Button("Close"))
            {
                HidePanel();
            }

            GUILayout.EndArea();
        }
    }
}
