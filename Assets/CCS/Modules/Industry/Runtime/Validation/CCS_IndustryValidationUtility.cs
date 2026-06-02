using System.Collections.Generic;
using CCS.Survival;

namespace CCS.Modules.Industry
{
    public static class CCS_IndustryValidationUtility
    {
        public static CCS_SurvivalValidationResult ValidateProfile(CCS_IndustryProfile profile)
        {
            if (profile == null)
            {
                return CCS_SurvivalValidationResult.Fail("Industry profile is null.");
            }

            HashSet<string> processIds = new HashSet<string>();
            if (profile.ProcessDefinitions != null)
            {
                for (int index = 0; index < profile.ProcessDefinitions.Count; index++)
                {
                    CCS_IndustryDefinition definition = profile.ProcessDefinitions[index];
                    if (definition == null)
                    {
                        continue;
                    }

                    CCS_SurvivalValidationResult definitionValidation = ValidateProcessDefinition(definition);
                    if (!definitionValidation.IsSuccess)
                    {
                        return definitionValidation;
                    }

                    if (!processIds.Add(definition.ProcessId))
                    {
                        return CCS_SurvivalValidationResult.Fail($"Duplicate industry process id: {definition.ProcessId}");
                    }
                }
            }

            HashSet<string> blacksmithIds = new HashSet<string>();
            if (profile.BlacksmithRecipes != null)
            {
                for (int index = 0; index < profile.BlacksmithRecipes.Count; index++)
                {
                    CCS_BlacksmithRecipeDefinition recipe = profile.BlacksmithRecipes[index];
                    if (recipe == null)
                    {
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(recipe.BlacksmithRecipeId))
                    {
                        return CCS_SurvivalValidationResult.Fail("Blacksmith recipe is missing id.");
                    }

                    if (!blacksmithIds.Add(recipe.BlacksmithRecipeId))
                    {
                        return CCS_SurvivalValidationResult.Fail($"Duplicate blacksmith recipe id: {recipe.BlacksmithRecipeId}");
                    }

                    if (recipe.CraftingRecipe == null)
                    {
                        return CCS_SurvivalValidationResult.Fail(
                            $"Blacksmith recipe {recipe.BlacksmithRecipeId} is missing crafting recipe.");
                    }
                }
            }

            return CCS_SurvivalValidationResult.Pass("Industry profile is valid.");
        }

        public static CCS_SurvivalValidationResult ValidateProcessDefinition(CCS_IndustryDefinition definition)
        {
            if (definition == null)
            {
                return CCS_SurvivalValidationResult.Fail("Industry process definition is null.");
            }

            if (string.IsNullOrWhiteSpace(definition.ProcessId))
            {
                return CCS_SurvivalValidationResult.Fail("Industry process is missing process id.");
            }

            if (string.IsNullOrWhiteSpace(definition.RequiredWorkstationRoleId))
            {
                return CCS_SurvivalValidationResult.Fail(
                    $"Industry process {definition.ProcessId} is missing workstation role.");
            }

            if (definition.Inputs == null || definition.Inputs.Count == 0)
            {
                return CCS_SurvivalValidationResult.Fail(
                    $"Industry process {definition.ProcessId} has no inputs.");
            }

            if (definition.Outputs == null || definition.Outputs.Count == 0)
            {
                return CCS_SurvivalValidationResult.Fail(
                    $"Industry process {definition.ProcessId} has no outputs.");
            }

            return CCS_SurvivalValidationResult.Pass("Industry process definition is valid.");
        }
    }
}
