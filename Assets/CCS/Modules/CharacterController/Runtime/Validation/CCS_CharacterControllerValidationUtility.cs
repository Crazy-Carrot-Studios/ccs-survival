using System.Collections.Generic;
using System.IO;
using CCS.Project;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

// =============================================================================
// SCRIPT: CCS_CharacterControllerValidationUtility
// CATEGORY: Modules / CharacterController / Runtime / Validation
// PURPOSE: Static validation for character controller module assets and prefab wiring.
// PLACEMENT: Runtime utility. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: No test scene required for v0.2.0.
// =============================================================================

namespace CCS.Modules.CharacterController
{
    public static class CCS_CharacterControllerValidationUtility
    {
        #region Public Methods

        public static CCS_SurvivalValidationResult ValidateModuleFoundation()
        {
            List<string> failures = new List<string>();

            AppendIfMissing(failures, File.Exists(CCS_CharacterControllerConstants.RuntimeAsmdefPath), "Runtime asmdef missing.");
            AppendIfMissing(failures, File.Exists(CCS_CharacterControllerConstants.EditorAsmdefPath), "Editor asmdef missing.");
            AppendIfMissing(failures, File.Exists(CCS_CharacterControllerConstants.InputActionsAssetPath), "Input actions asset missing.");
            AppendIfMissing(failures, File.Exists(CCS_CharacterControllerConstants.DefaultMovementProfilePath), "Default movement profile missing.");
            AppendIfMissing(failures, File.Exists(CCS_CharacterControllerConstants.DefaultCameraProfilePath), "Default camera profile missing.");
            AppendIfMissing(failures, File.Exists(CCS_CharacterControllerConstants.DefaultCameraProfileSetPath), "Default camera profile set missing.");
            AppendIfMissing(failures, File.Exists(CCS_CharacterControllerConstants.TestPrefabPath), "Test prefab missing.");
            AppendIfMissing(failures, File.Exists(CCS_CharacterControllerConstants.TestGroundPrefabPath), "Test ground prefab missing.");

            CCS_SurvivalValidationResult legacyInputValidation = ValidateNoLegacyInputUsage();
            if (!legacyInputValidation.IsSuccess)
            {
                failures.Add(legacyInputValidation.Message);
            }

            if (failures.Count > 0)
            {
                return CCS_SurvivalValidationResult.Fail(string.Join(" ", failures));
            }

            return CCS_SurvivalValidationResult.Pass("Character controller foundation assets validated.");
        }

        public static CCS_SurvivalValidationResult ValidateInputActionsAsset(InputActionAsset inputActionsAsset)
        {
            if (inputActionsAsset == null)
            {
                return CCS_SurvivalValidationResult.Fail("Input actions asset reference is null.");
            }

            InputActionMap gameplayMap = inputActionsAsset.FindActionMap(
                CCS_CharacterControllerConstants.InputActionMapName,
                false);
            if (gameplayMap == null)
            {
                return CCS_SurvivalValidationResult.Fail(
                    $"Input action map '{CCS_CharacterControllerConstants.InputActionMapName}' is missing.");
            }

            List<string> missingActions = new List<string>();
            AppendIfMissingAction(missingActions, gameplayMap, CCS_CharacterControllerConstants.MoveActionName);
            AppendIfMissingAction(missingActions, gameplayMap, CCS_CharacterControllerConstants.LookActionName);
            AppendIfMissingAction(missingActions, gameplayMap, CCS_CharacterControllerConstants.SprintActionName);
            AppendIfMissingAction(missingActions, gameplayMap, CCS_CharacterControllerConstants.JumpActionName);
            AppendIfMissingAction(missingActions, gameplayMap, CCS_CharacterControllerConstants.ToggleCursorActionName);
            AppendIfMissingAction(missingActions, gameplayMap, CCS_CharacterControllerConstants.CameraZoomActionName);

            if (missingActions.Count > 0)
            {
                return CCS_SurvivalValidationResult.Fail(
                    "Missing input actions: " + string.Join(", ", missingActions));
            }

            CCS_SurvivalValidationResult jumpBindingValidation = ValidateJumpInputBindings(gameplayMap);
            if (!jumpBindingValidation.IsSuccess)
            {
                return jumpBindingValidation;
            }

            return CCS_SurvivalValidationResult.Pass("Character controller input actions validated.");
        }

