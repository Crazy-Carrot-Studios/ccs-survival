// =============================================================================
// SCRIPT: CCS_BuildingShelterContributionsChangedEventArgs
// CATEGORY: Modules / Building / Runtime / Events
// PURPOSE: Event payload when building shelter contributions are recalculated.
// PLACEMENT: Raised by CCS_BuildingService after placement, restore, or clear.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-31
// NOTES: Consumed by shelter integration and HUD presenters.
// =============================================================================

namespace CCS.Modules.Building
{
    public sealed class CCS_BuildingShelterContributionsChangedEventArgs
    {
        #region Public Methods

        public CCS_BuildingShelterContributionsChangedEventArgs(int contributionCount, string message)
        {
            ContributionCount = contributionCount < 0 ? 0 : contributionCount;
            Message = message ?? string.Empty;
        }

        #endregion

        #region Properties

        public int ContributionCount { get; }

        public string Message { get; }

        #endregion
    }

    public delegate void BuildingShelterContributionsChangedHandler(
        CCS_BuildingShelterContributionsChangedEventArgs eventArgs);
}
