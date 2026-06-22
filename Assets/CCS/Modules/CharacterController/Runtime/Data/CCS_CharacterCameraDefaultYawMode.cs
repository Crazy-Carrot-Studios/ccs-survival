// =============================================================================
// SCRIPT: CCS_CharacterCameraDefaultYawMode
// CATEGORY: Modules / CharacterController / Runtime / Data
// PURPOSE: Defines how camera yaw initializes when a player camera rig binds.
// PLACEMENT: Enum used by CCS_CharacterCameraProfile and rig target setup.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: PlayerForward aligns spawn view with body facing for neutral third-person start.
// =============================================================================

namespace CCS.Modules.CharacterController
{
    public enum CCS_CharacterCameraDefaultYawMode
    {
        PlayerForward = 0
    }
}
