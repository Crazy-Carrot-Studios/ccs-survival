using UnityEngine;

// =============================================================================
// SCRIPT: CCS_WeaponHitscanResult
// CATEGORY: Modules / Weapons / Runtime / Data
// PURPOSE: Immutable hitscan ray result payload for weapon fire events.
// PLACEMENT: Runtime data struct. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Gameplay ray originates from camera center for third-person aiming.
// =============================================================================

namespace CCS.Modules.Weapons
{
    public readonly struct CCS_WeaponHitscanResult
    {
        public CCS_WeaponHitscanResult(
            bool didHit,
            Vector3 hitPoint,
            Vector3 hitNormal,
            GameObject hitObject,
            float hitDistance,
            Vector3 rayOrigin,
            Vector3 rayDirection)
        {
            DidHit = didHit;
            HitPoint = hitPoint;
            HitNormal = hitNormal;
            HitObject = hitObject;
            HitDistance = hitDistance;
            RayOrigin = rayOrigin;
            RayDirection = rayDirection;
        }

        public bool DidHit { get; }

        public Vector3 HitPoint { get; }

        public Vector3 HitNormal { get; }

        public GameObject HitObject { get; }

        public float HitDistance { get; }

        public Vector3 RayOrigin { get; }

        public Vector3 RayDirection { get; }
    }
}
