// =============================================================================
// SCRIPT: CCS_ModuleState
// CATEGORY: Core / Runtime / Modules / Data
// PURPOSE: Tracks basic module lifecycle state for future framework modules.
// PLACEMENT: Runtime assembly contract. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-05-24
// NOTES: Enum only. No implementation. Compiled by CCS.Core.Runtime.
// =============================================================================

namespace CCS.Core
{
    public enum CCS_ModuleState
    {
        Uninitialized = 0,
        Initialized = 1,
        Installed = 2,
        Shutdown = 3
    }
}
