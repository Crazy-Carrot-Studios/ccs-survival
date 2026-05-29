// =============================================================================
// SCRIPT: CCS_SurvivalStatChangedEventArgs
// CATEGORY: Survival / Runtime / SurvivalCore / Events
// PURPOSE: Event payload when a survival core stat value changes.
// PLACEMENT: Passed to CCS_SurvivalCoreService event subscribers.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-28
// NOTES: Includes previous and current snapshots for event-driven consumers.
// =============================================================================

namespace CCS.Survival.SurvivalCore
{
    public sealed class CCS_SurvivalStatChangedEventArgs
    {
        #region Public Methods

        public CCS_SurvivalStatChangedEventArgs(
            CCS_SurvivalStatSnapshot previousSnapshot,
            CCS_SurvivalStatSnapshot currentSnapshot)
        {
            PreviousSnapshot = previousSnapshot;
            CurrentSnapshot = currentSnapshot;
        }

        #endregion

        #region Properties

        public CCS_SurvivalStatSnapshot PreviousSnapshot { get; }

        public CCS_SurvivalStatSnapshot CurrentSnapshot { get; }

        public CCS_SurvivalStatType StatType => CurrentSnapshot.StatType;

        #endregion
    }
}
