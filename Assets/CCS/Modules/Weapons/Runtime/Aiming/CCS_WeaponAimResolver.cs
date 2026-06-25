using UnityEngine;

// =============================================================================
// SCRIPT: CCS_WeaponAimResolver
// CATEGORY: Modules / Weapons / Runtime / Aiming
// PURPOSE: Resolves reticle-aligned aim point and muzzle-to-aim shot direction for weapons.
// PLACEMENT: Runtime utility invoked by CCS_RevolverController and hitscan raycaster.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Camera viewport center is aim source of truth. Muzzle ray may hit nearby cover first.
// =============================================================================

namespace CCS.Modules.Weapons
{
    public static class CCS_WeaponAimResolver
    {
        public static readonly Vector2 DefaultReticleViewportPoint = new Vector2(0.5f, 0.5f);

        public static CCS_WeaponAimSolution Resolve(
            Camera aimCamera,
            Vector2 viewportPoint,
            Transform equippedVisualMuzzle,
            Transform fallbackMuzzle,
            float maxRange,
            LayerMask hitMask,
            Transform ignoreRoot)
        {
            if (aimCamera == null)
            {
                return default;
            }

            Vector3 viewport = new Vector3(viewportPoint.x, viewportPoint.y, 0f);
            Ray cameraRay = aimCamera.ViewportPointToRay(viewport);
            bool hasCameraHit = TryRaycast(
                cameraRay,
                maxRange,
                hitMask,
                ignoreRoot,
                out RaycastHit cameraHit);
            Vector3 aimPoint = hasCameraHit
                ? cameraHit.point
                : cameraRay.origin + (cameraRay.direction * maxRange);

            bool usedVisualMuzzle = equippedVisualMuzzle != null;
            Transform muzzleTransform = usedVisualMuzzle ? equippedVisualMuzzle : fallbackMuzzle;
            Vector3 muzzleOrigin = muzzleTransform != null
                ? muzzleTransform.position
                : cameraRay.origin;
            Vector3 muzzleToAimDirection = aimPoint - muzzleOrigin;
            if (muzzleToAimDirection.sqrMagnitude <= 0.0001f)
            {
                muzzleToAimDirection = cameraRay.direction;
            }
            else
            {
                muzzleToAimDirection.Normalize();
            }

            float distance = Vector3.Distance(muzzleOrigin, aimPoint);
            return new CCS_WeaponAimSolution(
                hasCameraHit,
                cameraRay.origin,
                cameraRay.direction,
                aimPoint,
                muzzleOrigin,
                muzzleToAimDirection,
                distance,
                hasCameraHit ? cameraHit : default,
                usedVisualMuzzle);
        }

        public static CCS_WeaponHitscanResult ResolveMuzzleAuthoritativeHitscan(
            Transform equippedVisualMuzzle,
            Transform fallbackMuzzle,
            float maxRange,
            float spreadDegrees,
            LayerMask hitMask,
            Transform ignoreRoot,
            bool drawMuzzleRayDebug,
            float debugRayDuration = 0.35f)
        {
            Transform muzzleTransform = equippedVisualMuzzle != null ? equippedVisualMuzzle : fallbackMuzzle;
            if (muzzleTransform == null)
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

            Vector3 origin = muzzleTransform.position;
            Vector3 direction = muzzleTransform.forward.normalized;
            Vector3 shotDirection = ApplySpread(direction, spreadDegrees);

            if (drawMuzzleRayDebug)
            {
                Debug.DrawRay(origin, shotDirection * maxRange, Color.blue, debugRayDuration);
            }

            if (TryRaycast(
                    new Ray(origin, shotDirection),
                    maxRange,
                    hitMask,
                    ignoreRoot,
                    out RaycastHit muzzleHit))
            {
                return new CCS_WeaponHitscanResult(
                    true,
                    muzzleHit.point,
                    muzzleHit.normal,
                    muzzleHit.collider.gameObject,
                    muzzleHit.distance,
                    origin,
                    shotDirection);
            }

            Vector3 missPoint = origin + (shotDirection * maxRange);
            DrawDebugPoint(missPoint, Color.red, 0.05f, debugRayDuration);
            return new CCS_WeaponHitscanResult(
                false,
                missPoint,
                Vector3.up,
                null,
                maxRange,
                origin,
                shotDirection);
        }

