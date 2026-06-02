using System.Collections.Generic;
using CCS.Survival;

namespace CCS.Modules.Firearms
{
    public static class CCS_FirearmValidationUtility
    {
        public static CCS_SurvivalValidationResult ValidateProfile(CCS_FirearmProfile profile)
        {
            if (profile == null)
            {
                return CCS_SurvivalValidationResult.Fail("Firearm profile is missing.");
            }

            HashSet<string> firearmIds = new HashSet<string>();
            IReadOnlyList<CCS_FirearmDefinition> firearms = profile.FirearmDefinitions;
            for (int index = 0; index < firearms.Count; index++)
            {
                CCS_FirearmDefinition definition = firearms[index];
                if (definition == null)
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(definition.FirearmId))
                {
                    return CCS_SurvivalValidationResult.Fail("Firearm definition has an empty firearmId.");
                }

                if (!firearmIds.Add(definition.FirearmId))
                {
                    return CCS_SurvivalValidationResult.Fail($"Duplicate firearmId: {definition.FirearmId}");
                }

                if (string.IsNullOrWhiteSpace(definition.InventoryItemId))
                {
                    return CCS_SurvivalValidationResult.Fail(
                        $"Firearm '{definition.FirearmId}' is missing inventoryItemId.");
                }

                if (definition.AmmoDefinition == null)
                {
                    return CCS_SurvivalValidationResult.Fail(
                        $"Firearm '{definition.FirearmId}' is missing ammo definition.");
                }
            }

            HashSet<string> ammoIds = new HashSet<string>();
            IReadOnlyList<CCS_AmmoDefinition> ammoDefinitions = profile.AmmoDefinitions;
            for (int index = 0; index < ammoDefinitions.Count; index++)
            {
                CCS_AmmoDefinition ammo = ammoDefinitions[index];
                if (ammo == null)
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(ammo.AmmoId))
                {
                    return CCS_SurvivalValidationResult.Fail("Ammo definition has an empty ammoId.");
                }

                if (!ammoIds.Add(ammo.AmmoId))
                {
                    return CCS_SurvivalValidationResult.Fail($"Duplicate ammoId: {ammo.AmmoId}");
                }
            }

            return CCS_SurvivalValidationResult.Pass("Firearm profile validated.");
        }
    }
}
