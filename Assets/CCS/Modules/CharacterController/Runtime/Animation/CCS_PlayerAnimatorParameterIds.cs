using UnityEngine;

// =============================================================================
// SCRIPT: CCS_PlayerAnimatorParameterIds
// CATEGORY: Modules / CharacterController / Runtime / Animation
// PURPOSE: Hashed Animator parameter IDs matching AC_CCS_Player_Locomotion_StarterAssets.
// PLACEMENT: Runtime constants. Used by locomotion, interaction, and revolver animators.
// AUTHOR: James Schilz
// CREATED: 2026-06-28
// NOTES: v0.8.1 — single source of truth for controller parameter names.
// =============================================================================

namespace CCS.Modules.CharacterController
{
    public static class CCS_PlayerAnimatorParameterIds
    {
        public const string SpeedNormalizedName = "SpeedNormalized";
        public const string IsGroundedName = "IsGrounded";
        public const string IsSprintingName = "IsSprinting";
        public const string JumpTriggerName = "JumpTrigger";
        public const string PickUpRightHandTriggerName = "PickUp_RH";
        public const string WalkThroughDoorRightHandTriggerName = "WalkThroughDoor_RH";
        public const string IsAimingMovementModeName = "IsAimingMovementMode";
        public const string AimMoveXName = "AimMoveX";
        public const string AimMoveYName = "AimMoveY";
        public const string RevolverAimHeldName = "RevolverAimHeld";
        public const string RevolverFireTriggerName = "RevolverFireTrigger";
        public const string RevolverReloadTriggerName = "RevolverReloadTrigger";
        public const string RevolverIsReloadingName = "RevolverIsReloading";
        public const string RevolverIsMovingName = "RevolverIsMoving";
        public const string RevolverAimPitchName = "RevolverAimPitch";

        public static readonly int SpeedNormalized = Animator.StringToHash(SpeedNormalizedName);
        public static readonly int IsGrounded = Animator.StringToHash(IsGroundedName);
        public static readonly int IsSprinting = Animator.StringToHash(IsSprintingName);
        public static readonly int JumpTrigger = Animator.StringToHash(JumpTriggerName);
        public static readonly int PickUpRightHandTrigger = Animator.StringToHash(PickUpRightHandTriggerName);
        public static readonly int WalkThroughDoorRightHandTrigger =
            Animator.StringToHash(WalkThroughDoorRightHandTriggerName);
        public static readonly int IsAimingMovementMode = Animator.StringToHash(IsAimingMovementModeName);
        public static readonly int AimMoveX = Animator.StringToHash(AimMoveXName);
        public static readonly int AimMoveY = Animator.StringToHash(AimMoveYName);
        public static readonly int RevolverAimHeld = Animator.StringToHash(RevolverAimHeldName);
        public static readonly int RevolverFireTrigger = Animator.StringToHash(RevolverFireTriggerName);
        public static readonly int RevolverReloadTrigger = Animator.StringToHash(RevolverReloadTriggerName);
        public static readonly int RevolverIsReloading = Animator.StringToHash(RevolverIsReloadingName);
        public static readonly int RevolverIsMoving = Animator.StringToHash(RevolverIsMovingName);
        public static readonly int RevolverAimPitch = Animator.StringToHash(RevolverAimPitchName);

        public static readonly string[] RequiredControllerParameterNames =
        {
            SpeedNormalizedName,
            IsGroundedName,
            IsSprintingName,
            JumpTriggerName,
            PickUpRightHandTriggerName,
            WalkThroughDoorRightHandTriggerName,
            IsAimingMovementModeName,
            AimMoveXName,
            AimMoveYName,
            RevolverAimHeldName,
            RevolverFireTriggerName,
            RevolverReloadTriggerName,
            RevolverIsReloadingName,
        };

        public static readonly string[] OptionalControllerParameterNames =
        {
            RevolverIsMovingName,
            RevolverAimPitchName,
        };
    }
}
