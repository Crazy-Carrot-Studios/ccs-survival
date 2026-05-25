// =============================================================================
// SCRIPT: CCS_ModuleDiagnosticsInfo
// CATEGORY: Core / Runtime / Diagnostics
// PURPOSE: Read-only snapshot of a registered module for diagnostics reports.
// PLACEMENT: Runtime data type. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-05-24
// NOTES: Allocated only when diagnostics are manually requested.
// =============================================================================

namespace CCS.Core
{
    public readonly struct CCS_ModuleDiagnosticsInfo
    {
        #region Properties

        public string ModuleId { get; }

        public CCS_ModuleLifecycleState LifecycleState { get; }

        #endregion

        #region Public Methods

        public CCS_ModuleDiagnosticsInfo(string moduleId, CCS_ModuleLifecycleState lifecycleState)
        {
            ModuleId = moduleId ?? string.Empty;
            LifecycleState = lifecycleState;
        }

        #endregion
    }
}
