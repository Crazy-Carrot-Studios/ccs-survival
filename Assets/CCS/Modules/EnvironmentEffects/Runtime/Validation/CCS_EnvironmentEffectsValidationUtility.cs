using CCS.Survival;

// =============================================================================
// SCRIPT: CCS_EnvironmentEffectsValidationUtility
// CATEGORY: Modules / EnvironmentEffects / Runtime / Validation
// PURPOSE: Profile validation helpers for runtime and editor checks.
// PLACEMENT: Used by environment service initialization and editor validation pipeline.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-31
// NOTES: Foundation validation only. No Survival Core mutation requirements.
// =============================================================================

namespace CCS.Modules.EnvironmentEffects
{
    public static class CCS_EnvironmentEffectsValidationUtility
    {
        #region Public Methods

        public static CCS_SurvivalValidationResult ValidateProfile(CCS_EnvironmentEffectsProfile profile)
        {
            if (profile == null)
            {
                return CCS_SurvivalValidationResult.Fail("Environment effects profile is null.");
            }

            CCS_SurvivalValidationResult baseValidation = CCS_SurvivalProfileValidationUtility.ValidateProfile(profile);
            if (!baseValidation.IsSuccess)
            {
                return baseValidation;
            }

            return CCS_SurvivalValidationResult.Pass("Environment effects profile validated.");
        }

        public static string FormatEnvironmentDisplay(CCS_EnvironmentSnapshot snapshot)
        {
            string shelteredLabel = snapshot.IsSheltered ? "Yes" : "No";

            return
                $"Env Temp: {snapshot.RawTemperature:0.#}\n" +
                $"Wetness: {snapshot.RawWetness:0.##}\n" +
                $"Exposure: {snapshot.RawExposure:0.##}\n" +
                $"Sheltered: {shelteredLabel}\n" +
                $"Shelter Wet: {snapshot.ShelterModifierSnapshot.WetnessProtection:0.##}\n" +
                $"Shelter Exp: {snapshot.ShelterModifierSnapshot.ExposureProtection:0.##}\n" +
                $"Shelter Temp: {snapshot.ShelterModifierSnapshot.TemperatureProtection:0.#}\n" +
                $"Temp Res: {snapshot.EquipmentModifierSnapshot.TemperatureResistance:0.#}\n" +
                $"Wet Res: {snapshot.EquipmentModifierSnapshot.WetnessResistance:0.##}\n" +
                $"Exp Res: {snapshot.EquipmentModifierSnapshot.ExposureResistance:0.##}\n" +
                $"Eff Temp: {snapshot.EffectiveTemperature:0.#}\n" +
                $"Eff Wet: {snapshot.EffectiveWetness:0.##}\n" +
                $"Eff Exp: {snapshot.EffectiveExposure:0.##}";
        }

        #endregion
    }
}
