using UnityEngine;

// =============================================================================
// SCRIPT: CCS_WeaponShotResolver
// CATEGORY: Modules / Weapons / Runtime / Aiming
// PURPOSE: Shared shot aim resolution for player camera-center and AI target modes.
// PLACEMENT: Used by CCS_RevolverController and future AI weapon controllers.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Default player mode uses camera viewport center, not raw muzzle forward.
// =============================================================================

namespace CCS.Modules.Weapons
{
    public enum CCS_WeaponShotAimMode
    {
        LocalPlayerCameraCenter = 0,
        AIAimTarget = 1,
        DebugMuzzleForwardOnly = 2,
    }

    public readonly struct CCS_WeaponAimPoint
    {
        public Vector3 AimPoint { get; }

        public Vector3 MuzzleOrigin { get; }

        public Vector3 ShotDirectionFromMuzzle { get; }

        public bool HasCameraHit { get; }

        public CCS_WeaponAimPoint(
            Vector3 aimPoint,
            Vector3 muzzleOrigin,
            Vector3 shotDirectionFromMuzzle,
            bool hasCameraHit)
        {
            AimPoint = aimPoint;
            MuzzleOrigin = muzzleOrigin;
            ShotDirectionFromMuzzle = shotDirectionFromMuzzle;
            HasCameraHit = hasCameraHit;
        }
    }

    public readonly struct CCS_RevolverShotRequest
    {
        public CCS_WeaponShotAimMode AimMode { get; }

        public Camera AimCamera { get; }

        public Vector2 ViewportPoint { get; }

        public Transform EquippedVisualMuzzle { get; }

        public Transform FallbackMuzzle { get; }

        public Vector3 AiTargetPoint { get; }

        public float MaxRange { get; }

        public float SpreadDegrees { get; }

        public LayerMask HitMask { get; }

        public Transform IgnoreRoot { get; }

        public bool DrawCameraRayDebug { get; }

        public bool DrawMuzzleRayDebug { get; }

        public float DebugRayDuration { get; }

        public CCS_RevolverShotRequest(
            CCS_WeaponShotAimMode aimMode,
            Camera aimCamera,
            Vector2 viewportPoint,
            Transform equippedVisualMuzzle,
            Transform fallbackMuzzle,
            Vector3 aiTargetPoint,
            float maxRange,
            float spreadDegrees,
            LayerMask hitMask,
            Transform ignoreRoot,
            bool drawCameraRayDebug = false,
            bool drawMuzzleRayDebug = false,
            float debugRayDuration = 0.35f)
        {
            AimMode = aimMode;
            AimCamera = aimCamera;
            ViewportPoint = viewportPoint;
            EquippedVisualMuzzle = equippedVisualMuzzle;
            FallbackMuzzle = fallbackMuzzle;
            AiTargetPoint = aiTargetPoint;
            MaxRange = maxRange;
            SpreadDegrees = spreadDegrees;
            HitMask = hitMask;
            IgnoreRoot = ignoreRoot;
            DrawCameraRayDebug = drawCameraRayDebug;
            DrawMuzzleRayDebug = drawMuzzleRayDebug;
            DebugRayDuration = debugRayDuration;
        }
    }

    public readonly struct CCS_RevolverShotResult
    {
        public CCS_WeaponAimSolution AimSolution { get; }

        public CCS_WeaponHitscanResult HitscanResult { get; }

        public bool Success { get; }

        public CCS_RevolverShotResult(
            CCS_WeaponAimSolution aimSolution,
            CCS_WeaponHitscanResult hitscanResult,
            bool success)
        {
            AimSolution = aimSolution;
            HitscanResult = hitscanResult;
            Success = success;
        }
    }

    public static class CCS_WeaponShotResolver
    {
        public static CCS_RevolverShotResult ResolveShot(in CCS_RevolverShotRequest request)
        {
            switch (request.AimMode)
            {
                case CCS_WeaponShotAimMode.DebugMuzzleForwardOnly:
                    return ResolveMuzzleForwardShot(request);
                case CCS_WeaponShotAimMode.AIAimTarget:
                    return ResolveAiTargetShot(request);
                default:
                    return ResolveCameraCenterShot(request);
            }
        }

