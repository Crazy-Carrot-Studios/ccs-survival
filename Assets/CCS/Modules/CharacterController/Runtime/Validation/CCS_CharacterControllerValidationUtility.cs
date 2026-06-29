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

namespace CCS.Modules.CharacterController.Validation {
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
            AppendIfMissing(failures, File.Exists(CCS_CharacterControllerConstants.DefaultCameraProfilePath), "Third-person survival camera profile missing.");
            AppendIfMissing(failures, File.Exists(CCS_CharacterControllerConstants.FirstPersonBodyAwareCameraProfilePath), "First-person body-aware camera profile missing.");
            AppendIfMissing(failures, File.Exists(CCS_CharacterControllerConstants.FirstPersonAimCameraProfilePath), "First-person aim camera profile missing.");
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

            if (profile.VerticalOrbitMin >= profile.VerticalOrbitMax)
            {
                return CCS_SurvivalValidationResult.Fail("verticalOrbitMin must be less than verticalOrbitMax.");
            }

            if (profile.TrackingTargetLocalHeight < CCS_CharacterControllerConstants.CameraPitchTargetMinimumLocalHeight
                || profile.TrackingTargetLocalHeight > CCS_CharacterControllerConstants.CameraPitchTargetMaximumLocalHeight)
            {
                return CCS_SurvivalValidationResult.Fail(
                    "Tracking target local height must be between 1.40 and 1.60.");
            }

            if (profile.CameraMode == CCS_CharacterCameraMode.ThirdPersonSurvival)
            {
                if (profile.ThirdPersonCameraDistance < CCS_CharacterControllerConstants.ThirdPersonCameraDistanceMinimum
                    || profile.ThirdPersonCameraDistance > CCS_CharacterControllerConstants.ThirdPersonCameraDistanceMaximum)
                {
                    return CCS_SurvivalValidationResult.Fail(
                        "Third-person camera distance must be between 2.85 and 3.25.");
                }
            }

