// =============================================================================
// SCRIPT: CCS_CharacterCameraMode
// CATEGORY: Modules / CharacterController / Runtime / Data
// PURPOSE: Defines supported character camera modes for the controller module.
// PLACEMENT: Runtime data enum. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: v0.6.9 defaults to FirstPersonBodyAware. ThirdPersonSurvival retained for future mode switching.
// =============================================================================

namespace CCS.Modules.CharacterController
{
    public enum CCS_CharacterCameraMode
    {
        ThirdPersonSurvival = 0,
        FirstPerson = 1,
        TopDown = 2,
        AimOverShoulder = 3,
        Horse = 4,
        Vehicle = 5,
        FirstPersonBodyAware = 6,
        FirstPersonAim = 7
    }
}
