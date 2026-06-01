// =============================================================================
// SCRIPT: CCS_CharacterCameraMode
// CATEGORY: Modules / CharacterController / Runtime / Profiles
// PURPOSE: Profile-driven camera mode identifiers for present and planned controller setups.
// PLACEMENT: Used by CCS_CharacterCameraProfile.activeCameraMode.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: Only ThirdPersonSurvival is implemented. Other modes are architecture placeholders.
// =============================================================================

namespace CCS.Modules.CharacterController
{
    public enum CCS_CharacterCameraMode
    {
        ThirdPersonSurvival = 0,
        FirstPerson = 1,
        TopDown = 2,
        AimOverShoulder = 3,
        Vehicle = 4,
        Horse = 5
    }
}
