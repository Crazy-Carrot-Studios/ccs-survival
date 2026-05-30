using CCS.Survival;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_HudProfile
// CATEGORY: Modules / UI / Runtime / Profiles
// PURPOSE: HUD visibility and layout tuning profile for presentation layer.
// PLACEMENT: Assets/CCS/Survival/Profiles/UI/
// AUTHOR: James Schilz
// CREATED: 2026-05-30
// NOTES: Read-only presentation configuration. No gameplay logic.
// =============================================================================

namespace CCS.Modules.UI
{
    [CreateAssetMenu(
        fileName = "CCS_HudProfile",
        menuName = "CCS/Survival/UI/HUD Profile")]
    public sealed class CCS_HudProfile : CCS_SurvivalProfileBase
    {
        #region Variables

        [Header("HUD Visibility")]
        [Tooltip("When enabled, survival stat bars are shown.")]
        [SerializeField] private bool showSurvivalBars = true;

        [Tooltip("When enabled, the interaction prompt is shown.")]
        [SerializeField] private bool showInteractionPrompt = true;

        [Tooltip("When enabled, the inventory summary panel is shown.")]
        [SerializeField] private bool showInventorySummary = true;

        [Tooltip("When enabled, the equipment summary panel is shown.")]
        [SerializeField] private bool showEquipmentSummary = true;

        [Tooltip("When enabled, the notification queue is shown.")]
        [SerializeField] private bool showNotifications = true;

        [Header("Notifications")]
        [SerializeField] private CCS_NotificationProfile notificationProfile = new CCS_NotificationProfile();

        [Header("Layout")]
        [SerializeField] private CCS_HudLayoutSettings layoutSettings = new CCS_HudLayoutSettings();

        #endregion

        #region Properties

        public bool ShowSurvivalBars => showSurvivalBars;

        public bool ShowInteractionPrompt => showInteractionPrompt;

        public bool ShowInventorySummary => showInventorySummary;

        public bool ShowEquipmentSummary => showEquipmentSummary;

        public bool ShowNotifications => showNotifications;

        public CCS_NotificationProfile NotificationProfile => notificationProfile;

        public CCS_HudLayoutSettings LayoutSettings => layoutSettings;

        #endregion
    }
}