        public static CCS_SurvivalValidationResult ValidateJumpInputBindings(InputActionMap gameplayMap)
        {
            if (gameplayMap == null)
            {
                return CCS_SurvivalValidationResult.Fail("Gameplay input action map is null.");
            }

            InputAction jumpAction = gameplayMap.FindAction(CCS_CharacterControllerConstants.JumpActionName, false);
            if (jumpAction == null)
            {
                return CCS_SurvivalValidationResult.Fail(
                    $"Jump action '{CCS_CharacterControllerConstants.JumpActionName}' is missing.");
            }

            bool hasKeyboardSpace = false;
            bool hasGamepadSouth = false;
            for (int i = 0; i < jumpAction.bindings.Count; i++)
            {
                string path = jumpAction.bindings[i].path;
                if (path.Contains("/space", System.StringComparison.OrdinalIgnoreCase))
                {
                    hasKeyboardSpace = true;
                }

                if (path.Contains("buttonSouth", System.StringComparison.OrdinalIgnoreCase))
                {
                    hasGamepadSouth = true;
                }
            }

            List<string> failures = new List<string>();
            if (!hasKeyboardSpace)
            {
                failures.Add("Jump action must bind keyboard Space.");
            }

            if (!hasGamepadSouth)
            {
                failures.Add("Jump action must bind gamepad South button.");
            }

            if (failures.Count > 0)
            {
                return CCS_SurvivalValidationResult.Fail(string.Join(" ", failures));
            }

            return CCS_SurvivalValidationResult.Pass("Jump input bindings validated.");
        }

        public static CCS_SurvivalValidationResult ValidateMovementProfile(CCS_CharacterMovementProfile profile)
        {
            if (profile == null)
            {
                return CCS_SurvivalValidationResult.Fail("Movement profile reference is null.");
            }

            CCS_SurvivalValidationResult baseValidation = CCS_SurvivalProfileValidationUtility.ValidateProfile(profile);
            if (!baseValidation.IsSuccess)
            {
                return baseValidation;
            }

            List<string> failures = new List<string>();
            if (profile.WalkSpeed <= 0f)
            {
                failures.Add("walkSpeed must be positive.");
            }

            if (profile.SprintSpeed < profile.WalkSpeed)
            {
                failures.Add("sprintSpeed must be >= walkSpeed.");
            }

            if (profile.Acceleration <= 0f || profile.Deceleration <= 0f)
            {
                failures.Add("acceleration and deceleration must be positive.");
            }

            if (!profile.JumpEnabled)
            {
                // Jump may be disabled on custom profiles.
            }
            else
            {
                if (profile.JumpHeight <= 0f)
                {
                    failures.Add("jumpHeight must be positive when jump is enabled.");
                }

                if (profile.CoyoteTime < 0f)
                {
                    failures.Add("coyoteTime must be >= 0 when jump is enabled.");
                }

                if (profile.JumpBufferTime < 0f)
                {
                    failures.Add("jumpBufferTime must be >= 0 when jump is enabled.");
                }
            }

            if (profile.Gravity >= 0f)
            {
                failures.Add("gravity must be negative.");
            }

            ValidateAimMovementSettings(profile, failures);

            if (failures.Count > 0)
            {
                return CCS_SurvivalValidationResult.Fail(string.Join(" ", failures));
            }

            return CCS_SurvivalValidationResult.Pass("Movement profile validated.");
        }

        public static CCS_SurvivalValidationResult ValidatePlayerJumpConfiguration(
            CCS_CharacterMotor motor,
            string contextLabel)
        {
            if (motor == null)
            {
                return CCS_SurvivalValidationResult.Fail($"{contextLabel} motor reference is null.");
            }

            CCS_CharacterMovementProfile profile = motor.MovementProfile;
            if (profile == null)
            {
                return CCS_SurvivalValidationResult.Fail($"{contextLabel} movement profile is not assigned.");
            }

            List<string> failures = new List<string>();
            if (!profile.JumpEnabled)
            {
                failures.Add("jump must be enabled.");
            }

            AppendIfApproximatelyUnequal(
                failures,
                profile.JumpHeight,
                CCS_CharacterControllerConstants.DefaultJumpHeight,
                "jumpHeight");
            AppendIfApproximatelyUnequal(
                failures,
                profile.Gravity,
                CCS_CharacterControllerConstants.DefaultGravity,
                "gravity");
            AppendIfApproximatelyUnequal(
                failures,
                profile.CoyoteTime,
                CCS_CharacterControllerConstants.DefaultCoyoteTime,
                "coyoteTime");
            AppendIfApproximatelyUnequal(
                failures,
                profile.JumpBufferTime,
                CCS_CharacterControllerConstants.DefaultJumpBufferTime,
                "jumpBufferTime");
            AppendIfApproximatelyUnequal(
                failures,
                profile.AirControl,
                CCS_CharacterControllerConstants.DefaultAirControl,
                "airControl");

            if (failures.Count > 0)
            {
                return CCS_SurvivalValidationResult.Fail($"{contextLabel} jump configuration invalid: {string.Join(" ", failures)}");
            }

            return CCS_SurvivalValidationResult.Pass($"{contextLabel} jump configuration validated.");
        }

