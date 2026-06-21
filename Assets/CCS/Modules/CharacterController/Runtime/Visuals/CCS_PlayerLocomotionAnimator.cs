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

        private static readonly int SpeedNormalizedHash = Animator.StringToHash("SpeedNormalized");
        private static readonly int IsGroundedHash = Animator.StringToHash("IsGrounded");
        private static readonly int IsSprintingHash = Animator.StringToHash("IsSprinting");
        private static readonly int JumpTriggerHash = Animator.StringToHash("JumpTrigger");

        [SerializeField] private Animator animator;
        [SerializeField] private CCS_CharacterMotor motor;

        private bool loggedMissingController;
        private float previousVerticalVelocity;

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

            resolvedAnimator.SetFloat(SpeedNormalizedHash, speedNormalized);
            resolvedAnimator.SetBool(IsGroundedHash, motor.IsGrounded);
            resolvedAnimator.SetBool(IsSprintingHash, motor.IsSprinting);

            if (previousVerticalVelocity <= 0f
                && verticalVelocity > JumpVerticalVelocityThreshold)
            {
                resolvedAnimator.SetTrigger(JumpTriggerHash);
            }

            previousVerticalVelocity = verticalVelocity;
        }

        #endregion

        #region Private Methods

        private void ResolveReferences()
        {
            if (motor == null)
            {
                motor = GetComponentInParent<CCS_CharacterMotor>();
            }

            TryResolveAnimator(out _);
        }

        private bool TryResolveAnimator(out Animator resolvedAnimator)
        {
            if (animator != null && HasPlayableController(animator))
            {
                resolvedAnimator = animator;
                return true;
            }

            Animator[] animators = GetComponentsInChildren<Animator>(true);
            for (int i = 0; i < animators.Length; i++)
            {
                Animator candidate = animators[i];
                if (candidate == null || !HasPlayableController(candidate))
                {
                    continue;
                }

                animator = candidate;
                resolvedAnimator = candidate;
                loggedMissingController = false;
                return true;
            }

            resolvedAnimator = null;

            if (!loggedMissingController)
            {
                loggedMissingController = true;
                Debug.LogWarning(
                    "[CCS Player Locomotion Animator] No child Animator with a runtime AnimatorController was found. Visual locomotion parameters were skipped.",
                    this);
            }

            return false;
        }

        private static bool HasPlayableController(Animator candidate)
        {
            return candidate != null
                && candidate.isActiveAndEnabled
                && candidate.runtimeAnimatorController != null;
        }

        #endregion
    }
}
