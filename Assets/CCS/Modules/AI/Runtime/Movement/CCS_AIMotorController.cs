using CCS.Modules.Attributes;
using UnityEngine;
using UnityEngine.AI;

// =============================================================================
// SCRIPT: CCS_AIMotorController
// CATEGORY: Modules / AI / Runtime / Movement
// PURPOSE: NavMesh-first AI motor with continuous repathing while chasing targets.
// PLACEMENT: AI bandit root with NavMeshAgent and/or CharacterController.
// AUTHOR: James Schilz
// CREATED: 2026-06-25
// NOTES: Server drives destination; clients receive transform via NetworkTransform.
// =============================================================================

namespace CCS.Modules.AI
{
    [DefaultExecutionOrder(80)]
    public sealed class CCS_AIMotorController : MonoBehaviour
    {
        private const float NavMeshWarpSampleRadius = 5f;
        private static readonly float[] RingSampleDistances = { 1.5f, 3f, 5f, 7f, 9f };
        private static readonly float[] RingSampleAngles = { 0f, 45f, 90f, 135f, 180f, 225f, 270f, 315f };

        [SerializeField] private UnityEngine.CharacterController characterController;
        [SerializeField] private NavMeshAgent navMeshAgent;
        [SerializeField] private float gravity = -25f;
        [SerializeField] private bool debugAiPathfinding;
        [SerializeField] private bool debugStalking;
        [SerializeField] private bool drawPathGizmos;

        private float verticalVelocity;
        private bool loggedNavMeshFallback;
        private Vector3 lastDestination;
        private Vector3 lastSampledTargetDestination;
        private Vector3 lastTargetPosition;
        private float lastRepathTime = -999f;
        private NavMeshPath navMeshPath;

        public Vector3 LastDestination => lastDestination;

        public Vector3 LastSampledTargetDestination => lastSampledTargetDestination;

        public float TimeSinceLastRepath => Time.time - lastRepathTime;

        public void ChaseTarget(Vector3 worldTarget, CCS_AIBanditProfile profile)
        {
            if (profile == null)
            {
                return;
            }

            TryResolveReachableDestination(worldTarget, profile.TargetSampleRadius, out Vector3 sampledDestination);
            lastSampledTargetDestination = sampledDestination;
            lastDestination = sampledDestination;
            lastTargetPosition = sampledDestination;
            lastRepathTime = Time.time;

            MoveTowards(
                worldTarget,
                profile.MoveSpeed,
                profile.RotationSpeed,
                profile.MovementStopDistance,
                profile.MinimumPreferredRange,
                profile.RepathIntervalSeconds,
                profile.DestinationUpdateThreshold,
                profile.TargetSampleRadius,
                profile.PathRefreshWhenStale);
        }

        public void MoveTowards(
            Vector3 worldTarget,
            float moveSpeed,
            float turnSpeedDegreesPerSecond,
            float movementStopDistance,
            float minimumPreferredRange,
            float repathIntervalSeconds = 0.15f,
            float destinationUpdateThreshold = 0.35f,
            float targetSampleRadius = 4f,
            bool pathRefreshWhenStale = true)
        {
            if (IsMovementBlocked())
            {
                Stop();
                return;
            }

            if (TryMoveWithNavMeshAgent(
                worldTarget,
                moveSpeed,
                movementStopDistance,
                minimumPreferredRange,
                turnSpeedDegreesPerSecond,
                repathIntervalSeconds,
                destinationUpdateThreshold,
                targetSampleRadius,
                pathRefreshWhenStale))
            {
                return;
            }

            EnableCharacterControllerForFallback();
            MoveWithCharacterController(
                worldTarget,
                moveSpeed,
                turnSpeedDegreesPerSecond,
                movementStopDistance,
                minimumPreferredRange);
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
            ResolveReferences();

            if (navMeshAgent != null && navMeshAgent.enabled && navMeshAgent.isOnNavMesh)
            {
                navMeshAgent.isStopped = true;
                navMeshAgent.ResetPath();
            }

            if (characterController != null && characterController.isGrounded)
            {
                verticalVelocity = -1.5f;
            }

            lastTargetPosition = transform.position;
            LogMotorDebug(worldTarget: transform.position, stopDistance: 0f, moving: false);
        }

