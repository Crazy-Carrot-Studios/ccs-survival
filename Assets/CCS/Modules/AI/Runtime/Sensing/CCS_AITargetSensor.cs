using CCS.Modules.Attributes;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_AITargetSensor
// CATEGORY: Modules / AI / Runtime / Sensing
// PURPOSE: Finds nearest valid damageable target in radius for bandit AI.
// PLACEMENT: AI bandit root.
// AUTHOR: James Schilz
// CREATED: 2026-06-25
// NOTES: Filters dead/self targets; prefers nearest living CCS_IDamageable.
// =============================================================================

namespace CCS.Modules.AI
{
    public sealed class CCS_AITargetSensor : MonoBehaviour
    {
        [SerializeField] private float detectionRadius = 20f;
        [SerializeField] private LayerMask targetMask = ~0;
        [SerializeField] private bool enableSensorDebugLogs;

        private readonly Collider[] overlapBuffer = new Collider[48];
        private Transform cachedBestTarget;
        private CCS_IDamageable cachedBestDamageable;

        public Transform CurrentTargetTransform => cachedBestTarget;

        public CCS_IDamageable CurrentTargetDamageable => cachedBestDamageable;

        public bool TryAcquireTarget(out Transform targetTransform, out CCS_IDamageable damageable)
        {
            int overlapCount = Physics.OverlapSphereNonAlloc(
                transform.position,
                detectionRadius,
                overlapBuffer,
                targetMask,
                QueryTriggerInteraction.Ignore);

            Transform bestTarget = null;
            CCS_IDamageable bestDamageable = null;
            float bestDistanceSqr = float.PositiveInfinity;

            for (int i = 0; i < overlapCount; i++)
            {
                Collider candidateCollider = overlapBuffer[i];
                if (candidateCollider == null)
                {
                    continue;
                }

                Transform candidateTransform = candidateCollider.transform;
                if (candidateTransform == transform || candidateTransform.IsChildOf(transform))
                {
                    continue;
                }

                CCS_IDamageable candidateDamageable = candidateCollider.GetComponentInParent<CCS_IDamageable>();
                if (candidateDamageable == null || candidateDamageable.IsDead)
                {
                    continue;
                }

                Transform damageableTransform = (candidateDamageable as Component)?.transform;
                if (damageableTransform == null || damageableTransform == transform || damageableTransform.IsChildOf(transform))
                {
                    continue;
                }

                float distanceSqr = (damageableTransform.position - transform.position).sqrMagnitude;
                if (distanceSqr >= bestDistanceSqr)
                {
                    continue;
                }

                bestDistanceSqr = distanceSqr;
                bestTarget = damageableTransform;
                bestDamageable = candidateDamageable;
            }

            cachedBestTarget = bestTarget;
            cachedBestDamageable = bestDamageable;
            targetTransform = bestTarget;
            damageable = bestDamageable;

            if (enableSensorDebugLogs && bestTarget != null)
            {
                Debug.Log($"[AI] Target acquired: {bestTarget.name}", this);
            }

            return bestTarget != null;
        }

        public void Configure(float radius, LayerMask mask)
        {
            detectionRadius = Mathf.Max(0.1f, radius);
            targetMask = mask;
        }
    }
}
