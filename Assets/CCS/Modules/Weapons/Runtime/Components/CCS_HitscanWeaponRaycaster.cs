using UnityEngine;

// =============================================================================
// SCRIPT: CCS_HitscanWeaponRaycaster
// CATEGORY: Modules / Weapons / Runtime / Components
// PURPOSE: Camera-center aim selection with muzzle-origin shot validation for third-person weapons.
// PLACEMENT: Static utility. Invoked by CCS_RevolverController.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Reticle uses viewport center ray. Gameplay hit follows camera aim. Tracer starts at muzzle.
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
            return CastFromCameraCenter(
                aimCamera,
                muzzlePoint: null,
                maxRange,
                spreadDegrees,
                hitMask,
                ignoreRoot,
                drawDebugRay,
                drawMuzzleDebug: false,
                debugRayDuration);
        }

        public static CCS_WeaponHitscanResult CastFromCameraCenter(
            Camera aimCamera,
            Transform muzzlePoint,
            float maxRange,
            float spreadDegrees,
            LayerMask hitMask,
            Transform ignoreRoot,
            bool drawDebugRay,
            bool drawMuzzleDebug,
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

            Ray cameraAimRay = aimCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            Vector3 targetPoint = cameraAimRay.origin + (cameraAimRay.direction * maxRange);
            bool didCameraHit = TryRaycast(cameraAimRay, maxRange, hitMask, ignoreRoot, out RaycastHit cameraHit);
            if (didCameraHit)
            {
                targetPoint = cameraHit.point;
            }

            Vector3 muzzleOrigin = muzzlePoint != null
                ? muzzlePoint.position
                : cameraAimRay.origin;
            Vector3 shotDirection = ApplySpread((targetPoint - muzzleOrigin).normalized, spreadDegrees);

            if (drawDebugRay)
            {
                Debug.DrawRay(cameraAimRay.origin, cameraAimRay.direction * maxRange, Color.cyan, debugRayDuration);
            }

            if (drawMuzzleDebug)
            {
                Debug.DrawRay(muzzleOrigin, shotDirection * maxRange, Color.yellow, debugRayDuration);
            }

            if (TryRaycast(new Ray(muzzleOrigin, shotDirection), maxRange, hitMask, ignoreRoot, out RaycastHit muzzleHit))
            {
                return new CCS_WeaponHitscanResult(
                    true,
                    muzzleHit.point,
                    muzzleHit.normal,
                    muzzleHit.collider.gameObject,
                    muzzleHit.distance,
                    muzzleOrigin,
                    shotDirection);
            }

            if (didCameraHit)
            {
                return new CCS_WeaponHitscanResult(
                    true,
                    cameraHit.point,
                    cameraHit.normal,
                    cameraHit.collider.gameObject,
                    Vector3.Distance(muzzleOrigin, cameraHit.point),
                    muzzleOrigin,
                    shotDirection);
            }

            return new CCS_WeaponHitscanResult(
                false,
                targetPoint,
                Vector3.up,
                null,
                maxRange,
                muzzleOrigin,
                shotDirection);
        }

        #endregion

        #region Private Methods

        private static bool TryRaycast(
            Ray ray,
            float maxRange,
            LayerMask hitMask,
            Transform ignoreRoot,
            out RaycastHit closestHit)
        {
            closestHit = default;
            RaycastHit[] hits = Physics.RaycastAll(
                ray.origin,
                ray.direction,
                maxRange,
                hitMask,
                QueryTriggerInteraction.Ignore);
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

                closestHit = hit;
                return true;
            }

            return false;
        }

        private static Vector3 ApplySpread(Vector3 forward, float spreadDegrees)
        {
            if (spreadDegrees <= 0.01f || forward.sqrMagnitude <= 0.0001f)
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
