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

            return CCS_SurvivalValidationResult.Pass("Character controller input actions validated.");
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
                // Expected default for v0.2.0.
            }
            else if (profile.JumpHeight <= 0f)
            {
                failures.Add("jumpHeight must be positive when jump is enabled.");
            }

            if (failures.Count > 0)
            {
                return CCS_SurvivalValidationResult.Fail(string.Join(" ", failures));
            }

            return CCS_SurvivalValidationResult.Pass("Movement profile validated.");
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

            if (profile.MinPitch >= profile.MaxPitch)
            {
                return CCS_SurvivalValidationResult.Fail("minPitch must be less than maxPitch.");
            }

            if (profile.ZoomDistanceMin <= 0f || profile.ZoomDistanceMax < profile.ZoomDistanceMin)
            {
                return CCS_SurvivalValidationResult.Fail("Zoom distance range is invalid.");
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
            CCS_CharacterControllerDebugHud debugHud = prefabRoot.GetComponent<CCS_CharacterControllerDebugHud>();

            AppendIfMissing(failures, unityCharacterController != null, "Prefab missing CharacterController.");
            AppendIfMissing(failures, motor != null, "Prefab missing CCS_CharacterMotor.");
            AppendIfMissing(failures, inputProvider != null, "Prefab missing CCS_CharacterInputActionProvider.");
            AppendIfMissing(failures, cameraController != null, "Prefab missing CCS_CharacterCameraController.");
            AppendIfMissing(failures, service != null, "Prefab missing CCS_CharacterControllerService.");
            AppendIfMissing(failures, debugHud != null, "Prefab missing CCS_CharacterControllerDebugHud.");

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
                else if (motor.MovementProfile.JumpEnabled)
                {
                    failures.Add("Jump must be disabled by default.");
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
                    CinemachineThirdPersonFollow follow =
                        cameraController.CinemachineCamera.GetComponent<CinemachineThirdPersonFollow>();
                    if (follow == null)
                    {
                        failures.Add("CinemachineCamera is missing CinemachineThirdPersonFollow.");
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

        #endregion
    }
}
