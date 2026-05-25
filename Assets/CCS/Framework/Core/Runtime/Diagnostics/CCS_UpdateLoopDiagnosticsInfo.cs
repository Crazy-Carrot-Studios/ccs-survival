// =============================================================================
// SCRIPT: CCS_UpdateLoopDiagnosticsInfo
// CATEGORY: Core / Runtime / Diagnostics
// PURPOSE: Read-only snapshot of update loop registration counts.
// PLACEMENT: Runtime data type. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-05-24
// NOTES: Allocated only when diagnostics are manually requested.
// =============================================================================

namespace CCS.Core
{
    public readonly struct CCS_UpdateLoopDiagnosticsInfo
    {
        #region Properties

        public int UpdatableSystemCount { get; }

        public int FixedUpdatableSystemCount { get; }

        public int LateUpdatableSystemCount { get; }

        #endregion

        #region Public Methods

        public CCS_UpdateLoopDiagnosticsInfo(
            int updatableSystemCount,
            int fixedUpdatableSystemCount,
            int lateUpdatableSystemCount)
        {
            UpdatableSystemCount = updatableSystemCount;
            FixedUpdatableSystemCount = fixedUpdatableSystemCount;
            LateUpdatableSystemCount = lateUpdatableSystemCount;
        }

        #endregion
    }
}
