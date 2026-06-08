using CCS.Project;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_CharacterMovementProfile
// CATEGORY: Modules / CharacterController / Runtime / Profiles
// PURPOSE: Profile-driven movement tuning for the character controller module.
// PLACEMENT: ScriptableObject asset under Profiles/Movement/.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Configuration only. Jump disabled by default in v0.2.0.
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
        [SerializeField] private bool jumpEnabled;

        [Tooltip("Jump height in meters when jump is enabled.")]
        [SerializeField] private float jumpHeight = 1.2f;

        [Header("Rotation")]
        [Tooltip("Maximum rotation speed in degrees per second.")]
        [SerializeField] private float rotationSmoothing = 540f;

        [Header("Air Control")]
        [Tooltip("Placeholder air control multiplier.")]
        [SerializeField] private float airControl = 0.25f;

        #endregion

        #region Properties

        public float WalkSpeed => walkSpeed;

        public float SprintSpeed => sprintSpeed;

        public float Acceleration => acceleration;

        public float Deceleration => deceleration;

        public float Gravity => gravity;

        public bool JumpEnabled => jumpEnabled;

        public float JumpHeight => jumpHeight;

        public float RotationSmoothing => rotationSmoothing;

        public float AirControl => airControl;

        #endregion
    }
}