        private bool TryMoveWithNavMeshAgent(
            Vector3 worldTarget,
            float moveSpeed,
            float movementStopDistance,
            float minimumPreferredRange,
            float turnSpeedDegreesPerSecond,
            float repathIntervalSeconds,
            float destinationUpdateThreshold,
            float targetSampleRadius,
            bool pathRefreshWhenStale)
        {
            ResolveReferences();
            if (navMeshAgent == null || !EnsureNavMeshAgentReady())
            {
                if (debugAiPathfinding && !loggedNavMeshFallback)
                {
                    Debug.LogWarning("[AI] NavMeshAgent unavailable; using direct CharacterController fallback.", this);
                    loggedNavMeshFallback = true;
                }

                return false;
            }

            DisableCharacterControllerForNavMesh();

            Vector3 currentPosition = transform.position;
            Vector3 toTarget = worldTarget - currentPosition;
            toTarget.y = 0f;
            float distance = toTarget.magnitude;

            navMeshAgent.speed = Mathf.Max(0.1f, moveSpeed);
            navMeshAgent.stoppingDistance = Mathf.Max(0.1f, movementStopDistance);
            navMeshAgent.updateRotation = false;
            navMeshAgent.updatePosition = true;
            navMeshAgent.autoRepath = true;
            navMeshAgent.autoBraking = false;

            if (distance <= movementStopDistance)
            {
                navMeshAgent.isStopped = true;
                navMeshAgent.ResetPath();
                LogMotorDebug(worldTarget, movementStopDistance, moving: false);
                return true;
            }

            Vector3 destination = worldTarget;
            if (distance < minimumPreferredRange)
            {
                Vector3 awayDirection = (currentPosition - worldTarget).normalized;
                destination = currentPosition + (awayDirection * 2f);
            }

            TryResolveReachableDestination(destination, targetSampleRadius, out Vector3 sampledDestination);
            lastSampledTargetDestination = sampledDestination;

            bool targetMoved = (sampledDestination - lastTargetPosition).sqrMagnitude
                >= destinationUpdateThreshold * destinationUpdateThreshold;
            bool repathIntervalElapsed = Time.time - lastRepathTime >= repathIntervalSeconds;
            bool pathInvalid = navMeshAgent.pathStatus == NavMeshPathStatus.PathInvalid
                || navMeshAgent.pathStatus == NavMeshPathStatus.PathPartial;
            bool pathStale = pathRefreshWhenStale && navMeshAgent.isOnNavMesh && navMeshAgent.isPathStale;
            bool needsRepath = !navMeshAgent.hasPath
                || targetMoved
                || repathIntervalElapsed
                || pathInvalid
                || pathStale;

            if (needsRepath)
            {
                lastDestination = sampledDestination;
                lastTargetPosition = sampledDestination;
                lastRepathTime = Time.time;
            }

            navMeshAgent.isStopped = false;
            if (needsRepath)
            {
                navMeshAgent.SetDestination(sampledDestination);
            }

            RotateTowards(worldTarget, turnSpeedDegreesPerSecond);
            LogMotorDebug(worldTarget, movementStopDistance, moving: true);
            return true;
        }

        private bool TryResolveReachableDestination(
            Vector3 targetPosition,
            float targetSampleRadius,
            out Vector3 destination)
        {
            if (NavMesh.SamplePosition(
                targetPosition,
                out NavMeshHit directHit,
                targetSampleRadius,
                NavMesh.AllAreas))
            {
                destination = directHit.position;
                if (HasCompletePathTo(destination))
                {
                    return true;
                }
            }

            Vector3 origin = transform.position;
            for (int ringIndex = 0; ringIndex < RingSampleDistances.Length; ringIndex++)
            {
                float ringDistance = RingSampleDistances[ringIndex];
                for (int angleIndex = 0; angleIndex < RingSampleAngles.Length; angleIndex++)
                {
                    float radians = RingSampleAngles[angleIndex] * Mathf.Deg2Rad;
                    Vector3 samplePoint = targetPosition + new Vector3(
                        Mathf.Cos(radians) * ringDistance,
                        0f,
                        Mathf.Sin(radians) * ringDistance);
                    if (!NavMesh.SamplePosition(
                        samplePoint,
                        out NavMeshHit ringHit,
                        targetSampleRadius,
                        NavMesh.AllAreas))
                    {
                        continue;
                    }

                    if (!HasCompletePathTo(ringHit.position))
                    {
                        continue;
                    }

                    destination = ringHit.position;
                    return true;
                }
            }

            if (NavMesh.SamplePosition(
                targetPosition,
                out NavMeshHit fallbackHit,
                targetSampleRadius * 2f,
                NavMesh.AllAreas))
            {
                destination = fallbackHit.position;
                return true;
            }

            destination = targetPosition;
            return false;
        }

