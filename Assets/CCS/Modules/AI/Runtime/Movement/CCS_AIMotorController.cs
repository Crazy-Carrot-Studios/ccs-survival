using UnityEngine;

// =============================================================================
// SCRIPT: CCS_AIMotorController
// CATEGORY: Modules / AI / Runtime / Movement
// PURPOSE: Simple flat XZ steering motor for AI using CharacterController.Move.
// PLACEMENT: AI bandit root with CharacterController.
// AUTHOR: James Schilz
// CREATED: 2026-06-25
// NOTES: Keeps Y movement gravity-only for stable deterministic combat movement.
// =============================================================================

namespace CCS.Modules.AI
{
    [DefaultExecutionOrder(80)]
    public sealed class CCS_AIMotorController : MonoBehaviour
    {
        [SerializeField] private UnityEngine.CharacterController characterController;
        [SerializeField] private float gravity = -25f;
        [SerializeField] private bool enableMotorDebugLogs;

        private float verticalVelocity;

        public void MoveTowards(
            Vector3 worldTarget,
            float moveSpeed,
            float turnSpeedDegreesPerSecond,
            float stopDistance,
            float minimumPreferredRange)
        {
            if (characterController == null)
            {
                characterController = GetComponent<UnityEngine.CharacterController>();
            }

            if (characterController == null)
            {
                return;
            }

            Vector3 currentPosition = transform.position;
            Vector3 toTarget = worldTarget - currentPosition;
            toTarget.y = 0f;
            float distance = toTarget.magnitude;

            if (distance <= stopDistance)
            {
                Stop();
                return;
            }

            Vector3 moveDirection = toTarget.normalized;
            if (distance < minimumPreferredRange)
            {
                moveDirection = -moveDirection;
            }

            if (moveDirection.sqrMagnitude > 0.0001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
                transform.rotation = Quaternion.RotateTowards(
                    transform.rotation,
                    targetRotation,
                    turnSpeedDegreesPerSecond * Time.deltaTime);
            }

            if (characterController.isGrounded && verticalVelocity < 0f)
            {
                verticalVelocity = -1.5f;
            }

            verticalVelocity += gravity * Time.deltaTime;
            Vector3 velocity = (transform.forward * moveSpeed) + (Vector3.up * verticalVelocity);
            characterController.Move(velocity * Time.deltaTime);

            if (enableMotorDebugLogs)
            {
                Debug.DrawRay(transform.position + Vector3.up * 1.3f, transform.forward * 1.2f, Color.cyan);
            }
        }

        public void RotateTowards(Vector3 worldTarget, float turnSpeedDegreesPerSecond)
        {
            Vector3 toTarget = worldTarget - transform.position;
            toTarget.y = 0f;
            if (toTarget.sqrMagnitude <= 0.0001f)
            {
                return;
            }

            Quaternion targetRotation = Quaternion.LookRotation(toTarget.normalized, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                turnSpeedDegreesPerSecond * Time.deltaTime);
        }

        public void Stop()
        {
            if (characterController != null && characterController.isGrounded)
            {
                verticalVelocity = -1.5f;
            }
        }
    }
}
