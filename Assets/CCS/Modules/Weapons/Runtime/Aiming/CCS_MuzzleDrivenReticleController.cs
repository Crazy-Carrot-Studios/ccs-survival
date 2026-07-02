using CCS.Modules.CharacterController;
using Unity.Netcode;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_MuzzleDrivenReticleController
// CATEGORY: Modules / Weapons / Runtime / Aiming
// PURPOSE: Screen-safe aim reticle — hybrid camera center with clamped muzzle drift.
// PLACEMENT: WeaponHudRoot on PF_CCS_CharacterController_Player_Networked.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Default hybrid mode. Gun is not pulled to reticle. Raw muzzle mode is debug-only.
// =============================================================================

namespace CCS.Modules.Weapons
{
    [DefaultExecutionOrder(225)]
    public sealed class CCS_MuzzleDrivenReticleController : MonoBehaviour
    {
        #region Variables

        [SerializeField] private CCS_AimReticleMode reticleMode =
            CCS_AimReticleMode.HybridCameraCenterWithMuzzleDrift;
        [SerializeField] private bool enableReticleClamp = true;
        [SerializeField] private float maxMuzzleReticleOffsetPixels =
            CCS_WeaponsConstants.MasterTestMaxReticleDriftPixelsDefault;
        [SerializeField] private float safeScreenPaddingPixels =
            CCS_WeaponsConstants.MasterTestReticleSafeScreenPaddingPixelsDefault;
        [SerializeField] private float maxDistance = 100f;
        [SerializeField] private LayerMask aimMask = Physics.DefaultRaycastLayers;
        [SerializeField] private RectTransform reticleTransform;
        [SerializeField] private Canvas reticleCanvas;
        [SerializeField] private Camera aimCamera;
        [SerializeField] private CCS_RevolverController revolverController;
        [SerializeField] private CCS_PlayerEquipmentVisualController equipmentVisualController;
        [SerializeField] private CCS_RevolverHudPresenter hudPresenter;
        [SerializeField] private Component aimPresentationReadinessSourceComponent;
        [SerializeField] private Component aimTargetSourceComponent;
        [SerializeField] private CCS_RevolverReticlePresentationProfile reticlePresentationProfile;
        [SerializeField] private float screenSmoothing = CCS_WeaponsConstants.MasterTestMuzzleReticleScreenSmoothingDefault;
        [SerializeField] private bool debugRays;

        private CCS_IRevolverAimPresentationReadinessSource aimPresentationReadinessSource;
        private CCS_IRevolverAimTargetSource aimTargetSource;
        private bool resolvedAimPresentationReadinessSource;
        private bool resolvedAimTargetSource;
        private bool loggedMissingAimTargetSource;
        private NetworkObject cachedNetworkObject;
        private Vector2 smoothedScreenPosition;
        private Vector2 screenSmoothVelocity;
        private Vector2 lastValidScreenTarget;
        private float lastValidTargetTimestamp;
        private bool hasLastValidScreenTarget;
        private bool hasValidReticlePosition;
        private bool wasAiming;
        private float reticleVisibilityAlpha;
        private bool loggedHiddenAtStartup;
        private bool loggedSnapClampApplied;
        private bool loggedNoHitFallback;
        private bool loggedLastValidTargetHold;
        private Vector3 lastMuzzleHitPoint;
        private bool hasLastMuzzleHitPoint;

        #endregion

        #region Properties

        public CCS_AimReticleMode ReticleMode => reticleMode;

        public bool EnableMuzzleDrivenReticle => reticleMode != CCS_AimReticleMode.CenterLocked;

        public bool EnableReticleClamp => enableReticleClamp;

        public float MaxMuzzleReticleOffsetPixels => maxMuzzleReticleOffsetPixels;

        public bool HasLastMuzzleHitPoint => hasLastMuzzleHitPoint;

        public Vector3 LastMuzzleHitPoint => lastMuzzleHitPoint;