        private bool HasCompletePathTo(Vector3 destination)
        {
            if (navMeshAgent == null || !navMeshAgent.isOnNavMesh)
            {
                return false;
            }

            if (navMeshPath == null)
            {
                navMeshPath = new NavMeshPath();
            }

            if (!NavMesh.CalculatePath(transform.position, destination, NavMesh.AllAreas, navMeshPath))
            {
                return false;
            }

            return navMeshPath.status == NavMeshPathStatus.PathComplete;
        }

        private bool EnsureNavMeshAgentReady()
        {
            if (navMeshAgent == null)
            {
                return false;
            }

            if (!navMeshAgent.enabled)
            {
                navMeshAgent.enabled = true;
            }

            if (!navMeshAgent.isOnNavMesh
                && NavMesh.SamplePosition(
                    transform.position,
                    out NavMeshHit hit,
                    NavMeshWarpSampleRadius,
                    NavMesh.AllAreas))
            {
                navMeshAgent.Warp(hit.position);
            }

            return navMeshAgent.isOnNavMesh;
        }

        private void DisableCharacterControllerForNavMesh()
        {
            if (characterController != null && characterController.enabled)
            {
                characterController.enabled = false;
            }
        }

        private void EnableCharacterControllerForFallback()
        {
            if (characterController != null && !characterController.enabled)
            {
                characterController.enabled = true;
            }
        }

        private void MoveWithCharacterController(
            Vector3 worldTarget,
            float moveSpeed,
            float turnSpeedDegreesPerSecond,
            float movementStopDistance,
            float minimumPreferredRange)
        {
            ResolveReferences();
            if (characterController == null)
            {
                return;
            }

            Vector3 currentPosition = transform.position;
            Vector3 toTarget = worldTarget - currentPosition;
            toTarget.y = 0f;
            float distance = toTarget.magnitude;

            if (distance <= movementStopDistance)
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

            if (debugAiPathfinding || debugStalking)
            {
                Debug.DrawRay(transform.position + Vector3.up * 1.3f, transform.forward * 1.2f, Color.cyan);
            }
        }

        private void OnDrawGizmosSelected()
        {
            ResolveReferences();
            if (!drawPathGizmos && !debugStalking)
            {
                return;
            }

            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(lastDestination, 0.2f);
            Gizmos.color = Color.magenta;
            Gizmos.DrawSphere(lastSampledTargetDestination, 0.15f);

            if (navMeshAgent == null || !navMeshAgent.hasPath)
            {
                return;
            }

            Vector3[] corners = navMeshAgent.path.corners;
            if (corners == null || corners.Length == 0)
            {
                return;
            }

            Gizmos.color = Color.cyan;
            for (int i = 0; i < corners.Length - 1; i++)
            {
                Gizmos.DrawLine(corners[i], corners[i + 1]);
            }
        }

        private void LogMotorDebug(Vector3 worldTarget, float stopDistance, bool moving)
        {
            if ((!debugAiPathfinding && !debugStalking) || navMeshAgent == null)
            {
                return;
            }

            Debug.Log(
                "[AI Motor] Stalking\n"
                + "Target Distance: "
                + Vector3.Distance(transform.position, worldTarget).ToString("0.00")
                + "\nCurrent Destination: "
                + lastDestination
                + "\nSampled Target Destination: "
                + lastSampledTargetDestination
                + "\nDestination Age: "
                + TimeSinceLastRepath.ToString("0.00")
                + "\nRepath Timer: "
                + TimeSinceLastRepath.ToString("0.00")
                + "\nAgent HasPath: "
                + navMeshAgent.hasPath
                + "\nPath Status: "
                + navMeshAgent.pathStatus
                + "\nRemaining Distance: "
                + navMeshAgent.remainingDistance.ToString("0.00")
                + "\nIsStopped: "
                + navMeshAgent.isStopped
                + "\nMoving: "
                + moving
                + "\nStop Distance: "
                + stopDistance.ToString("0.00"),
                this);
        }

        private void ResolveReferences()
        {
            if (characterController == null)
            {
                characterController = GetComponent<UnityEngine.CharacterController>();
            }

            if (navMeshAgent == null)
            {
                navMeshAgent = GetComponent<NavMeshAgent>();
            }
        }

        private bool IsMovementBlocked()
        {
            CCS_NetworkHealth networkHealth = GetComponent<CCS_NetworkHealth>();
            if (networkHealth != null && networkHealth.IsDead)
            {
                return true;
            }

            CCS_AIBanditBrain brain = GetComponent<CCS_AIBanditBrain>();
            return brain != null && brain.CurrentState == CCS_AIBanditState.Dead;
        }
    }
}
