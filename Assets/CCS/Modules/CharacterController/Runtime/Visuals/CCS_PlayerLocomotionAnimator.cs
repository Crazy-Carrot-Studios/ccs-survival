using UnityEngine;

// =============================================================================
// SCRIPT: CCS_PlayerLocomotionAnimator
// CATEGORY: Modules / CharacterController / Runtime / Visuals
// PURPOSE: Drives CC4 locomotion Animator parameters from CCS_CharacterMotor state.
// PLACEMENT: PF_CCS_CharacterController_TestPlayer_Networked / VisualRoot.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Visual-only bridge. Starter Assets locomotion + jump. No root motion.
// =============================================================================

namespace CCS.Modules.CharacterController
{
    [DefaultExecutionOrder(200)]
    public sealed class CCS_PlayerLocomotionAnimator : MonoBehaviour
    {
        #region Variables

        private const float JumpVerticalVelocityThreshold = 0.5f;
        private const float AimMoveParameterSmoothSpeed = 12f;

        [SerializeField] private Animator animator;
        [SerializeField] private CCS_CharacterMotor motor;

        private bool loggedMissingController;
        private bool loggedFallbackAnimator;
        private float previousVerticalVelocity;
        private float smoothedAimMoveX;
        private float smoothedAimMoveY;

        #endregion

        #region Unity Callbacks

        private void Awake()
        {
            ResolveReferences();
        }

        private void LateUpdate()
        {
            if (motor == null)
            {
                return;
            }

            if (!TryResolveAnimator(out Animator resolvedAnimator))
            {
                return;
            }

            CCS_CharacterMovementProfile profile = motor.MovementProfile;
            float sprintSpeed = profile != null ? profile.SprintSpeed : 0f;
            float speedNormalized = sprintSpeed > 0f
                ? Mathf.Clamp01(motor.CurrentSpeed / sprintSpeed)
                : 0f;

            float verticalVelocity = motor.VerticalVelocity;

            resolvedAnimator.SetFloat(CCS_PlayerAnimatorParameterIds.SpeedNormalized, speedNormalized);
            resolvedAnimator.SetBool(CCS_PlayerAnimatorParameterIds.IsGrounded, motor.IsGrounded);
            resolvedAnimator.SetBool(CCS_PlayerAnimatorParameterIds.IsSprinting, motor.IsSprinting);

            bool isAimingMovement = motor.IsAimMovementActive;
            resolvedAnimator.SetBool(CCS_PlayerAnimatorParameterIds.IsAimingMovementMode, isAimingMovement);

            Vector2 aimMoveInput = motor.AimMoveInput;
            float smoothFactor = Time.deltaTime * AimMoveParameterSmoothSpeed;
            smoothedAimMoveX = Mathf.Lerp(smoothedAimMoveX, aimMoveInput.x, smoothFactor);
            smoothedAimMoveY = Mathf.Lerp(smoothedAimMoveY, aimMoveInput.y, smoothFactor);
            resolvedAnimator.SetFloat(
                CCS_PlayerAnimatorParameterIds.AimMoveX,
                isAimingMovement ? smoothedAimMoveX : 0f);
            resolvedAnimator.SetFloat(
                CCS_PlayerAnimatorParameterIds.AimMoveY,
                isAimingMovement ? smoothedAimMoveY : 0f);

            if (previousVerticalVelocity <= 0f
                && verticalVelocity > JumpVerticalVelocityThreshold)
            {
                resolvedAnimator.SetTrigger(CCS_PlayerAnimatorParameterIds.JumpTrigger);
            }

            previousVerticalVelocity = verticalVelocity;
        }

        #endregion

        #region Private Methods

        private void ResolveReferences()
        {
            if (motor == null)
            {
                CCS_PlayerRuntimeFacade facade = GetComponentInParent<CCS_PlayerRuntimeFacade>(true);
                motor = facade != null ? facade.Motor : GetComponentInParent<CCS_CharacterMotor>();
            }

            TryResolveAnimator(out _);
        }

        private bool TryResolveAnimator(out Animator resolvedAnimator)
        {
            if (animator != null && CCS_PlayerAnimatorResolver.IsAuthoritativeGameplayAnimator(animator))
            {
                resolvedAnimator = animator;
                return true;
            }

            CCS_PlayerRuntimeFacade facade = GetComponentInParent<CCS_PlayerRuntimeFacade>(true);
            if (facade != null
                && facade.Animator != null
                && CCS_PlayerAnimatorResolver.IsAuthoritativeGameplayAnimator(facade.Animator))
            {
                animator = facade.Animator;
                resolvedAnimator = animator;
                loggedMissingController = false;
                return true;
            }

            if (CCS_PlayerAnimatorResolver.TryResolveAuthoritativeAnimator(
                    transform,
                    out resolvedAnimator,
                    out bool usedFallback))
            {
                animator = resolvedAnimator;
                loggedMissingController = false;
                if (usedFallback && !loggedFallbackAnimator)
                {
                    loggedFallbackAnimator = true;
                    Debug.LogWarning(
                        "[CCS Player Locomotion Animator] Used fallback authoritative Animator resolution.",
                        this);
                }

                return true;
            }

            resolvedAnimator = null;

            if (!loggedMissingController)
            {
                loggedMissingController = true;
                Debug.LogWarning(
                    "[CCS Player Locomotion Animator] No authoritative humanoid Animator was found. Visual locomotion parameters were skipped.",
                    this);
            }

            return false;
        }

        #endregion
    }
}
