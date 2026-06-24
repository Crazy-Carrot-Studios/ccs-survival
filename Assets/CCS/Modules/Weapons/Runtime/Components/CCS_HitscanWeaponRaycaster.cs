using UnityEngine;

// =============================================================================
// SCRIPT: CCS_HitscanWeaponRaycaster
// CATEGORY: Modules / Weapons / Runtime / Components
// PURPOSE: Camera-center aim selection with muzzle-origin shot validation for third-person weapons.
// PLACEMENT: Static utility. Invoked by CCS_RevolverController via CCS_WeaponAimResolver.
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
            return CCS_WeaponAimResolver.ResolveHitscan(
                aimCamera,
                CCS_WeaponAimResolver.DefaultReticleViewportPoint,
                equippedVisualMuzzle: null,
                fallbackMuzzle: muzzlePoint,
                maxRange,
                spreadDegrees,
                hitMask,
                ignoreRoot,
                drawCameraRayDebug: drawDebugRay,
                drawMuzzleRayDebug: drawMuzzleDebug,
                debugRayDuration);
        }

        public static CCS_WeaponHitscanResult CastFromAimResolver(
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
            return CCS_WeaponAimResolver.ResolveHitscan(
                aimCamera,
                viewportPoint,
                equippedVisualMuzzle,
                fallbackMuzzle,
                maxRange,
                spreadDegrees,
                hitMask,
                ignoreRoot,
                drawCameraRayDebug,
                drawMuzzleRayDebug,
                debugRayDuration);
        }

        #endregion
    }
}