        #endregion

        #region Unity Callbacks

        private void Awake()
        {
            ResolveReferences();
            EnsureReticleHiddenAtStartup();
        }

        private void Start()
        {
            EnsureReticleHiddenAtStartup();
        }

        private void LateUpdate()
        {
            if (reticleTransform == null)
            {
                return;
            }

            bool shouldShowReticle = ShouldShowReticle();
            if (shouldShowReticle && !wasAiming)
            {
                HandleAimStarted();
            }
            else if (!shouldShowReticle && wasAiming)
            {
                HandleAimEnded();
            }

            wasAiming = shouldShowReticle;
            if (!shouldShowReticle)
            {
                return;
            }

            Camera resolvedCamera = ResolveAimCamera();
            if (resolvedCamera == null)
            {
                return;
            }

            Transform muzzle = ResolveEquippedMuzzle();
            Vector2 centerScreen = GetCenterScreenPosition(resolvedCamera);
            Vector2 targetScreen = ResolveTargetScreenPosition(resolvedCamera, muzzle, centerScreen);
            targetScreen = ClampToSafeScreen(targetScreen, resolvedCamera);

            if (!hasValidReticlePosition)
            {
                smoothedScreenPosition = targetScreen;
                screenSmoothVelocity = Vector2.zero;
                hasValidReticlePosition = true;
            }
            else
            {
                Vector2 previousSmoothed = smoothedScreenPosition;
                float smoothTime = ResolveScreenSmoothTime();
                smoothedScreenPosition = Vector2.SmoothDamp(
                    smoothedScreenPosition,
                    targetScreen,
                    ref screenSmoothVelocity,
                    smoothTime);

                float maxSnap = ResolveMaxScreenSnapPixelsPerFrame();
                Vector2 frameDelta = smoothedScreenPosition - previousSmoothed;
                if (frameDelta.magnitude > maxSnap)
                {
                    smoothedScreenPosition = previousSmoothed + frameDelta.normalized * maxSnap;
                    screenSmoothVelocity = Vector2.zero;
                    LogReticleTransition("Snap clamp applied.");
                    loggedSnapClampApplied = true;
                }
            }

            smoothedScreenPosition = ClampToSafeScreen(smoothedScreenPosition, resolvedCamera);
            float targetAlpha = 1f;
            reticleVisibilityAlpha = Mathf.MoveTowards(
                reticleVisibilityAlpha,
                targetAlpha,
                Time.unscaledDeltaTime / Mathf.Max(0.001f, ResolveReticleFadeInSeconds()));
            ApplyReticleScreenPosition(smoothedScreenPosition, reticleVisibilityAlpha > 0.01f, reticleVisibilityAlpha);

            if (debugRays && muzzle != null)
            {
                Debug.DrawRay(muzzle.position, muzzle.forward * maxDistance, Color.cyan, Time.deltaTime);
                if (hasLastMuzzleHitPoint)
                {
                    Debug.DrawLine(muzzle.position, lastMuzzleHitPoint, Color.yellow, Time.deltaTime);
                }
            }
        }

        #endregion

        #region Public Methods

        public void ConfigureReticle(
            CCS_AimReticleMode mode,
            bool clampEnabled,
            float maxDriftPixels,
            bool debugRaysEnabled)
        {
            reticleMode = mode;
            enableReticleClamp = clampEnabled;
            maxMuzzleReticleOffsetPixels = Mathf.Max(0f, maxDriftPixels);
            debugRays = debugRaysEnabled;
            if (!wasAiming)
            {
                ResetReticleToCenter();
            }
        }

        public void SetReticleMode(CCS_AimReticleMode mode)
        {
            reticleMode = mode;
            if (!wasAiming)
            {
                ResetReticleToCenter();
            }
        }

