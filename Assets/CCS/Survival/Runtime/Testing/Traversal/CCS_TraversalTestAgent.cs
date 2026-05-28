using CCS.Survival;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_TraversalTestAgent
// CATEGORY: Survival / Testing / Traversal
// PURPOSE: Dev-only CharacterController agent that follows a serialized traversal test route.
// PLACEMENT: Attach to CCS_TraversalTestAgent in SCN_CCS_Survival_Bootstrap (disabled by default).
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Does not replace manual player input. No Input System. Not final AI.
// =============================================================================

namespace CCS.Survival.Testing.Traversal
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class CCS_TraversalTestAgent : MonoBehaviour
    {
        private const string LogPrefix = "[CCS Traversal Test]";

        #region Variables

        [Header("Test Control")]
        [Tooltip("When enabled, this agent follows the assigned traversal route.")]
        [SerializeField] private bool enableTraversalTest;

        [Tooltip("Traversal route asset in the scene.")]
        [SerializeField] private CCS_TraversalTestRoute traversalRoute;

        [Header("Manual Player")]
        [Tooltip("Manual player root (CCS_PlayerRoot). Movement and CharacterController are disabled while the traversal test runs.")]
        [SerializeField] private GameObject manualPlayerRoot;

        [Tooltip("When enabled, disables manual player locomotion during traversal tests to avoid CharacterController overlap at spawn.")]
        [SerializeField] private bool disableManualPlayerDuringTest = true;

        [Header("Movement")]
        [Tooltip("Horizontal movement speed in meters per second.")]
        [SerializeField] private float moveSpeed = 4f;

        [Tooltip("Gravity acceleration applied each frame when airborne.")]
        [SerializeField] private float gravity = -20f;

        [Tooltip("Rotation smoothing time when turning toward the next waypoint.")]
        [SerializeField] private float rotationSmoothTime = 0.12f;

        [Header("Debug")]
        [Tooltip("Logs route start, waypoint advances, and loop completion.")]
        [SerializeField] private bool enableDebugLogs;

        private CharacterController characterController;
        private Vector3 verticalVelocity;
        private float rotationVelocity;
        private int currentWaypointIndex;
        private float waitTimer;
        private bool isWaitingAtWaypoint;
        private bool loggedInvalidRoute;
        private bool loggedRouteStart;
        private CharacterController manualPlayerCharacterController;
        private CCS_SurvivalPrototypeCharacterController manualPlayerMovement;
        private bool manualPlayerLocomotionDisabledByAgent;

        #endregion

        #region Unity Callbacks

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
            CacheManualPlayerComponents();
        }

        private void OnEnable()
        {
            ResetRouteState();
            SyncManualPlayerForTraversalTest();
        }

        private void OnDisable()
        {
            SetManualPlayerLocomotionEnabled(true);
        }

        private void Update()
        {
            SyncManualPlayerForTraversalTest();

            if (!enableTraversalTest)
            {
                return;
            }

            if (!TryValidateRoute())
            {
                return;
            }

            ApplyGravityAndGrounding();

            if (isWaitingAtWaypoint)
            {
                UpdateWaitState();
                characterController.Move(verticalVelocity * Time.deltaTime);
                return;
            }

            if (!traversalRoute.TryGetWaypoint(currentWaypointIndex, out CCS_TraversalTestWaypoint waypoint))
            {
                return;
            }

            Vector3 targetPosition = waypoint.WorldPosition;
            Vector3 toTarget = targetPosition - transform.position;

            if (HasArrivedAtWaypoint(waypoint, toTarget))
            {
                BeginWaypointWait(waypoint);
                characterController.Move(verticalVelocity * Time.deltaTime);
                return;
            }

            Vector3 horizontalDirection = new Vector3(toTarget.x, 0f, toTarget.z);
            if (horizontalDirection.sqrMagnitude > 0.0001f)
            {
                horizontalDirection.Normalize();
                RotateTowardDirection(horizontalDirection);
            }

            Vector3 moveDirection = toTarget.normalized;
            Vector3 moveDelta = moveDirection * (moveSpeed * Time.deltaTime);
            characterController.Move(moveDelta + (verticalVelocity * Time.deltaTime));
        }

        #endregion

        #region Private Methods

        private bool TryValidateRoute()
        {
            if (traversalRoute == null)
            {
                LogOnce(ref loggedInvalidRoute, $"{LogPrefix} Traversal route is not assigned.");
                return false;
            }

            if (!traversalRoute.ValidateRoute(out string validationMessage))
            {
                LogOnce(ref loggedInvalidRoute, $"{LogPrefix} {validationMessage}");
                return false;
            }

            loggedInvalidRoute = false;
            return true;
        }

        private void ApplyGravityAndGrounding()
        {
            if (characterController.isGrounded && verticalVelocity.y < 0f)
            {
                verticalVelocity.y = -2f;
            }

            verticalVelocity.y += gravity * Time.deltaTime;
        }

        private bool HasArrivedAtWaypoint(CCS_TraversalTestWaypoint waypoint, Vector3 toTarget)
        {
            float horizontalDistance = new Vector2(toTarget.x, toTarget.z).magnitude;
            float verticalDistance = Mathf.Abs(toTarget.y);
            return horizontalDistance <= waypoint.ArrivalRadius &&
                   verticalDistance <= waypoint.VerticalArrivalTolerance;
        }

        private void BeginWaypointWait(CCS_TraversalTestWaypoint waypoint)
        {
            if (waypoint.WaitDurationSeconds > 0f)
            {
                isWaitingAtWaypoint = true;
                waitTimer = waypoint.WaitDurationSeconds;
                return;
            }

            AdvanceWaypoint(waypoint);
        }

        private void UpdateWaitState()
        {
            waitTimer -= Time.deltaTime;
            if (waitTimer > 0f)
            {
                return;
            }

            isWaitingAtWaypoint = false;

            if (!traversalRoute.TryGetWaypoint(currentWaypointIndex, out CCS_TraversalTestWaypoint waypoint))
            {
                return;
            }

            AdvanceWaypoint(waypoint);
        }

        private void AdvanceWaypoint(CCS_TraversalTestWaypoint completedWaypoint)
        {
            if (enableDebugLogs)
            {
                Debug.Log($"{LogPrefix} Reached '{completedWaypoint.name}' (index {currentWaypointIndex}).");
            }

            currentWaypointIndex++;

            if (currentWaypointIndex >= traversalRoute.WaypointCount)
            {
                if (traversalRoute.LoopRoute)
                {
                    if (enableDebugLogs)
                    {
                        Debug.Log($"{LogPrefix} Route loop complete — restarting.");
                    }

                    currentWaypointIndex = 0;
                    loggedRouteStart = false;
                    return;
                }

                if (enableDebugLogs)
                {
                    Debug.Log($"{LogPrefix} Route complete.");
                }

                enableTraversalTest = false;
                SyncManualPlayerForTraversalTest();
                return;
            }

            if (enableDebugLogs && traversalRoute.TryGetWaypoint(currentWaypointIndex, out CCS_TraversalTestWaypoint nextWaypoint))
            {
                Debug.Log($"{LogPrefix} Advancing to '{nextWaypoint.name}' (index {currentWaypointIndex}).");
            }
        }

        private void RotateTowardDirection(Vector3 direction)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            float smoothedAngle = Mathf.SmoothDampAngle(
                transform.eulerAngles.y,
                targetAngle,
                ref rotationVelocity,
                rotationSmoothTime);
            transform.rotation = Quaternion.Euler(0f, smoothedAngle, 0f);
        }

        private void ResetRouteState()
        {
            currentWaypointIndex = 0;
            waitTimer = 0f;
            isWaitingAtWaypoint = false;
            verticalVelocity = Vector3.zero;
            rotationVelocity = 0f;
            loggedInvalidRoute = false;
            loggedRouteStart = false;

            if (enableTraversalTest && enableDebugLogs && TryValidateRoute())
            {
                LogOnce(ref loggedRouteStart, $"{LogPrefix} Traversal test started.");
            }
        }

        private static void LogOnce(ref bool loggedFlag, string message)
        {
            if (loggedFlag)
            {
                return;
            }

            Debug.Log(message);
            loggedFlag = true;
        }

        private void CacheManualPlayerComponents()
        {
            if (manualPlayerRoot == null)
            {
                return;
            }

            manualPlayerCharacterController = manualPlayerRoot.GetComponent<CharacterController>();
            manualPlayerMovement = manualPlayerRoot.GetComponent<CCS_SurvivalPrototypeCharacterController>();
        }

        private void SyncManualPlayerForTraversalTest()
        {
            SetManualPlayerLocomotionEnabled(!enableTraversalTest);
        }

        private void SetManualPlayerLocomotionEnabled(bool locomotionEnabled)
        {
            if (!disableManualPlayerDuringTest || manualPlayerRoot == null)
            {
                return;
            }

            bool shouldDisable = !locomotionEnabled;
            if (shouldDisable == manualPlayerLocomotionDisabledByAgent)
            {
                return;
            }

            if (manualPlayerCharacterController != null)
            {
                manualPlayerCharacterController.enabled = locomotionEnabled;
            }

            if (manualPlayerMovement != null)
            {
                manualPlayerMovement.enabled = locomotionEnabled;
            }

            manualPlayerLocomotionDisabledByAgent = shouldDisable;
        }

        #endregion
    }
}
