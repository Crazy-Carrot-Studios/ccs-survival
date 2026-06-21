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
            }

            RefreshHud(new CCS_RevolverStateChangedEvent(0, 0, false, false));
        }

        private void OnDisable()
        {
            if (revolverController != null)
            {
                revolverController.StateChanged -= HandleStateChanged;
            }

            SetReticleVisible(false);
        }

        #endregion

        #region Private Methods

        private void HandleStateChanged(CCS_RevolverStateChangedEvent stateChangedEvent)
        {
            RefreshHud(stateChangedEvent);
        }

        private void RefreshHud(CCS_RevolverStateChangedEvent stateChangedEvent)
        {
            if (hudText != null)
            {
                string aimLabel = stateChangedEvent.IsAiming ? "Aiming" : "Hip";
                string reloadLabel = stateChangedEvent.IsReloading ? "\nReloading..." : string.Empty;
                hudText.text =
                    $"Ammo: {stateChangedEvent.CurrentAmmo} / {stateChangedEvent.MaxAmmo}\n"
                    + $"Aim: {aimLabel}{reloadLabel}";
            }

            SetReticleVisible(showReticleWhileAiming && stateChangedEvent.IsAiming);
        }

        private void SetReticleVisible(bool visible)
        {
            if (reticleImage != null)
            {
                reticleImage.enabled = visible;
            }
        }

        #endregion
    }
}