        public static CCS_WeaponAimPoint ResolveAimPoint(in CCS_RevolverShotRequest request)
        {
            CCS_RevolverShotResult result = ResolveShot(request);
            if (!result.Success)
            {
                return default;
            }

            return new CCS_WeaponAimPoint(
                result.AimSolution.AimPoint,
                result.AimSolution.MuzzleOrigin,
                result.HitscanResult.RayDirection,
                result.AimSolution.HasCameraHit);
        }

        private static CCS_RevolverShotResult ResolveCameraCenterShot(in CCS_RevolverShotRequest request)
        {
            if (request.AimCamera == null)
            {
                return default;
            }

            CCS_WeaponAimSolution aimSolution = CCS_WeaponAimResolver.Resolve(
                request.AimCamera,
                request.ViewportPoint,
                request.EquippedVisualMuzzle,
                request.FallbackMuzzle,
                request.MaxRange,
                request.HitMask,
                request.IgnoreRoot);

            CCS_WeaponHitscanResult hitscanResult = CCS_HitscanWeaponRaycaster.CastFromAimResolver(
                request.AimCamera,
                request.ViewportPoint,
                request.EquippedVisualMuzzle,
                request.FallbackMuzzle,
                request.MaxRange,
                request.SpreadDegrees,
                request.HitMask,
                request.IgnoreRoot,
                request.DrawCameraRayDebug,
                request.DrawMuzzleRayDebug,
                request.DebugRayDuration);

            return new CCS_RevolverShotResult(aimSolution, hitscanResult, true);
        }

        private static CCS_RevolverShotResult ResolveMuzzleForwardShot(in CCS_RevolverShotRequest request)
        {
            CCS_WeaponAimSolution aimSolution = CCS_WeaponAimResolver.ResolveMuzzleForward(
                request.EquippedVisualMuzzle,
                request.FallbackMuzzle,
                request.MaxRange,
                request.HitMask,
                request.IgnoreRoot);

            CCS_WeaponHitscanResult hitscanResult = CCS_WeaponAimResolver.ResolveMuzzleAuthoritativeHitscan(
                request.EquippedVisualMuzzle,
                request.FallbackMuzzle,
                request.MaxRange,
                request.SpreadDegrees,
                request.HitMask,
                request.IgnoreRoot,
                request.DrawMuzzleRayDebug,
                request.DebugRayDuration);

            return new CCS_RevolverShotResult(aimSolution, hitscanResult, aimSolution.MuzzleOrigin != Vector3.zero);
        }

        private static CCS_RevolverShotResult ResolveAiTargetShot(in CCS_RevolverShotRequest request)
        {
            Transform muzzleTransform = request.EquippedVisualMuzzle != null
                ? request.EquippedVisualMuzzle
                : request.FallbackMuzzle;
            if (muzzleTransform == null)
            {
                return default;
            }

            Vector3 muzzleOrigin = muzzleTransform.position;
            Vector3 shotDirection = request.AiTargetPoint - muzzleOrigin;
            if (shotDirection.sqrMagnitude <= 0.0001f)
            {
                shotDirection = muzzleTransform.forward;
            }
            else
            {
                shotDirection.Normalize();
            }

            shotDirection = ApplySpread(shotDirection, request.SpreadDegrees);
            bool hasHit = TryRaycast(
                new Ray(muzzleOrigin, shotDirection),
                request.MaxRange,
                request.HitMask,
                request.IgnoreRoot,
                out RaycastHit hit);
            Vector3 aimPoint = hasHit ? hit.point : muzzleOrigin + (shotDirection * request.MaxRange);

            CCS_WeaponAimSolution aimSolution = new CCS_WeaponAimSolution(
                hasHit,
                muzzleOrigin,
                shotDirection,
                aimPoint,
                muzzleOrigin,
                shotDirection,
                Vector3.Distance(muzzleOrigin, aimPoint),
                hasHit ? hit : default,
                request.EquippedVisualMuzzle != null);

            CCS_WeaponHitscanResult hitscanResult = hasHit
                ? new CCS_WeaponHitscanResult(
                    true,
                    hit.point,
                    hit.normal,
                    hit.collider.gameObject,
                    hit.distance,
                    muzzleOrigin,
                    shotDirection)
                : new CCS_WeaponHitscanResult(
                    false,
                    aimPoint,
                    Vector3.up,
                    null,
                    request.MaxRange,
                    muzzleOrigin,
                    shotDirection);

            return new CCS_RevolverShotResult(aimSolution, hitscanResult, true);
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
    }
}
