using System;
using System.Collections.Generic;

// =============================================================================
// SCRIPT: CCS_SurvivalValidationPipeline
// CATEGORY: Survival / Editor / Development / Validation
// PURPOSE: Central validation report pipeline with registrable validators.
// PLACEMENT: Invoked by CCS_SurvivalValidationMenu. Modules register validators at load time.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-28
// NOTES: Avoid hard-coding checks in menu classes. Combat/Building/AI add validators later.
// =============================================================================

namespace CCS.Survival.Editor.Development
{
    public static class CCS_SurvivalValidationPipeline
    {
        private static readonly List<CCS_ISurvivalValidationValidator> registeredValidators =
            new List<CCS_ISurvivalValidationValidator>(16);

        private static bool foundationValidatorRegistered;

        #region Public Methods

        public static void RegisterValidator(CCS_ISurvivalValidationValidator validator)
        {
            if (validator == null)
            {
                return;
            }

            for (int index = 0; index < registeredValidators.Count; index++)
            {
                if (string.Equals(
                        registeredValidators[index].ValidatorId,
                        validator.ValidatorId,
                        StringComparison.Ordinal))
                {
                    registeredValidators[index] = validator;
                    return;
                }
            }

            registeredValidators.Add(validator);
        }

        public static void UnregisterValidator(string validatorId)
        {
            if (string.IsNullOrWhiteSpace(validatorId))
            {
                return;
            }

            for (int index = registeredValidators.Count - 1; index >= 0; index--)
            {
                if (string.Equals(registeredValidators[index].ValidatorId, validatorId, StringComparison.Ordinal))
                {
                    registeredValidators.RemoveAt(index);
                }
            }
        }

        public static CCS_SurvivalValidationReport RunAll()
        {
            EnsureFoundationValidatorRegistered();

            CCS_SurvivalValidationReport report = new CCS_SurvivalValidationReport();

            for (int index = 0; index < registeredValidators.Count; index++)
            {
                CCS_ISurvivalValidationValidator validator = registeredValidators[index];
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Info,
                    "Validation Pipeline",
                    $"Running validator: {validator.ValidatorId}");

                validator.Validate(report);
            }

            if (registeredValidators.Count == 0)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Warning,
                    "Validation Pipeline",
                    "No validators registered.");
            }

            return report;
        }

        public static int GetRegisteredValidatorCount()
        {
            EnsureFoundationValidatorRegistered();
            return registeredValidators.Count;
        }

        #endregion

        #region Private Methods

        private static void EnsureFoundationValidatorRegistered()
        {
            if (foundationValidatorRegistered)
            {
                return;
            }

            RegisterValidator(new CCS_SurvivalFoundationValidationValidator());
            foundationValidatorRegistered = true;
        }

        #endregion
    }
}
