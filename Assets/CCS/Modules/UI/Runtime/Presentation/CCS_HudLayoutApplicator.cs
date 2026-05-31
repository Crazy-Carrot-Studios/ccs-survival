using UnityEngine;
using UnityEngine.UI;

// =============================================================================
// SCRIPT: CCS_HudLayoutApplicator
// CATEGORY: Modules / UI / Runtime / Presentation
// PURPOSE: Applies HUD profile layout settings to anchored presentation areas.
// PLACEMENT: Called by CCS_HudRootPresenter during bind/awake.
// AUTHOR: James Schilz
// CREATED: 2026-05-30
// NOTES: Keeps HUD elements out of center gameplay view except low interaction prompt.
// =============================================================================

namespace CCS.Modules.UI
{
    public static class CCS_HudLayoutApplicator
    {
        #region Public Methods

        public static void Apply(CCS_HudRootPresenter rootPresenter, CCS_HudProfile profile)
        {
            if (rootPresenter == null || profile == null)
            {
                return;
            }

            CCS_HudLayoutSettings layout = profile.LayoutSettings;
            if (layout == null)
            {
                return;
            }

            ApplyCanvasScale(rootPresenter, layout);
            ApplySurvivalArea(rootPresenter.SurvivalBarArea, layout);
            ApplyInteractionPromptArea(rootPresenter.InteractionPromptArea, layout);
            ApplySummaryArea(rootPresenter.InventorySummaryArea, layout, true);
            ApplySummaryArea(rootPresenter.EquipmentSummaryArea, layout, false);
            ApplyNotificationArea(rootPresenter.NotificationArea, profile, layout);
        }

        public static void ApplyTypography(Text text, int fontSize)
        {
            if (text == null)
            {
                return;
            }

            text.fontSize = fontSize;
            text.resizeTextForBestFit = false;
        }

        #endregion

        #region Private Methods

        private static void ApplyCanvasScale(CCS_HudRootPresenter rootPresenter, CCS_HudLayoutSettings layout)
        {
            CanvasScaler scaler = rootPresenter.GetComponent<CanvasScaler>();
            if (scaler == null)
            {
                return;
            }

            float scale = layout.HudScale;
            scaler.referenceResolution = new Vector2(1920f / scale, 1080f / scale);
        }

        private static void ApplySurvivalArea(RectTransform area, CCS_HudLayoutSettings layout)
        {
            if (area == null)
            {
                return;
            }

            float barSpacing = layout.SurvivalBarHeight + 6f;
            float panelHeight = (barSpacing * 6f) + 12f;

            ConfigureRect(
                area,
                new Vector2(0f, 0f),
                new Vector2(0f, 0f),
                new Vector2(0f, 0f),
                new Vector2(layout.SafeMargin, layout.SafeMargin),
                new Vector2(layout.SurvivalBarWidth, panelHeight));
        }

        private static void ApplyInteractionPromptArea(RectTransform area, CCS_HudLayoutSettings layout)
        {
            if (area == null)
            {
                return;
            }

            ConfigureRect(
                area,
                new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f),
                new Vector2(0f, layout.InteractionPromptVerticalOffset),
                new Vector2(layout.InteractionPromptWidth, layout.InteractionPromptHeight));
        }

        private static void ApplySummaryArea(RectTransform area, CCS_HudLayoutSettings layout, bool isInventory)
        {
            if (area == null)
            {
                return;
            }

            float verticalOffset = isInventory
                ? layout.SafeMargin + layout.SummaryPanelHeight + 8f
                : layout.SafeMargin;

            ConfigureRect(
                area,
                new Vector2(1f, 0f),
                new Vector2(1f, 0f),
                new Vector2(1f, 0f),
                new Vector2(-layout.SafeMargin, verticalOffset),
                new Vector2(layout.SummaryPanelWidth, layout.SummaryPanelHeight));
        }

        private static void ApplyNotificationArea(
            RectTransform area,
            CCS_HudProfile profile,
            CCS_HudLayoutSettings layout)
        {
            if (area == null)
            {
                return;
            }

            CCS_NotificationProfile notificationProfile = profile.NotificationProfile;
            float notificationWidth = notificationProfile != null
                ? notificationProfile.NotificationWidth
                : layout.SummaryPanelWidth;

            ConfigureRect(
                area,
                new Vector2(1f, 1f),
                new Vector2(1f, 1f),
                new Vector2(1f, 1f),
                new Vector2(-layout.SafeMargin, -layout.SafeMargin),
                new Vector2(notificationWidth, layout.NotificationAreaHeight));

            VerticalLayoutGroup layoutGroup = area.GetComponent<VerticalLayoutGroup>();
            if (layoutGroup == null)
            {
                layoutGroup = area.gameObject.AddComponent<VerticalLayoutGroup>();
            }

            layoutGroup.childAlignment = TextAnchor.UpperRight;
            layoutGroup.spacing = 8f;
            layoutGroup.childControlWidth = true;
            layoutGroup.childControlHeight = false;
            layoutGroup.childForceExpandWidth = true;
            layoutGroup.childForceExpandHeight = false;
            layoutGroup.padding = new RectOffset(0, 0, 0, 0);
        }

        private static void ConfigureRect(
            RectTransform rectTransform,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 pivot,
            Vector2 anchoredPosition,
            Vector2 sizeDelta)
        {
            rectTransform.anchorMin = anchorMin;
            rectTransform.anchorMax = anchorMax;
            rectTransform.pivot = pivot;
            rectTransform.anchoredPosition = anchoredPosition;
            rectTransform.sizeDelta = sizeDelta;
        }

        #endregion
    }
}
