using CCS.Survival;

// =============================================================================
// SCRIPT: CCS_CharacterControllerValidationUtility
// CATEGORY: Modules / CharacterController / Runtime / Validation
// PURPOSE: Runtime-safe validation for character controller profiles and tuning.
// PLACEMENT: Used by editor validators and future bootstrap checks.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Returns CCS_SurvivalValidationResult. No UnityEditor references.
// =============================================================================

namespace CCS.Modules.CharacterController
{
    public static class CCS_CharacterControllerValidationUtility
    {
        #region Public Methods

        public static CCS_SurvivalValidationResult ValidateProfile(CCS_CharacterControllerProfile profile)
        {
            if (profile == null)
            {
                return CCS_SurvivalValidationResult.Fail("Character controller profile is null.");
            }

            CCS_SurvivalValidationResult baseValidation = CCS_SurvivalProfileValidationUtility.ValidateProfile(profile);
            if (!baseValidation.IsSuccess)
            {
                return baseValidation;
            }

            CCS_SurvivalValidationResult movementValidation = ValidateMovementProfile(profile.Movement);
            if (!movementValidation.IsSuccess)
            {
                return movementValidation;
            }

            CCS_SurvivalValidationResult cameraValidation = ValidateCameraProfile(profile.Camera);
            if (!cameraValidation.IsSuccess)
            {
                return cameraValidation;
            }

            return CCS_SurvivalValidationResult.Pass("Character controller profile validated.");
        }

        public static CCS_SurvivalValidationResult ValidateMovementProfile(CCS_CharacterMovementProfile movement)
        {
            if (movement == null)
            {
                return CCS_SurvivalValidationResult.Fail("Movement profile section is null.");
            }

            if (movement.WalkSpeed <= 0f)
            {
                return CCS_SurvivalValidationResult.Fail("Walk speed must be greater than zero.");
            }

            if (movement.RunSpeed <= 0f)
            {
                return CCS_SurvivalValidationResult.Fail("Run speed must be greater than zero.");
            }

            if (movement.CrouchSpeed <= 0f)
            {
                return CCS_SurvivalValidationResult.Fail("Crouch speed must be greater than zero.");
            }

            if (movement.RunSpeed < movement.WalkSpeed)
            {
                return CCS_SurvivalValidationResult.Warn("Run speed is lower than walk speed.");
            }

            if (movement.JumpHeight <= 0f)
            {
                return CCS_SurvivalValidationResult.Fail("Jump height must be greater than zero.");
            }

            if (movement.Gravity >= 0f)
            {
                return CCS_SurvivalValidationResult.Fail("Gravity must be negative for downward acceleration.");
            }

            if (movement.ControllerHeight <= 0f)
            {
                return CCS_SurvivalValidationResult.Fail("CharacterController height must be greater than zero.");
            }

            if (movement.ControllerRadius <= 0f)
            {
                return CCS_SurvivalValidationResult.Fail("CharacterController radius must be greater than zero.");
            }

            if (movement.StepOffset < 0f)
            {
                return CCS_SurvivalValidationResult.Fail("Step offset cannot be negative.");
            }

            if (movement.SlopeLimit <= 0f || movement.SlopeLimit > 89f)
            {
                return CCS_SurvivalValidationResult.Fail("Slope limit must be between 0 and 89 degrees.");
            }

            if (movement.StaminaDrainPerSecondWhileSprinting < 0f)
            {
                return CCS_SurvivalValidationResult.Fail("Sprint stamina drain cannot be negative.");
            }

            if (movement.Acceleration <= 0f)
            {
                return CCS_SurvivalValidationResult.Fail("Acceleration must be greater than zero.");
            }

            if (movement.Deceleration <= 0f)
            {
                return CCS_SurvivalValidationResult.Fail("Deceleration must be greater than zero.");
            }

            if (movement.SprintAcceleration <= 0f)
            {
                return CCS_SurvivalValidationResult.Fail("Sprint acceleration must be greater than zero.");
            }

            if (movement.RotationSmoothing <= 0f)
            {
                return CCS_SurvivalValidationResult.Fail("Rotation smoothing must be greater than zero.");
            }

            if (movement.AirControl < 0f || movement.AirControl > 1f)
            {
                return CCS_SurvivalValidationResult.Fail("Air control must be between 0 and 1.");
            }

            return CCS_SurvivalValidationResult.Pass();
        }

        public static CCS_SurvivalValidationResult ValidateCameraProfile(CCS_CharacterCameraProfile camera)
        {
            if (camera == null)
            {
                return CCS_SurvivalValidationResult.Fail("Camera profile section is null.");
            }

            if (camera.ActiveCameraMode != CCS_CharacterCameraMode.ThirdPersonSurvival)
            {
                return CCS_SurvivalValidationResult.Fail(
                    $"Unsupported active camera mode '{camera.ActiveCameraMode}'. Default profile must use ThirdPersonSurvival.");
            }

            if (camera.MouseSensitivityX <= 0f)
            {
                return CCS_SurvivalValidationResult.Fail("Mouse sensitivity X must be greater than zero.");
            }

            if (camera.MouseSensitivityY <= 0f)
            {
                return CCS_SurvivalValidationResult.Fail("Mouse sensitivity Y must be greater than zero.");
            }

            if (camera.GamepadSensitivityX <= 0f)
            {
                return CCS_SurvivalValidationResult.Fail("Gamepad sensitivity X must be greater than zero.");
            }

            if (camera.GamepadSensitivityY <= 0f)
            {
                return CCS_SurvivalValidationResult.Fail("Gamepad sensitivity Y must be greater than zero.");
            }

            if (camera.MinPitch >= camera.MaxPitch)
            {
                return CCS_SurvivalValidationResult.Fail("Camera min pitch must be less than max pitch.");
            }

            if (camera.MinPitch < -89f || camera.MaxPitch > 89f)
            {
                return CCS_SurvivalValidationResult.Warn("Camera pitch limits are outside the safe -89 to 89 range.");
            }

            if (camera.CameraDistance <= 0.5f)
            {
                return CCS_SurvivalValidationResult.Fail("Third-person camera distance must be greater than 0.5.");
            }

            if (camera.PivotHeight <= 0f)
            {
                return CCS_SurvivalValidationResult.Fail("Camera pivot height must be greater than zero.");
            }

            if (camera.ZoomDistanceMin <= 0f)
            {
                return CCS_SurvivalValidationResult.Fail("Zoom distance min must be greater than zero.");
            }

            if (camera.ZoomDistanceMax < camera.ZoomDistanceMin)
            {
                return CCS_SurvivalValidationResult.Fail("Zoom distance max must be greater than or equal to zoom distance min.");
            }

            if (camera.AimTransitionSpeed <= 0f)
            {
                return CCS_SurvivalValidationResult.Fail("Aim transition speed must be greater than zero.");
            }

            if (camera.EnableObstacleAvoidance && camera.ObstacleCameraRadius <= 0f)
            {
                return CCS_SurvivalValidationResult.Fail("Obstacle camera radius must be greater than zero when avoidance is enabled.");
            }

            return CCS_SurvivalValidationResult.Pass();
        }

        #endregion
    }
}
