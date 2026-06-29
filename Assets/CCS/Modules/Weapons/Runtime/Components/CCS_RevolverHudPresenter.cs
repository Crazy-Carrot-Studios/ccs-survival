using CCS.Modules.CharacterController;
using TMPro;



using UnityEngine;

using UnityEngine.UI;



// =============================================================================

// SCRIPT: CCS_RevolverHudPresenter

// CATEGORY: Modules / Weapons / Runtime / Components

// PURPOSE: Test-only HUD text for revolver ammo, aim state, and reload status.

// PLACEMENT: WeaponHudRoot on PF_CCS_CharacterController_Player_Networked.

// AUTHOR: James Schilz

// CREATED: 2026-06-07

// NOTES: Local-owner only. Module-owned test HUD; not production combat UI.

// =============================================================================



namespace CCS.Modules.Weapons

{

    [DefaultExecutionOrder(220)]

    public sealed class CCS_RevolverHudPresenter : MonoBehaviour

    {

        #region Variables



        [SerializeField] private CCS_RevolverController revolverController;

        [SerializeField] private TextMeshProUGUI hudText;

        [SerializeField] private Image reticleImage;

        [SerializeField] private Vector2 reticleViewportPoint = CCS_WeaponAimResolver.DefaultReticleViewportPoint;

        [SerializeField] private bool showReticleWhileAiming = true;

        [SerializeField] private bool useMuzzleDrivenScreenPosition;

        private Vector2 centerLockedAnchoredPosition;
        private bool capturedCenterLockedAnchoredPosition;

        [SerializeField] private float statusFlashSeconds = 0.75f;

        [SerializeField] private int lowAmmoThreshold = CCS_WeaponsConstants.WeaponHudLowAmmoThreshold;

        [SerializeField] private Color ammoNormalColor = CCS_WeaponsConstants.WeaponHudAmmoNormalColor;

        [SerializeField] private Color ammoLowColor = CCS_WeaponsConstants.WeaponHudAmmoLowColor;

        [SerializeField] private Color reloadColor = CCS_WeaponsConstants.WeaponHudReloadColor;



        private string statusFlashLabel = string.Empty;

        private float statusFlashEndTime;



        #endregion



        #region Unity Callbacks



        private void Awake()

        {

            if (revolverController == null)

            {

                revolverController = GetComponentInParent<CCS_RevolverController>();

            }

            ApplyReticleAppearance();

        }



        private void OnEnable()

        {

            if (revolverController != null)

            {

                revolverController.StateChanged += HandleStateChanged;

                revolverController.FireResolved += HandleFireResolved;

                revolverController.DryFired += HandleDryFired;

            }



            RefreshHud(new CCS_RevolverStateChangedEvent(0, 0, false, false));

        }



        private void OnDisable()

        {

            if (revolverController != null)

            {

                revolverController.StateChanged -= HandleStateChanged;

                revolverController.FireResolved -= HandleFireResolved;

                revolverController.DryFired -= HandleDryFired;

            }



            SetReticleVisible(false);

            ApplyReticleAppearance();

        }



        private void Update()

        {

            if (string.IsNullOrEmpty(statusFlashLabel))

            {

                return;

            }



            if (Time.unscaledTime >= statusFlashEndTime)

            {

                statusFlashLabel = string.Empty;

                if (revolverController != null)

                {

                    RefreshHud(new CCS_RevolverStateChangedEvent(

                        revolverController.CurrentAmmo,

                        revolverController.MaxAmmo,

                        revolverController.IsAiming,

                        revolverController.IsReloading));

                }

            }

        }



        #endregion



        #region Public Methods



        public Vector2 GetReticleViewportPoint()
        {
            if (useMuzzleDrivenScreenPosition)
            {
                Camera activeCamera = CCS_CharacterMovementCameraContext.HasActiveCamera
                    ? CCS_CharacterMovementCameraContext.ActiveCamera
                    : Camera.main;
                if (activeCamera != null && reticleImage != null)
                {
                    Vector3 screenPoint = RectTransformUtility.WorldToScreenPoint(
                        activeCamera,
                        reticleImage.rectTransform.position);
                    return new Vector2(
                        screenPoint.x / Mathf.Max(1f, activeCamera.pixelWidth),
                        screenPoint.y / Mathf.Max(1f, activeCamera.pixelHeight));
                }
            }

            return reticleViewportPoint;
        }

        public RectTransform ReticleRectTransform => reticleImage != null ? reticleImage.rectTransform : null;

        public void SetMuzzleDrivenReticleActive(bool active)
        {
            if (active == useMuzzleDrivenScreenPosition)
            {
                return;
            }

            useMuzzleDrivenScreenPosition = active;
            if (!active)
            {
                ResetReticleToCenterLocked();
            }
        }

        public void SetReticleScreenPosition(Vector2 screenPosition, bool visible)
        {
            if (reticleImage == null)
            {
                return;
            }

            Canvas canvas = reticleImage.GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                return;
            }

