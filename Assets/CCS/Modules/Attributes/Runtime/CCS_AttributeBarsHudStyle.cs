using UnityEngine;

// =============================================================================
// SCRIPT: CCS_AttributeBarsHudStyle
// CATEGORY: Modules / Attributes / Runtime
// PURPOSE: Canonical layout and color tokens for the Master Test attribute bar HUD.
// PLACEMENT: Runtime constants. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: v0.4.9 gameplay-style HUD tokens for Health, Stamina, Hunger, and Thirst.
// =============================================================================

namespace CCS.Modules.Attributes
{
    public static class CCS_AttributeBarsHudStyle
    {
        public const float PanelWidth = 360f;

        public const float PanelHeight = 190f;

        public const float PanelOffsetX = 32f;

        public const float PanelOffsetY = 32f;

        public const float BarWidth = 300f;

        public const float BarHeight = 20f;

        public const float BarSpacing = 16f;

        public const float BarBlockHeight = 38f;

        public const float PanelPaddingX = 30f;

        public const float PanelPaddingTop = 12f;

        public const float LabelFontSize = 11f;

        public const float ValueFontSize = 11f;

        public const float StatusFontSize = 9f;

        public static readonly Color HealthFillColor = ParseHex("#C83A3A");

        public static readonly Color StaminaFillColor = ParseHex("#D8C64A");

        public static readonly Color HungerFillColor = ParseHex("#D8842F");

        public static readonly Color ThirstFillColor = ParseHex("#2F7FD8");

        public static readonly Color BarBackgroundColor = ParseHex("#101722");

        public static readonly Color PanelBackgroundColor = new Color(8f / 255f, 18f / 255f, 32f / 255f, 190f / 255f);

        public static readonly Color BorderColor = ParseHex("#2E4A64");

        public static readonly Color TextColor = ParseHex("#F5F7FB");

        public static readonly Color MutedTextColor = ParseHex("#9FB0C8");

        public const string HealthBarLabel = "HEALTH";

        public const string StaminaBarLabel = "STAMINA";

        public const string HungerBarLabel = "HUNGER";

        public const string ThirstBarLabel = "THIRST";

        public const string PlaceholderStatusSuffix = "Not implemented";

        public const float StaminaMax = 100f;

        public const float StaminaDrainPerSecond = 18f;

        public const float StaminaRegenPerSecond = 12f;

        public const float PlaceholderMax = 100f;

        private static Color ParseHex(string hex)
        {
            if (ColorUtility.TryParseHtmlString(hex, out Color color))
            {
                return color;
            }

            return Color.white;
        }
    }
}
