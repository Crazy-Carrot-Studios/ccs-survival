using CCS.Core;
using CCS.Survival;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_SurvivalInventoryBootstrapSelfTest
// CATEGORY: Survival / Runtime / Inventory / Validation
// PURPOSE: One-shot container smoke test invoked at inventory module install when debug logs are on.
// PLACEMENT: Called from CCS_SurvivalInventoryModule.OnInstall only.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Uses temporary ScriptableObject definitions; does not mutate player inventory state.
// =============================================================================

namespace CCS.Survival.Inventory
{
    internal static class CCS_SurvivalInventoryBootstrapSelfTest
    {
        private const string LogCategory = CCS_SurvivalRuntimeConstants.InventoryLogCategory;

        #region Public Methods

        public static bool RunContainerSmokeTest(bool enableDebugLogs)
        {
            CCS_SurvivalInventoryContainer testContainer = new CCS_SurvivalInventoryContainer(8);

            CCS_SurvivalItemDefinition foodTin = CreateTestDefinition(
                "survival.item.food_tin",
                "Food Tin",
                CCS_SurvivalItemCategory.Food,
                true,
                12);

            CCS_SurvivalItemDefinition kindling = CreateTestDefinition(
                "survival.item.kindling",
                "Kindling",
                CCS_SurvivalItemCategory.Material,
                true,
                24);

            int addedFood = testContainer.AddItem(foodTin, 1, out int foodRemaining);
            int addedKindling = testContainer.AddItem(kindling, 3, out int kindlingRemaining);

            bool hasFood = testContainer.HasItem(foodTin.ItemId, 1);
            bool hasKindling = testContainer.HasItem(kindling.ItemId, 3);
            int removed = testContainer.RemoveItem(kindling.ItemId, 1);
            int kindlingCount = testContainer.GetItemCount(kindling.ItemId);

            Object.Destroy(foodTin);
            Object.Destroy(kindling);

            bool passed = addedFood == 1
                && foodRemaining == 0
                && addedKindling == 3
                && kindlingRemaining == 0
                && hasFood
                && hasKindling
                && removed == 1
                && kindlingCount == 2;

            if (enableDebugLogs)
            {
                string message = passed
                    ? "Inventory bootstrap self-test PASSED."
                    : "Inventory bootstrap self-test FAILED.";
                CCS_Logger.Log(LogCategory, message, true);
            }

            return passed;
        }

        #endregion

        #region Private Methods

        private static CCS_SurvivalItemDefinition CreateTestDefinition(
            string itemId,
            string displayName,
            CCS_SurvivalItemCategory category,
            bool isStackable,
            int maxStackSize)
        {
            CCS_SurvivalItemDefinition definition = ScriptableObject.CreateInstance<CCS_SurvivalItemDefinition>();
            definition.ConfigureRuntimeTestData(itemId, displayName, category, isStackable, maxStackSize);
            return definition;
        }

        #endregion
    }
}
