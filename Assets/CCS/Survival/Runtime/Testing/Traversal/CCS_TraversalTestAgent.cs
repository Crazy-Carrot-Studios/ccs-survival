using UnityEngine;

// =============================================================================
// SCRIPT: CCS_TraversalTestAgent
// CATEGORY: Survival / Testing / Traversal
// PURPOSE: Dev-only CharacterController traversal harness with telemetry, stuck detection, and pass/fail validation.
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
        private const string ManualPlayerCameraTargetName = "CCS_PlayerCameraTarget";

        private enum CCS_TraversalRouteResultStatus
        {
            Idle,
            Running,
            Passed,
            FailedStuck,
            FailedTimeout,
            Stopped
        }

        #region Variables

        [Header("Test Control")]
        [Tooltip("When enabled, this agent follows the assigned traversal route.")]
        [SerializeField] private bool enableTraversalTest;

        [Tooltip("Traversal route asset in the scene.")]
        [SerializeField] private CCS_TraversalTestRoute traversalRoute;

        [Header("Manual Player")]
        [Tooltip("Manual player root (CCS_PlayerRoot). Deactivated during traversal tests when isolation is enabled.")]
        [SerializeField] private GameObject manualPlayerRoot;

        [Tooltip("When enabled, deactivates the manual player root during traversal tests so CharacterControllers do not overlap the route.")]
        [SerializeField] private bool disableManualPlayerDuringTest = true;

        [Tooltip("Optional Cinemachine follow target while the manual player is hidden. Defaults to this agent transform.")]
        [SerializeField] private Transform traversalCameraFollowTarget;

        [Header("Movement")]
        [Tooltip("Horizontal movement speed in meters per second.")]
        [SerializeField] private float moveSpeed = 4f;

        [Tooltip("Gravity acceleration applied each frame when airborne.")]
        [SerializeField] private float gravity = -20f;

        [Tooltip("Rotation smoothing time when turning toward the next waypoint.")]
        [SerializeField] private float rotationSmoothTime = 0.12f;

        [Header("Validation")]
        [Tooltip("Logs traversal start, pass summaries, and failure warnings.")]
        [SerializeField] private bool enableTelemetryLogging = true;

        [Tooltip("Fails the route when the agent stops moving toward a waypoint for too long.")]
        [SerializeField] private bool enableStuckDetection = true;

        [Tooltip("Minimum world-space movement required to reset stuck detection.")]
        [SerializeField] private float stuckDistanceThreshold = 0.15f;

        [Tooltip("Seconds without sufficient movement before the route is marked stuck.")]
        [SerializeField] private float stuckTimeLimit = 5f;

        [Tooltip("Maximum seconds allowed to complete one route pass before failure.")]
        [SerializeField] private float maxRouteDurationSeconds = 120f;

        [Tooltip("Logs a concise PASSED summary when a route pass completes.")]
        [SerializeField] private bool logRouteSummaryOnComplete = true;

        [Tooltip("Stops the traversal test and restores manual player state when validation fails.")]
        [SerializeField] private bool stopTestOnFailure = true;

        [Header("Debug")]
        [Tooltip("Logs per-waypoint reach and advance messages (verbose).")]
        [SerializeField] private bool enableDebugLogs;

        private CharacterController characterController;
        private Vector3 verticalVelocity;
        private float rotationVelocity;
        private int currentWaypointIndex;
        private float waitTimer;
        private bool isWaitingAtWaypoint;
        private bool loggedInvalidRoute;
        private bool loggedRouteStart;
        private bool manualPlayerCachedActive;
        private bool manualPlayerHiddenByAgent;
        private Transform manualPlayerCameraTarget;
        private Transform manualPlayerCameraTargetCachedParent;
        private bool manualPlayerCameraTargetReparented;
        private bool lastEnableTraversalTest;
        private float testStartTime;
        private float currentRouteStartTime;
        private int completedRouteCount;
        private int failedRouteCount;
        private float lastWaypointAdvanceTime;
        private float distanceToCurrentWaypoint;
        private int totalWaypointAdvances;
        private Vector3 lastKnownPosition;
        private float stuckTimer;
        private CCS_TraversalRouteResultStatus routeResultStatus;

        #endregion

        #region Properties

        public int CompletedRouteCount => completedRouteCount;

        public int FailedRouteCount => failedRouteCount;

        public float CurrentRouteElapsedTime => currentRouteElapsedTime;

        public bool IsTraversalRunning => enableTraversalTest && routeResultStatus == CCS_TraversalRouteResultStatus.Running;

        public int CurrentWaypointIndex => currentWaypointIndex;

        private float currentRouteElapsedTime => Time.time - currentRouteStartTime;

        #endregion

        #region Unity Callbacks

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();

            if (traversalCameraFollowTarget == null)
            {
                traversalCameraFollowTarget = transform;
            }

            ResolveManualPlayerCameraTarget();
        }

        private void OnEnable()
        {
            lastEnableTraversalTest = enableTraversalTest;
            ResetRouteState();
            SyncManualPlayerForTraversalTest();

            if (enableTraversalTest)
            {
                BeginTraversalSession();
            }
        }

        private void OnDisable()
        {
            RestoreManualPlayerAfterTraversalTest();
            routeResultStatus = CCS_TraversalRouteResultStatus.Idle;
        }

        private void OnDestroy()
        {
            RestoreManualPlayerAfterTraversalTest();
        }

        private void Update()
        {
            if (enableTraversalTest && !lastEnableTraversalTest)
            {
                BeginTraversalSession();
            }
            else if (!enableTraversalTest && lastEnableTraversalTest)
            {
                routeResultStatus = CCS_TraversalRouteResultStatus.Idle;
            }

            lastEnableTraversalTest = enableTraversalTest;
            SyncManualPlayerForTraversalTest();

            if (!enableTraversalTest)
            {
                return;
            }

            if (!TryValidateRoute())
            {
                return;
            }

            UpdateTelemetry();

            if (TryFailRouteFromValidation())
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
            distanceToCurrentWaypoint = toTarget.magnitude;

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
            UpdateStuckDetectionAfterMove();
        }

        #endregion

        #region Private Methods

        private void BeginTraversalSession()
        {
            testStartTime = Time.time;
            completedRouteCount = 0;
            failedRouteCount = 0;
            routeResultStatus = CCS_TraversalRouteResultStatus.Running;
            ResetRouteState();
            BeginRoutePass();

            if (enableTelemetryLogging)
            {
                LogOnce(ref loggedRouteStart, $"{LogPrefix} Traversal validation started.");
            }
        }

        private void BeginRoutePass()
        {
            currentRouteStartTime = Time.time;
            lastWaypointAdvanceTime = Time.time;
            currentWaypointIndex = 0;
            waitTimer = 0f;
            isWaitingAtWaypoint = false;
            verticalVelocity = Vector3.zero;
            rotationVelocity = 0f;
            totalWaypointAdvances = 0;
            lastKnownPosition = transform.position;
            stuckTimer = 0f;
            routeResultStatus = CCS_TraversalRouteResultStatus.Running;

            if (enableDebugLogs && enableTelemetryLogging)
            {
                Debug.Log($"{LogPrefix} Route pass started.");
            }
        }

        private void UpdateTelemetry()
        {
            if (traversalRoute.TryGetWaypoint(currentWaypointIndex, out CCS_TraversalTestWaypoint waypoint))
            {
                distanceToCurrentWaypoint = Vector3.Distance(transform.position, waypoint.WorldPosition);
            }
            else
            {
                distanceToCurrentWaypoint = 0f;
            }

            if (enableStuckDetection && !isWaitingAtWaypoint)
            {
                UpdateStuckDetectionWhileMoving();
            }
        }

        private void UpdateStuckDetectionWhileMoving()
        {
            float movedDistance = Vector3.Distance(transform.position, lastKnownPosition);
            if (movedDistance >= stuckDistanceThreshold)
            {
                lastKnownPosition = transform.position;
                stuckTimer = 0f;
                return;
            }

            stuckTimer += Time.deltaTime;
        }

        private void UpdateStuckDetectionAfterMove()
        {
            float movedDistance = Vector3.Distance(transform.position, lastKnownPosition);
            if (movedDistance >= stuckDistanceThreshold)
            {
                lastKnownPosition = transform.position;
                stuckTimer = 0f;
            }
        }

        private bool TryFailRouteFromValidation()
        {
            if (enableStuckDetection && !isWaitingAtWaypoint && stuckTimer >= stuckTimeLimit)
            {
                string waypointLabel = GetCurrentWaypointLabel();
                FailRoute(
                    CCS_TraversalRouteResultStatus.FailedStuck,
                    $"{LogPrefix} FAILED: Agent stuck near waypoint {waypointLabel}.");
                return true;
            }

            if (currentRouteElapsedTime > maxRouteDurationSeconds)
            {
                FailRoute(
                    CCS_TraversalRouteResultStatus.FailedTimeout,
                    $"{LogPrefix} FAILED: Route exceeded max duration.");
                return true;
            }

            return false;
        }

        private void FailRoute(CCS_TraversalRouteResultStatus failureStatus, string message)
        {
            if (routeResultStatus != CCS_TraversalRouteResultStatus.Running)
            {
                return;
            }

            failedRouteCount++;
            routeResultStatus = failureStatus;

            if (enableTelemetryLogging)
            {
                Debug.LogWarning(message);
            }

            if (stopTestOnFailure)
            {
                StopTraversalTest(CCS_TraversalRouteResultStatus.Stopped);
            }
        }

        private void CompleteRoutePass()
        {
            completedRouteCount++;
            routeResultStatus = CCS_TraversalRouteResultStatus.Passed;

            if (logRouteSummaryOnComplete && enableTelemetryLogging)
            {
                Debug.Log(
                    $"{LogPrefix} PASSED: Route completed in {currentRouteElapsedTime:F2}s. " +
                    $"Waypoints={traversalRoute.WaypointCount}. Loops={completedRouteCount}.");
            }
        }

        private void StopTraversalTest(CCS_TraversalRouteResultStatus stopStatus)
        {
            routeResultStatus = stopStatus;
            enableTraversalTest = false;
            lastEnableTraversalTest = false;
            SyncManualPlayerForTraversalTest();
        }

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
            lastKnownPosition = transform.position;
            stuckTimer = 0f;

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

            totalWaypointAdvances++;
            lastWaypointAdvanceTime = Time.time;
            lastKnownPosition = transform.position;
            stuckTimer = 0f;
            currentWaypointIndex++;

            if (currentWaypointIndex >= traversalRoute.WaypointCount)
            {
                CompleteRoutePass();

                if (traversalRoute.LoopRoute)
                {
                    if (enableDebugLogs)
                    {
                        Debug.Log($"{LogPrefix} Route loop complete — restarting.");
                    }

                    BeginRoutePass();
                    return;
                }

                if (enableDebugLogs)
                {
                    Debug.Log($"{LogPrefix} Route complete.");
                }

                StopTraversalTest(CCS_TraversalRouteResultStatus.Passed);
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
            lastKnownPosition = transform.position;
            stuckTimer = 0f;
            distanceToCurrentWaypoint = 0f;
        }

        private string GetCurrentWaypointLabel()
        {
            if (traversalRoute.TryGetWaypoint(currentWaypointIndex, out CCS_TraversalTestWaypoint waypoint))
            {
                return $"'{waypoint.name}' (index {currentWaypointIndex})";
            }

            return $"index {currentWaypointIndex}";
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

        private void ResolveManualPlayerCameraTarget()
        {
            if (manualPlayerRoot == null || manualPlayerCameraTarget != null)
            {
                return;
            }

            manualPlayerCameraTarget = manualPlayerRoot.transform.Find(ManualPlayerCameraTargetName);
        }

        private void SyncManualPlayerForTraversalTest()
        {
            if (enableTraversalTest)
            {
                HideManualPlayerForTraversalTest();
                return;
            }

            RestoreManualPlayerAfterTraversalTest();
        }

        private void HideManualPlayerForTraversalTest()
        {
            if (!disableManualPlayerDuringTest || manualPlayerRoot == null || manualPlayerHiddenByAgent)
            {
                return;
            }

            manualPlayerCachedActive = manualPlayerRoot.activeSelf;
            ResolveManualPlayerCameraTarget();

            if (manualPlayerCameraTarget != null && traversalCameraFollowTarget != null)
            {
                manualPlayerCameraTargetCachedParent = manualPlayerCameraTarget.parent;
                manualPlayerCameraTarget.SetParent(traversalCameraFollowTarget, true);
                manualPlayerCameraTargetReparented = true;
            }

            manualPlayerRoot.SetActive(false);
            manualPlayerHiddenByAgent = true;
        }

        private void RestoreManualPlayerAfterTraversalTest()
        {
            if (!manualPlayerHiddenByAgent || manualPlayerRoot == null)
            {
                return;
            }

            manualPlayerRoot.SetActive(manualPlayerCachedActive);

            if (manualPlayerCameraTargetReparented && manualPlayerCameraTarget != null && manualPlayerCameraTargetCachedParent != null)
            {
                manualPlayerCameraTarget.SetParent(manualPlayerCameraTargetCachedParent, false);
                manualPlayerCameraTargetReparented = false;
            }

            manualPlayerHiddenByAgent = false;
        }

        #endregion
    }
}
