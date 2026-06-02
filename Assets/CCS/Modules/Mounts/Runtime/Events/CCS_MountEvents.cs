// =============================================================================
// SCRIPT: CCS_MountEvents
// CATEGORY: Modules / Mounts / Runtime / Events
// PURPOSE: Mount service event delegates for playtest and composition wiring.
// PLACEMENT: Raised by CCS_MountService.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// =============================================================================

namespace CCS.Modules.Mounts
{
    public delegate void MountStateChangedHandler(CCS_MountInstance instance, CCS_MountState previousState);

    public delegate void HorseOwnershipChangedHandler(bool ownsHorse);
}
