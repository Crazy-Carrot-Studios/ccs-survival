// =============================================================================
// SCRIPT: CCS_MountState
// CATEGORY: Modules / Mounts / Runtime / Data
// PURPOSE: Runtime state for owned mount instances (horse foundation).
// PLACEMENT: Used by CCS_MountService and CCS_MountSnapshot.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// =============================================================================

namespace CCS.Modules.Mounts
{
    public enum CCS_MountState
    {
        Idle = 0,
        Following = 1,
        Mounted = 2,
        Waiting = 3,
        Returning = 4
    }
}
