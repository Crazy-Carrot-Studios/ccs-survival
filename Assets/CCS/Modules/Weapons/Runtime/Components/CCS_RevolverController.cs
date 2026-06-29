using System;
using System.Collections;

using CCS.Modules.Attributes;
using CCS.Modules.CharacterController;

using Unity.Netcode;

using UnityEngine;
using UnityEngine.Serialization;

// =============================================================================
// SCRIPT: CCS_RevolverController
// CATEGORY: Modules / Weapons / Runtime / Components
// PURPOSE: Test revolver hitscan controller with aim, fire, reload, and ammo state.
// PLACEMENT: PF_CCS_CharacterController_Player_Networked root or WeaponRoot child.
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
        [SerializeField] private CCS_PlayerEquipmentVisualController equipmentVisualController;
        [SerializeField] private CCS_RevolverHudPresenter hudPresenter;
        [SerializeField] private Camera aimCamera;
        [SerializeField] private Transform muzzlePoint;
        [FormerlySerializedAs("enableWeaponDebugLogs")]
        [SerializeField] private bool enableRuntimeWeaponDebug;
        [SerializeField] private bool enableAimRayDebug;
        [SerializeField] private bool enableMuzzleDebug;
        [SerializeField] private bool debugAimAlignment;
        [SerializeField] private bool debugVisualConvergence;
        [SerializeField] private bool debugAimCameraAlignment;

        [Header("Experimental Visual Barrel Convergence")]
        [Tooltip("Default OFF. Rotates the equipped gun after the hand profile is applied and can break hand fit.")]
        [SerializeField] private bool enableVisualAimConvergence;
        [Tooltip("Debug only. When ON, fire/hitscan uses raw muzzle forward instead of camera-center reticle aim.")]
        [SerializeField] private bool enableMuzzleAuthoritativeShots;
        [SerializeField] private float convergenceSpeed = 18f;
        [SerializeField] private float maxYawCorrectionDegrees = 15f;
        [SerializeField] private float maxPitchCorrectionDegrees = 10f;
        [SerializeField] private float maxRollCorrectionDegrees = 3f;
        [SerializeField] private float nearTargetDistance = 2f;

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
        private CCS_WeaponAimSolution lastAimSolution;
        private bool hasLastAimSolution;
        private string lastAimDebugSummary = string.Empty;

        #endregion

        #region Properties

        public CCS_RevolverDefinition RevolverDefinition => revolverDefinition;

        public int CurrentAmmo => currentAmmo;

        public int MaxAmmo => revolverDefinition != null ? revolverDefinition.CylinderCapacity : 0;

        public bool IsReloading => isReloading;

        public bool IsAiming =>
            aimLocomotionController != null && aimLocomotionController.IsAimMovementActive;

        public bool IsRevolverOwned => weaponOwnershipActive;

        public bool RevolverAimHeld =>
            weaponOwnershipActive
            && inputProvider != null
            && inputProvider.InputAccepted
            && inputProvider.AimHeld;

        public bool RevolverIsReloading => isReloading;

        public bool HasWeaponOwnership => weaponOwnershipActive;

        public Transform MuzzlePointTransform => muzzlePoint;

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
                equipmentVisualController?.SetVisualAimConvergenceActive(false);
                return;
            }

            TickVisualAimConvergence();
            HandleReloadInput();
            HandleFireInput();
            UpdateAimAlignmentDebugPreview();
        }

        private void LateUpdate()
        {
            if (!IsLocalWeaponOwner() || !weaponOwnershipActive)
            {
                return;
            }

            if (!IsAiming)
            {
                equipmentVisualController?.SetVisualAimConvergenceActive(false);
            }
        }

        private void OnGUI()
        {
            if ((!debugAimAlignment && !debugVisualConvergence && !debugAimCameraAlignment) || !IsAiming
                || !weaponOwnershipActive || string.IsNullOrEmpty(lastAimDebugSummary))
            {
                return;
            }

            GUI.Label(new Rect(12f, Screen.height - 120f, 620f, 100f), lastAimDebugSummary);
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

        public void ConfigureAimVisualTestSettings(
            bool armToReticleIkEnabled,
            bool visualAimConvergenceEnabled,
            CCS_AimReticleMode reticleMode,
            bool reticleClampEnabled,
            float maxReticleDriftPixels,
            bool muzzleAuthoritativeShotsEnabled,
            bool aimDebugRaysEnabled)
        {
            enableVisualAimConvergence = visualAimConvergenceEnabled;
            enableMuzzleAuthoritativeShots = muzzleAuthoritativeShotsEnabled;
            enableAimRayDebug = aimDebugRaysEnabled;
            enableMuzzleDebug = aimDebugRaysEnabled;

            CCS_RevolverArmReticleIK armReticleIk = GetComponentInChildren<CCS_RevolverArmReticleIK>(true);
            armReticleIk?.SetArmToReticleIkEnabled(armToReticleIkEnabled);

            CCS_MuzzleDrivenReticleController muzzleReticle =
                GetComponentInChildren<CCS_MuzzleDrivenReticleController>(true);
            muzzleReticle?.ConfigureReticle(
                reticleMode,
                reticleClampEnabled,
                maxReticleDriftPixels,
                aimDebugRaysEnabled);
        }

        public bool EnableMuzzleAuthoritativeShots => enableMuzzleAuthoritativeShots;

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

            if (equipmentVisualController == null)
            {
                equipmentVisualController = GetComponent<CCS_PlayerEquipmentVisualController>();
            }

            if (hudPresenter == null)
            {
                hudPresenter = GetComponentInChildren<CCS_RevolverHudPresenter>(true);
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

        private void TickVisualAimConvergence()
        {
            if (equipmentVisualController == null || revolverDefinition == null)
            {
                return;
            }

            bool shouldConverge = IsAiming && enableVisualAimConvergence;
            if (!shouldConverge)
            {
                equipmentVisualController.SetVisualAimConvergenceActive(false);
                return;
            }

            Camera resolvedCamera = ResolveAimCamera();
            if (resolvedCamera == null)
            {
                return;
            }

            Vector2 viewportPoint = hudPresenter != null
                ? hudPresenter.GetReticleViewportPoint()
                : CCS_WeaponAimResolver.DefaultReticleViewportPoint;
            bool drawDebug = debugAimAlignment || debugVisualConvergence || debugAimCameraAlignment;
            equipmentVisualController.SetVisualAimConvergenceActive(true);
            equipmentVisualController.TickEquippedVisualAimConvergence(
                resolvedCamera,
                viewportPoint,
                muzzlePoint,
                revolverDefinition.MaxRange,
                revolverDefinition.HitMask,
                transform.root,
                BuildVisualAimConvergenceSettings(),
                drawDebug);
        }

        private CCS_RevolverVisualAimConvergenceSettings BuildVisualAimConvergenceSettings()
        {
            return new CCS_RevolverVisualAimConvergenceSettings(
                enableVisualAimConvergence,
                convergenceSpeed,
                maxYawCorrectionDegrees,
                maxPitchCorrectionDegrees,
                maxRollCorrectionDegrees,
                nearTargetDistance);
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

            Vector2 viewportPoint = hudPresenter != null
                ? hudPresenter.GetReticleViewportPoint()
                : CCS_WeaponAimResolver.DefaultReticleViewportPoint;
            Transform equippedMuzzle = equipmentVisualController != null && equipmentVisualController.HasEquippedMuzzlePoint
                ? equipmentVisualController.CurrentEquippedMuzzlePoint
                : null;
            bool drawCameraDebug = enableRuntimeWeaponDebug || enableAimRayDebug || debugAimAlignment
                || debugVisualConvergence || debugAimCameraAlignment;
            bool drawMuzzleDebug = enableRuntimeWeaponDebug || enableMuzzleDebug || debugAimAlignment
                || debugVisualConvergence || debugAimCameraAlignment;

            CCS_MuzzleDrivenReticleController muzzleReticle =
                GetComponentInChildren<CCS_MuzzleDrivenReticleController>(true);
            if (muzzleReticle != null && muzzleReticle.ReticleMode != CCS_AimReticleMode.CenterLocked)
            {
                viewportPoint = muzzleReticle.GetMuzzleReticleViewportPoint(resolvedCamera);
            }

            CCS_WeaponShotAimMode aimMode = enableMuzzleAuthoritativeShots
                ? CCS_WeaponShotAimMode.DebugMuzzleForwardOnly
                : CCS_WeaponShotAimMode.LocalPlayerCameraCenter;
            CCS_RevolverShotRequest shotRequest = new CCS_RevolverShotRequest(
                aimMode,
                resolvedCamera,
                viewportPoint,
                equippedMuzzle,
                muzzlePoint,
                Vector3.zero,
                revolverDefinition.MaxRange,
                spread,
                revolverDefinition.HitMask,
                transform.root,
                drawCameraDebug,
                drawMuzzleDebug);
            CCS_RevolverShotResult shotResult = CCS_WeaponShotResolver.ResolveShot(shotRequest);
            if (!shotResult.Success)
            {
                if (enableRuntimeWeaponDebug)
                {
                    Debug.LogWarning("[Weapons] Revolver fire skipped: shot resolver failed.", this);
                }

                return;
            }

            lastAimSolution = shotResult.AimSolution;
            hasLastAimSolution = true;
            UpdateAimDebugSummary(shotResult.AimSolution, equippedMuzzle, resolvedCamera, viewportPoint);
            CompleteFireShot(shotResult.HitscanResult, equippedMuzzle, resolvedCamera);
        }

        private void CompleteFireShot(
            CCS_WeaponHitscanResult hitscanResult,
            Transform equippedMuzzle,
            Camera resolvedCamera)
        {
            currentAmmo--;
            lastFireTime = Time.time;
            RaiseStateChanged();

            if (enableRuntimeWeaponDebug || enableAimRayDebug || enableMuzzleDebug)
            {
                string hitLabel = hitscanResult.DidHit && hitscanResult.HitObject != null
                    ? hitscanResult.HitObject.name
                    : "miss";
                string muzzleLabel = equippedMuzzle != null
                    ? equippedMuzzle.name + " (visual)"
                    : muzzlePoint != null
                        ? muzzlePoint.name + " (fallback)"
                        : "missing";
                Debug.Log(
                    $"[Weapons] Shot fired. Camera={resolvedCamera.name} Muzzle={muzzleLabel} "
                    + $"MuzzlePos={hitscanResult.RayOrigin} Ammo={currentAmmo}/{MaxAmmo} Hit={hitLabel} "
                    + $"MuzzleAuth={enableMuzzleAuthoritativeShots}",
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

            CCS_IDamageable damageable = null;
            if (!CCS_DamageableLookupUtility.TryResolveDamageable(hitscanResult.HitObject, out damageable))
            {
                CCS_TestDamageTarget testDamageTarget = hitscanResult.HitObject.GetComponentInParent<CCS_TestDamageTarget>();
                if (testDamageTarget == null)
                {
                    return;
                }

                testDamageTarget.ApplyWeaponDamage(revolverDefinition.Damage);
                return;
            }

            if (damageable != null && !damageable.IsDead && damageable.IsDamageReady)
            {
                ulong sourceNetworkObjectId = cachedNetworkObject != null ? cachedNetworkObject.NetworkObjectId : 0ul;
                CCS_DamageInfo damageInfo = new CCS_DamageInfo(
                    revolverDefinition.Damage,
                    hitscanResult.HitPoint,
                    -hitscanResult.HitNormal,
                    CCS_DamageSourceType.RevolverShot,
                    gameObject,
                    sourceNetworkObjectId);
                damageable.ApplyDamage(damageInfo);
                return;
            }
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

        private void UpdateAimAlignmentDebugPreview()
        {
            if ((!debugAimAlignment && !debugVisualConvergence && !debugAimCameraAlignment)
                || !weaponOwnershipActive || !IsAiming)
            {
                return;
            }

            Camera resolvedCamera = ResolveAimCamera();
            if (resolvedCamera == null || revolverDefinition == null)
            {
                return;
            }

            Vector2 viewportPoint = hudPresenter != null
                ? hudPresenter.GetReticleViewportPoint()
                : CCS_WeaponAimResolver.DefaultReticleViewportPoint;
            Transform equippedMuzzle = equipmentVisualController != null && equipmentVisualController.HasEquippedMuzzlePoint
                ? equipmentVisualController.CurrentEquippedMuzzlePoint
                : null;

            if (enableVisualAimConvergence
                && equipmentVisualController != null
                && equipmentVisualController.HasEquippedAimConvergence
                && equipmentVisualController.CurrentAimConvergence.HasLastAimSolution)
            {
                lastAimSolution = equipmentVisualController.CurrentAimConvergence.LastAimSolution;
            }
            else
            {
                lastAimSolution = CCS_WeaponAimResolver.Resolve(
                    resolvedCamera,
                    viewportPoint,
                    equippedMuzzle,
                    muzzlePoint,
                    revolverDefinition.MaxRange,
                    revolverDefinition.HitMask,
                    transform.root);
            }

            hasLastAimSolution = true;
            UpdateAimDebugSummary(lastAimSolution, equippedMuzzle);

            if (debugVisualConvergence && enableVisualAimConvergence)
            {
                return;
            }

            Debug.DrawRay(
                lastAimSolution.CameraRayOrigin,
                lastAimSolution.CameraRayDirection * revolverDefinition.MaxRange,
                Color.green);
            Debug.DrawRay(
                lastAimSolution.MuzzleOrigin,
                lastAimSolution.MuzzleToAimDirection * Mathf.Min(revolverDefinition.MaxRange, lastAimSolution.Distance),
                Color.blue);
            Transform muzzleForwardSource = equippedMuzzle != null ? equippedMuzzle : muzzlePoint;
            if (muzzleForwardSource != null)
            {
                Debug.DrawRay(lastAimSolution.MuzzleOrigin, muzzleForwardSource.forward * 0.35f, Color.yellow);
            }

            DrawAimDebugPoint(lastAimSolution.AimPoint, Color.red, 0.05f);
            if (equippedMuzzle != null)
            {
                DrawAimDebugPoint(lastAimSolution.MuzzleOrigin, Color.cyan, 0.04f);
            }
        }

        private static void DrawAimDebugPoint(Vector3 position, Color color, float size)
        {
            Debug.DrawLine(position - Vector3.right * size, position + Vector3.right * size, color);
            Debug.DrawLine(position - Vector3.up * size, position + Vector3.up * size, color);
            Debug.DrawLine(position - Vector3.forward * size, position + Vector3.forward * size, color);
        }

        private void UpdateAimDebugSummary(
            CCS_WeaponAimSolution aimSolution,
            Transform equippedMuzzle,
            Camera resolvedCamera = null,
            Vector2? viewportPoint = null)
        {
            if (!debugAimAlignment && !debugVisualConvergence && !debugAimCameraAlignment
                && !enableAimRayDebug)
            {
                lastAimDebugSummary = string.Empty;
                return;
            }

            Transform barrelForwardSource = equippedMuzzle != null ? equippedMuzzle : muzzlePoint;
            float barrelToAimError = barrelForwardSource != null
                ? Vector3.Angle(barrelForwardSource.forward, aimSolution.MuzzleToAimDirection)
                : 0f;
            float visualBarrelError = enableVisualAimConvergence
                && equipmentVisualController != null
                && equipmentVisualController.HasEquippedAimConvergence
                ? equipmentVisualController.CurrentAimConvergence.LastVisualBarrelErrorDegrees
                : barrelToAimError;
            string muzzleSource = aimSolution.UsedVisualMuzzle ? "Visual" : "Fallback";
            string cameraVsMuzzleSummary = string.Empty;
            if (resolvedCamera != null && barrelForwardSource != null)
            {
                CCS_WeaponAimSolution cameraSolution = CCS_WeaponAimResolver.Resolve(
                    resolvedCamera,
                    viewportPoint ?? CCS_WeaponAimResolver.DefaultReticleViewportPoint,
                    equippedMuzzle,
                    muzzlePoint,
                    revolverDefinition != null ? revolverDefinition.MaxRange : 100f,
                    revolverDefinition != null ? revolverDefinition.HitMask : Physics.DefaultRaycastLayers,
                    transform.root);
                CCS_WeaponAimSolution muzzleSolution = CCS_WeaponAimResolver.ResolveMuzzleForward(
                    equippedMuzzle,
                    muzzlePoint,
                    revolverDefinition != null ? revolverDefinition.MaxRange : 100f,
                    revolverDefinition != null ? revolverDefinition.HitMask : Physics.DefaultRaycastLayers,
                    transform.root);
                Vector3 cameraScreen = resolvedCamera.WorldToScreenPoint(cameraSolution.AimPoint);
                Vector3 muzzleScreen = resolvedCamera.WorldToScreenPoint(muzzleSolution.AimPoint);
                float screenPixelDelta = new Vector2(
                    cameraScreen.x - muzzleScreen.x,
                    cameraScreen.y - muzzleScreen.y).magnitude;
                float aimDirectionDelta = Vector3.Angle(
                    cameraSolution.MuzzleToAimDirection,
                    muzzleSolution.MuzzleToAimDirection);
                cameraVsMuzzleSummary =
                    "\nCamera vs Muzzle screen px: "
                    + screenPixelDelta.ToString("F1")
                    + "\nCamera vs Muzzle deg: "
                    + aimDirectionDelta.ToString("F1")
                    + "\nMuzzleAuthShots: "
                    + enableMuzzleAuthoritativeShots;
            }

            CCS_CharacterCameraProfile aimProfile = sceneCameraController != null
                && sceneCameraController.CameraProfileSet != null
                ? sceneCameraController.CameraProfileSet.AimOverShoulderProfile
                : null;
            string cameraSummary = aimProfile != null
                ? "Shoulder X="
                  + aimProfile.ThirdPersonShoulderOffset.x.ToString("F2")
                  + " Y="
                  + aimProfile.ThirdPersonShoulderOffset.y.ToString("F2")
                  + " Dist="
                  + aimProfile.ThirdPersonCameraDistance.ToString("F2")
                  + " FOV="
                  + aimProfile.FieldOfView.ToString("F0")
                : "Shoulder profile unavailable";
            lastAimDebugSummary =
                "Camera Aim: "
                + (aimSolution.HasCameraHit ? "OK" : "Miss")
                + "\nMuzzle Aim: OK"
                + "\nBarrel vs Aim Error: "
                + visualBarrelError.ToString("F1")
                + " deg\nMuzzle Source: "
                + muzzleSource
                + cameraVsMuzzleSummary
                + "\n"
                + cameraSummary;
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
