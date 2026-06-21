using UnityEngine;

// =============================================================================
// SCRIPT: CCS_HitscanWeaponRaycaster
// CATEGORY: Modules / Weapons / Runtime / Components
// PURPOSE: Camera-center hitscan ray with configurable spread for third-person weapons.
// PLACEMENT: Static utility. Invoked by CCS_RevolverController.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Muzzle transform is visual only. Gameplay ray uses camera center + forward.
// =============================================================================

namespace CCS.Modules.Weapons
{
    public static class CCS_HitscanWeaponRaycaster
    {
        #region Public Methods

        public static CCS_WeaponHitscanResult CastFromCamera(
            Camera aimCamera,
            float maxRange,
            float spreadDegrees,
            LayerMask hitMask,
            Transform ignoreRoot,
            bool drawDebugRay,
            float debugRayDuration = 0.35f)
        {
            if (aimCamera == null)
            {
                return new CCS_WeaponHitscanResult(
                    false,
                    Vector3.zero,
                    Vector3.up,
                    null,
                    0f,
                    Vector3.zero,
                    Vector3.forward);
            }

            Vector3 rayOrigin = aimCamera.transform.position;
            Vector3 rayDirection = ApplySpread(aimCamera.transform.forward, spreadDegrees);

            if (drawDebugRay)
            {
                Debug.DrawRay(rayOrigin, rayDirection * maxRange, Color.red, debugRayDuration);
            }

            RaycastHit[] hits = Physics.RaycastAll(rayOrigin, rayDirection, maxRange, hitMask, QueryTriggerInteraction.Ignore);
            System.Array.Sort(hits, (left, right) => left.distance.CompareTo(right.distance));

            for (int i = 0; i < hits.Length; i++)
            {
                RaycastHit hit = hits[i];
                if (hit.collider == null)
                {
                    continue;
                }

                if (ignoreRoot != null && hit.collider.transform.IsChildOf(ignoreRoot))
                {
                    continue;
                }

                return new CCS_WeaponHitscanResult(
                    true,
                    hit.point,
                    hit.normal,
                    hit.collider.gameObject,
                    hit.distance,
                    rayOrigin,
                    rayDirection);
            }

            return new CCS_WeaponHitscanResult(
                false,
                rayOrigin + (rayDirection * maxRange),
                Vector3.up,
                null,
                maxRange,
                rayOrigin,
                rayDirection);
        }

        #endregion

        #region Private Methods

        private static Vector3 ApplySpread(Vector3 forward, float spreadDegrees)
        {
            if (spreadDegrees <= 0.01f)
            {
                return forward.normalized;
            }

            float yaw = Random.Range(-spreadDegrees, spreadDegrees);
            float pitch = Random.Range(-spreadDegrees, spreadDegrees);
            Quaternion spreadRotation = Quaternion.Euler(pitch, yaw, 0f);
            return (spreadRotation * forward).normalized;
        }

        #endregion
    }
}
