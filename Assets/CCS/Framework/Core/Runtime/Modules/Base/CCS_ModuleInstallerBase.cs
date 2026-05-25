// =============================================================================
// SCRIPT: CCS_ModuleInstallerBase
// CATEGORY: Core / Runtime / Modules / Base
// PURPOSE: Abstract base for module installers that plug into CCS_BootstrapRunner.
// PLACEMENT: Runtime assembly. Not attached to GameObjects. Inherit for module installers.
// AUTHOR: James Schilz
// CREATED: 2026-05-24
// NOTES: Duplicate install preflight runs before OnBeforeInstall and module.Install.
// =============================================================================

namespace CCS.Core
{
    public abstract class CCS_ModuleInstallerBase : CCS_IModuleInstaller
    {
        #region Variables

        private readonly CCS_IModule module;
        private readonly bool enableDebugLogs;

        #endregion

        #region Properties

        public CCS_IModule Module => module;

        #endregion

        #region Public Methods

        protected CCS_ModuleInstallerBase(CCS_IModule module, bool enableDebugLogs = false)
        {
            this.module = module;
            this.enableDebugLogs = enableDebugLogs;
        }

        public void Install(CCS_RuntimeHost runtimeHost)
        {
            string logCategory = GetLogCategory();

            CCS_Result hostValidation = CCS_CoreValidation.ValidateRuntimeHost(runtimeHost);
            if (!hostValidation.IsSuccess)
            {
                CCS_Logger.LogWarning(logCategory, hostValidation.Message);
                SetModuleLifecycleState(CCS_ModuleLifecycleState.Failed);
                return;
            }

            CCS_Result moduleValidation = CCS_CoreValidation.ValidateModule(module);
            if (!moduleValidation.IsSuccess)
            {
                CCS_Logger.LogWarning(logCategory, moduleValidation.Message);
                SetModuleLifecycleState(CCS_ModuleLifecycleState.Failed);
                return;
            }

            CCS_Result dependencyResult = CCS_ModuleDependencyValidation.ValidateInstallDependencies(runtimeHost, module);
            if (!dependencyResult.IsSuccess)
            {
                CCS_Logger.LogWarning(logCategory, dependencyResult.Message);
                SetModuleLifecycleState(CCS_ModuleLifecycleState.Failed);
                return;
            }

            // Preflight duplicate check before any installer hooks or module.Install.
            CCS_Result preflightResult = runtimeHost.ModuleHost.TryPreflightInstall(module.Metadata.ModuleId);
            if (!preflightResult.IsSuccess)
            {
                CCS_Logger.LogWarning(logCategory, preflightResult.Message);
                SetModuleLifecycleState(CCS_ModuleLifecycleState.Failed);
                return;
            }

            SetModuleLifecycleState(CCS_ModuleLifecycleState.Installing);

            CCS_Result beforeResult = OnBeforeInstall(runtimeHost);
            if (!beforeResult.IsSuccess)
            {
                CCS_Logger.LogWarning(logCategory, $"Before install failed: {beforeResult.Message}");
                SetModuleLifecycleState(CCS_ModuleLifecycleState.Failed);
                return;
            }

            CCS_Result installResult = module.Install(runtimeHost);
            if (!installResult.IsSuccess)
            {
                CCS_Logger.LogWarning(logCategory, $"Module install failed: {installResult.Message}");
                SetModuleLifecycleState(CCS_ModuleLifecycleState.Failed);
                return;
            }

            CCS_Result afterResult = OnAfterInstall(runtimeHost);
            if (!afterResult.IsSuccess)
            {
                CCS_Logger.LogWarning(logCategory, $"After install failed: {afterResult.Message}");
                SetModuleLifecycleState(CCS_ModuleLifecycleState.Failed);
                return;
            }

            CCS_Result registerResult = runtimeHost.ModuleHost.RegisterInstalledModule(module);
            if (!registerResult.IsSuccess)
            {
                SetModuleLifecycleState(CCS_ModuleLifecycleState.Failed);
                return;
            }

            SetModuleLifecycleState(CCS_ModuleLifecycleState.Installed);
            CCS_Logger.Log(logCategory, "Module installer completed.", enableDebugLogs);
        }

        #endregion

        #region Protected Methods

        protected virtual CCS_Result OnBeforeInstall(CCS_RuntimeHost runtimeHost)
        {
            return CCS_Result.Success();
        }

        protected virtual CCS_Result OnAfterInstall(CCS_RuntimeHost runtimeHost)
        {
            return CCS_Result.Success();
        }

        protected virtual string GetLogCategory()
        {
            return "Module Installer";
        }

        #endregion

        #region Private Methods

        private void SetModuleLifecycleState(CCS_ModuleLifecycleState lifecycleState)
        {
            if (module is CCS_ModuleBase moduleBase)
            {
                moduleBase.SetLifecycleState(lifecycleState);
            }
        }

        #endregion
    }
}
