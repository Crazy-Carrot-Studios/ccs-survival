using System;
using System.Collections.Generic;
using CCS.Modules.Inventory;
using CCS.Survival;

// =============================================================================
// SCRIPT: CCS_RanchValidationUtility
// CATEGORY: Modules / Ranching / Runtime / Validation
// PURPOSE: Profile and content validation for the generic ranching module.
// PLACEMENT: Used by editor validators and runtime profile initialization.
// AUTHOR: James Schilz
// CREATED: 2026-06-02
// NOTES: Milestone 2.1.0 ranching foundation.
// =============================================================================

namespace CCS.Modules.Ranching
{
    public static class CCS_RanchValidationUtility
    {
        public static CCS_SurvivalValidationResult ValidateProfile(CCS_LivestockProfile profile)
        {
            if (profile == null)
            {
                return CCS_SurvivalValidationResult.Fail("Livestock profile is missing.");
            }

            CCS_SurvivalValidationResult baseValidation = CCS_SurvivalProfileValidationUtility.ValidateProfile(profile);
            if (!baseValidation.IsSuccess)
            {
                return baseValidation;
            }

            HashSet<string> livestockIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            CCS_LivestockDefinition[] livestockDefinitions = profile.LivestockDefinitions;
            if (livestockDefinitions == null || livestockDefinitions.Length == 0)
            {
                return CCS_SurvivalValidationResult.Fail("Livestock profile requires at least one livestock definition.");
            }

            for (int index = 0; index < livestockDefinitions.Length; index++)
            {
                CCS_SurvivalValidationResult definitionResult = ValidateLivestockDefinition(livestockDefinitions[index]);
                if (!definitionResult.IsSuccess)
                {
                    return definitionResult;
                }

                CCS_LivestockDefinition definition = livestockDefinitions[index];
                if (definition != null && !string.IsNullOrWhiteSpace(definition.LivestockId))
                {
                    if (!livestockIds.Add(definition.LivestockId))
                    {
                        return CCS_SurvivalValidationResult.Fail(
                            $"Duplicate livestock id '{definition.LivestockId}'.");
                    }
                }
            }

            if (!profile.TryGetLivestockById(CCS_RanchingContentIds.ChickenLivestockId, out _))
            {
                return CCS_SurvivalValidationResult.Fail(
                    $"Livestock profile missing chicken definition '{CCS_RanchingContentIds.ChickenLivestockId}'.");
            }

            HashSet<string> structureIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            CCS_RanchStructureDefinition[] structureDefinitions = profile.RanchStructureDefinitions;
            if (structureDefinitions == null || structureDefinitions.Length == 0)
            {
                return CCS_SurvivalValidationResult.Fail("Livestock profile requires at least one ranch structure definition.");
            }

            for (int index = 0; index < structureDefinitions.Length; index++)
            {
                CCS_SurvivalValidationResult structureResult = ValidateStructureDefinition(structureDefinitions[index]);
                if (!structureResult.IsSuccess)
                {
                    return structureResult;
                }

                CCS_RanchStructureDefinition definition = structureDefinitions[index];
                if (definition != null && !string.IsNullOrWhiteSpace(definition.StructureDefinitionId))
                {
                    if (!structureIds.Add(definition.StructureDefinitionId))
                    {
                        return CCS_SurvivalValidationResult.Fail(
                            $"Duplicate ranch structure id '{definition.StructureDefinitionId}'.");
                    }
                }
            }

            if (!profile.TryGetStructureById(CCS_RanchingContentIds.ChickenCoopStructureId, out _))
            {
                return CCS_SurvivalValidationResult.Fail(
                    $"Livestock profile missing chicken coop '{CCS_RanchingContentIds.ChickenCoopStructureId}'.");
            }

            return CCS_SurvivalValidationResult.Pass("Livestock profile is valid.");
        }

        public static CCS_SurvivalValidationResult ValidateLivestockDefinition(CCS_LivestockDefinition definition)
        {
            if (definition == null)
            {
                return CCS_SurvivalValidationResult.Fail("Livestock definition is null.");
            }

            if (string.IsNullOrWhiteSpace(definition.LivestockId))
            {
                return CCS_SurvivalValidationResult.Fail("Livestock definition has empty livestockId.");
            }

            if (definition.PurchaseItem == null)
            {
                return CCS_SurvivalValidationResult.Fail(
                    $"Livestock '{definition.LivestockId}' is missing purchase item reference.");
            }

            if (definition.ProductionItem == null && string.IsNullOrWhiteSpace(definition.ProductionItemId))
            {
                return CCS_SurvivalValidationResult.Fail(
                    $"Livestock '{definition.LivestockId}' is missing production item reference.");
            }

            if (definition.ProductionIntervalSeconds <= 0f)
            {
                return CCS_SurvivalValidationResult.Fail(
                    $"Livestock '{definition.LivestockId}' production interval must be positive.");
            }

            return CCS_SurvivalValidationResult.Pass($"Livestock definition '{definition.LivestockId}' is valid.");
        }

        public static CCS_SurvivalValidationResult ValidateStructureDefinition(CCS_RanchStructureDefinition definition)
        {
            if (definition == null)
            {
                return CCS_SurvivalValidationResult.Fail("Ranch structure definition is null.");
            }

            if (string.IsNullOrWhiteSpace(definition.StructureDefinitionId))
            {
                return CCS_SurvivalValidationResult.Fail("Ranch structure definition has empty structureDefinitionId.");
            }

            if (definition.PlaceableKitItem == null)
            {
                return CCS_SurvivalValidationResult.Fail(
                    $"Ranch structure '{definition.StructureDefinitionId}' is missing placeable kit item.");
            }

            return CCS_SurvivalValidationResult.Pass(
                $"Ranch structure definition '{definition.StructureDefinitionId}' is valid.");
        }

        public static CCS_SurvivalValidationResult ValidateProductItem(CCS_ItemDefinition itemDefinition, string expectedItemId)
        {
            if (itemDefinition == null)
            {
                return CCS_SurvivalValidationResult.Fail($"Missing ranch product item '{expectedItemId}'.");
            }

            if (!string.Equals(itemDefinition.ItemId, expectedItemId, StringComparison.OrdinalIgnoreCase))
            {
                return CCS_SurvivalValidationResult.Fail(
                    $"Ranch product item id mismatch. Expected '{expectedItemId}', found '{itemDefinition.ItemId}'.");
            }

            return CCS_SurvivalValidationResult.Pass($"Ranch product item '{expectedItemId}' is valid.");
        }
    }
}
