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

            return CCS_SurvivalValidationResult.Pass();
        }

        public static CCS_SurvivalValidationResult ValidateCameraProfile(CCS_CharacterCameraProfile camera)
        {
            if (camera == null)
            {
                return CCS_SurvivalValidationResult.Fail("Camera profile section is null.");
            }

            if (camera.HorizontalSensitivity <= 0f)
            {
                return CCS_SurvivalValidationResult.Fail("Horizontal look sensitivity must be greater than zero.");
            }

            if (camera.VerticalSensitivity <= 0f)
            {
                return CCS_SurvivalValidationResult.Fail("Vertical look sensitivity must be greater than zero.");
            }

            if (camera.MinPitch >= camera.MaxPitch)
            {
                return CCS_SurvivalValidationResult.Fail("Camera min pitch must be less than max pitch.");
            }

            return CCS_SurvivalValidationResult.Pass();
        }

        #endregion
    }
}