        public static CCS_SurvivalValidationResult ValidateCameraProfile(CCS_CharacterCameraProfile profile)
        {
            if (profile == null)
            {
                return CCS_SurvivalValidationResult.Fail("Camera profile reference is null.");
            }

            CCS_SurvivalValidationResult baseValidation = CCS_SurvivalProfileValidationUtility.ValidateProfile(profile);
            if (!baseValidation.IsSuccess)
            {
                return baseValidation;
            }

            if (profile.CameraMode != CCS_CharacterCameraMode.ThirdPersonSurvival)
            {
                return CCS_SurvivalValidationResult.Fail(
                    "Default camera profile must use ThirdPersonSurvival mode in v0.2.0.");
            }

            if (profile.VerticalOrbitMin >= profile.VerticalOrbitMax)
            {
                return CCS_SurvivalValidationResult.Fail("verticalOrbitMin must be less than verticalOrbitMax.");
            }

            if (profile.ThirdPersonCameraDistance < CCS_CharacterControllerConstants.ThirdPersonCameraDistanceMinimum
                || profile.ThirdPersonCameraDistance > CCS_CharacterControllerConstants.ThirdPersonCameraDistanceMaximum)
            {
                return CCS_SurvivalValidationResult.Fail(
                    "Third-person camera distance must be between 2.85 and 3.25.");
            }

            if (profile.TrackingTargetLocalHeight < CCS_CharacterControllerConstants.CameraPitchTargetMinimumLocalHeight
                || profile.TrackingTargetLocalHeight > CCS_CharacterControllerConstants.CameraPitchTargetMaximumLocalHeight)
            {
                return CCS_SurvivalValidationResult.Fail(
                    "Tracking target local height must be between 1.40 and 1.60 for third-person framing.");
            }

            return CCS_SurvivalValidationResult.Pass("Camera profile validated.");
        }

        public static CCS_SurvivalValidationResult ValidateCameraProfileSet(CCS_CharacterCameraProfileSet profileSet)
        {
            if (profileSet == null)
            {
                return CCS_SurvivalValidationResult.Fail("Camera profile set reference is null.");
            }

            if (profileSet.DefaultProfile == null)
            {
                return CCS_SurvivalValidationResult.Fail("Camera profile set default profile is missing.");
            }

            CCS_SurvivalValidationResult defaultValidation = ValidateCameraProfile(profileSet.DefaultProfile);
            if (!defaultValidation.IsSuccess)
            {
                return defaultValidation;
            }

            if (profileSet.DefaultProfile.CameraMode != CCS_CharacterCameraMode.ThirdPersonSurvival)
            {
                return CCS_SurvivalValidationResult.Fail(
                    "Camera profile set default must be ThirdPersonSurvival.");
            }

            return CCS_SurvivalValidationResult.Pass("Camera profile set validated.");
        }

