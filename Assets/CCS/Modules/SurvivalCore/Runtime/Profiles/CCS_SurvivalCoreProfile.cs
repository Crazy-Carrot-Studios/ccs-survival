using System.Collections.Generic;
using CCS.Survival;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_SurvivalCoreProfile
// CATEGORY: Survival / Runtime / SurvivalCore / Profiles
// PURPOSE: ScriptableObject tuning profile for survival core stats and decay rates.
// PLACEMENT: Assets/CCS/Survival/Profiles/SurvivalCore/. Referenced by CCS_SurvivalCoreService.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-28
// NOTES: Health/Stamina heal-damage placeholders use stat definitions; passive health decay optional via decay list.
// =============================================================================

namespace CCS.Modules.SurvivalCore
{
    [CreateAssetMenu(
        fileName = "CCS_SurvivalCoreProfile",
        menuName = "CCS/Survival/Survival Core/Core Profile")]
    public sealed class CCS_SurvivalCoreProfile : CCS_SurvivalProfileBase
    {
        #region Variables

        [Header("Stat Definitions")]
        [Tooltip("Min, max, and starting values per survival stat.")]
        [SerializeField] private List<CCS_SurvivalStatDefinition> statDefinitions = new List<CCS_SurvivalStatDefinition>();

        [Header("Decay And Recovery")]
        [Tooltip("Per-second change rules for hunger, thirst, fatigue, temperature, stamina placeholders.")]
        [SerializeField] private List<CCS_SurvivalStatDecayDefinition> decayDefinitions =
            new List<CCS_SurvivalStatDecayDefinition>();

        [Header("Health Placeholders")]
        [Tooltip("Placeholder heal rate per second when passive heal is enabled in service (0 = disabled).")]
        [SerializeField] private float passiveHealthHealPerSecond;

        [Tooltip("Placeholder damage per second when passive health drain is enabled (0 = disabled).")]
        [SerializeField] private float passiveHealthDamagePerSecond;

        [Header("Stamina Placeholders")]
        [Tooltip("Placeholder stamina recovery per second when recovery rule is active.")]
        [SerializeField] private float staminaRecoveryPerSecond = 2f;

        [Tooltip("Placeholder stamina drain per second when exertion rule is active.")]
        [SerializeField] private float staminaDrainPerSecond = 4f;

        [Header("Environment Integration (0.7.3)")]
        [Tooltip("Per-second temperature gain scale when ambient temperature is above neutral.")]
        [SerializeField] private float temperatureRecoveryRate = 0.15f;

        [Tooltip("Per-second temperature loss scale when ambient temperature is below neutral.")]
        [SerializeField] private float temperatureDecayRate = 0.12f;

        [Tooltip("Fatigue gain per second per exposure unit from environment effects.")]
        [SerializeField] private float exposureFatigueMultiplier = 0.008f;

        [Tooltip("Thirst drain per second per wetness unit from environment effects.")]
        [SerializeField] private float wetnessThirstMultiplier = 0.01f;

        [Tooltip("Minimum clamp applied to temperature stat when environment influence is active.")]
        [SerializeField] private float minimumTemperatureClamp;

        [Tooltip("Maximum clamp applied to temperature stat when environment influence is active.")]
        [SerializeField] private float maximumTemperatureClamp = 100f;

        #endregion

        #region Properties

        public IReadOnlyList<CCS_SurvivalStatDefinition> StatDefinitions => statDefinitions;

        public IReadOnlyList<CCS_SurvivalStatDecayDefinition> DecayDefinitions => decayDefinitions;

        public float PassiveHealthHealPerSecond => passiveHealthHealPerSecond;

        public float PassiveHealthDamagePerSecond => passiveHealthDamagePerSecond;

        public float StaminaRecoveryPerSecond => staminaRecoveryPerSecond;

        public float StaminaDrainPerSecond => staminaDrainPerSecond;

        public float TemperatureRecoveryRate => temperatureRecoveryRate;

        public float TemperatureDecayRate => temperatureDecayRate;

        public float ExposureFatigueMultiplier => exposureFatigueMultiplier;

        public float WetnessThirstMultiplier => wetnessThirstMultiplier;

        public float MinimumTemperatureClamp => minimumTemperatureClamp;

        public float MaximumTemperatureClamp => maximumTemperatureClamp;

        #endregion
    }
}