        public void SetMuzzleDrivenReticleEnabled(bool enabled)
        {
            reticleMode = enabled
                ? CCS_AimReticleMode.HybridCameraCenterWithMuzzleDrift
                : CCS_AimReticleMode.CenterLocked;
            if (!enabled)
            {
                ResetReticleToCenter();
                hudPresenter?.SetMuzzleDrivenReticleActive(false);
            }
        }

        public void SetDebugRaysEnabled(bool enabled)
        {
            debugRays = enabled;
        }

        public void HandleAimStarted()
        {
            ResetReticleToCenter();
            Camera resolvedCamera = ResolveAimCamera();
            if (resolvedCamera == null)
            {
                return;
            }

            Vector2 centerScreen = GetCenterScreenPosition(resolvedCamera);
            smoothedScreenPosition = centerScreen;
            hasValidReticlePosition = true;
        }

        public void HandleAimEnded()
        {
            hasValidReticlePosition = false;
            hasLastMuzzleHitPoint = false;
            hasLastValidScreenTarget = false;
            screenSmoothVelocity = Vector2.zero;
            reticleVisibilityAlpha = 0f;
            loggedSnapClampApplied = false;
            loggedNoHitFallback = false;
            loggedLastValidTargetHold = false;
            ResetReticleToCenter();
            hudPresenter?.SetMuzzleDrivenReticleActive(false);
            LogReticleTransition("Holster started — reticle hidden.");
        }

        public Vector2 GetMuzzleReticleViewportPoint(Camera camera)
        {
            if (camera == null || !hasValidReticlePosition)
            {
                return CCS_WeaponAimResolver.DefaultReticleViewportPoint;
            }

            return new Vector2(
                smoothedScreenPosition.x / Mathf.Max(1f, camera.pixelWidth),
                smoothedScreenPosition.y / Mathf.Max(1f, camera.pixelHeight));
        }

        #endregion

        #region Private Methods

        private void ResolveReferences()
        {
            if (revolverController == null)
            {
                revolverController = GetComponentInParent<CCS_RevolverController>();
            }

            if (equipmentVisualController == null)
            {
                equipmentVisualController = GetComponentInParent<CCS_PlayerEquipmentVisualController>();
            }

            if (hudPresenter == null)
            {
                hudPresenter = GetComponent<CCS_RevolverHudPresenter>()
                    ?? GetComponentInParent<CCS_RevolverHudPresenter>();
            }

            if (reticleTransform == null && hudPresenter != null)
            {
                reticleTransform = hudPresenter.ReticleRectTransform;
            }

            if (reticleCanvas == null)
            {
                reticleCanvas = reticleTransform != null
                    ? reticleTransform.GetComponentInParent<Canvas>()
                    : GetComponentInParent<Canvas>();
            }

            ResolveAimPresentationReadinessSource();
            ResolveAimTargetSource();
        }

        private void ResolveAimTargetSource()
        {
            if (resolvedAimTargetSource)
            {
                return;
            }

            resolvedAimTargetSource = true;
            if (aimTargetSourceComponent is CCS_IRevolverAimTargetSource fromComponent)
            {
                aimTargetSource = fromComponent;
                return;
            }

            Transform modelRoot = CCS_PlayerModelRootUtility.FindModelRoot(transform.root);
            if (modelRoot == null)
            {
                LogMissingAimTargetSourceOnce();
                return;
            }

            Transform aimingRoot = modelRoot.Find(CCS_CharacterControllerConstants.RevolverAimTargetResolverObjectName);
            if (aimingRoot == null)
            {
                LogMissingAimTargetSourceOnce();
                return;
            }

            MonoBehaviour[] behaviours = aimingRoot.GetComponents<MonoBehaviour>();
            for (int i = 0; i < behaviours.Length; i++)
            {
                if (behaviours[i] is CCS_IRevolverAimTargetSource targetSource)
                {
                    aimTargetSource = targetSource;
                    return;
                }
            }

            LogMissingAimTargetSourceOnce();
        }