            if (profile.CameraMode == CCS_CharacterCameraMode.FirstPersonBodyAware
                || profile.CameraMode == CCS_CharacterCameraMode.FirstPerson)
            {
                if (profile.FirstPersonForwardEyeOffset < CCS_CharacterControllerConstants.FirstPersonForwardEyeOffsetMinimum
                    || profile.FirstPersonForwardEyeOffset > CCS_CharacterControllerConstants.FirstPersonForwardEyeOffsetMaximum)
                {
                    return CCS_SurvivalValidationResult.Fail(
                        "First-person forward eye offset must stay in front of the face (0.08 to 0.14).");
                }

                if (profile.FieldOfView < CCS_CharacterControllerConstants.FirstPersonFieldOfViewMinimum
                    || profile.FieldOfView > CCS_CharacterControllerConstants.FirstPersonFieldOfViewMaximum)
                {
                    return CCS_SurvivalValidationResult.Fail(
                        "First-person field of view must be between 65 and 75.");
                }

                if (profile.NearClipPlane < CCS_CharacterControllerConstants.FirstPersonNearClipMinimum
                    || profile.NearClipPlane > CCS_CharacterControllerConstants.FirstPersonNearClipMaximum)
                {
                    return CCS_SurvivalValidationResult.Fail(
                        "First-person near clip must be between 0.02 and 0.05.");
                }
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
                    "Camera profile set default must be ThirdPersonSurvival in v0.6.9.");
            }

            if (profileSet.ThirdPersonSurvivalProfile == null)
            {
                return CCS_SurvivalValidationResult.Fail(
                    "Camera profile set must retain ThirdPersonSurvival profile for future mode switching.");
            }

            if (profileSet.AimOverShoulderProfile == null)
            {
                return CCS_SurvivalValidationResult.Fail(
                    "Camera profile set must retain AimOverShoulder profile for future mode switching.");
            }

            return CCS_SurvivalValidationResult.Pass("Camera profile set validated.");
        }

        public static CCS_SurvivalValidationResult ValidateFirstPersonBodyAwareCameraFoundation()
        {
            List<string> failures = new List<string>();
            string controllerPath = CCS_CharacterControllerConstants.ModuleRootPath
                + "/Runtime/Components/CCS_CharacterCameraController.cs";
            string visibilityPath = CCS_CharacterControllerConstants.ModuleRootPath
                + "/Runtime/Components/CCS_LocalFirstPersonHeadVisibility.cs";
            string layerUtilityPath = CCS_CharacterControllerConstants.ModuleRootPath
                + "/Runtime/Components/CCS_CharacterCameraLayerUtility.cs";
            string followAnchorPath = CCS_CharacterControllerConstants.ModuleRootPath
                + "/Runtime/Components/CCS_CharacterCameraFollowAnchor.cs";
            string motorPath = CCS_CharacterControllerConstants.ModuleRootPath
                + "/Runtime/Components/CCS_CharacterMotor.cs";
            string rigBuilderPath = CCS_CharacterControllerConstants.ModuleRootPath
                + "/Editor/CCS_CharacterCameraRigInputBuilder.cs";
            string headTrackerPath = CCS_CharacterControllerConstants.ModuleRootPath
                + "/Runtime/Components/CCS_FirstPersonBodyCameraAnchor.cs";
            string prefabPath = CCS_CharacterControllerConstants.TestPrefabPath;

            AppendIfMissing(
                failures,
                File.Exists(CCS_CharacterControllerConstants.FirstPersonBodyAwareCameraProfilePath),
                "Missing CCS_CharacterCameraProfile_FirstPersonBodyAware.asset.");
            AppendIfMissing(
                failures,
                File.Exists(visibilityPath),
                "Missing CCS_LocalFirstPersonHeadVisibility.");
            AppendIfMissing(
                failures,
                File.Exists(layerUtilityPath),
                "Missing CCS_CharacterCameraLayerUtility runtime helper.");

            if (File.Exists(controllerPath))
            {
                string controllerSource = File.ReadAllText(controllerPath);
                AppendIfMissing(
                    failures,
                    controllerSource.Contains("ThirdPersonSurvival"),
                    "CCS_CharacterCameraController must default to ThirdPersonSurvival in v0.6.9.");
                AppendIfMissing(
                    failures,
                    controllerSource.Contains("AimOverShoulder"),
                    "CCS_CharacterCameraController must switch to AimOverShoulder while firearm aiming.");
                AppendIfMissing(
                    failures,
                    controllerSource.Contains("WantsAimOverShoulderCamera"),
                    "CCS_CharacterCameraController must use WantsAimOverShoulderCamera from carry state.");
                AppendIfMissing(
                    failures,
                    controllerSource.Contains("AimCameraActivePriority"),
                    "CCS_CharacterCameraController must raise CinemachineCamera_Aim priority during third-person aim.");
                AppendIfMissing(
                    failures,
                    controllerSource.Contains("DeactivateLegacyFirstPersonAimCamera"),
                    "CCS_CharacterCameraController must deactivate legacy CinemachineCamera_FP_Aim.");
                AppendIfMissing(
                    failures,
                    controllerSource.Contains("BindCarryStateSourceFromPlayer"),
                    "CCS_CharacterCameraController must bind weapon carry state from the local player.");
                AppendIfMissing(
                    failures,
                    controllerSource.Contains("CCS_IWeaponCarryStateCameraSource"),
                    "CCS_CharacterCameraController must subscribe to weapon carry state for local camera switching.");
                AppendIfMissing(
                    failures,
                    controllerSource.Contains("debugCameraModeTransitions"),
                    "CCS_CharacterCameraController must expose debugCameraModeTransitions (default off).");
                AppendIfMissing(
                    failures,
                    !controllerSource.Contains("enableRuntimeCameraDebug = true"),
                    "enableRuntimeCameraDebug must default off in source.");
            }

            string carryStatePath = CCS_CharacterControllerConstants.ModuleRootPath
                + "/../Weapons/Runtime/Data/CCS_WeaponCarryState.cs";
            string carryControllerPath = CCS_CharacterControllerConstants.ModuleRootPath
                + "/../Weapons/Runtime/Components/CCS_WeaponCarryStateController.cs";
            string aimLocomotionPath = CCS_CharacterControllerConstants.ModuleRootPath
                + "/Runtime/Components/CCS_CharacterAimLocomotionController.cs";
            AppendIfMissing(failures, File.Exists(carryStatePath), "Missing CCS_WeaponCarryState enum.");
            AppendIfMissing(failures, File.Exists(carryControllerPath), "Missing CCS_WeaponCarryStateController.");
            if (File.Exists(carryControllerPath))
            {
                string carryControllerSource = File.ReadAllText(carryControllerPath);
                AppendIfMissing(
                    failures,
                    carryControllerSource.Contains("CCS_IWeaponCarryStateCameraSource"),
                    "CCS_WeaponCarryStateController must implement CCS_IWeaponCarryStateCameraSource.");
                AppendIfMissing(
                    failures,
                    carryControllerSource.Contains("ShouldDriveLocalCameraInternal"),
                    "CCS_WeaponCarryStateController must gate local camera ownership for solo/host/client.");
            }

            string loadoutPath = CCS_CharacterControllerConstants.ModuleRootPath
                + "/../Weapons/Runtime/Components/CCS_PlayerWeaponLoadout.cs";
            if (File.Exists(loadoutPath))
            {
                string loadoutSource = File.ReadAllText(loadoutPath);
                AppendIfMissing(
                    failures,
                    !loadoutSource.Contains("CCS_IWeaponAimGate"),
                    "CCS_PlayerWeaponLoadout must not implement CCS_IWeaponAimGate; carry state owns aim routing.");
            }

            if (File.Exists(aimLocomotionPath))
            {
                string aimLocomotionSource = File.ReadAllText(aimLocomotionPath);
                AppendIfMissing(
                    failures,
                    aimLocomotionSource.Contains("CCS_IWeaponCarryStateCameraSource"),
                    "CCS_CharacterAimLocomotionController must resolve weapon carry state for combat locomotion.");
            }

            if (File.Exists(followAnchorPath))
            {
                string followAnchorSource = File.ReadAllText(followAnchorPath);
                AppendIfMissing(
                    failures,
                    followAnchorSource.Contains("FirstPersonBodyAware"),
                    "CCS_CharacterCameraFollowAnchor must couple body yaw during FirstPersonBodyAware aim.");
                AppendIfMissing(
                    failures,
                    followAnchorSource.Contains("bodyRoot.rotation = Quaternion.Euler(0f, yawDegrees, 0f)")
                        || (followAnchorSource.Contains("bodyRoot.rotation = Quaternion.Euler(0f,")
                            && followAnchorSource.Contains("nextBodyYaw, 0f)")),
                    "First-person yaw must rotate the player body.");
                AppendIfMissing(
                    failures,
                    followAnchorSource.Contains("pitchTarget.localRotation = Quaternion.Euler(pitchDegrees, 0f, 0f)"),
                    "First-person pitch must remain camera-only on CameraPitchTarget.");
            }

            if (File.Exists(motorPath))
            {
                string motorSource = File.ReadAllText(motorPath);
                AppendIfMissing(
                    failures,
                    motorSource.Contains("ApplyFirstPersonMovement"),
                    "CCS_CharacterMotor must support first-person locomotion without third-person rotation lag.");
                AppendIfMissing(
                    failures,
                    motorSource.Contains("ApplyFirstPersonAimMovement"),
                    "CCS_CharacterMotor must route first-person aim through body-yaw-coupled locomotion.");
                AppendIfMissing(
                    failures,
                    motorSource.Contains("skipBodyRotation: true"),
                    "First-person locomotion must not apply motor body rotation lag.");
            }

            if (File.Exists(headTrackerPath))
            {
                string headTrackerSource = File.ReadAllText(headTrackerPath);
                AppendIfMissing(
                    failures,
                    headTrackerSource.Contains("GetBoneTransform(HumanBodyBones.Head)"),
                    "CCS_FirstPersonBodyCameraAnchor must resolve the humanoid head bone.");
                AppendIfMissing(
                    failures,
                    headTrackerSource.Contains("InheritHeadBoneRotation"),
                    "CCS_FirstPersonBodyCameraAnchor must keep inheritHeadBoneRotation disabled by default.");
                AppendIfMissing(
                    failures,
                    !headTrackerSource.Contains("headBone.rotation"),
                    "Head-tracked first-person camera must not copy animated head rotation by default.");
            }

            if (File.Exists(visibilityPath))
            {
                string visibilitySource = File.ReadAllText(visibilityPath);
                AppendIfMissing(
                    failures,
                    !visibilitySource.Contains("renderer.enabled"),
                    "CCS_LocalFirstPersonHeadVisibility must not globally disable head renderers.");
                AppendIfMissing(
                    failures,
                    visibilitySource.Contains("LocalSelfHeadHiddenLayerName"),
                    "CCS_LocalFirstPersonHeadVisibility must use CCS_LocalSelfHeadHidden layer masking.");
                AppendIfMissing(
                    failures,
                    visibilitySource.Contains("LocalFirstPersonBodyLayerName"),
                    "CCS_LocalFirstPersonHeadVisibility must use CCS_LocalFirstPersonBody for headless fallback.");
                AppendIfMissing(
                    failures,
                    visibilitySource.Contains("CombinedBodyHeadlessFallback"),
                    "CCS_LocalFirstPersonHeadVisibility must support combined-body headless fallback.");
                AppendIfMissing(
                    failures,
                    visibilitySource.Contains("CCS_LocalFirstPersonHeadMaskMode"),
                    "CCS_LocalFirstPersonHeadVisibility must report SeparateRendererMask vs CombinedBodyHeadlessFallback.");
                AppendIfMissing(
                    failures,
                    visibilitySource.Contains("headlessBodyRenderer"),
                    "CCS_LocalFirstPersonHeadVisibility must reference local headless body renderer.");
                AppendIfMissing(
                    failures,
                    visibilitySource.Contains("IsLocalOwner"),
                    "CCS_LocalFirstPersonHeadVisibility must gate masking to local owner only.");
                AppendIfMissing(
                    failures,
                    !visibilitySource.Contains("HideFullBody") && !visibilitySource.Contains("torsoRenderers"),
                    "CCS_LocalFirstPersonHeadVisibility must not hide torso/arms/legs/feet.");
                AppendIfMissing(
                    failures,
                    visibilitySource.Contains("ProtectedNameTokens"),
                    "CCS_LocalFirstPersonHeadVisibility must protect weapon/hand renderers from masking.");
            }

            string headlessMeshBuilderPath = CCS_CharacterControllerConstants.ModuleRootPath
                + "/Editor/CCS_FirstPersonHeadlessBodyMeshBuilder.cs";
            AppendIfMissing(
                failures,
                File.Exists(headlessMeshBuilderPath),
                "Missing CCS_FirstPersonHeadlessBodyMeshBuilder editor utility.");
            AppendIfMissing(
                failures,
                File.Exists(CCS_CharacterControllerConstants.FirstPersonHeadlessBodyMeshAssetPath),
                "Missing baked CCS_CC3_FirstPerson_HeadlessBody mesh asset for combined body fallback.");

            if (File.Exists(CCS_CharacterControllerConstants.DefaultCameraProfileSetPath))
            {
                string profileSetText = File.ReadAllText(CCS_CharacterControllerConstants.DefaultCameraProfileSetPath);
                AppendIfMissing(
                    failures,
                    profileSetText.Contains("guid: 007dc032c141ed74293f8742e2b02f63"),
                    "Default camera profile set must reference ThirdPersonSurvival as defaultProfile.");
            }

            if (File.Exists(rigBuilderPath))
            {
                string rigBuilderSource = File.ReadAllText(rigBuilderPath);
                AppendIfMissing(
                    failures,
                    rigBuilderSource.Contains("trackerSettings.PositionDamping = Vector3.zero"),
                    "First-person Cinemachine follow must use zero position damping.");
                AppendIfMissing(
                    failures,
                    rigBuilderSource.Contains("RemoveLegacyFirstPersonAimCamera"),
                    "Camera rig builder must remove legacy CinemachineCamera_FP_Aim from the active rig.");
            }

            if (File.Exists(CCS_CharacterControllerConstants.FirstPersonBodyAwareCameraProfilePath))
            {
                string profileText = File.ReadAllText(
                    CCS_CharacterControllerConstants.FirstPersonBodyAwareCameraProfilePath);
                AppendIfMissing(
                    failures,
                    profileText.Contains("headTrackedLocalOffset: {x: 0, y: 0.06, z: 0.1}"),
                    "FirstPersonBodyAware head-tracked offset must sit closer to eye position (v0.6.14: 0.06 / 0.10).");
                AppendIfMissing(
                    failures,
                    !profileText.Contains("headTrackedLocalOffset: {x: 0, y: 0.04, z: 0.18}"),
                    "FirstPersonBodyAware must not retain pre-v0.6.14 hand-forward head-tracked offset.");
                AppendIfMissing(
                    failures,
                    profileText.Contains("lookSmoothing: 0")
                        && profileText.Contains("followDampingX: 0"),
                    "FirstPersonBodyAware profile must use zero damping and look smoothing.");
                AppendIfMissing(
                    failures,
                    profileText.Contains("verticalOrbitMin: -53"),
                    "FirstPersonBodyAware pitch min must clamp look-down before torso clipping (v0.6.12: -53).");
                AppendIfMissing(
                    failures,
                    !profileText.Contains("verticalOrbitMin: -58"),
                    "FirstPersonBodyAware must not retain legacy -58 look-down clamp.");
                AppendIfMissing(
                    failures,
                    !profileText.Contains("verticalOrbitMin: -50"),
                    "v0.6.14 reverted v0.6.13 pitch tweak; BodyAware must use -53 look-down clamp.");
                AppendIfMissing(
                    failures,
                    profileText.Contains("useHeadTrackedAnchor: 1"),
                    "FirstPersonBodyAware must enable head-tracked camera anchor.");
                AppendIfMissing(
                    failures,
                    profileText.Contains("inheritHeadBoneRotation: 0"),
                    "FirstPersonBodyAware must not inherit head bone rotation by default.");
            }

            string revolverAimRigPath = CCS_CharacterControllerConstants.ModuleRootPath
                + "/../Weapons/Runtime/Aiming/CCS_RevolverReticleAimRig.cs";
            AppendIfMissing(
                failures,
                !File.Exists(revolverAimRigPath),
                "v0.6.13 direct weapon reticle aim rig must not return (CCS_RevolverReticleAimRig).");

            if (File.Exists(prefabPath))
            {
                string prefabText = File.ReadAllText(prefabPath);
                AppendIfMissing(
                    failures,
                    !prefabText.Contains("CCS_RevolverReticleAimRig"),
                    prefabPath + " must not include v0.6.13 CCS_RevolverReticleAimRig.");
                AppendIfMissing(
                    failures,
                    !prefabText.Contains("m_Name: Rig_RevolverAim"),
                    prefabPath + " must not retain v0.6.13 Rig_RevolverAim.");
                AppendIfMissing(
                    failures,
                    prefabText.Contains("m_Name: " + CCS_CharacterControllerConstants.FirstPersonCameraAnchorObjectName),
                    prefabPath + " must contain FirstPersonCameraAnchor under CameraPitchTarget.");
                AppendIfMissing(
                    failures,
                    !prefabText.Contains("m_Name: " + CCS_CharacterControllerConstants.FirstPersonAimCameraAnchorObjectName),
                    prefabPath + " must not require FirstPersonAimCameraAnchor for runtime aim.");
                AppendIfMissing(
                    failures,
                    prefabText.Contains(CCS_CharacterControllerConstants.LocalFirstPersonHeadVisibilityTypeName),
                    prefabPath + " must include CCS_LocalFirstPersonHeadVisibility.");
                AppendIfMissing(
                    failures,
                    !prefabText.Contains("CCS_FirstPersonBodyVisibilityController"),
                    prefabPath + " must not retain global renderer-disable CCS_FirstPersonBodyVisibilityController.");
                AppendIfMissing(
                    failures,
                    prefabText.Contains(CCS_CharacterControllerConstants.FirstPersonBodyCameraAnchorTypeName),
                    prefabPath + " must include CCS_FirstPersonBodyCameraAnchor.");
                AppendIfMissing(
                    failures,
                    prefabText.Contains("CCS_WeaponCarryStateController"),
                    prefabPath + " must include CCS_WeaponCarryStateController for carry/aim visual sync.");
                AppendIfMissing(
                    failures,
                    prefabText.Contains("weaponAimGateComponent: {fileID: 4076866618874431801}"),
                    prefabPath + " weaponAimGateComponent must reference CCS_WeaponCarryStateController.");
                AppendIfMissing(
                    failures,
                    prefabText.Contains(CCS_CharacterControllerConstants.FirstPersonHeadlessBodyObjectName),
                    prefabPath + " must include CCS_FirstPersonHeadlessBody headless renderer.");
                AppendIfMissing(
                    failures,
                    prefabText.Contains(CCS_CharacterControllerConstants.FirstPersonHeadlessBodyMeshAssetName),
                    prefabPath + " must reference baked CCS_CC3_FirstPerson_HeadlessBody mesh asset.");
            }

            string cameraRigPath = CCS_CharacterControllerConstants.CameraRigPrefabPath;
            if (File.Exists(cameraRigPath))
            {
                string rigText = File.ReadAllText(cameraRigPath);
                AppendIfMissing(
                    failures,
                    rigText.Contains("m_Name: " + CCS_CharacterControllerConstants.FirstPersonBodyAwareCinemachineCameraName),
                    cameraRigPath + " must contain CinemachineCamera_FP_BodyAware.");
                AppendIfMissing(
                    failures,
                    !rigText.Contains("m_Name: " + CCS_CharacterControllerConstants.FirstPersonAimCinemachineCameraName),
                    cameraRigPath + " must not retain active CinemachineCamera_FP_Aim.");
                AppendIfMissing(
                    failures,
                    rigText.Contains("m_Name: " + CCS_CharacterControllerConstants.ThirdPersonCinemachineCameraName),
                    cameraRigPath + " must retain CinemachineCamera_TP.");
                AppendIfMissing(
                    failures,
                    rigText.Contains("m_Name: " + CCS_CharacterControllerConstants.AimCinemachineCameraName),
                    cameraRigPath + " must retain CinemachineCamera_Aim.");
                AppendIfMissing(
                    failures,
                    rigText.Contains("PositionDamping: {x: 0, y: 0, z: 0}"),
                    cameraRigPath + " first-person CinemachineFollow must use zero position damping.");
            }

            string weaponsResolverPath = CCS_CharacterControllerConstants.ModuleRootPath
                + "/../Weapons/Runtime/Aiming/CCS_WeaponAimResolver.cs";
            if (File.Exists(weaponsResolverPath))
            {
                string resolverSource = File.ReadAllText(weaponsResolverPath);
                AppendIfMissing(
                    failures,
                    resolverSource.Contains("CCS_WeaponAimResolver"),
                    "CCS_WeaponAimResolver must remain available for reticle/tracer alignment.");
            }

            return failures.Count > 0
                ? CCS_SurvivalValidationResult.Fail(string.Join(" ", failures))
                : CCS_SurvivalValidationResult.Pass(
                    "Third-person default, BodyAware-only FirstPersonAim routing, separated head mask, combined-body headless fallback, and carry-state camera switching validated.");
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
