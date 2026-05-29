// =============================================================================
// SCRIPT: CCS_SurvivalItemDefinitionValidationUtility
// CATEGORY: Survival / Runtime / Inventory / Definitions
// PURPOSE: Validates survival item definition authoring data and save-stable item IDs.
// PLACEMENT: Runtime utility. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Item IDs use survival.item.* prefix and lowercase reverse-DNS formatting.
// =============================================================================

using CCS.Survival;

namespace CCS.Survival.Inventory
{
    public static class CCS_SurvivalItemDefinitionValidationUtility
    {
        public const string ItemIdPrefix = "survival.item.";

        #region Public Methods

        public static CCS_SurvivalValidationResult ValidateDefinition(CCS_SurvivalItemDefinition definition)
        {
            if (definition == null)
            {
                return CCS_SurvivalValidationResult.Fail("Item definition reference is null.");
            }

            CCS_SurvivalValidationResult itemIdValidation = ValidateItemId(definition.ItemId);
            if (!itemIdValidation.IsSuccess)
            {
                return itemIdValidation;
            }

            if (string.IsNullOrWhiteSpace(definition.DisplayName))
            {
                return CCS_SurvivalValidationResult.Fail("Item display name is null or empty.");
            }

            int effectiveMaxStack = definition.GetEffectiveMaxStackSize();
            if (effectiveMaxStack < 1)
            {
                return CCS_SurvivalValidationResult.Fail("Item max stack size must be at least 1.");
            }

            return CCS_SurvivalValidationResult.Pass($"Item definition validated: {definition.ItemId}.");
        }

        public static CCS_SurvivalValidationResult ValidateItemId(string itemId)
        {
            if (string.IsNullOrWhiteSpace(itemId))
            {
                return CCS_SurvivalValidationResult.Fail("Item ID is null or empty.");
            }

            if (!itemId.StartsWith(ItemIdPrefix))
            {
                return CCS_SurvivalValidationResult.Fail(
                    $"Item ID must start with '{ItemIdPrefix}'. Got: {itemId}");
            }

            return CCS_SurvivalIdentityUtility.ValidateStableRuntimeId(
                itemId,
                ItemIdPrefix,
                "Survival item ID");
        }

        #endregion
    }
}
