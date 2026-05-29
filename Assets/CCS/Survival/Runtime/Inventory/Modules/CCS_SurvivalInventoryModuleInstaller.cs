using CCS.Core;
using CCS.Survival;

// =============================================================================
// SCRIPT: CCS_SurvivalInventoryModuleInstaller
// CATEGORY: Survival / Runtime / Inventory / Modules
// PURPOSE: Installs the survival inventory module and registers inventory service.
// PLACEMENT: Invoked from CCS_SurvivalInstaller after character module install.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Default slot count 16. No pickup bridge in Phase 2B.
// =============================================================================

namespace CCS.Survival.Inventory
{
    public sealed class CCS_SurvivalInventoryModuleInstaller : CCS_SurvivalModuleInstallerBase
    {
        private const int DefaultSlotCount = 16;

        #region Variables

        private readonly int slotCount;

        #endregion

        #region Public Methods

        public CCS_SurvivalInventoryModuleInstaller(int slotCount, bool enableDebugLogs)
            : base(
                new CCS_SurvivalInventoryModule(slotCount < 1 ? DefaultSlotCount : slotCount, enableDebugLogs),
                CCS_SurvivalRuntimeConstants.InventoryInstallerLogCategory,
                enableDebugLogs)
        {
            this.slotCount = slotCount < 1 ? DefaultSlotCount : slotCount;
        }

        #endregion

        #region Protected Methods

        protected override CCS_Result OnBeforeInstall(CCS_RuntimeHost runtimeHost)
        {
            LogSurvivalInstaller($"Inventory module installer before install. Slots={slotCount}.");
            return CCS_Result.Success();
        }

        protected override CCS_Result OnAfterInstall(CCS_RuntimeHost runtimeHost)
        {
            CCS_SurvivalValidationResult moduleValidation = CCS_SurvivalModuleValidationUtility.ValidateModule(Module);
            if (!moduleValidation.IsSuccess)
            {
                LogSurvivalInstaller($"Inventory module validation failed: {moduleValidation.Message}");
                return moduleValidation.ToCoreResult();
            }

            if (!runtimeHost.ServiceRegistry.TryGetService(out CCS_ISurvivalInventoryService inventoryService))
            {
                CCS_Logger.LogWarning(
                    CCS_SurvivalRuntimeConstants.InventoryInstallerLogCategory,
                    "CCS_ISurvivalInventoryService was not registered after inventory module install.");
                return CCS_Result.Failure(
                    CCS_CoreErrorCode.ValidationFailed,
                    "Inventory service missing after install.");
            }

            LogSurvivalInstaller(
                $"Inventory module installer after install. Slots={inventoryService.SlotCount}, Occupied={inventoryService.OccupiedSlotCount}.");
            return CCS_Result.Success();
        }

        #endregion
    }
}
