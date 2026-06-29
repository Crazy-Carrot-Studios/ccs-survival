using CCS.Modules.CharacterController;
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
        [SerializeField] private CCS_RevolverUpperBodyAnimator revolverUpperBodyAnimator;
        [SerializeField] private float screenSmoothing = CCS_WeaponsConstants.MasterTestMuzzleReticleScreenSmoothingDefault;
        [SerializeField] private bool debugRays;

        private Vector2 smoothedScreenPosition;
        private bool hasValidReticlePosition;
        private bool wasAiming;
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
        }

        private void LateUpdate()
        {
            if (reticleTransform == null)
            {
                return;
            }

            bool isAiming = ShouldDriveReticle();
            if (isAiming && !wasAiming)
            {
                HandleAimStarted();
            }
            else if (!isAiming && wasAiming)
            {
                HandleAimEnded();
            }

            wasAiming = isAiming;
            if (!isAiming)
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
                smoothedScreenPosition = centerScreen;
                hasValidReticlePosition = true;
            }
            else
            {
                float delta = Vector2.Distance(smoothedScreenPosition, targetScreen);
                if (delta > Screen.width * 0.5f)
                {
                    smoothedScreenPosition = centerScreen;
                }
                else
                {
                    float smoothFactor = 1f - Mathf.Exp(-screenSmoothing * Time.deltaTime);
                    smoothedScreenPosition = Vector2.Lerp(smoothedScreenPosition, targetScreen, smoothFactor);
                }
            }

            smoothedScreenPosition = ClampToSafeScreen(smoothedScreenPosition, resolvedCamera);
            ApplyReticleScreenPosition(smoothedScreenPosition, visible: true);

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
            ApplyReticleScreenPosition(centerScreen, visible: true);
            hudPresenter?.SetMuzzleDrivenReticleActive(true);
        }

        public void HandleAimEnded()
        {
            hasValidReticlePosition = false;
            hasLastMuzzleHitPoint = false;
            ResetReticleToCenter();
            hudPresenter?.SetMuzzleDrivenReticleActive(false);
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

            if (revolverUpperBodyAnimator == null)
            {
                revolverUpperBodyAnimator = GetComponentInParent<CCS_RevolverUpperBodyAnimator>();
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
        }

        private bool ShouldDriveReticle()
        {
            if (revolverController == null
                || !revolverController.HasWeaponOwnership
                || !revolverController.IsAiming)
            {
                return false;
            }

            if (revolverUpperBodyAnimator != null)
            {
                return revolverUpperBodyAnimator.IsReticleAimPhaseActive;
            }

            return false;
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
            if (reticleMode == CCS_AimReticleMode.CenterLocked)
            {
                return centerScreen;
            }

            if (muzzle == null)
            {
                return centerScreen;
            }

            Vector3 muzzleHitPoint = ResolveMuzzleHitPoint(muzzle);
            lastMuzzleHitPoint = muzzleHitPoint;
            hasLastMuzzleHitPoint = true;

            Vector3 muzzleScreen3 = camera.WorldToScreenPoint(muzzleHitPoint);
            if (!IsValidScreenProjection(muzzleScreen3))
            {
                return centerScreen;
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

            return centerScreen + offset;
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

        private void ApplyReticleScreenPosition(Vector2 screenPosition, bool visible)
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
                    hudPresenter.SetReticleScreenPosition(screenPosition, visible);
                }
                else
                {
                    reticleTransform.anchoredPosition = localPoint;
                    if (reticleTransform.TryGetComponent(out UnityEngine.UI.Image reticleImage))
                    {
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