        public static CCS_SurvivalValidationResult ValidateTestPrefab(GameObject prefabRoot)
        {
            if (prefabRoot == null)
            {
                return CCS_SurvivalValidationResult.Fail("Test prefab root is null.");
            }

            List<string> failures = new List<string>();

            UnityEngine.CharacterController unityCharacterController =
                prefabRoot.GetComponent<UnityEngine.CharacterController>();
            CCS_CharacterMotor motor = prefabRoot.GetComponent<CCS_CharacterMotor>();
            CCS_CharacterInputActionProvider inputProvider = prefabRoot.GetComponent<CCS_CharacterInputActionProvider>();
            CCS_CharacterCameraController cameraController = prefabRoot.GetComponent<CCS_CharacterCameraController>();
            CCS_CharacterControllerService service = prefabRoot.GetComponent<CCS_CharacterControllerService>();

            AppendIfMissing(failures, unityCharacterController != null, "Prefab missing CharacterController.");
            AppendIfMissing(failures, motor != null, "Prefab missing CCS_CharacterMotor.");
            AppendIfMissing(failures, inputProvider != null, "Prefab missing CCS_CharacterInputActionProvider.");
            AppendIfMissing(failures, cameraController != null, "Prefab missing CCS_CharacterCameraController.");
            AppendIfMissing(failures, service != null, "Prefab missing CCS_CharacterControllerService.");
            AppendIfMissing(
                failures,
                !HasComponentNamed(prefabRoot, "CCS_CharacterControllerDebugHud"),
                "Test player prefab must not contain CCS_CharacterControllerDebugHud.");

            if (motor != null && motor.MovementProfile == null)
            {
                failures.Add("Motor movement profile is not assigned.");
            }
            else if (motor != null)
            {
                CCS_SurvivalValidationResult movementValidation = ValidateMovementProfile(motor.MovementProfile);
                if (!movementValidation.IsSuccess)
                {
                    failures.Add(movementValidation.Message);
                }
                else
                {
                    CCS_SurvivalValidationResult jumpValidation = ValidatePlayerJumpConfiguration(
                        motor,
                        "Test player prefab");
                    if (!jumpValidation.IsSuccess)
                    {
                        failures.Add(jumpValidation.Message);
                    }
                }
            }

            if (inputProvider != null && inputProvider.InputActionsAsset == null)
            {
                failures.Add("Input actions asset is not assigned on prefab.");
            }
            else if (inputProvider != null)
            {
                CCS_SurvivalValidationResult inputValidation = ValidateInputActionsAsset(inputProvider.InputActionsAsset);
                if (!inputValidation.IsSuccess)
                {
                    failures.Add(inputValidation.Message);
                }
            }

            if (cameraController != null)
            {
                if (cameraController.CameraProfileSet == null)
                {
                    failures.Add("Camera profile set is not assigned.");
                }
                else
                {
                    CCS_SurvivalValidationResult setValidation = ValidateCameraProfileSet(cameraController.CameraProfileSet);
                    if (!setValidation.IsSuccess)
                    {
                        failures.Add(setValidation.Message);
                    }
                }

                if (cameraController.CameraPivot == null || cameraController.CameraLookTarget == null)
                {
                    failures.Add("Camera pivot or look target is not assigned.");
                }

                if (cameraController.CinemachineCamera == null)
                {
                    failures.Add("CinemachineCamera reference is missing.");
                }
                else
                {
                    CinemachineThirdPersonFollow thirdPersonFollow =
                        cameraController.CinemachineCamera.GetComponent<CinemachineThirdPersonFollow>();
                    if (thirdPersonFollow == null)
                    {
                        failures.Add("CinemachineCamera is missing CinemachineThirdPersonFollow.");
                    }
                    else if (thirdPersonFollow.CameraDistance < CCS_CharacterControllerConstants.ThirdPersonCameraDistanceMinimum
                             || thirdPersonFollow.CameraDistance > CCS_CharacterControllerConstants.ThirdPersonCameraDistanceMaximum)
                    {
                        failures.Add("CinemachineCamera third-person distance is outside the tuned survival range.");
                    }

                    if (cameraController.CinemachineCamera.GetComponent<CinemachineThirdPersonAim>() != null)
                    {
                        failures.Add("Third-person CinemachineCamera must not use Third Person Aim.");
                    }
                }
            }

            CinemachineBrain brain = prefabRoot.GetComponentInChildren<CinemachineBrain>(true);
            AppendIfMissing(failures, brain != null, "Main Camera is missing CinemachineBrain.");

            if (failures.Count > 0)
            {
                return CCS_SurvivalValidationResult.Fail(string.Join(" ", failures));
            }

            return CCS_SurvivalValidationResult.Pass("Character controller test prefab validated.");
        }

        public static CCS_SurvivalValidationResult ValidateNoLegacyInputUsage()
        {
            string runtimeRoot = CCS_CharacterControllerConstants.ModuleRootPath + "/Runtime";
            if (!Directory.Exists(runtimeRoot))
            {
                return CCS_SurvivalValidationResult.Fail("Character controller runtime folder is missing.");
            }

            string[] csharpFiles = Directory.GetFiles(runtimeRoot, "*.cs", SearchOption.AllDirectories);
            foreach (string filePath in csharpFiles)
            {
                string content = File.ReadAllText(filePath);
                if (content.Contains("UnityEngine.Input") && !content.Contains("UnityEngine.InputSystem"))
                {
                    return CCS_SurvivalValidationResult.Fail(
                        $"Legacy UnityEngine.Input usage detected in {filePath}.");
                }
            }

            return CCS_SurvivalValidationResult.Pass("No legacy UnityEngine.Input usage detected.");
        }

