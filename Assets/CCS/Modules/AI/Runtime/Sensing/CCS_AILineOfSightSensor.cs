using UnityEngine;

// =============================================================================
// SCRIPT: CCS_AILineOfSightSensor
// CATEGORY: Modules / AI / Runtime / Sensing
// PURPOSE: Validates unobstructed line-of-sight from AI eye point to target point.
// PLACEMENT: AI bandit root.
// AUTHOR: James Schilz
// CREATED: 2026-06-25
// NOTES: Uses raycast against visibility mask with self exclusion.
// =============================================================================

namespace CCS.Modules.AI
{
    public sealed class CCS_AILineOfSightSensor : MonoBehaviour
    {
        [SerializeField] private Transform eyeTransform;
        [SerializeField] private float maxDistance = 30f;
        [SerializeField] private LayerMask visibilityMask = ~0;
        [SerializeField] private bool debugLineOfSight;

        public bool HasLineOfSight(Transform target, Vector3 targetPoint)
        {
            if (target == null)
            {
                return false;
            }

            Vector3 origin = eyeTransform != null ? eyeTransform.position : transform.position + Vector3.up * 1.6f;
            Vector3 toTarget = targetPoint - origin;
            float distance = toTarget.magnitude;
            if (distance <= 0.0001f || distance > maxDistance)
            {
                return false;
            }

            Vector3 direction = toTarget / distance;
            bool blocked = Physics.Raycast(
                origin,
                direction,
                out RaycastHit hit,
                distance,
                visibilityMask,
                QueryTriggerInteraction.Ignore);

            if (debugLineOfSight)
            {
                Debug.DrawRay(origin, direction * distance, blocked ? Color.red : Color.green);
            }

            if (!blocked)
            {
                return true;
            }

            return hit.collider != null
                && (hit.collider.transform == target || hit.collider.transform.IsChildOf(target));
        }

        public void Configure(float nextMaxDistance, LayerMask nextMask)
        {
            maxDistance = Mathf.Max(0.1f, nextMaxDistance);
            visibilityMask = nextMask;
        }
    }
}
