using System;
using System.Collections;

using CCS.Modules.CharacterController;

using Unity.Netcode;

using UnityEngine;
using UnityEngine.Serialization;

// =============================================================================
// SCRIPT: CCS_RevolverController
// CATEGORY: Modules / Weapons / Runtime / Components
// PURPOSE: Test revolver hitscan controller with aim, fire, reload, and ammo state.
// PLACEMENT: PF_CCS_CharacterController_TestPlayer_Networked root or WeaponRoot child.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: v0.6.1 local-owner solo path with scene aim camera integration.
//        TODO: server-authoritative fire must validate owner, cooldown, ammo,
//        origin, range, and hit target before applying damage.
// =============================================================================

namespace CCS.Modules.Weapons
{
    [DefaultExecutionOrder(120)]
    public sealed class CCS_RevolverController : MonoBehaviour, CCS_IRevolverAnimationState
    {
        #region Variables

        [Header("Definition")]
        [SerializeField] private CCS_RevolverDefinition revolverDefinition;

        [Header("References")]
        [SerializeField] private CCS_CharacterInputActionProvider inputProvider;
        [SerializeField] private CCS_CharacterAimLocomotionController aimLocomotionController;
        [SerializeField] private CCS_CharacterCameraController sceneCameraController;
        [SerializeField] private Camera aimCamera;
        [SerializeField] private Transform muzzlePoint;
        [FormerlySerializedAs("enableWeaponDebugLogs")]
        [SerializeField] private bool enableRuntimeWeaponDebug;
        [SerializeField] private bool enableAimRayDebug;
        [SerializeField] private bool enableMuzzleDebug;

        private NetworkObject cachedNetworkObject;
        private Coroutine reloadCoroutine;
        private int currentAmmo;
        private bool isReloading;
        private float lastFireTime = float.NegativeInfinity;
        private bool loggedMissingInputProvider;
        private bool loggedMissingDefinition;
        private bool loggedMissingMuzzlePoint;
        private bool loggedMissingSceneCamera;
        private bool weaponOwnershipActive;

        #endregion

        #region Properties

        public CCS_RevolverDefinition RevolverDefinition => revolverDefinition;

        public int CurrentAmmo => currentAmmo;

        public int MaxAmmo => revolverDefinition != null ? revolverDefinition.CylinderCapacity : 0;

        public bool IsReloading => isReloading;

        public bool IsAiming =>
            aimLocomotionController != null && aimLocomotionController.IsAimMovementActive;

        public bool RevolverAimHeld => IsAiming;

        public bool HasWeaponOwnership => weaponOwnershipActive;

        public Transform MuzzlePointTransform => muzzlePoint;

        public bool RevolverIsReloading => isReloading;

        #endregion

        #region Events

        public event Action RevolverFired;

        public event Action RevolverReloadStarted;

        public event Action RevolverReloadCompleted;

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
            RaiseStateChanged();
        }

