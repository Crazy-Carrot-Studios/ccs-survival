// =============================================================================
// SCRIPT: CCS_ModuleLifecycleState
// CATEGORY: Core / Runtime / Modules / Data
// PURPOSE: Tracks install-focused module lifecycle state for registry integration.
// PLACEMENT: Runtime assembly contract. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-05-24
// NOTES: Complements CCS_ModuleState system lifecycle. Does not replace CCS_Result.
// =============================================================================

namespace CCS.Core
{
    public enum CCS_ModuleLifecycleState
    {
        Uninstalled = 0,
        Installing = 1,
        Installed = 2,
        Failed = 3,
        Uninstalling = 4
    }
}
