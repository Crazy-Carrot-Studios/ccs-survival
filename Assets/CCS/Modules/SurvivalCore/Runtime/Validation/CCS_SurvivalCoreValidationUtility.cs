using System.Collections.Generic;
using CCS.Survival;

// =============================================================================
// SCRIPT: CCS_SurvivalCoreValidationUtility
// CATEGORY: Survival / Runtime / SurvivalCore / Validation
// PURPOSE: Runtime-safe validation helpers for survival core profiles and stat definitions.
// PLACEMENT: Used by editor validators and future bootstrap checks.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-28
// NOTES: Returns CCS_SurvivalValidationResult for consistency with survival foundation.
// =============================================================================

namespace CCS.Modules.SurvivalCore
{
    public static class CCS_SurvivalCoreValidationUtility
    {
        private static readonly CCS_SurvivalStatType[] RequiredStatTypes =
        {
            CCS_SurvivalStatType.Health,
            CCS_SurvivalStatType.Stamina,
            CCS_SurvivalStatType.Hunger,
            CCS_SurvivalStatType.Thirst,
            CCS_SurvivalStatType.Temperature,
            CCS_SurvivalStatType.Fatigue
        };

        #region Public Methods

        public static CCS_SurvivalValidationResult ValidateProfile(CCS_SurvivalCoreProfile profile)
        {
            if (profile == null)
            {
                return CCS_SurvivalValidationResult.Fail("Survival core profile is null.");
            }

            CCS_SurvivalValidationResult baseValidation = CCS_SurvivalProfileValidationUtility.ValidateProfile(profile);
            if (!baseValidation.IsSuccess)
            {
                return baseValidation;
            }

            if (profile.StatDefinitions == null || profile.StatDefinitions.Count == 0)
            {
                return CCS_SurvivalValidationResult.Fail("Survival core profile has no stat definitions.");
            }

            HashSet<CCS_SurvivalStatType> seenStatTypes = new HashSet<CCS_SurvivalStatType>();

            for (int index = 0; index < profile.StatDefinitions.Count; index++)
            {
                CCS_SurvivalStatDefinition definition = profile.StatDefinitions[index];
                if (definition == null)
                {
                    return CCS_SurvivalValidationResult.Fail($"Stat definition at index {index} is null.");
                }

                if (!seenStatTypes.Add(definition.StatType))
                {
                    return CCS_SurvivalValidationResult.Fail(
                        $"Duplicate stat definition for {definition.StatType}.");
                }

                CCS_SurvivalValidationResult definitionValidation = ValidateStatDefinition(definition);
                if (!definitionValidation.IsSuccess)
                {
                    return definitionValidation;
                }
            }

            for (int requiredIndex = 0; requiredIndex < RequiredStatTypes.Length; requiredIndex++)
            {
                CCS_SurvivalStatType requiredType = RequiredStatTypes[requiredIndex];
                if (!seenStatTypes.Contains(requiredType))
                {
                    return CCS_SurvivalValidationResult.Fail(
                        $"Survival core profile is missing stat definition for {requiredType}.");
                }
            }

            if (profile.DecayDefinitions != null)
            {
                for (int decayIndex = 0; decayIndex < profile.DecayDefinitions.Count; decayIndex++)
                {
                    CCS_SurvivalStatDecayDefinition decayDefinition = profile.DecayDefinitions[decayIndex];
                    if (decayDefinition == null)
                    {
                        return CCS_SurvivalValidationResult.Fail(
                            $"Decay definition at index {decayIndex} is null.");
                    }

                    CCS_SurvivalValidationResult decayValidation = ValidateDecayDefinition(decayDefinition);
                    if (!decayValidation.IsSuccess)
                    {
                        return decayValidation;
                    }
                }
            }

            if (profile.PassiveHealthHealPerSecond < 0f || profile.PassiveHealthDamagePerSecond < 0f)
            {
                return CCS_SurvivalValidationResult.Fail(
                    "Passive health heal/damage per second must be non-negative.");
            }

            if (profile.StaminaRecoveryPerSecond < 0f || profile.StaminaDrainPerSecond < 0f)
            {
                return CCS_SurvivalValidationResult.Fail(
                    "Stamina recovery/drain per second must be non-negative.");
            }

            if (profile.TemperatureRecoveryRate < 0f || profile.TemperatureDecayRate < 0f)
            {
                return CCS_SurvivalValidationResult.Fail(
                    "Temperature recovery/decay rates must be non-negative.");
            }

            if (profile.ExposureFatigueMultiplier < 0f || profile.WetnessThirstMultiplier < 0f)
            {
                return CCS_SurvivalValidationResult.Fail(
                    "Exposure fatigue and wetness thirst multipliers must be non-negative.");
            }

            if (profile.MinimumTemperatureClamp > profile.MaximumTemperatureClamp)
            {
                return CCS_SurvivalValidationResult.Fail(
                    "Minimum temperature clamp must be <= maximum temperature clamp.");
            }

            return CCS_SurvivalValidationResult.Pass("Survival core profile validated.");
        }

        public static CCS_SurvivalValidationResult ValidateStatDefinition(CCS_SurvivalStatDefinition definition)
        {
            if (definition == null)
            {
                return CCS_SurvivalValidationResult.Fail("Stat definition is null.");
            }

            if (!CCS_SurvivalStatUtility.IsValidRange(definition.MinValue, definition.MaxValue))
            {
                return CCS_SurvivalValidationResult.Fail(
                    $"{definition.StatType}: max must be >= min.");
            }

            if (!CCS_SurvivalStatUtility.IsValidStartingValue(
                    definition.StartingValue,
                    definition.MinValue,
                    definition.MaxValue))
            {
                return CCS_SurvivalValidationResult.Fail(
                    $"{definition.StatType}: starting value must be within min/max.");
            }

            return CCS_SurvivalValidationResult.Pass();
        }

        public static CCS_SurvivalValidationResult ValidateDecayDefinition(CCS_SurvivalStatDecayDefinition definition)
        {
            if (definition == null)
            {
                return CCS_SurvivalValidationResult.Fail("Decay definition is null.");
            }

            if (!CCS_SurvivalStatUtility.IsValidDecayRate(definition.ChangePerSecond))
            {
                return CCS_SurvivalValidationResult.Fail(
                    $"{definition.StatType}: changePerSecond must be a non-negative finite value.");
            }

            if (definition.UseTemperatureComfortDrift
                && definition.StatType != CCS_SurvivalStatType.Temperature)
            {
                return CCS_SurvivalValidationResult.Warn(
                    "Temperature comfort drift is enabled on a non-temperature decay entry.");
            }

            return CCS_SurvivalValidationResult.Pass();
        }

        #endregion
    }
}
