using System;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_HudLayoutSettings
// CATEGORY: Modules / UI / Runtime / Profiles
// PURPOSE: Serializable HUD layout and readability tuning for presentation layer.
// PLACEMENT: Embedded on CCS_HudProfile.
// AUTHOR: James Schilz
// CREATED: 2026-05-30
// NOTES: Presentation-only settings. Applied at runtime by CCS_HudLayoutApplicator.
// =============================================================================

namespace CCS.Modules.UI
{
    [Serializable]
    public sealed class CCS_HudLayoutSettings
    {
        #region Variables

        [Tooltip("Overall HUD scale multiplier applied to canvas scaler reference resolution.")]
        [SerializeField] private float hudScale = 1f;

        [Tooltip("Screen-edge margin in pixels at 1080p reference resolution.")]
        [SerializeField] private float safeMargin = 28f;

        [Tooltip("Width of the survival bar panel.")]
        [SerializeField] private float survivalBarWidth = 400f;

        [Tooltip("Height of each survival bar row.")]
        [SerializeField] private float survivalBarHeight = 34f;

        [Tooltip("Font size for survival bar labels and values.")]
        [SerializeField] private int survivalBarFontSize = 17;

        [Tooltip("Font size for inventory and equipment summary lines.")]
        [SerializeField] private int summaryFontSize = 16;

        [Tooltip("Font size for the interaction prompt.")]
        [SerializeField] private int interactionPromptFontSize = 22;

        [Tooltip("Vertical offset from the bottom edge for the interaction prompt.")]
        [SerializeField] private float interactionPromptVerticalOffset = 56f;

        [Tooltip("Width of the interaction prompt panel.")]
        [SerializeField] private float interactionPromptWidth = 480f;

        [Tooltip("Height of the interaction prompt panel.")]
        [SerializeField] private float interactionPromptHeight = 44f;

        [Tooltip("Width of inventory and equipment summary panels.")]
        [SerializeField] private float summaryPanelWidth = 420f;

        [Tooltip("Height of inventory and equipment summary panels.")]
        [SerializeField] private float summaryPanelHeight = 38f;

        [Tooltip("Height reserved for the notification stack area.")]
        [SerializeField] private float notificationAreaHeight = 240f;

        #endregion

        #region Properties

        public float HudScale => hudScale < 0.75f ? 0.75f : hudScale;

        public float SafeMargin => safeMargin < 12f ? 12f : safeMargin;

        public float SurvivalBarWidth => survivalBarWidth < 320f ? 320f : survivalBarWidth;

        public float SurvivalBarHeight => survivalBarHeight < 28f ? 28f : survivalBarHeight;

        public int SurvivalBarFontSize => survivalBarFontSize < 14 ? 14 : survivalBarFontSize;

        public int SummaryFontSize => summaryFontSize < 14 ? 14 : summaryFontSize;

        public int InteractionPromptFontSize => interactionPromptFontSize < 16 ? 16 : interactionPromptFontSize;

        public float InteractionPromptVerticalOffset =>
            interactionPromptVerticalOffset < 32f ? 32f : interactionPromptVerticalOffset;

        public float InteractionPromptWidth => interactionPromptWidth < 320f ? 320f : interactionPromptWidth;

        public float InteractionPromptHeight => interactionPromptHeight < 36f ? 36f : interactionPromptHeight;

        public float SummaryPanelWidth => summaryPanelWidth < 320f ? 320f : summaryPanelWidth;

        public float SummaryPanelHeight => summaryPanelHeight < 32f ? 32f : summaryPanelHeight;

        public float NotificationAreaHeight => notificationAreaHeight < 160f ? 160f : notificationAreaHeight;

        #endregion
    }
}
