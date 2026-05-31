using CCS.Modules.Equipment;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_EnvironmentEffectiveValueUtility
// CATEGORY: Modules / EnvironmentEffects / Runtime / Data
// PURPOSE: Applies equipment environmental modifiers to raw environment values.
// PLACEMENT: Used by CCS_EnvironmentEffectsService when building snapshots.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-31
// NOTES: Wetness and exposure subtract resistance and clamp at zero minimum.
// =============================================================================

namespace CCS.Modules.EnvironmentEffects
{
    public static class CCS_EnvironmentEffectiveValueUtility
    {
        #region Public Methods

        public static void ApplyEquipmentModifiers(
            float rawTemperature,
            float rawWetness,
            float rawExposure,
            CCS_EquipmentEnvironmentalModifierSnapshot equipmentModifiers,
            out float effectiveTemperature,
            out float effectiveWetness,
            out float effectiveExposure)
        {
            effectiveTemperature = rawTemperature + equipmentModifiers.TemperatureResistance;
            effectiveWetness = Mathf.Max(0f, rawWetness - equipmentModifiers.WetnessResistance);
            effectiveExposure = Mathf.Max(0f, rawExposure - equipmentModifiers.ExposureResistance);
        }

        #endregion
    }
}
