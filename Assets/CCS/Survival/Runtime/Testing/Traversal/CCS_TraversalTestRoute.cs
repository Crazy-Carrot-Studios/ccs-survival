using System.Text;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_TraversalTestRoute
// CATEGORY: Survival / Testing / Traversal
// PURPOSE: Ordered traversal test route with validation and optional loop for the prototype course.
// PLACEMENT: Attach to CCS_PrototypeTraversalRoute in SCN_CCS_Survival_Bootstrap.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Dev/test-only. Uses serialized waypoint references only.
// =============================================================================

namespace CCS.Survival.Testing.Traversal
{
    public sealed class CCS_TraversalTestRoute : MonoBehaviour
    {
        #region Variables

        [Header("Route")]
        [Tooltip("Ordered waypoints for the automated traversal test.")]
        [SerializeField] private CCS_TraversalTestWaypoint[] orderedWaypoints;

        [Tooltip("When enabled, the route restarts at the first waypoint after the last.")]
        [SerializeField] private bool loopRoute = true;

#if UNITY_EDITOR
        [Header("Gizmos")]
        [Tooltip("Draw route lines and waypoint markers in the Scene view when selected.")]
        [SerializeField] private bool drawRouteGizmos = true;
#endif

        #endregion

        #region Properties

        public bool LoopRoute => loopRoute;

        public int WaypointCount => orderedWaypoints != null ? orderedWaypoints.Length : 0;

        #endregion

        #region Public Methods

        public bool TryGetWaypoint(int index, out CCS_TraversalTestWaypoint waypoint)
        {
            waypoint = null;

            if (orderedWaypoints == null || index < 0 || index >= orderedWaypoints.Length)
            {
                return false;
            }

            waypoint = orderedWaypoints[index];
            return waypoint != null;
        }

        public bool ValidateRoute(out string validationMessage)
        {
            StringBuilder builder = new StringBuilder();

            if (orderedWaypoints == null || orderedWaypoints.Length == 0)
            {
                validationMessage = "Route has no waypoints assigned.";
                return false;
            }

            int validCount = 0;
            for (int i = 0; i < orderedWaypoints.Length; i++)
            {
                if (orderedWaypoints[i] == null)
                {
                    builder.Append($"Waypoint index {i} is null. ");
                    continue;
                }

                validCount++;
            }

            if (validCount == 0)
            {
                validationMessage = "Route has no valid waypoint references.";
                return false;
            }

            if (validCount < orderedWaypoints.Length)
            {
                validationMessage = $"Route has {validCount}/{orderedWaypoints.Length} valid waypoints. {builder}";
                return false;
            }

            validationMessage = $"Route valid ({validCount} waypoints).";
            return true;
        }

        #endregion

        #region Unity Callbacks

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (!drawRouteGizmos || orderedWaypoints == null || orderedWaypoints.Length == 0)
            {
                return;
            }

            Gizmos.color = new Color(0.2f, 0.85f, 1f, 0.9f);

            Vector3? previousPosition = null;
            for (int i = 0; i < orderedWaypoints.Length; i++)
            {
                CCS_TraversalTestWaypoint waypoint = orderedWaypoints[i];
                if (waypoint == null)
                {
                    continue;
                }

                Vector3 currentPosition = waypoint.WorldPosition;
                Gizmos.DrawWireSphere(currentPosition, waypoint.ArrivalRadius);

                if (previousPosition.HasValue)
                {
                    Gizmos.DrawLine(previousPosition.Value, currentPosition);
                }

                previousPosition = currentPosition;
            }

            if (loopRoute && previousPosition.HasValue && orderedWaypoints[0] != null)
            {
                Gizmos.color = new Color(0.2f, 0.85f, 1f, 0.35f);
                Gizmos.DrawLine(previousPosition.Value, orderedWaypoints[0].WorldPosition);
            }
        }
#endif

        #endregion
    }
}