        private void LogMissingAimTargetSourceOnce()
        {
            if (loggedMissingAimTargetSource)
            {
                return;
            }

            loggedMissingAimTargetSource = true;
            Debug.LogWarning(
                "[Reticle Presentation] Missing CCS_IRevolverAimTargetSource — using legacy camera ray fallback.",
                this);
        }

        private bool HasActiveAimTargetSource()
        {
            return aimTargetSource != null;
        }

        private void ResolveAimPresentationReadinessSource()
        {
            if (resolvedAimPresentationReadinessSource)
            {
                return;
            }

            resolvedAimPresentationReadinessSource = true;
            if (aimPresentationReadinessSourceComponent is CCS_IRevolverAimPresentationReadinessSource fromComponent)
            {
                aimPresentationReadinessSource = fromComponent;
                return;
            }

            Transform playerRoot = transform.root;
            MonoBehaviour[] behaviours = playerRoot.GetComponentsInChildren<MonoBehaviour>(true);
            for (int i = 0; i < behaviours.Length; i++)
            {
                if (behaviours[i] is CCS_IRevolverAimPresentationReadinessSource readinessSource)
                {
                    aimPresentationReadinessSource = readinessSource;
                    return;
                }
            }
        }

        private bool ShouldShowReticle()
        {
            if (!IsLocalPresentationOwner())
            {
                return false;
            }

            if (IsHandSocketPreviewActive())
            {
                return false;
            }

            Camera resolvedCamera = ResolveAimCamera();
            if (resolvedCamera == null)
            {
                return false;
            }

            if (!HasAimIntentActive())
            {
                return false;
            }

            if (aimPresentationReadinessSource == null
                || !IsReticlePresentationVisible(aimPresentationReadinessSource))
            {
                return false;
            }

            return true;
        }

        private static bool IsReticlePresentationVisible(CCS_IRevolverAimPresentationReadinessSource source)
        {
            return source.IsAimPresentationReadyForReticle;
        }

        private bool HasAimIntentActive()
        {
            if (IsDebugAimSetupPoseActive())
            {
                return true;
            }

            return revolverController != null
                && revolverController.HasWeaponOwnership
                && revolverController.IsAiming;
        }

        private bool IsLocalPresentationOwner()
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

        private static bool IsHandSocketPreviewActive()
        {
            CCS_IRevolverHandSocketPreviewDebugSource source = CCS_RevolverHandSocketPreviewDebugRegistry.ActiveSource;
            return source != null && source.ForceRevolverHandSocketPreview;
        }

        private static bool IsDebugAimSetupPoseActive()
        {
            CCS_IRevolverAimSetupPoseDebugSource source = CCS_RevolverAimSetupPoseDebugRegistry.ActiveSource;
            return source != null && source.ForceRevolverAimSetupPose;
        }

        private void EnsureReticleHiddenAtStartup()
        {
            wasAiming = false;
            hasValidReticlePosition = false;
            hasLastMuzzleHitPoint = false;
            hasLastValidScreenTarget = false;
            screenSmoothVelocity = Vector2.zero;
            reticleVisibilityAlpha = 0f;
            hudPresenter?.SetMuzzleDrivenReticleActive(false);
            hudPresenter?.SetReticleScreenVisible(false);
            ResetReticleToCenter();
            if (!loggedHiddenAtStartup)
            {
                loggedHiddenAtStartup = true;
                LogReticleTransition("Reticle hidden at startup.");
            }
        }

        private bool ShouldDriveReticle()
        {
            return ShouldShowReticle();
        }

        private Camera ResolveAimCamera()
        {
            if (aimCamera != null)
            {
                return aimCamera;
            }

            return CCS_CharacterMovementCameraContext.HasActiveCamera
                ? CCS_CharacterMovementCameraContext.ActiveCamera
                : Camera.main;
        }

