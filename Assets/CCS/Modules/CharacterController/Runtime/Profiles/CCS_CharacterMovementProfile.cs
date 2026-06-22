using CCS.Project;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_CharacterMovementProfile
// CATEGORY: Modules / CharacterController / Runtime / Profiles
// PURPOSE: Profile-driven movement tuning for the character controller module.
// PLACEMENT: ScriptableObject asset under Profiles/Movement/.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Configuration only. Jump enabled on default profile for controller test players.
// =============================================================================

namespace CCS.Modules.CharacterController
{
    [CreateAssetMenu(
        fileName = "CCS_CharacterMovementProfile",
        menuName = "CCS/Character Controller/Movement Profile",
        order = 0)]
    public sealed class CCS_CharacterMovementProfile : CCS_SurvivalProfileBase
    {
        #region Variables

        [Header("Movement Speeds")]
        [Tooltip("Base walk speed in meters per second.")]
        [SerializeField] private float walkSpeed = 4f;

        [Tooltip("Sprint speed in meters per second.")]
        [SerializeField] private float sprintSpeed = 6f;

        [Header("Acceleration")]
        [Tooltip("Acceleration toward target speed.")]
        [SerializeField] private float acceleration = 12f;

        [Tooltip("Deceleration when input is released.")]
        [SerializeField] private float deceleration = 16f;

        [Header("Gravity And Jump")]
        [Tooltip("Gravity applied when airborne.")]
        [SerializeField] private float gravity = -20f;

        [Tooltip("When false, jump input is ignored.")]
        [SerializeField] private bool jumpEnabled = true;

        [Tooltip("Jump height in meters when jump is enabled.")]
        [SerializeField] private float jumpHeight = 1.25f;

        [Tooltip("Seconds after leaving ground where jump is still allowed.")]
        [SerializeField] private float coyoteTime = 0.1f;

        [Tooltip("Seconds to buffer jump input before landing.")]
        [SerializeField] private float jumpBufferTime = 0.1f;

        [Header("Rotation")]
        [Tooltip("Maximum rotation speed in degrees per second.")]
        [SerializeField] private float rotationSmoothing = 540f;

        [Header("Air Control")]
        [Tooltip("Airborne movement acceleration multiplier.")]
        [SerializeField] private float airControl = 0.35f;

        [Header("Aim Strafe Locomotion")]
        [Tooltip("Walk speed multiplier while aiming.")]
        [SerializeField] private float aimMovementSpeedMultiplier = 0.55f;

        [Tooltip("Body rotation speed toward aim yaw while aiming, in degrees per second.")]
        [SerializeField] private float aimRotationSpeedDegrees = 720f;

        [Tooltip("When true, sprint input is ignored while aim movement is active.")]
        [SerializeField] private bool aimDisableSprint = true;

        [Tooltip("Move input magnitude below this is treated as idle while aiming.")]
        [SerializeField] private float aimStrafeDeadZone = 0.05f;

        [Tooltip("Backward aim move speed multiplier applied on top of aimMovementSpeedMultiplier.")]
        [SerializeField] private float aimBackpedalMultiplier = 0.80f;

        [Tooltip("Side strafe speed multiplier applied on top of aimMovementSpeedMultiplier.")]
        [SerializeField] private float aimSideStrafeMultiplier = 0.90f;

        #endregion

        #region Properties

        public float WalkSpeed => walkSpeed;

        public float SprintSpeed => sprintSpeed;

        public float Acceleration => acceleration;

        public float Deceleration => deceleration;

        public float Gravity => gravity;

        public bool JumpEnabled => jumpEnabled;

        public float JumpHeight => jumpHeight;

        public float CoyoteTime => coyoteTime;

        public float JumpBufferTime => jumpBufferTime;

        public float RotationSmoothing => rotationSmoothing;

        public float AirControl => airControl;

        public float AimMovementSpeedMultiplier => aimMovementSpeedMultiplier;

        public float AimRotationSpeedDegrees => aimRotationSpeedDegrees;

        public bool AimDisableSprint => aimDisableSprint;

        public float AimStrafeDeadZone => aimStrafeDeadZone;

        public float AimBackpedalMultiplier => aimBackpedalMultiplier;

        public float AimSideStrafeMultiplier => aimSideStrafeMultiplier;

        #endregion
    }
}
