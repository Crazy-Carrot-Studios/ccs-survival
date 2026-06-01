using UnityEngine;

// =============================================================================
// SCRIPT: CCS_WildlifeMovementController
// CATEGORY: Modules / Wildlife / Runtime / Movement
// PURPOSE: Simple transform-based wildlife movement without NavMesh or Rigidbody.
// PLACEMENT: Owned by CCS_WildlifeAgent for wander and flee locomotion.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: Keeps current Y position for bootstrap terrain prototype behavior.
// =============================================================================

namespace CCS.Modules.Wildlife
{
    public sealed class CCS_WildlifeMovementController
    {
        #region Variables

        private readonly Transform agentTransform;
        private float moveSpeed;
        private Vector3? destination;
        private float arrivalThreshold = 0.25f;

        #endregion

        #region Properties

        public bool HasDestination => destination.HasValue;

        public bool IsMoving { get; private set; }

        #endregion

        #region Public Methods

        public CCS_WildlifeMovementController(Transform transform)
        {
            agentTransform = transform;
        }

        public void SetMoveSpeed(float speed)
        {
            moveSpeed = Mathf.Max(0f, speed);
        }

        public void SetArrivalThreshold(float threshold)
        {
            arrivalThreshold = Mathf.Max(0.05f, threshold);
        }

        public void SetDestination(Vector3 worldDestination)
        {
            destination = worldDestination;
            IsMoving = true;
        }

        public void Stop()
        {
            destination = null;
            IsMoving = false;
        }

        public bool UpdateMovement(float deltaTime)
        {
            if (agentTransform == null || !destination.HasValue || moveSpeed <= 0f)
            {
                IsMoving = false;
                return false;
            }

            Vector3 currentPosition = agentTransform.position;
            Vector3 flatDestination = new Vector3(destination.Value.x, currentPosition.y, destination.Value.z);
            Vector3 offset = flatDestination - currentPosition;
            float distance = offset.magnitude;

            if (distance <= arrivalThreshold)
            {
                destination = null;
                IsMoving = false;
                return true;
            }

            Vector3 direction = offset / distance;
            agentTransform.position = currentPosition + direction * moveSpeed * deltaTime;

            if (direction.sqrMagnitude > 0.0001f)
            {
                agentTransform.rotation = Quaternion.LookRotation(direction, Vector3.up);
            }

            IsMoving = true;
            return false;
        }

        public static Vector3 PickRandomWanderPoint(Vector3 homePosition, float wanderRadius)
        {
            Vector2 randomOffset = Random.insideUnitCircle * Mathf.Max(0f, wanderRadius);
            return homePosition + new Vector3(randomOffset.x, 0f, randomOffset.y);
        }

        public static Vector3 PickFleePoint(Vector3 currentPosition, Vector3 threatPosition, float fleeDistance)
        {
            Vector3 awayDirection = currentPosition - threatPosition;
            awayDirection.y = 0f;

            if (awayDirection.sqrMagnitude < 0.01f)
            {
                awayDirection = new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f));
            }

            awayDirection.Normalize();
            return currentPosition + awayDirection * Mathf.Max(0f, fleeDistance);
        }

        #endregion
    }
}
