// =============================================================================
// SCRIPT: CCS_ShelterEvents
// CATEGORY: Modules / Shelter / Runtime / Events
// PURPOSE: Event delegate definitions for shelter lifecycle.
// PLACEMENT: Subscribed by Environment Effects and HUD/debug presenters.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-31
// NOTES: Event name constants for diagnostics and future tooling.
// =============================================================================

namespace CCS.Modules.Shelter
{
    public static class CCS_ShelterEvents
    {
        #region Variables

        public const string ShelterEntered = "ShelterEntered";

        public const string ShelterExited = "ShelterExited";

        public const string ShelterChanged = "ShelterChanged";

        #endregion
    }

    public delegate void ShelterEnteredHandler(CCS_ShelterEventArgs eventArgs);

    public delegate void ShelterExitedHandler(CCS_ShelterEventArgs eventArgs);

    public delegate void ShelterChangedHandler(CCS_ShelterEventArgs eventArgs);
}