        public static CCS_WeaponAimSolution ResolveMuzzleForward(
            Transform equippedVisualMuzzle,
            Transform fallbackMuzzle,
            float maxRange,
            LayerMask hitMask,
            Transform ignoreRoot)
        {
            Transform muzzleTransform = equippedVisualMuzzle != null ? equippedVisualMuzzle : fallbackMuzzle;
            if (muzzleTransform == null)
            {
                return default;
            }

            Vector3 origin = muzzleTransform.position;
            Vector3 direction = muzzleTransform.forward.normalized;
            bool hasHit = TryRaycast(
                new Ray(origin, direction),
                maxRange,
                hitMask,
                ignoreRoot,
                out RaycastHit hit);
            Vector3 aimPoint = hasHit
                ? hit.point
                : origin + (direction * maxRange);

            return new CCS_WeaponAimSolution(
                hasHit,
                origin,
                direction,
                aimPoint,
                origin,
                direction,
                Vector3.Distance(origin, aimPoint),
                hasHit ? hit : default,
                equippedVisualMuzzle != null);
        }

        public static CCS_WeaponHitscanResult ResolveHitscan(
            Camera aimCamera,
            Vector2 viewportPoint,
            Transform equippedVisualMuzzle,
            Transform fallbackMuzzle,
            float maxRange,
            float spreadDegrees,
            LayerMask hitMask,
            Transform ignoreRoot,
            bool drawCameraRayDebug,
            bool drawMuzzleRayDebug,
            float debugRayDuration = 0.35f)
        {
            CCS_WeaponAimSolution aimSolution = Resolve(
                aimCamera,
                viewportPoint,
                equippedVisualMuzzle,
                fallbackMuzzle,
                maxRange,
                hitMask,
                ignoreRoot);

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

            Vector3 shotDirection = ApplySpread(aimSolution.MuzzleToAimDirection, spreadDegrees);

            if (drawCameraRayDebug)
            {
                Debug.DrawRay(
                    aimSolution.CameraRayOrigin,
                    aimSolution.CameraRayDirection * maxRange,
                    Color.green,
                    debugRayDuration);
            }

            if (drawMuzzleRayDebug)
            {
                Debug.DrawRay(aimSolution.MuzzleOrigin, shotDirection * maxRange, Color.blue, debugRayDuration);
                DrawDebugPoint(aimSolution.AimPoint, Color.red, 0.05f, debugRayDuration);
            }

            if (TryRaycast(
                    new Ray(aimSolution.MuzzleOrigin, shotDirection),
                    maxRange,
                    hitMask,
                    ignoreRoot,
                    out RaycastHit muzzleHit))
            {
                return new CCS_WeaponHitscanResult(
                    true,
                    muzzleHit.point,
                    muzzleHit.normal,
                    muzzleHit.collider.gameObject,
                    muzzleHit.distance,
                    aimSolution.MuzzleOrigin,
                    shotDirection);
            }

            if (aimSolution.HasCameraHit)
            {
                RaycastHit cameraHit = aimSolution.Hit;
                return new CCS_WeaponHitscanResult(
                    true,
                    cameraHit.point,
                    cameraHit.normal,
                    cameraHit.collider.gameObject,
                    Vector3.Distance(aimSolution.MuzzleOrigin, cameraHit.point),
                    aimSolution.MuzzleOrigin,
                    shotDirection);
            }

            return new CCS_WeaponHitscanResult(
                false,
                aimSolution.AimPoint,
                Vector3.up,
                null,
                maxRange,
                aimSolution.MuzzleOrigin,
                shotDirection);
        }

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

        private static void DrawDebugPoint(Vector3 position, Color color, float size, float duration)
        {
            Debug.DrawLine(position - Vector3.right * size, position + Vector3.right * size, color, duration);
            Debug.DrawLine(position - Vector3.up * size, position + Vector3.up * size, color, duration);
            Debug.DrawLine(position - Vector3.forward * size, position + Vector3.forward * size, color, duration);
        }
    }
}
