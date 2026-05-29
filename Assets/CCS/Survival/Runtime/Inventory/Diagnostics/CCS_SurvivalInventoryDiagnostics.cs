// =============================================================================
// SCRIPT: CCS_SurvivalInventoryDiagnostics
// CATEGORY: Survival / Runtime / Inventory / Diagnostics
// PURPOSE: Inventory-layer diagnostic aliases for survival foundation constants.
// PLACEMENT: Static utility. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Canonical values live in CCS_SurvivalRuntimeConstants.
// =============================================================================

using CCS.Survival;

namespace CCS.Survival.Inventory
{
    public static class CCS_SurvivalInventoryDiagnostics
    {
        public const string LogCategory = CCS_SurvivalRuntimeConstants.InventoryLogCategory;

        public const string InstallerLogCategory = CCS_SurvivalRuntimeConstants.InventoryInstallerLogCategory;

        public const string ModuleId = CCS_SurvivalRuntimeConstants.InventoryModuleId;
    }
}
