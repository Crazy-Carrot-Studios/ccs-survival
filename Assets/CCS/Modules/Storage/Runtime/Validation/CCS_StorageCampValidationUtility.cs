using System.Collections.Generic;
using CCS.Survival;

namespace CCS.Modules.Storage
{
    public static class CCS_StorageCampValidationUtility
    {
        public static CCS_SurvivalValidationResult ValidateFrontierStorageCatalog(
            IReadOnlyList<CCS_StorageContainerDefinition> definitions)
        {
            if (definitions == null || definitions.Count == 0)
            {
                return CCS_SurvivalValidationResult.Fail("Frontier storage camp profile has no storage definitions.");
            }

            HashSet<string> ids = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase);
            bool hasCampContributor = false;
            for (int index = 0; index < definitions.Count; index++)
            {
                CCS_StorageContainerDefinition definition = definitions[index];
                if (definition == null || string.IsNullOrWhiteSpace(definition.ContainerId))
                {
                    return CCS_SurvivalValidationResult.Fail("Frontier storage definition id is required.");
                }

                if (!ids.Add(definition.ContainerId))
                {
                    return CCS_SurvivalValidationResult.Fail(
                        $"Duplicate frontier storage definition id '{definition.ContainerId}'.");
                }

                if (definition.ContributesToCampTier)
                {
                    hasCampContributor = true;
                    if (definition.PlaceableKitItem == null)
                    {
                        return CCS_SurvivalValidationResult.Fail(
                            $"Storage '{definition.ContainerId}' is missing placeable kit item.");
                    }

                    if (definition.PrefabReference == null)
                    {
                        return CCS_SurvivalValidationResult.Fail(
                            $"Storage '{definition.ContainerId}' is missing prefab reference.");
                    }
                }
            }

            if (!hasCampContributor)
            {
                return CCS_SurvivalValidationResult.Fail(
                    "At least one frontier storage definition must contribute to camp tier.");
            }

            return CCS_SurvivalValidationResult.Pass("Frontier storage catalog is valid.");
        }
    }
}
