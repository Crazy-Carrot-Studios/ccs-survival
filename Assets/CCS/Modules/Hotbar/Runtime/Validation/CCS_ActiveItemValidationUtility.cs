using CCS.Modules.Inventory;
using CCS.Survival;

// =============================================================================
// SCRIPT: CCS_ActiveItemValidationUtility
// CATEGORY: Modules / Hotbar / Runtime / Validation
// PURPOSE: Runtime validation helpers for active item module foundation.
// PLACEMENT: Used by CCS_HotbarValidationValidator.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: Editor asset checks remain in the editor validator.
// =============================================================================

namespace CCS.Modules.Hotbar
{
    public static class CCS_ActiveItemValidationUtility
    {
        private const string ModuleRoot = "Assets/CCS/Modules/Hotbar";
        private const string ActiveItemRoot = ModuleRoot + "/Runtime/ActiveItem";
        private const string DefaultProfilePath = "Assets/CCS/Survival/Profiles/Hotbar/CCS_DefaultActiveItemProfile.asset";

        private static readonly string[] RequiredRoutingScriptNames =
        {
            "CCS_ActiveItemTargetResolver",
            "CCS_ActiveItemGatheringToolUtility",
            "CCS_ActiveItemTargetContext"
        };

        private static readonly string[] PrimitiveWeaponItemIds =
        {
            "ccs.survival.item.starter.spear"
        };

        private static readonly string[] PrimitiveToolItemIds =
        {
            "ccs.survival.item.tool.hatchet.bone",
            "ccs.survival.item.tool.pick.bone"
        };

        public static CCS_SurvivalValidationResult ValidateModuleFolders()
        {
            if (!System.IO.Directory.Exists(ActiveItemRoot))
            {
                return CCS_SurvivalValidationResult.Fail($"Missing active item folder: {ActiveItemRoot}");
            }

            if (!System.IO.Directory.Exists(ModuleRoot + "/Runtime/Profiles"))
            {
                return CCS_SurvivalValidationResult.Fail("Missing Hotbar Runtime/Profiles folder.");
            }

            return CCS_SurvivalValidationResult.Pass("Active item module folders are present.");
        }

        public static CCS_SurvivalValidationResult ValidateProfile(CCS_ActiveItemProfile profile)
        {
            if (profile == null)
            {
                return CCS_SurvivalValidationResult.Fail($"Missing active item profile: {DefaultProfilePath}");
            }

            if (profile.UseCooldownSeconds < 0f)
            {
                return CCS_SurvivalValidationResult.Fail("Active item use cooldown cannot be negative.");
            }

            return CCS_SurvivalValidationResult.Pass("Active item profile validated.");
        }

        public static CCS_SurvivalValidationResult ValidateToolRoutingScriptsPresent()
        {
            for (int index = 0; index < RequiredRoutingScriptNames.Length; index++)
            {
                string scriptPath = ActiveItemRoot + "/" + RequiredRoutingScriptNames[index] + ".cs";
                if (!System.IO.File.Exists(scriptPath))
                {
                    return CCS_SurvivalValidationResult.Fail($"Missing active item routing script: {scriptPath}");
                }
            }

            return CCS_SurvivalValidationResult.Pass("Active item tool routing scripts are present.");
        }

        public static CCS_SurvivalValidationResult ValidatePrimitiveItemClassifications(
            CCS_ItemDefinition[] itemDefinitions)
        {
            if (itemDefinitions == null || itemDefinitions.Length == 0)
            {
                return CCS_SurvivalValidationResult.Pass(
                    "Primitive item classification check skipped (no definitions supplied).");
            }

            for (int index = 0; index < PrimitiveWeaponItemIds.Length; index++)
            {
                string weaponItemId = PrimitiveWeaponItemIds[index];
                CCS_ItemDefinition weaponDefinition = FindItemDefinition(itemDefinitions, weaponItemId);
                if (weaponDefinition == null)
                {
                    continue;
                }

                if (!CCS_ItemGameplayUtility.IsWeaponItem(weaponDefinition))
                {
                    return CCS_SurvivalValidationResult.Fail(
                        $"Primitive weapon item is not classified as weapon: {weaponItemId}");
                }
            }

            for (int index = 0; index < PrimitiveToolItemIds.Length; index++)
            {
                string toolItemId = PrimitiveToolItemIds[index];
                CCS_ItemDefinition toolDefinition = FindItemDefinition(itemDefinitions, toolItemId);
                if (toolDefinition == null)
                {
                    continue;
                }

                if (!CCS_ItemGameplayUtility.IsToolItem(toolDefinition))
                {
                    return CCS_SurvivalValidationResult.Fail(
                        $"Primitive tool item is not classified as tool: {toolItemId}");
                }
            }

            return CCS_SurvivalValidationResult.Pass("Primitive active item classifications validated.");
        }

        public static string DefaultProfileAssetPath => DefaultProfilePath;

        public static string[] PrimitiveWeaponItemIdsForValidation => PrimitiveWeaponItemIds;

        public static string[] PrimitiveToolItemIdsForValidation => PrimitiveToolItemIds;

        private static CCS_ItemDefinition FindItemDefinition(CCS_ItemDefinition[] definitions, string itemId)
        {
            for (int index = 0; index < definitions.Length; index++)
            {
                CCS_ItemDefinition definition = definitions[index];
                if (definition != null && definition.ItemId == itemId)
                {
                    return definition;
                }
            }

            return null;
        }
    }
}