        private void Update()
        {
            if (!IsLocalWeaponOwner())
            {
                return;
            }

            LogMissingSetupOnce();

            if (revolverDefinition == null || inputProvider == null || !inputProvider.InputAccepted)
            {
                return;
            }

            SyncAimStateForEvents();

            if (!weaponOwnershipActive)
            {
                return;
            }

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

        public void SetWeaponOwnershipActive(bool active)
        {
            weaponOwnershipActive = active;
            RaiseStateChanged();
        }

        public void SetMuzzlePoint(Transform nextMuzzlePoint)
        {
            muzzlePoint = nextMuzzlePoint;
        }

        public void ConfigureSceneWeaponCamera(
            CCS_CharacterCameraController cameraController,
            Camera outputCamera)
        {
            sceneCameraController = cameraController;
            if (outputCamera != null)
            {
                aimCamera = outputCamera;
            }

            if (aimLocomotionController != null)
            {
                aimLocomotionController.ConfigureSceneCamera(cameraController);
            }

            if (enableRuntimeWeaponDebug)
            {
                if (sceneCameraController == null)
                {
                    Debug.LogWarning("[Weapons] No scene camera controller assigned for revolver aim.", this);
                }
                else if (!sceneCameraController.HasAimCameraConfigured)
                {
                    Debug.LogWarning("[Weapons] Scene camera rig is missing CinemachineCamera_Aim.", this);
                }
                else
                {
                    Debug.Log("[Weapons] Revolver scene camera configured.", this);
                }
            }
        }

        #endregion

        #region Private Methods

        private void ResolveReferences()
        {
            if (inputProvider == null)
            {
                inputProvider = GetComponent<CCS_CharacterInputActionProvider>();
            }

            if (aimLocomotionController == null)
            {
                aimLocomotionController = GetComponent<CCS_CharacterAimLocomotionController>();
            }

            if (cachedNetworkObject == null)
            {
                cachedNetworkObject = GetComponentInParent<NetworkObject>();
            }
        }

        private void LogMissingSetupOnce()
        {
            if (!enableRuntimeWeaponDebug)
            {
                return;
            }

            if (inputProvider == null && !loggedMissingInputProvider)
            {
                loggedMissingInputProvider = true;
                Debug.LogWarning("[Weapons] No input provider found on revolver owner.", this);
            }

            if (revolverDefinition == null && !loggedMissingDefinition)
            {
                loggedMissingDefinition = true;
                Debug.LogWarning("[Weapons] No revolver definition assigned.", this);
            }

            if (muzzlePoint == null && !loggedMissingMuzzlePoint)
            {
                loggedMissingMuzzlePoint = true;
                Debug.LogWarning("[Weapons] No muzzle point assigned.", this);
            }
            else if (enableMuzzleDebug && muzzlePoint != null && !loggedMissingMuzzlePoint)
            {
                Debug.Log(
                    $"[Weapons] Muzzle point={muzzlePoint.name} worldPos={muzzlePoint.position}",
                    this);
            }

            if (sceneCameraController == null && !loggedMissingSceneCamera)
            {
                loggedMissingSceneCamera = true;
                Debug.LogWarning("[Weapons] No scene camera controller assigned for aim mode.", this);
            }
        }

        private bool cachedAimStateForEvents;

        private void SyncAimStateForEvents()
        {
            bool nextAiming = IsAiming;
            if (cachedAimStateForEvents == nextAiming)
            {
                return;
            }

            cachedAimStateForEvents = nextAiming;

            if (enableRuntimeWeaponDebug)
            {
                Debug.Log(nextAiming ? "[Weapons] Aim started." : "[Weapons] Aim ended.", this);
            }

            RaiseStateChanged();
        }

        private void HandleReloadInput()
        {
            if (!weaponOwnershipActive || !IsAiming)
            {
                return;
            }

            if (!inputProvider.ReloadPressed || isReloading || currentAmmo >= MaxAmmo)
            {
                return;
            }

            StartReload();
        }

        private void HandleFireInput()
        {
            if (!weaponOwnershipActive || !IsAiming)
            {
                return;
            }

            if (!inputProvider.FirePressed)
            {
                return;
            }

            if (enableRuntimeWeaponDebug)
            {
                Debug.Log("[Weapons] Fire pressed.", this);
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
                if (enableRuntimeWeaponDebug)
                {
                    Debug.LogWarning("[Weapons] Revolver fire skipped: no aim camera.", this);
                }

                return;
            }

            float spread = IsAiming
                ? revolverDefinition.AimSpreadDegrees
                : revolverDefinition.HipSpreadDegrees;

            CCS_WeaponHitscanResult hitscanResult = CCS_HitscanWeaponRaycaster.CastFromCameraCenter(
                resolvedCamera,
                muzzlePoint,
                revolverDefinition.MaxRange,
                spread,
                revolverDefinition.HitMask,
                transform.root,
                drawDebugRay: enableRuntimeWeaponDebug || enableAimRayDebug,
                drawMuzzleDebug: enableRuntimeWeaponDebug || enableMuzzleDebug);

            currentAmmo--;
            lastFireTime = Time.time;
            RaiseStateChanged();

            if (enableRuntimeWeaponDebug || enableAimRayDebug || enableMuzzleDebug)
            {
                string hitLabel = hitscanResult.DidHit && hitscanResult.HitObject != null
                    ? hitscanResult.HitObject.name
                    : "miss";
                string muzzleLabel = muzzlePoint != null ? muzzlePoint.name : "missing";
                Debug.Log(
                    $"[Weapons] Shot fired. Camera={resolvedCamera.name} Muzzle={muzzleLabel} "
                    + $"MuzzlePos={hitscanResult.RayOrigin} Ammo={currentAmmo}/{MaxAmmo} Hit={hitLabel}",
                    this);
            }

            ApplyDamageToHitTarget(hitscanResult);

            CCS_RevolverFireResultEvent fireEvent = new CCS_RevolverFireResultEvent(
                hitscanResult,
                currentAmmo,
                false);
            FireResolved?.Invoke(fireEvent);
            RevolverFired?.Invoke();
        }

        private void HandleDryFire()
        {
            lastFireTime = Time.time;

            if (enableRuntimeWeaponDebug)
            {
                Debug.Log("[Weapons] Dry fire.", this);
            }

            DryFired?.Invoke(new CCS_RevolverDryFireEvent(currentAmmo));
            RevolverFired?.Invoke();
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
            RevolverReloadStarted?.Invoke();

            if (enableRuntimeWeaponDebug)
            {
                Debug.Log("[Weapons] Reload started.", this);
            }

            reloadCoroutine = StartCoroutine(ReloadAfterDuration());
        }

        private IEnumerator ReloadAfterDuration()
        {
            yield return new WaitForSeconds(revolverDefinition.ReloadSeconds);

            reloadCoroutine = null;
            isReloading = false;
            currentAmmo = MaxAmmo;
            RaiseStateChanged();
            RevolverReloadCompleted?.Invoke();

            if (enableRuntimeWeaponDebug)
            {
                Debug.Log($"[Weapons] Reload completed. Ammo={currentAmmo}/{MaxAmmo}", this);
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
                IsAiming,
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
