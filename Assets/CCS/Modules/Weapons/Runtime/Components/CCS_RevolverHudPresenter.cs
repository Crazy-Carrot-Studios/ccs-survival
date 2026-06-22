using TMPro;



using UnityEngine;

using UnityEngine.UI;



// =============================================================================

// SCRIPT: CCS_RevolverHudPresenter

// CATEGORY: Modules / Weapons / Runtime / Components

// PURPOSE: Test-only HUD text for revolver ammo, aim state, and reload status.

// PLACEMENT: WeaponHudRoot on PF_CCS_CharacterController_TestPlayer_Networked.

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

        [SerializeField] private bool showReticleWhileAiming = true;

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



            SetReticleVisible(showReticleWhileAiming && stateChangedEvent.IsAiming);

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

                reticleImage.enabled = visible;

                if (!visible)

                {

                    reticleImage.rectTransform.localScale = Vector3.one;

                }

            }

        }



        #endregion

    }

}


