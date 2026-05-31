using CCS.Modules.Equipment;
using CCS.Modules.TimeOfDay;
using CCS.Modules.Weather;

// =============================================================================
// SCRIPT: CCS_EnvironmentSnapshot
// CATEGORY: Modules / EnvironmentEffects / Runtime / Data
// PURPOSE: Read-only environment snapshot with raw and equipment-adjusted effective values.
// PLACEMENT: Produced by CCS_EnvironmentEffectsService.GetSnapshot().
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-31
// NOTES: Raw values from Time/Weather. Effective values apply equipment resistances.
// =============================================================================

namespace CCS.Modules.EnvironmentEffects
{
    public readonly struct CCS_EnvironmentSnapshot
    {
        #region Public Methods

        public CCS_EnvironmentSnapshot(
            float rawTemperature,
            float rawWetness,
            float rawExposure,
            float effectiveTemperature,
            float effectiveWetness,
            float effectiveExposure,
            CCS_EquipmentEnvironmentalModifierSnapshot equipmentModifierSnapshot,
            CCS_WeatherType weatherType,
            CCS_TimeOfDayPhase timePhase)
        {
            RawTemperature = rawTemperature;
            RawWetness = rawWetness < 0f ? 0f : rawWetness;
            RawExposure = rawExposure < 0f ? 0f : rawExposure;
            EffectiveTemperature = effectiveTemperature;
            EffectiveWetness = effectiveWetness < 0f ? 0f : effectiveWetness;
            EffectiveExposure = effectiveExposure < 0f ? 0f : effectiveExposure;
            EquipmentModifierSnapshot = equipmentModifierSnapshot;
            WeatherType = weatherType;
            TimePhase = timePhase;
        }

        public static CCS_EnvironmentSnapshot Empty =>
            new CCS_EnvironmentSnapshot(
                0f,
                0f,
                0f,
                0f,
                0f,
                0f,
                CCS_EquipmentEnvironmentalModifierSnapshot.Empty,
                CCS_WeatherType.Clear,
                CCS_TimeOfDayPhase.Dawn);

        #endregion

        #region Properties

        public float RawTemperature { get; }

        public float RawWetness { get; }

        public float RawExposure { get; }

        public float EffectiveTemperature { get; }

        public float EffectiveWetness { get; }

        public float EffectiveExposure { get; }

        public CCS_EquipmentEnvironmentalModifierSnapshot EquipmentModifierSnapshot { get; }

        public float AmbientTemperature => EffectiveTemperature;

        public float Wetness => EffectiveWetness;

        public float Exposure => EffectiveExposure;

        public CCS_WeatherType WeatherType { get; }

        public CCS_TimeOfDayPhase TimePhase { get; }

        #endregion
    }
}
