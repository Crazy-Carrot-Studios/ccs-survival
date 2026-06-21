using System.Collections.Generic;
using CCS.Modules.CharacterController;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_ControllerTestNpcRunner
// CATEGORY: Modules / CharacterController / Tests / Runtime
// PURPOSE: Drives the offline test NPC along master test traversal points.
// PLACEMENT: PF_CCS_CharacterController_TestNPC root.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Resolves route targets by CCS_ControllerTestPoint ids when routePoints is empty.
// =============================================================================

namespace CCS.Modules.CharacterController.Tests
{
    public sealed class CCS_ControllerTestNpcRunner : MonoBehaviour
    {
        #region Serialized Fields

        [SerializeField] private CCS_CharacterMotor motor;

        [SerializeField] private CCS_CharacterInputActionProvider inputProvider;

        [SerializeField] private float arrivalRadius = 0.75f;

        [SerializeField] private float pauseAtPointSeconds = 1f;

        [SerializeField] private bool loopRoute;

        [SerializeField] private string[] routeTestPointIds;

        [SerializeField] private Transform[] routePoints;

        #endregion

        #region Variables

        private readonly List<Transform> resolvedRoutePoints = new List<Transform>();

        private int currentRouteIndex;

        private float pauseTimer;

        private bool routeReady;

        #endregion

        #region Unity Callbacks

        private void Awake()
        {
            if (motor == null)
            {
                motor = GetComponent<CCS_CharacterMotor>();
            }

            if (inputProvider == null)
            {
                inputProvider = GetComponent<CCS_CharacterInputActionProvider>();
            }
        }

        private void Start()
        {
            ResolveRoute();
        }

        private void Update()
        {
            if (!routeReady || inputProvider == null || resolvedRoutePoints.Count == 0)
            {
                return;
            }

            if (pauseTimer > 0f)
            {
                pauseTimer -= Time.deltaTime;
                inputProvider.ClearExternalMoveInput();
                return;
            }

            Transform target = resolvedRoutePoints[currentRouteIndex];
            if (target == null)
            {
                inputProvider.ClearExternalMoveInput();
                AdvanceRouteIndex();
                return;
            }

            Vector3 toTarget = target.position - transform.position;
            toTarget.y = 0f;

            if (toTarget.sqrMagnitude <= arrivalRadius * arrivalRadius)
            {
                inputProvider.ClearExternalMoveInput();
                pauseTimer = pauseAtPointSeconds;
                AdvanceRouteIndex();
                return;
            }

            Vector3 moveDirection = toTarget.normalized;
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                540f * Time.deltaTime);
            inputProvider.SetExternalMoveInput(Vector2.up);
        }

        private void OnDisable()
        {
            if (inputProvider != null)
            {
                inputProvider.ClearExternalMoveInput();
            }
        }

        #endregion

        #region Private Methods

        private void ResolveRoute()
        {
            resolvedRoutePoints.Clear();

            if (routePoints != null && routePoints.Length > 0)
            {
                for (int i = 0; i < routePoints.Length; i++)
                {
                    if (routePoints[i] != null)
                    {
                        resolvedRoutePoints.Add(routePoints[i]);
                    }
                }
            }
            else if (routeTestPointIds != null && routeTestPointIds.Length > 0)
            {
                Dictionary<string, Transform> pointsById = BuildTestPointLookup();
                for (int i = 0; i < routeTestPointIds.Length; i++)
                {
                    string pointId = routeTestPointIds[i];
                    if (string.IsNullOrWhiteSpace(pointId))
                    {
                        continue;
                    }

                    if (pointsById.TryGetValue(pointId, out Transform point))
                    {
                        resolvedRoutePoints.Add(point);
                    }
                    else
                    {
                        Debug.LogWarning(
                            $"[Controller Test NPC] Missing test point id '{pointId}' for {name}.",
                            this);
                    }
                }
            }

            routeReady = resolvedRoutePoints.Count > 0;
            currentRouteIndex = 0;

            if (!routeReady)
            {
                Debug.LogWarning($"[Controller Test NPC] No route points resolved for {name}.", this);
            }
        }

        private static Dictionary<string, Transform> BuildTestPointLookup()
        {
            Dictionary<string, Transform> pointsById = new Dictionary<string, Transform>();
            CCS_ControllerTestPoint[] testPoints = FindObjectsByType<CCS_ControllerTestPoint>(
                FindObjectsInactive.Include, FindObjectsSortMode.None);

            for (int i = 0; i < testPoints.Length; i++)
            {
                CCS_ControllerTestPoint testPoint = testPoints[i];
                if (testPoint == null || string.IsNullOrWhiteSpace(testPoint.TestPointId))
                {
                    continue;
                }

                pointsById[testPoint.TestPointId] = testPoint.transform;
            }

            return pointsById;
        }

        private void AdvanceRouteIndex()
        {
            currentRouteIndex++;
            if (currentRouteIndex >= resolvedRoutePoints.Count)
            {
                if (loopRoute)
                {
                    currentRouteIndex = 0;
                }
                else
                {
                    currentRouteIndex = resolvedRoutePoints.Count - 1;
                    enabled = false;
                }
            }
        }

        #endregion
    }
}
