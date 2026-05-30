using CCS.Modules.SurvivalCore;
using UnityEngine;
using UnityEngine.UI;

// =============================================================================
// SCRIPT: CCS_SurvivalBarPresenter
// CATEGORY: Modules / UI / Runtime / Presentation
// PURPOSE: Displays survival core stat bars from HUD presentation snapshots.
// PLACEMENT: Child of PF_CCS_HUD_Root survival bar area.
// AUTHOR: James Schilz
// CREATED: 2026-05-30
// NOTES: Read-only display. No stat mutation.
// =============================================================================

namespace CCS.Modules.UI
{
    public sealed class CCS_SurvivalBarPresenter : MonoBehaviour
    {
        #region Variables

        [SerializeField] private Text healthLabel;
        [SerializeField] private Image healthFill;
        [SerializeField] private Text staminaLabel;
        [SerializeField] private Image staminaFill;
        [SerializeField] private Text hungerLabel;
        [SerializeField] private Image hungerFill;
        [SerializeField] private Text thirstLabel;
        [SerializeField] private Image thirstFill;

        private CCS_HudPresentationService presentationService;

        #endregion

        #region Public Methods

        public void Bind(CCS_HudPresentationService service, CCS_HudProfile profile)
        {
            Unbind();
            presentationService = service;

            if (presentationService != null)
            {
                presentationService.HudDataRefreshed += HandleHudDataRefreshed;
                presentationService.HudInitialized += HandleHudDataRefreshed;
            }

            gameObject.SetActive(profile == null || profile.ShowSurvivalBars);
            RefreshDisplay();
        }

        public void Unbind()
        {
            if (presentationService != null)
            {
                presentationService.HudDataRefreshed -= HandleHudDataRefreshed;
                presentationService.HudInitialized -= HandleHudDataRefreshed;
            }

            presentationService = null;
        }

        #endregion

        #region Unity Callbacks

        private void OnDestroy()
        {
            Unbind();
        }

        #endregion

        #region Private Methods

        private void HandleHudDataRefreshed(CCS_HudEventArgs eventArgs)
        {
            RefreshDisplay();
        }

        private void RefreshDisplay()
        {
            UpdateBar(healthLabel, healthFill, "Health", presentationService, CCS_SurvivalStatType.Health);
            UpdateBar(staminaLabel, staminaFill, "Stamina", presentationService, CCS_SurvivalStatType.Stamina);
            UpdateBar(hungerLabel, hungerFill, "Hunger", presentationService, CCS_SurvivalStatType.Hunger);
            UpdateBar(thirstLabel, thirstFill, "Thirst", presentationService, CCS_SurvivalStatType.Thirst);
        }

        private void UpdateBar(
            Text label,
            Image fill,
            string statName,
            CCS_HudPresentationService service,
            CCS_SurvivalStatType statType)
        {
            if (label == null)
            {
                return;
            }

            CCS_SurvivalStatSnapshot snapshot = default;
            bool hasSnapshot = service != null && TryGetSnapshot(service, statType, out snapshot);

            if (!hasSnapshot || snapshot.MaxValue <= 0f)
            {
                label.text = $"{statName}: --";
                if (fill != null)
                {
                    fill.fillAmount = 0f;
                }

                return;
            }

            label.text = $"{statName}: {snapshot.CurrentValue:0}/{snapshot.MaxValue:0}";

            if (fill != null)
            {
                fill.fillAmount = snapshot.NormalizedValue;
            }
        }

        private static bool TryGetSnapshot(
            CCS_HudPresentationService service,
            CCS_SurvivalStatType statType,
            out CCS_SurvivalStatSnapshot snapshot)
        {
            snapshot = statType switch
            {
                CCS_SurvivalStatType.Health => service.HealthSnapshot,
                CCS_SurvivalStatType.Stamina => service.StaminaSnapshot,
                CCS_SurvivalStatType.Hunger => service.HungerSnapshot,
                CCS_SurvivalStatType.Thirst => service.ThirstSnapshot,
                _ => default
            };

            return snapshot.MaxValue > 0f;
        }

        #endregion
    }
}
