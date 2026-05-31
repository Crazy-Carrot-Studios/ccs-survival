// =============================================================================
// SCRIPT: CCS_EnvironmentEffectsEventArgs
// CATEGORY: Modules / EnvironmentEffects / Runtime / Events
// PURPOSE: Event payload carrying environment snapshots and diagnostic messages.
// PLACEMENT: Passed to environment service event subscribers.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-31
// NOTES: Read-only snapshot reference for HUD and future Survival Core systems.
// =============================================================================

namespace CCS.Modules.EnvironmentEffects
{
    public sealed class CCS_EnvironmentEffectsEventArgs
    {
        #region Public Methods

        public CCS_EnvironmentEffectsEventArgs(CCS_EnvironmentSnapshot snapshot, string message)
        {
            Snapshot = snapshot;
            Message = message ?? string.Empty;
        }

        #endregion

        #region Properties

        public CCS_EnvironmentSnapshot Snapshot { get; }

        public string Message { get; }

        #endregion
    }
}
