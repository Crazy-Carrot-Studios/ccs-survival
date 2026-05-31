using CCS.Modules.Equipment;
using CCS.Modules.Shelter;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_EnvironmentEffectiveValueUtility
// CATEGORY: Modules / EnvironmentEffects / Runtime / Data
// PURPOSE: Applies shelter and equipment protection to raw environment values.
// PLACEMENT: Used by CCS_EnvironmentEffectsService when building snapshots.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-31
// NOTES: Order: raw -> shelter -> equipment -> effective. Wetness/exposure clamp at zero.
// =============================================================================

namespace CCS.Modules.EnvironmentEffects
{
    public static class CCS_EnvironmentEffectiveValueUtility
    {
        #region Public Methods

        public static void ApplyShelterModifiers(
            float rawTemperature,
            float rawWetness,
            float rawExposure,
            bool isSheltered,
            CCS_ShelterModifierSnapshot shelterModifiers,
            out float shelteredTemperature,
            out float shelteredWetness,
            out float shelteredExposure)
        {
            shelteredTemperature = rawTemperature;
            shelteredWetness = rawWetness < 0f ? 0f : rawWetness;
            shelteredExposure = rawExposure < 0f ? 0f : rawExposure;

            if (!isSheltered)
            {
                return;
            }

            float multiplier = shelterModifiers.ProtectionMultiplier;
            shelteredTemperature += shelterModifiers.TemperatureProtection * multiplier;
            shelteredWetness = Mathf.Max(
                0f,
                shelteredWetness - shelterModifiers.WetnessProtection * multiplier);
            shelteredExposure = Mathf.Max(
                0f,
                shelteredExposure - shelterModifiers.ExposureProtection * multiplier);
        }

        public static void ApplyEquipmentModifiers(
            float shelteredTemperature,
            float shelteredWetness,
            float shelteredExposure,
            CCS_EquipmentEnvironmentalModifierSnapshot equipmentModifiers,
            out float effectiveTemperature,
            out float effectiveWetness,
            out float effectiveExposure)
        {
            effectiveTemperature = shelteredTemperature + equipmentModifiers.TemperatureResistance;
            effectiveWetness = Mathf.Max(0f, shelteredWetness - equipmentModifiers.WetnessResistance);
            effectiveExposure = Mathf.Max(0f, shelteredExposure - equipmentModifiers.ExposureResistance);
        }

        public static void ApplyProtectionChain(
            float rawTemperature,
            float rawWetness,
            float rawExposure,
            bool isSheltered,
            CCS_ShelterModifierSnapshot shelterModifiers,
            CCS_EquipmentEnvironmentalModifierSnapshot equipmentModifiers,
            out float effectiveTemperature,
            out float effectiveWetness,
            out float effectiveExposure)
        {
            ApplyShelterModifiers(
                rawTemperature,
                rawWetness,
                rawExposure,
                isSheltered,
                shelterModifiers,
                out float shelteredTemperature,
                out float shelteredWetness,
                out float shelteredExposure);

            ApplyEquipmentModifiers(
                shelteredTemperature,
                shelteredWetness,
                shelteredExposure,
                equipmentModifiers,
                out effectiveTemperature,
                out effectiveWetness,
                out effectiveExposure);
        }

        #endregion
    }
}