        private Transform ResolveEquippedMuzzle()
        {
            if (equipmentVisualController != null && equipmentVisualController.HasEquippedMuzzlePoint)
            {
                return equipmentVisualController.CurrentEquippedMuzzlePoint;
            }

            return revolverController != null ? revolverController.MuzzlePointTransform : null;
        }

        private Vector2 ResolveTargetScreenPosition(Camera camera, Transform muzzle, Vector2 centerScreen)
        {
            if (HasActiveAimTargetSource())
            {
                return ResolveStableCameraTargetScreen(camera, centerScreen);
            }

            Vector2 stableCameraScreen = ResolveStableCameraTargetScreen(camera, centerScreen);

            if (reticleMode == CCS_AimReticleMode.CenterLocked)
            {
                return stableCameraScreen;
            }

            if (muzzle == null)
            {
                return stableCameraScreen;
            }

            Vector3 muzzleHitPoint = ResolveMuzzleHitPoint(muzzle);
            lastMuzzleHitPoint = muzzleHitPoint;
            hasLastMuzzleHitPoint = true;

            Vector3 muzzleScreen3 = camera.WorldToScreenPoint(muzzleHitPoint);
            if (!IsValidScreenProjection(muzzleScreen3))
            {
                return stableCameraScreen;
            }

            Vector2 muzzleScreen = new Vector2(muzzleScreen3.x, muzzleScreen3.y);
            if (reticleMode == CCS_AimReticleMode.RawMuzzleProjection)
            {
                return muzzleScreen;
            }

            Vector2 offset = muzzleScreen - centerScreen;
            if (enableReticleClamp)
            {
                offset = Vector2.ClampMagnitude(offset, maxMuzzleReticleOffsetPixels);
            }

            return stableCameraScreen + offset;
        }

        private Vector2 ResolveStableCameraTargetScreen(Camera camera, Vector2 centerScreen)
        {
            if (HasActiveAimTargetSource())
            {
                return ResolveScreenFromAimTargetSource(camera, centerScreen);
            }

            if (IsCameraPitchNearHorizon(camera)
                && ResolveHoldLastValidTargetOnNoHit()
                && TryGetRecentValidScreenTarget(out Vector2 recentValidTarget))
            {
                LogReticleTransitionOnce(ref loggedLastValidTargetHold, "Reticle target invalid — using last valid target.");
                return recentValidTarget;
            }

            float fallbackDistance = ResolveNoHitFallbackDistance();
            Ray cameraRay = camera.ViewportPointToRay(CCS_WeaponAimResolver.DefaultReticleViewportPoint);
            Vector3 worldTarget;
            if (TryRaycast(cameraRay, fallbackDistance, aimMask, transform.root, out RaycastHit hit))
            {
                worldTarget = hit.point;
            }
            else
            {
                worldTarget = cameraRay.origin + (cameraRay.direction * fallbackDistance);
                LogReticleTransitionOnce(ref loggedNoHitFallback, "No hit fallback used.");
            }

            Vector3 screen3 = camera.WorldToScreenPoint(worldTarget);
            if (!IsValidScreenProjection(screen3))
            {
                if (ResolveHoldLastValidTargetOnNoHit() && TryGetRecentValidScreenTarget(out Vector2 heldTarget))
                {
                    LogReticleTransitionOnce(ref loggedLastValidTargetHold, "Reticle target invalid — using last valid target.");
                    return heldTarget;
                }

                return centerScreen;
            }

            Vector2 screenTarget = new Vector2(screen3.x, screen3.y);
            RememberValidScreenTarget(screenTarget);
            return screenTarget;
        }

