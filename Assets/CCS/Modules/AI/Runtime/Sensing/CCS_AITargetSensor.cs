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
            return TryRefreshLivingTarget(out targetTransform, out damageable);
        }

        public bool TryRefreshLivingTarget(out Transform targetTransform, out CCS_IDamageable damageable)
        {
            if (TryAcquireTargetFromOverlap(out targetTransform, out damageable)
                || TryAcquireTargetFromNetworkHealthScan(out targetTransform, out damageable))
            {
                CacheTarget(targetTransform, damageable);
                return true;
            }

            cachedBestTarget = null;
            cachedBestDamageable = null;
            targetTransform = null;
            damageable = null;
            return false;
        }

        public void Configure(float radius, LayerMask mask)
        {
            detectionRadius = Mathf.Max(0.1f, radius);
            targetMask = mask;
        }

        private bool TryAcquireTargetFromOverlap(out Transform targetTransform, out CCS_IDamageable damageable)
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

                if (!TryEvaluateCandidate(candidateCollider.transform, ref bestTarget, ref bestDamageable, ref bestDistanceSqr))
                {
                    continue;
                }
            }

            targetTransform = bestTarget;
            damageable = bestDamageable;
            return bestTarget != null;
        }

        private bool TryAcquireTargetFromNetworkHealthScan(out Transform targetTransform, out CCS_IDamageable damageable)
        {
            CCS_NetworkHealth[] healthComponents = FindObjectsByType<CCS_NetworkHealth>(FindObjectsSortMode.None);
            Transform bestTarget = null;
            CCS_IDamageable bestDamageable = null;
            float bestDistanceSqr = float.PositiveInfinity;

            for (int i = 0; i < healthComponents.Length; i++)
            {
                CCS_NetworkHealth health = healthComponents[i];
                if (health == null)
                {
                    continue;
                }

                if (!TryEvaluateCandidate(health.transform, ref bestTarget, ref bestDamageable, ref bestDistanceSqr))
                {
                    continue;
                }
            }

            targetTransform = bestTarget;
            damageable = bestDamageable;
            return bestTarget != null;
        }

        private bool TryEvaluateCandidate(
            Transform candidateTransform,
            ref Transform bestTarget,
            ref CCS_IDamageable bestDamageable,
            ref float bestDistanceSqr)
        {
            if (candidateTransform == null
                || candidateTransform == transform
                || candidateTransform.IsChildOf(transform))
            {
                return false;
            }

            CCS_IDamageable candidateDamageable = candidateTransform.GetComponentInParent<CCS_IDamageable>();
            if (candidateDamageable == null || candidateDamageable.IsDead || !candidateDamageable.IsDamageReady)
            {
                return false;
            }

            Transform damageableTransform = (candidateDamageable as Component)?.transform;
            if (damageableTransform == null
                || damageableTransform == transform
                || damageableTransform.IsChildOf(transform))
            {
                return false;
            }

            float distanceSqr = (damageableTransform.position - transform.position).sqrMagnitude;
            if (distanceSqr > detectionRadius * detectionRadius || distanceSqr >= bestDistanceSqr)
            {
                return false;
            }

            bestDistanceSqr = distanceSqr;
            bestTarget = damageableTransform;
            bestDamageable = candidateDamageable;
            return true;
        }

        private void CacheTarget(Transform targetTransform, CCS_IDamageable damageable)
        {
            cachedBestTarget = targetTransform;
            cachedBestDamageable = damageable;

            if (enableSensorDebugLogs && targetTransform != null)
            {
                Debug.Log($"[AI] Target acquired: {targetTransform.name}", this);
            }
        }
    }
}
