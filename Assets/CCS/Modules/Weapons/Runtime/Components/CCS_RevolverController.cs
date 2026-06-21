using System;
using System.Collections;

using CCS.Modules.CharacterController;

using Unity.Netcode;

using UnityEngine;

// =============================================================================
// SCRIPT: CCS_RevolverController
// CATEGORY: Modules / Weapons / Runtime / Components
// PURPOSE: Test revolver hitscan controller with aim, fire, reload, and ammo state.
// PLACEMENT: PF_CCS_CharacterController_TestPlayer_Networked root or WeaponRoot child.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: v0.6.0 local-owner solo path. TODO: server-authoritative fire must validate
//        owner, cooldown, ammo, origin, range, and hit target before applying damage.
// =============================================================================

namespace CCS.Modules.Weapons
{
    [DefaultExecutionOrder(120)]
    public sealed class CCS_RevolverController : MonoBehaviour
    {
        #region Variables

        [Header("Definition")]
        [SerializeField] private CCS_RevolverDefinition revolverDefinition;

        [Header("References")]
        [SerializeField] private CCS_CharacterInputActionProvider inputProvider;
        [SerializeField] private Camera aimCamera;
        [SerializeField] private Transform muzzlePoint;
        [SerializeField] private bool enableWeaponDebugLogs;

        private NetworkObject cachedNetworkObject;
        private Coroutine reloadCoroutine;
        private int currentAmmo;
        private bool isReloading;
        private float lastFireTime = float.NegativeInfinity;
        private bool isAiming;

        #endregion

        #region Properties

        public CCS_RevolverDefinition RevolverDefinition => revolverDefinition;

        public int CurrentAmmo => currentAmmo;

        public int MaxAmmo => revolverDefinition != null ? revolverDefinition.CylinderCapacity : 0;

        public bool IsReloading => isReloading;

        public bool IsAiming => isAiming;

        #endregion

        #region Events

        public event Action<CCS_RevolverFireResultEvent> FireResolved;

        public event Action<CCS_RevolverDryFireEvent> DryFired;

        public event Action<CCS_RevolverStateChangedEvent> StateChanged;

        #endregion

        #region Unity Callbacks

        private void Awake()
        {
            ResolveReferences();
            ResetAmmoToFull();
        }

        private void OnDisable()
        {
            StopReloadCoroutine();
            isReloading = false;
            isAiming = false;
            RaiseStateChanged();
        }

        private void Update()
        {
            if (!IsLocalWeaponOwner() || revolverDefinition == null || inputProvider == null || !inputProvider.InputAccepted)
            {
                if (isAiming)
                {
                    isAiming = false;
                    RaiseStateChanged();
                }

                return;
            }

            UpdateAimState();
            HandleReloadInput();
            HandleFireInput();
        }

        #endregion

        #region Public Methods

        public void ResetAmmoToFull()
        {
            currentAmmo = revolverDefinition != null ? revolverDefinition.CylinderCapacity : 0;
            RaiseStateChanged();
        }

        #endregion

        #region Private Methods

        private void ResolveReferences()
        {
            if (inputProvider == null)
            {
                inputProvider = GetComponent<CCS_CharacterInputActionProvider>();
            }

            if (cachedNetworkObject == null)
            {
                cachedNetworkObject = GetComponentInParent<NetworkObject>();
            }
        }

        private void UpdateAimState()
        {
            bool nextAiming = inputProvider.AimHeld;
            if (isAiming == nextAiming)
            {
                return;
            }

            isAiming = nextAiming;
            RaiseStateChanged();
        }

        private void HandleReloadInput()
        {
            if (!inputProvider.ReloadPressed || isReloading || currentAmmo >= MaxAmmo)
            {
                return;
            }

            StartReload();
        }

        private void HandleFireInput()
        {
            if (!inputProvider.FirePressed)
            {
                return;
            }

            if (isReloading && !revolverDefinition.AllowFireWhileReloading)
            {
                return;
            }

            if (Time.time - lastFireTime < revolverDefinition.FireCooldownSeconds)
            {
                return;
            }

            if (currentAmmo <= 0)
            {
                HandleDryFire();
                return;
            }

            FireShot();
        }

