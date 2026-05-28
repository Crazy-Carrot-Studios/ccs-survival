using UnityEngine;

// =============================================================================
// SCRIPT: CCS_TraversalTestWaypoint
// CATEGORY: Survival / Testing / Traversal
// PURPOSE: Marker for automated traversal test route points with arrival tolerance and optional wait.
// PLACEMENT: Child objects under CCS_PrototypeTraversalRoute in SCN_CCS_Survival_Bootstrap.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Dev/test-only. Not gameplay AI. No Input System dependency.
// =============================================================================

namespace CCS.Survival.Testing.Traversal
{
    public sealed class CCS_TraversalTestWaypoint : MonoBehaviour
    {
        #region Variables

        [Header("Arrival")]
        [Tooltip("Distance from the agent required to count as arrived at this waypoint.")]
        [SerializeField] private float arrivalRadius = 0.5f;

        [Tooltip("Extra vertical tolerance when checking arrival (helps on stairs and ramps).")]
        [SerializeField] private float verticalArrivalTolerance = 1.25f;

        [Header("Timing")]
        [Tooltip("Seconds to wait at this waypoint after arrival before advancing.")]
        [SerializeField] private float waitDurationSeconds;

        #endregion

        #region Properties

        public float ArrivalRadius => arrivalRadius;

        public float VerticalArrivalTolerance => verticalArrivalTolerance;

        public float WaitDurationSeconds => waitDurationSeconds;

        public Vector3 WorldPosition => transform.position;

        #endregion

        #region Unity Callbacks

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.2f, 0.85f, 1f, 0.85f);
            Gizmos.DrawWireSphere(transform.position, arrivalRadius);
            Gizmos.color = new Color(0.2f, 0.85f, 1f, 0.25f);
            Gizmos.DrawSphere(transform.position, arrivalRadius * 0.15f);
        }
#endif

        #endregion
    }
}
