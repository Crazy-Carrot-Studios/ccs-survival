using CCS.Modules.Attributes;
using CCS.Modules.CharacterController;
using CCS.Modules.Weapons;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_AIWeaponController
// CATEGORY: Modules / AI / Runtime / Combat
// PURPOSE: AI revolver firing bridge using CCS_WeaponShotResolver AIAimTarget mode.
// PLACEMENT: AI bandit root with CCS_RevolverController.
// AUTHOR: James Schilz
// CREATED: 2026-06-25
// NOTES: Server-authoritative fire cadence and damage dispatch via CCS_IDamageable.
// =============================================================================

namespace CCS.Modules.AI
{
    [DefaultExecutionOrder(125)]
    public sealed class CCS_AIWeaponController : MonoBehaviour
    {
        [SerializeField] private CCS_RevolverController revolverController;
        [SerializeField] private Transform muzzlePoint;
        [SerializeField] private CCS_RevolverUpperBodyAnimator upperBodyAnimator;
        [SerializeField] private bool enableWeaponDebugLogs;

        private float nextFireTime;

        public bool CanFire => Time.time >= nextFireTime;

        public void ConfigureInitialOwnership()
        {
            if (revolverController != null)
            {
                revolverController.ResetAmmoToFull();
                revolverController.SetWeaponOwnershipActive(true);
            }
        }

        public void SetAimHeld(bool aimHeld)
        {
            if (upperBodyAnimator != null)
            {
                upperBodyAnimator.SetRevolverAimHeldExternal(aimHeld, revolverOwned: true, isReloading: false);
            }
        }

        public bool TryFireAtTarget(
            Transform targetTransform,
            Vector3 targetPoint,
            float maxRange,
            float spreadDegrees,
            LayerMask hitMask,
            float fallbackDamage,
            float cooldownSeconds)
        {
            if (targetTransform == null || !CanFire)
            {
                return false;
            }

            ResolveReferences();
            if (muzzlePoint == null)
            {
                return false;
            }

            CCS_RevolverShotRequest shotRequest = new CCS_RevolverShotRequest(
                CCS_WeaponShotAimMode.AIAimTarget,
                aimCamera: null,
                viewportPoint: Vector2.zero,
                equippedVisualMuzzle: muzzlePoint,
                fallbackMuzzle: muzzlePoint,
                aiTargetPoint: targetPoint,
                maxRange: maxRange,
                spreadDegrees: spreadDegrees,
                hitMask: hitMask,
                ignoreRoot: transform.root,
                drawCameraRayDebug: false,
                drawMuzzleRayDebug: enableWeaponDebugLogs);

            CCS_RevolverShotResult shotResult = CCS_WeaponShotResolver.ResolveShot(shotRequest);
            if (!shotResult.Success)
            {
                return false;
            }

            nextFireTime = Time.time + Mathf.Max(0.05f, cooldownSeconds);
            ApplyDamageToHitTarget(shotResult.HitscanResult, fallbackDamage);

            if (enableWeaponDebugLogs)
            {
                Debug.Log(
                    $"[AI] Bandit fired at {targetTransform.name}. Hit={shotResult.HitscanResult.DidHit}",
                    this);
            }

            return true;
        }

        private void ResolveReferences()
        {
            if (revolverController == null)
            {
                revolverController = GetComponent<CCS_RevolverController>();
            }

            if (upperBodyAnimator == null)
            {
                upperBodyAnimator = GetComponentInChildren<CCS_RevolverUpperBodyAnimator>(true);
            }

            if (muzzlePoint == null)
            {
                muzzlePoint = revolverController != null ? revolverController.MuzzlePointTransform : null;
            }

            if (muzzlePoint == null)
            {
                Transform candidate = transform.Find(CCS_WeaponsConstants.MuzzlePointObjectName);
                if (candidate != null)
                {
                    muzzlePoint = candidate;
                }
            }
        }

        private static void ApplyDamageToHitTarget(CCS_WeaponHitscanResult hitscanResult, float fallbackDamage)
        {
            if (!hitscanResult.DidHit || hitscanResult.HitObject == null)
            {
                return;
            }

            CCS_IDamageable damageable = hitscanResult.HitObject.GetComponentInParent<CCS_IDamageable>();
            if (damageable == null || damageable.IsDead)
            {
                return;
            }

            CCS_DamageInfo damageInfo = new CCS_DamageInfo(
                Mathf.Max(0f, fallbackDamage),
                hitscanResult.HitPoint,
                -hitscanResult.HitNormal,
                CCS_DamageSourceType.AIRevolverShot,
                sourceObject: null,
                sourceNetworkObjectId: 0ul,
                attributeId: CCS_AttributesConstants.HealthAttributeId);
            damageable.ApplyDamage(damageInfo);
        }
    }
}