        private void FireShot()
        {
            Camera resolvedCamera = ResolveAimCamera();
            if (resolvedCamera == null)
            {
                if (enableWeaponDebugLogs)
                {
                    Debug.LogWarning("[Weapons] Revolver fire skipped: no aim camera.", this);
                }

                return;
            }

            float spread = isAiming
                ? revolverDefinition.AimSpreadDegrees
                : revolverDefinition.HipSpreadDegrees;

            CCS_WeaponHitscanResult hitscanResult = CCS_HitscanWeaponRaycaster.CastFromCamera(
                resolvedCamera,
                revolverDefinition.MaxRange,
                spread,
                revolverDefinition.HitMask,
                transform.root,
                enableWeaponDebugLogs);

            currentAmmo--;
            lastFireTime = Time.time;
            RaiseStateChanged();

            if (enableWeaponDebugLogs)
            {
                string hitLabel = hitscanResult.DidHit && hitscanResult.HitObject != null
                    ? hitscanResult.HitObject.name
                    : "miss";
                Debug.Log(
                    $"[Weapons] Revolver fired. Ammo={currentAmmo}/{MaxAmmo} Hit={hitLabel}",
                    this);
            }

            ApplyDamageToHitTarget(hitscanResult);

            CCS_RevolverFireResultEvent fireEvent = new CCS_RevolverFireResultEvent(
                hitscanResult,
                currentAmmo,
                false);
            FireResolved?.Invoke(fireEvent);
        }

        private void HandleDryFire()
        {
            lastFireTime = Time.time;

            if (enableWeaponDebugLogs)
            {
                Debug.Log("[Weapons] Revolver dry fire.", this);
            }

            DryFired?.Invoke(new CCS_RevolverDryFireEvent(currentAmmo));
            FireResolved?.Invoke(new CCS_RevolverFireResultEvent(
                new CCS_WeaponHitscanResult(
                    false,
                    Vector3.zero,
                    Vector3.up,
                    null,
                    0f,
                    Vector3.zero,
                    Vector3.forward),
                currentAmmo,
                true));
        }

        private void ApplyDamageToHitTarget(CCS_WeaponHitscanResult hitscanResult)
        {
            if (!hitscanResult.DidHit || hitscanResult.HitObject == null)
            {
                return;
            }

            CCS_TestDamageTarget damageTarget = hitscanResult.HitObject.GetComponentInParent<CCS_TestDamageTarget>();
            if (damageTarget == null)
            {
                return;
            }

            damageTarget.ApplyWeaponDamage(revolverDefinition.Damage);
        }

        private void StartReload()
        {
            StopReloadCoroutine();
            isReloading = true;
            RaiseStateChanged();
            reloadCoroutine = StartCoroutine(ReloadAfterDuration());
        }

        private IEnumerator ReloadAfterDuration()
        {
            yield return new WaitForSeconds(revolverDefinition.ReloadSeconds);

            reloadCoroutine = null;
            isReloading = false;
            currentAmmo = MaxAmmo;
            RaiseStateChanged();

            if (enableWeaponDebugLogs)
            {
                Debug.Log($"[Weapons] Revolver reload complete. Ammo={currentAmmo}/{MaxAmmo}", this);
            }
        }

        private void StopReloadCoroutine()
        {
            if (reloadCoroutine == null)
            {
                return;
            }

            StopCoroutine(reloadCoroutine);
            reloadCoroutine = null;
        }

        private Camera ResolveAimCamera()
        {
            if (aimCamera != null && aimCamera.isActiveAndEnabled)
            {
                return aimCamera;
            }

            return CCS_CharacterMovementCameraContext.HasActiveCamera
                ? CCS_CharacterMovementCameraContext.ActiveCamera
                : null;
        }

        private void RaiseStateChanged()
        {
            StateChanged?.Invoke(new CCS_RevolverStateChangedEvent(
                currentAmmo,
                MaxAmmo,
                isAiming,
                isReloading));
        }

        private bool IsLocalWeaponOwner()
        {
            if (cachedNetworkObject == null)
            {
                cachedNetworkObject = GetComponentInParent<NetworkObject>();
            }

            NetworkObject networkObject = cachedNetworkObject;
            if (networkObject == null || !networkObject.IsSpawned)
            {
                return true;
            }

            return networkObject.IsOwner;
        }

        #endregion
    }
}
