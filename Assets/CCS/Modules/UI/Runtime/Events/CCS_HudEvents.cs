// =============================================================================
// SCRIPT: CCS_HudEvents
// CATEGORY: Modules / UI / Runtime / Events
// PURPOSE: Event name constants and delegate contracts for HUD presentation.
// PLACEMENT: Raised by CCS_HudPresentationService and notification queue.
// AUTHOR: James Schilz
// CREATED: 2026-05-30
// NOTES: Presentation-only events. No gameplay mutation.
// =============================================================================

namespace CCS.Modules.UI
{
    public static class CCS_HudEvents
    {
        public const string HudInitializedEventName = "UI.HudInitialized";
        public const string HudDataRefreshedEventName = "UI.HudDataRefreshed";
        public const string InteractionPromptChangedEventName = "UI.InteractionPromptChanged";
        public const string NotificationQueuedEventName = "UI.NotificationQueued";
        public const string NotificationDismissedEventName = "UI.NotificationDismissed";
    }

    public delegate void HudInitializedHandler(CCS_HudEventArgs eventArgs);

    public delegate void HudDataRefreshedHandler(CCS_HudEventArgs eventArgs);

    public delegate void InteractionPromptChangedHandler(CCS_HudEventArgs eventArgs);

    public delegate void NotificationQueuedHandler(CCS_HudEventArgs eventArgs);

    public delegate void NotificationDismissedHandler(CCS_HudEventArgs eventArgs);
}
