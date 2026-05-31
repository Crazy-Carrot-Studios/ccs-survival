using System.Collections.Generic;
using CCS.Survival;

// =============================================================================
// SCRIPT: CCS_WorldResourceValidationUtility
// CATEGORY: Modules / WorldResources / Runtime / Validation
// PURPOSE: Runtime-safe validation for world resource profiles and definitions.
// PLACEMENT: Used by editor validators and harvest service preflight checks.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Returns CCS_SurvivalValidationResult. No UnityEditor references.
// =============================================================================

namespace CCS.Modules.WorldResources
{
    public static class CCS_WorldResourceValidationUtility
    {
        #region Public Methods

        public static CCS_SurvivalValidationResult ValidateProfile(CCS_WorldResourceProfile profile)
        {
            if (profile == null)
            {
                return CCS_SurvivalValidationResult.Fail("World resource profile is null.");
            }

            CCS_SurvivalValidationResult baseValidation = CCS_SurvivalProfileValidationUtility.ValidateProfile(profile);
            if (!baseValidation.IsSuccess)
            {
                return baseValidation;
            }

            if (profile.GlobalRespawnMultiplier < 0f)
            {
                return CCS_SurvivalValidationResult.Fail("Global respawn multiplier cannot be negative.");
            }

            return CCS_SurvivalValidationResult.Pass("World resource profile validated.");
        }

        public static CCS_SurvivalValidationResult ValidateResourceDefinition(
            CCS_ResourceDefinition resourceDefinition)
        {
            if (resourceDefinition == null)
            {
                return CCS_SurvivalValidationResult.Fail("Resource definition is null.");
            }

            if (string.IsNullOrWhiteSpace(resourceDefinition.ResourceId))
            {
                return CCS_SurvivalValidationResult.Fail("Resource ID is required.");
            }

            if (resourceDefinition.MaxHarvestCount <= 0)
            {
                return CCS_SurvivalValidationResult.Fail("Max harvest count must be greater than zero.");
            }

            if (resourceDefinition.RespawnTimeSeconds < 0f)
            {
                return CCS_SurvivalValidationResult.Fail("Respawn time seconds cannot be negative.");
            }

            IReadOnlyList<CCS_ResourceDropDefinition> dropDefinitions = resourceDefinition.DropDefinitions;
            if (dropDefinitions == null || dropDefinitions.Count == 0)
            {
                return CCS_SurvivalValidationResult.Fail("Resource requires at least one drop definition.");
            }

            for (int i = 0; i < dropDefinitions.Count; i++)
            {
                CCS_ResourceDropDefinition dropDefinition = dropDefinitions[i];
                if (dropDefinition == null)
                {
                    return CCS_SurvivalValidationResult.Fail("Resource drop entry is null.");
                }

                if (dropDefinition.ItemDefinition == null)
                {
                    return CCS_SurvivalValidationResult.Fail("Resource drop item definition is null.");
                }

                if (dropDefinition.MinQuantity <= 0 || dropDefinition.MaxQuantity <= 0)
                {
                    return CCS_SurvivalValidationResult.Fail("Resource drop quantities must be greater than zero.");
                }
            }

            return CCS_SurvivalValidationResult.Pass("Resource definition validated.");
        }

        public static CCS_SurvivalValidationResult ValidateRequiredNodeTypes()
        {
            if (!System.Enum.IsDefined(typeof(CCS_ResourceNodeType), CCS_ResourceNodeType.Tree))
            {
                return CCS_SurvivalValidationResult.Fail("Resource node type Tree is not defined.");
            }

            if (!System.Enum.IsDefined(typeof(CCS_ResourceNodeType), CCS_ResourceNodeType.Rock))
            {
                return CCS_SurvivalValidationResult.Fail("Resource node type Rock is not defined.");
            }

            if (!System.Enum.IsDefined(typeof(CCS_ResourceNodeType), CCS_ResourceNodeType.Plant))
            {
                return CCS_SurvivalValidationResult.Fail("Resource node type Plant is not defined.");
            }

            if (!System.Enum.IsDefined(typeof(CCS_ResourceNodeType), CCS_ResourceNodeType.Gatherable))
            {
                return CCS_SurvivalValidationResult.Fail("Resource node type Gatherable is not defined.");
            }

            if (!System.Enum.IsDefined(typeof(CCS_ResourceNodeType), CCS_ResourceNodeType.Custom))
            {
                return CCS_SurvivalValidationResult.Fail("Resource node type Custom is not defined.");
            }

            return CCS_SurvivalValidationResult.Pass("Required resource node types validated.");
        }

        public static CCS_SurvivalValidationResult ValidateRequiredToolTypes()
        {
            if (!System.Enum.IsDefined(typeof(CCS_RequiredToolType), CCS_RequiredToolType.None))
            {
                return CCS_SurvivalValidationResult.Fail("Required tool type None is not defined.");
            }

            if (!System.Enum.IsDefined(typeof(CCS_RequiredToolType), CCS_RequiredToolType.Axe))
            {
                return CCS_SurvivalValidationResult.Fail("Required tool type Axe is not defined.");
            }

            if (!System.Enum.IsDefined(typeof(CCS_RequiredToolType), CCS_RequiredToolType.Pickaxe))
            {
                return CCS_SurvivalValidationResult.Fail("Required tool type Pickaxe is not defined.");
            }

            if (!System.Enum.IsDefined(typeof(CCS_RequiredToolType), CCS_RequiredToolType.Knife))
            {
                return CCS_SurvivalValidationResult.Fail("Required tool type Knife is not defined.");
            }

            if (!System.Enum.IsDefined(typeof(CCS_RequiredToolType), CCS_RequiredToolType.Shovel))
            {
                return CCS_SurvivalValidationResult.Fail("Required tool type Shovel is not defined.");
            }

            return CCS_SurvivalValidationResult.Pass("Required tool types validated.");
        }

        #endregion
    }
}