        private Vector2 ResolveScreenFromAimTargetSource(Camera camera, Vector2 centerScreen)
        {
            if (aimTargetSource.HasValidAimTarget)
            {
                Vector3 screen3 = camera.WorldToScreenPoint(aimTargetSource.AimWorldPoint);
                if (IsValidScreenProjection(screen3))
                {
                    Vector2 screenTarget = new Vector2(screen3.x, screen3.y);
                    RememberValidScreenTarget(screenTarget);
                    return screenTarget;
                }
            }

            if (ResolveHoldLastValidTargetOnNoHit() && TryGetRecentValidScreenTarget(out Vector2 heldTarget))
            {
                LogReticleTransitionOnce(
                    ref loggedLastValidTargetHold,
                    "Reticle target invalid — using last valid screen target from aim source path.");
                return heldTarget;
            }

            if (aimTargetSource.UsedLastValidTarget || aimTargetSource.UsedFallbackTarget)
            {
                Vector3 fallbackScreen3 = camera.WorldToScreenPoint(aimTargetSource.AimWorldPoint);
                if (IsValidScreenProjection(fallbackScreen3))
                {
                    Vector2 fallbackScreen = new Vector2(fallbackScreen3.x, fallbackScreen3.y);
                    RememberValidScreenTarget(fallbackScreen);
                    return fallbackScreen;
                }
            }

            return centerScreen;
        }

        private bool IsCameraPitchNearHorizon(Camera camera)
        {
            float pitch = camera.transform.eulerAngles.x;
            if (pitch > 180f)
            {
                pitch -= 360f;
            }

            return Mathf.Abs(pitch) < ResolvePitchSnapDeadZoneDegrees();
        }

        private void RememberValidScreenTarget(Vector2 screenTarget)
        {
            lastValidScreenTarget = screenTarget;
            lastValidTargetTimestamp = Time.unscaledTime;
            hasLastValidScreenTarget = true;
        }

        private bool TryGetRecentValidScreenTarget(out Vector2 screenTarget)
        {
            screenTarget = lastValidScreenTarget;
            if (!hasLastValidScreenTarget)
            {
                return false;
            }

            float holdSeconds = ResolveLastValidTargetHoldSeconds();
            return Time.unscaledTime - lastValidTargetTimestamp <= holdSeconds;
        }

        private float ResolveNoHitFallbackDistance()
        {
            return reticlePresentationProfile != null
                ? reticlePresentationProfile.NoHitFallbackDistance
                : maxDistance;
        }

        private float ResolveScreenSmoothTime()
        {
            return reticlePresentationProfile != null
                ? reticlePresentationProfile.ScreenSmoothTime
                : 1f / Mathf.Max(1f, screenSmoothing);
        }

        private float ResolveMaxScreenSnapPixelsPerFrame()
        {
            return reticlePresentationProfile != null
                ? reticlePresentationProfile.MaxScreenSnapPixelsPerFrame
                : 120f;
        }

        private float ResolvePitchSnapDeadZoneDegrees()
        {
            return reticlePresentationProfile != null
                ? reticlePresentationProfile.PitchSnapDeadZoneDegrees
                : 2f;
        }

        private bool ResolveHoldLastValidTargetOnNoHit()
        {
            return reticlePresentationProfile == null || reticlePresentationProfile.HoldLastValidTargetOnNoHit;
        }

        private float ResolveLastValidTargetHoldSeconds()
        {
            return reticlePresentationProfile != null
                ? reticlePresentationProfile.LastValidTargetHoldSeconds
                : 0.2f;
        }

        private float ResolveReticleFadeInSeconds()
        {
            return reticlePresentationProfile != null
                ? reticlePresentationProfile.ReticleFadeInSeconds
                : 0.08f;
        }

        private float ResolveReticleFadeOutSeconds()
        {
            return reticlePresentationProfile != null
                ? reticlePresentationProfile.ReticleFadeOutSeconds
                : 0.05f;
        }

        private static void LogReticleTransition(string message)
        {
            if (CCS_AimPresentationDiagnosticsRegistry.EnableReticleTransitionLogging)
            {
                Debug.Log("[Reticle Presentation] " + message);
            }
        }

