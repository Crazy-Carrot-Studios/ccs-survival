using UnityEngine;

// =============================================================================
// SCRIPT: CCS_WeaponAimSolution
// CATEGORY: Modules / Weapons / Runtime / Aiming
// PURPOSE: Shared aim data from camera reticle through muzzle toward aim point.
// PLACEMENT: Runtime struct produced by CCS_WeaponAimResolver.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Camera reticle is aim source of truth; muzzle/tracer follow resolved aim point.
// =============================================================================

namespace CCS.Modules.Weapons
{
    public readonly struct CCS_WeaponAimSolution
    {
        public CCS_WeaponAimSolution(
            bool hasCameraHit,
            Vector3 cameraRayOrigin,
            Vector3 cameraRayDirection,
            Vector3 aimPoint,
            Vector3 muzzleOrigin,
            Vector3 muzzleToAimDirection,
            float distance,
            RaycastHit hit,
            bool usedVisualMuzzle)
        {
            HasCameraHit = hasCameraHit;
            CameraRayOrigin = cameraRayOrigin;
            CameraRayDirection = cameraRayDirection;
            AimPoint = aimPoint;
            MuzzleOrigin = muzzleOrigin;
            MuzzleToAimDirection = muzzleToAimDirection;
            Distance = distance;
            Hit = hit;
            UsedVisualMuzzle = usedVisualMuzzle;
        }

        public bool HasCameraHit { get; }

        public Vector3 CameraRayOrigin { get; }

        public Vector3 CameraRayDirection { get; }

        public Vector3 AimPoint { get; }

        public Vector3 MuzzleOrigin { get; }

        public Vector3 MuzzleToAimDirection { get; }

        public float Distance { get; }

        public RaycastHit Hit { get; }

        public bool UsedVisualMuzzle { get; }
    }
}
