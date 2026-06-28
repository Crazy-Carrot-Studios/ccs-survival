using UnityEngine;

// =============================================================================
// SCRIPT: CCS_DamageableLookupUtility
// CATEGORY: Modules / Attributes / Runtime / Utilities
// PURPOSE: Resolves CCS_IDamageable from hit colliders on child bones, hitboxes, or targets.
// PLACEMENT: Static utility used by weapon hitscan and AI combat damage routes.
// AUTHOR: James Schilz
// CREATED: 2026-06-25
// NOTES: Parent/root lookup supports ragdoll limbs and dedicated alive hitboxes.
// =============================================================================

namespace CCS.Modules.Attributes
{
    public static class CCS_DamageableLookupUtility
    {
        public static bool TryResolveDamageable(Collider hitCollider, out CCS_IDamageable damageable)
        {
            damageable = null;
            if (hitCollider == null)
            {
                return false;
            }

            damageable = hitCollider.GetComponentInParent<CCS_IDamageable>();
            if (damageable != null)
            {
                return true;
            }

            damageable = hitCollider.GetComponentInParent<CCS_NetworkHealth>();
            return damageable != null;
        }

        public static bool TryResolveDamageable(GameObject hitObject, out CCS_IDamageable damageable)
        {
            damageable = null;
            if (hitObject == null)
            {
                return false;
            }

            damageable = hitObject.GetComponentInParent<CCS_IDamageable>();
            if (damageable != null)
            {
                return true;
            }

            damageable = hitObject.GetComponentInParent<CCS_NetworkHealth>();
            return damageable != null;
        }
    }
}