        #endregion

        #region Private Methods

        private static void AppendIfMissing(List<string> failures, bool condition, string message)
        {
            if (!condition)
            {
                failures.Add(message);
            }
        }

        private static void AppendIfMissingAction(List<string> missingActions, InputActionMap map, string actionName)
        {
            if (map.FindAction(actionName, false) == null)
            {
                missingActions.Add(actionName);
            }
        }

        private static bool HasComponentNamed(GameObject prefabRoot, string typeName)
        {
            MonoBehaviour[] behaviours = prefabRoot.GetComponentsInChildren<MonoBehaviour>(true);
            for (int i = 0; i < behaviours.Length; i++)
            {
                MonoBehaviour behaviour = behaviours[i];
                if (behaviour != null && behaviour.GetType().Name == typeName)
                {
                    return true;
                }
            }

            return false;
        }

        private static void AppendIfApproximatelyUnequal(
            List<string> failures,
            float actual,
            float expected,
            string fieldName,
            float tolerance = 0.001f)
        {
            if (Mathf.Abs(actual - expected) > tolerance)
            {
                failures.Add($"{fieldName} must be {expected.ToString("0.###")} (found {actual.ToString("0.###")}).");
            }
        }

        private static void ValidateAimMovementSettings(
            CCS_CharacterMovementProfile profile,
            List<string> failures)
        {
            if (profile.AimMovementSpeedMultiplier <= 0f || profile.AimMovementSpeedMultiplier > 1f)
            {
                failures.Add("aimMovementSpeedMultiplier must be > 0 and <= 1.");
            }

            if (!profile.AimDisableSprint)
            {
                failures.Add("aimDisableSprint must be enabled for aim strafe locomotion.");
            }

            if (profile.AimRotationSpeedDegrees < CCS_CharacterControllerConstants.MinimumAimRotationSpeedDegrees)
            {
                failures.Add(
                    "aimRotationSpeedDegrees must be >= "
                    + CCS_CharacterControllerConstants.MinimumAimRotationSpeedDegrees.ToString("0.###")
                    + ".");
            }

            if (profile.AimStrafeDeadZone < 0f)
            {
                failures.Add("aimStrafeDeadZone must be >= 0.");
            }

            if (profile.AimBackpedalMultiplier <= 0f || profile.AimBackpedalMultiplier > 1f)
            {
                failures.Add("aimBackpedalMultiplier must be > 0 and <= 1.");
            }

            if (profile.AimSideStrafeMultiplier <= 0f || profile.AimSideStrafeMultiplier > 1f)
            {
                failures.Add("aimSideStrafeMultiplier must be > 0 and <= 1.");
            }
        }

        public static CCS_SurvivalValidationResult ValidateAimLocomotionPlayerComponents(GameObject prefabRoot)
        {
            if (prefabRoot == null)
            {
                return CCS_SurvivalValidationResult.Fail("Player prefab reference is null.");
            }

            List<string> failures = new List<string>();
            AppendIfMissing(
                failures,
                prefabRoot.GetComponent<CCS_CharacterAimLocomotionController>() != null,
                "Prefab missing CCS_CharacterAimLocomotionController.");
            AppendIfMissing(
                failures,
                prefabRoot.GetComponent<CCS_CharacterMotor>() != null,
                "Prefab missing CCS_CharacterMotor for aim locomotion.");
            AppendIfMissing(
                failures,
                prefabRoot.GetComponentInChildren<CCS_CharacterCameraFollowAnchor>(true) != null,
                "Prefab missing CCS_CharacterCameraFollowAnchor for aim movement basis.");

            CCS_CharacterMotor motor = prefabRoot.GetComponent<CCS_CharacterMotor>();
            if (motor != null && motor.MovementProfile == null)
            {
                failures.Add("Prefab motor movement profile is not assigned.");
            }
            else if (motor != null)
            {
                CCS_SurvivalValidationResult profileValidation = ValidateMovementProfile(motor.MovementProfile);
                if (!profileValidation.IsSuccess)
                {
                    failures.Add(profileValidation.Message);
                }
            }

            return failures.Count > 0
                ? CCS_SurvivalValidationResult.Fail(string.Join(" ", failures))
                : CCS_SurvivalValidationResult.Pass("Aim locomotion player components validated.");
        }

        #endregion
    }
}
