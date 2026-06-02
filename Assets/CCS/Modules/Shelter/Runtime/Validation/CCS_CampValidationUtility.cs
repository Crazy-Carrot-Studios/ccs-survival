using System.Collections.Generic;
using CCS.Survival;

namespace CCS.Modules.Shelter
{
    public static class CCS_CampValidationUtility
    {
        public static CCS_SurvivalValidationResult ValidateCampDefinition(CCS_CampDefinition definition)
        {
            if (definition == null)
            {
                return CCS_SurvivalValidationResult.Fail("Camp definition is null.");
            }

            if (!definition.EnableCampTracking)
            {
                return CCS_SurvivalValidationResult.Fail("Camp tracking is disabled on camp definition.");
            }

            if (definition.CampDetectionRadius < 1f)
            {
                return CCS_SurvivalValidationResult.Fail("Camp detection radius must be at least 1.");
            }

            if (string.IsNullOrWhiteSpace(definition.CampfirePieceId))
            {
                return CCS_SurvivalValidationResult.Fail("Camp definition is missing campfire piece id.");
            }

            if (definition.CampTierProfile == null)
            {
                return CCS_SurvivalValidationResult.Fail("Camp definition is missing camp tier profile.");
            }

            CCS_SurvivalValidationResult tierResult = ValidateCampTierProfile(definition.CampTierProfile);
            if (!tierResult.IsSuccess)
            {
                return tierResult;
            }

            return ValidateShelterCatalog(definition.ShelterDefinitions);
        }

        public static CCS_SurvivalValidationResult ValidateCampTierProfile(CCS_CampTierProfile profile)
        {
            if (profile == null)
            {
                return CCS_SurvivalValidationResult.Fail("Camp tier profile is null.");
            }

            CCS_CampTierDefinition[] tiers = profile.GetTiersOrderedAscending();
            if (tiers.Length < 3)
            {
                return CCS_SurvivalValidationResult.Fail("Camp tier profile must include at least three tiers.");
            }

            bool hasTemporary = false;
            bool hasFrontier = false;
            bool hasHomestead = false;
            for (int index = 0; index < tiers.Length; index++)
            {
                CCS_CampTierDefinition tier = tiers[index];
                if (tier == null)
                {
                    continue;
                }

                switch (tier.CampTier)
                {
                    case CCS_CampTier.TemporaryCamp:
                        hasTemporary = true;
                        break;
                    case CCS_CampTier.FrontierCamp:
                        hasFrontier = true;
                        break;
                    case CCS_CampTier.FrontierHomestead:
                        hasHomestead = true;
                        break;
                }
            }

            if (!hasTemporary || !hasFrontier || !hasHomestead)
            {
                return CCS_SurvivalValidationResult.Fail(
                    "Camp tier profile must define TemporaryCamp, FrontierCamp, and FrontierHomestead.");
            }

            return CCS_SurvivalValidationResult.Pass("Camp tier profile is valid.");
        }

        public static CCS_SurvivalValidationResult ValidateWorkbenchCatalog(
            IReadOnlyList<CCS_WorkbenchDefinition> definitions)
        {
            if (definitions == null || definitions.Count == 0)
            {
                return CCS_SurvivalValidationResult.Fail("Camp definition has no workbench definitions.");
            }

            HashSet<string> ids = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase);
            for (int index = 0; index < definitions.Count; index++)
            {
                CCS_WorkbenchDefinition definition = definitions[index];
                if (definition == null || string.IsNullOrWhiteSpace(definition.WorkbenchDefinitionId))
                {
                    return CCS_SurvivalValidationResult.Fail("Workbench definition id is required.");
                }

                if (!ids.Add(definition.WorkbenchDefinitionId))
                {
                    return CCS_SurvivalValidationResult.Fail(
                        $"Duplicate workbench definition id '{definition.WorkbenchDefinitionId}'.");
                }

                if (definition.ContributesToCampTier && definition.PlaceableKitItem == null)
                {
                    return CCS_SurvivalValidationResult.Fail(
                        $"Workbench '{definition.WorkbenchDefinitionId}' is missing placeable kit item.");
                }
            }

            return CCS_SurvivalValidationResult.Pass("Frontier workbench catalog is valid.");
        }

        public static CCS_SurvivalValidationResult ValidateShelterDefinition(CCS_ShelterDefinition definition)
        {
            if (definition == null)
            {
                return CCS_SurvivalValidationResult.Fail("Shelter definition is null.");
            }

            if (string.IsNullOrWhiteSpace(definition.ShelterDefinitionId))
            {
                return CCS_SurvivalValidationResult.Fail("Shelter definition id is required.");
            }

            if (definition.IsFunctional && definition.PlaceableKitItem == null)
            {
                return CCS_SurvivalValidationResult.Fail(
                    $"Shelter '{definition.ShelterDefinitionId}' is missing placeable kit item.");
            }

            if (definition.GrantedCampTier == CCS_CampTier.None)
            {
                return CCS_SurvivalValidationResult.Fail(
                    $"Shelter '{definition.ShelterDefinitionId}' must grant a camp tier marker.");
            }

            return CCS_SurvivalValidationResult.Pass(
                $"Shelter definition '{definition.ShelterDefinitionId}' is valid.");
        }

        public static CCS_SurvivalValidationResult ValidateShelterCatalog(
            IReadOnlyList<CCS_ShelterDefinition> definitions)
        {
            if (definitions == null || definitions.Count == 0)
            {
                return CCS_SurvivalValidationResult.Fail("Camp definition has no shelter definitions.");
            }

            HashSet<string> ids = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase);
            bool hasFunctionalTierOne = false;

            for (int index = 0; index < definitions.Count; index++)
            {
                CCS_ShelterDefinition definition = definitions[index];
                CCS_SurvivalValidationResult definitionResult = ValidateShelterDefinition(definition);
                if (!definitionResult.IsSuccess)
                {
                    return definitionResult;
                }

                if (!ids.Add(definition.ShelterDefinitionId))
                {
                    return CCS_SurvivalValidationResult.Fail(
                        $"Duplicate shelter definition id '{definition.ShelterDefinitionId}'.");
                }

                if (definition.IsFunctional && definition.ShelterTier == 1)
                {
                    hasFunctionalTierOne = true;
                }
            }

            if (!hasFunctionalTierOne)
            {
                return CCS_SurvivalValidationResult.Fail(
                    "Camp definition must include at least one functional tier-one shelter.");
            }

            return CCS_SurvivalValidationResult.Pass("Frontier shelter catalog is valid.");
        }
    }
}
