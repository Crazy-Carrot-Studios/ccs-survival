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
        [SerializeField] private Text fatigueLabel;
        [SerializeField] private Image fatigueFill;
        [SerializeField] private Text temperatureLabel;
        [SerializeField] private Image temperatureFill;

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
            ApplyTypography(profile);
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
            UpdateBar(healthLabel, healthFill, "Health", CCS_SurvivalStatType.Health, useDecimalValues: false);
            UpdateBar(staminaLabel, staminaFill, "Stamina", CCS_SurvivalStatType.Stamina, useDecimalValues: false);
            UpdateBar(hungerLabel, hungerFill, "Hunger", CCS_SurvivalStatType.Hunger, useDecimalValues: false);
            UpdateBar(thirstLabel, thirstFill, "Thirst", CCS_SurvivalStatType.Thirst, useDecimalValues: false);
            UpdateBar(fatigueLabel, fatigueFill, "Fatigue", CCS_SurvivalStatType.Fatigue, useDecimalValues: false);
            UpdateBar(temperatureLabel, temperatureFill, "Temp", CCS_SurvivalStatType.Temperature, useDecimalValues: true);
        }

        private void ApplyTypography(CCS_HudProfile profile)
        {
            if (profile?.LayoutSettings == null)
            {
                return;
            }

            int fontSize = profile.LayoutSettings.SurvivalBarFontSize;
            CCS_HudLayoutApplicator.ApplyTypography(healthLabel, fontSize);
            CCS_HudLayoutApplicator.ApplyTypography(staminaLabel, fontSize);
            CCS_HudLayoutApplicator.ApplyTypography(hungerLabel, fontSize);
            CCS_HudLayoutApplicator.ApplyTypography(thirstLabel, fontSize);
            CCS_HudLayoutApplicator.ApplyTypography(fatigueLabel, fontSize);
            CCS_HudLayoutApplicator.ApplyTypography(temperatureLabel, fontSize);
        }

        private void UpdateBar(
            Text label,
            Image fill,
            string statName,
            CCS_SurvivalStatType statType,
            bool useDecimalValues)
        {
            if (label == null)
            {
                return;
            }

            if (presentationService == null ||
                !presentationService.TryGetStatSnapshot(statType, out CCS_SurvivalStatSnapshot snapshot))
            {
                label.text = $"{statName}: --";
                if (fill != null)
                {
                    fill.fillAmount = 0f;
                }

                return;
            }

            label.text = useDecimalValues
                ? $"{statName}: {snapshot.CurrentValue:0.#}/{snapshot.MaxValue:0.#}"
                : $"{statName}: {snapshot.CurrentValue:0}/{snapshot.MaxValue:0}";

            if (fill != null)
            {
                fill.fillAmount = snapshot.NormalizedValue;
            }
        }

        #endregion
    }
}
