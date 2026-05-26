using CCS.Core;

// =============================================================================
// SCRIPT: CCS_SurvivalInstaller
// CATEGORY: Survival / Installers
// PURPOSE: Survival-layer composition root for explicit gameplay module install sequencing.
// PLACEMENT: Registered on CCS_BootstrapRunner by CCS_SurvivalBootstrap.
// AUTHOR: James Schilz
// CREATED: 2026-05-24
// NOTES: 0.3.0 registers character module skeleton only. No inventory/crafting/save/net.
// =============================================================================

namespace CCS.Survival
{
    public sealed class CCS_SurvivalInstaller : CCS_IBootstrapInstaller
    {
        private const string LogCategory = CCS_SurvivalRuntimeConstants.SurvivalInstallerLogCategory;

        #region Variables

        private readonly CCS_SurvivalRuntimeContext survivalContext;
        private readonly bool enableDebugLogs;

        #endregion

        #region Public Methods

        public CCS_SurvivalInstaller(CCS_SurvivalRuntimeContext survivalContext, bool enableDebugLogs = false)
        {
            this.survivalContext = survivalContext;
            this.enableDebugLogs = enableDebugLogs;
        }

        public void Install(CCS_RuntimeHost runtimeHost)
        {
            CCS_Result hostValidation = CCS_CoreValidation.ValidateRuntimeHost(runtimeHost);
            if (!hostValidation.IsSuccess)
            {
                CCS_Logger.LogWarning(LogCategory, hostValidation.Message);
                return;
            }

            if (!CCS_Validation.IsObjectValid(survivalContext) || !survivalContext.IsSurvivalLayerInitialized)
            {
                CCS_Logger.LogWarning(LogCategory, "Survival context must be initialized before survival installer runs.");
                return;
            }

            InstallCharacterModuleSkeleton(runtimeHost);

            CCS_Logger.Log(LogCategory, "Survival installer completed.", enableDebugLogs);
        }

        #endregion

        #region Private Methods

        private void InstallCharacterModuleSkeleton(CCS_RuntimeHost runtimeHost)
        {
            CCS_SurvivalCharacterModuleInstaller characterModuleInstaller =
                new CCS_SurvivalCharacterModuleInstaller(enableDebugLogs);
            characterModuleInstaller.Install(runtimeHost);

            if (!runtimeHost.ModuleHost.IsModuleInstalled(CCS_SurvivalCharacterDiagnostics.ModuleId))
            {
                CCS_Logger.LogWarning(
                    LogCategory,
                    "Character module skeleton was not registered after install.");
            }
            else
            {
                CCS_Logger.Log(
                    LogCategory,
                    $"Character module skeleton registered: {CCS_SurvivalCharacterDiagnostics.ModuleId}",
                    enableDebugLogs);
            }
        }

        #endregion
    }
}