            CaptureCenterLockedAnchoredPosition();
            Camera canvasCamera = canvas.renderMode == RenderMode.ScreenSpaceOverlay
                ? null
                : canvas.worldCamera;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    canvas.transform as RectTransform,
                    screenPosition,
                    canvasCamera,
                    out Vector2 localPoint))
            {
                reticleImage.rectTransform.anchoredPosition = localPoint;
            }

            reticleImage.enabled = visible;
        }

        public void SetReticleScreenVisible(bool visible)
        {
            if (reticleImage != null)
            {
                reticleImage.enabled = visible;
            }
        }



        #endregion



        #region Private Methods



        private void HandleStateChanged(CCS_RevolverStateChangedEvent stateChangedEvent)

        {

            if (string.IsNullOrEmpty(statusFlashLabel))

            {

                RefreshHud(stateChangedEvent);

            }

        }



        private void HandleFireResolved(CCS_RevolverFireResultEvent fireResultEvent)

        {

            if (fireResultEvent.WasDryFire)

            {

                return;

            }



            FlashStatus("FIRED");

        }



        private void HandleDryFired(CCS_RevolverDryFireEvent dryFireEvent)

        {

            FlashStatus("DRY");

        }



        private void FlashStatus(string label)

        {

            statusFlashLabel = label;

            statusFlashEndTime = Time.unscaledTime + statusFlashSeconds;

            if (hudText != null && revolverController != null)

            {

                RefreshHud(new CCS_RevolverStateChangedEvent(

                    revolverController.CurrentAmmo,

                    revolverController.MaxAmmo,

                    revolverController.IsAiming,

                    revolverController.IsReloading));

            }

        }



        private void RefreshHud(CCS_RevolverStateChangedEvent stateChangedEvent)

        {

            if (revolverController != null && !revolverController.HasWeaponOwnership)

            {

                SetHudVisible(false);

                return;

            }



            SetHudVisible(true);



            if (hudText != null)

            {

                string aimLabel = stateChangedEvent.IsAiming ? "Aiming" : "Hip";

                string moveLabel = stateChangedEvent.IsAiming ? "\nMove: Aim Strafe" : string.Empty;

                string reloadLabel = stateChangedEvent.IsReloading ? "\nReloading..." : string.Empty;

                string statusLabel = string.IsNullOrEmpty(statusFlashLabel)

                    ? string.Empty

                    : $"\n{statusFlashLabel}";

                hudText.text =

                    $"Ammo: {stateChangedEvent.CurrentAmmo} / {stateChangedEvent.MaxAmmo}\n"

                    + $"Aim: {aimLabel}{moveLabel}{reloadLabel}{statusLabel}";

                hudText.color = ResolveHudTextColor(stateChangedEvent);

            }



            PulseReticleOnFireFlash();

        }



        private Color ResolveHudTextColor(CCS_RevolverStateChangedEvent stateChangedEvent)

        {

            if (stateChangedEvent.IsReloading)

            {

                return reloadColor;

            }



            if (stateChangedEvent.CurrentAmmo <= 0

                || stateChangedEvent.CurrentAmmo <= lowAmmoThreshold)

            {

                return ammoLowColor;

            }



            return ammoNormalColor;

        }



        private void ApplyReticleAppearance()

        {

            if (reticleImage == null)

            {

                return;

            }

            reticleImage.color = CCS_WeaponsConstants.WeaponReticleFillColor;

            Outline reticleOutline = reticleImage.GetComponent<Outline>();
            if (reticleOutline == null)
            {
                reticleOutline = reticleImage.gameObject.AddComponent<Outline>();
            }

            reticleOutline.effectColor = CCS_WeaponsConstants.WeaponReticleOutlineColor;
            reticleOutline.effectDistance = new Vector2(1f, -1f);
        }



        private void PulseReticleOnFireFlash()

        {

            if (reticleImage == null || string.IsNullOrEmpty(statusFlashLabel))

            {

                return;

            }



            if (statusFlashLabel == "FIRED" || statusFlashLabel == "DRY")

            {

                reticleImage.rectTransform.localScale = Vector3.one * 1.35f;

            }

            else

            {

                reticleImage.rectTransform.localScale = Vector3.one;

            }

        }



        private void SetReticleVisible(bool visible)

        {

            if (reticleImage != null)

            {

                if (!useMuzzleDrivenScreenPosition)
                {
                    reticleImage.enabled = visible;
                }

                if (!visible)

                {

                    reticleImage.rectTransform.localScale = Vector3.one;

                }

            }

        }

        private void CaptureCenterLockedAnchoredPosition()
        {
            if (reticleImage == null || capturedCenterLockedAnchoredPosition)
            {
                return;
            }

            centerLockedAnchoredPosition = reticleImage.rectTransform.anchoredPosition;
            capturedCenterLockedAnchoredPosition = true;
        }

        public void ResetReticleToCenterLocked()
        {
            CaptureCenterLockedAnchoredPosition();
            if (reticleImage != null)
            {
                reticleImage.rectTransform.anchoredPosition = centerLockedAnchoredPosition;
            }

            useMuzzleDrivenScreenPosition = false;
        }



        private void SetHudVisible(bool visible)

        {

            if (hudText != null)

            {

                hudText.enabled = visible;

            }



            if (!visible)

            {

                SetReticleVisible(false);

            }

        }



        #endregion

    }

}