        private static void LogReticleTransitionOnce(ref bool loggedFlag, string message)
        {
            if (loggedFlag || !CCS_AimPresentationDiagnosticsRegistry.EnableReticleTransitionLogging)
            {
                return;
            }

            loggedFlag = true;
            Debug.Log("[Reticle Presentation] " + message);
        }

        private Vector3 ResolveMuzzleHitPoint(Transform muzzle)
        {
            float range = maxDistance;
            if (revolverController != null && revolverController.RevolverDefinition != null)
            {
                range = Mathf.Min(maxDistance, revolverController.RevolverDefinition.MaxRange);
            }

            Ray muzzleRay = new Ray(muzzle.position, muzzle.forward);
            if (TryRaycast(muzzleRay, range, aimMask, transform.root, out RaycastHit hit))
            {
                return hit.point;
            }

            return muzzle.position + (muzzle.forward * range);
        }

        private static Vector2 GetCenterScreenPosition(Camera camera)
        {
            return new Vector2(camera.pixelWidth * 0.5f, camera.pixelHeight * 0.5f);
        }

        private Vector2 ClampToSafeScreen(Vector2 screenPosition, Camera camera)
        {
            float padding = Mathf.Max(0f, safeScreenPaddingPixels);
            float maxX = Mathf.Max(padding, camera.pixelWidth - padding);
            float maxY = Mathf.Max(padding, camera.pixelHeight - padding);
            return new Vector2(
                Mathf.Clamp(screenPosition.x, padding, maxX),
                Mathf.Clamp(screenPosition.y, padding, maxY));
        }

        private static bool IsValidScreenProjection(Vector3 screenPoint)
        {
            if (screenPoint.z <= 0.01f)
            {
                return false;
            }

            if (float.IsNaN(screenPoint.x) || float.IsNaN(screenPoint.y)
                || float.IsInfinity(screenPoint.x) || float.IsInfinity(screenPoint.y))
            {
                return false;
            }

            float bound = Mathf.Max(Screen.width, Screen.height) * 4f;
            return Mathf.Abs(screenPoint.x) <= bound && Mathf.Abs(screenPoint.y) <= bound;
        }

        private void ApplyReticleScreenPosition(Vector2 screenPosition, bool visible, float alpha)
        {
            if (reticleTransform == null)
            {
                return;
            }

            if (reticleCanvas == null)
            {
                reticleCanvas = reticleTransform.GetComponentInParent<Canvas>();
            }

            if (reticleCanvas == null)
            {
                return;
            }

            RectTransform canvasRect = reticleCanvas.transform as RectTransform;
            Camera canvasCamera = reticleCanvas.renderMode == RenderMode.ScreenSpaceOverlay
                ? null
                : reticleCanvas.worldCamera;

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    canvasRect,
                    screenPosition,
                    canvasCamera,
                    out Vector2 localPoint))
            {
                if (hudPresenter != null)
                {
                    hudPresenter.SetMuzzleDrivenReticleActive(true);
                    hudPresenter.SetReticleScreenPosition(screenPosition, visible, alpha);
                }
                else
                {
                    reticleTransform.anchoredPosition = localPoint;
                    if (reticleTransform.TryGetComponent(out UnityEngine.UI.Image reticleImage))
                    {
                        Color color = reticleImage.color;
                        color.a = alpha;
                        reticleImage.color = color;
                        reticleImage.enabled = visible;
                    }
                }
            }
        }

        private void ResetReticleToCenter()
        {
            hasValidReticlePosition = false;
            Camera resolvedCamera = ResolveAimCamera();
            if (resolvedCamera == null)
            {
                hudPresenter?.ResetReticleToCenterLocked();
                return;
            }

            Vector2 centerScreen = GetCenterScreenPosition(resolvedCamera);
            smoothedScreenPosition = centerScreen;
            hudPresenter?.SetMuzzleDrivenReticleActive(false);
            hudPresenter?.ResetReticleToCenterLocked();
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

        #endregion
    }
}
