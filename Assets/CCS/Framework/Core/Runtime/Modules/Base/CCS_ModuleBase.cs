// =============================================================================
// SCRIPT: CCS_ModuleBase
// CATEGORY: Core / Runtime / Modules / Base
// PURPOSE: Abstract base class for future CCS modules with shared lifecycle behavior.
// PLACEMENT: Runtime assembly. Not attached to GameObjects. Inherit for module types.
// AUTHOR: James Schilz
// CREATED: 2026-05-24
// NOTES: No gameplay logic, singletons, or automatic service/update registration.
// =============================================================================

using System.Collections.Generic;

namespace CCS.Core
{
    public abstract class CCS_ModuleBase : CCS_IModule
    {
        private const string LogCategory = "Module";

        private static readonly IReadOnlyCollection<CCS_ModuleDependency> EmptyDependencies =
            System.Array.Empty<CCS_ModuleDependency>();

        #region Variables

        private CCS_ModuleState moduleState;
        private CCS_ModuleLifecycleState lifecycleState;
        private readonly CCS_ModuleMetadata metadata;
        private readonly bool enableDebugLogs;

        #endregion

        #region Properties

        public CCS_ModuleMetadata Metadata => metadata;

        public virtual IReadOnlyCollection<CCS_ModuleDependency> Dependencies => EmptyDependencies;

        public CCS_ModuleState ModuleState => moduleState;

        public CCS_ModuleLifecycleState LifecycleState => lifecycleState;

        public bool IsInitialized => moduleState != CCS_ModuleState.Uninitialized;

        #endregion

        #region Public Methods

        protected CCS_ModuleBase(CCS_ModuleMetadata metadata, bool enableDebugLogs = false)
        {
            this.metadata = metadata;
            this.enableDebugLogs = enableDebugLogs;
            moduleState = CCS_ModuleState.Uninitialized;
            lifecycleState = CCS_ModuleLifecycleState.Uninstalled;
        }

        internal void SetLifecycleState(CCS_ModuleLifecycleState newLifecycleState)
        {
            lifecycleState = newLifecycleState;
            CCS_Logger.Log(LogCategory, $"{metadata.ModuleId} lifecycle -> {newLifecycleState}.", enableDebugLogs);
        }

        public void Initialize()
        {
            if (moduleState == CCS_ModuleState.Initialized || moduleState == CCS_ModuleState.Installed)
            {
                return;
            }

            CCS_Result result = OnInitialize();
            if (!result.IsSuccess)
            {
                CCS_Logger.LogWarning(LogCategory, $"Initialize failed for {metadata.ModuleId}: {result.Message}");
                return;
            }

            moduleState = CCS_ModuleState.Initialized;
            CCS_Logger.Log(LogCategory, $"{metadata.ModuleId} initialized.", enableDebugLogs);
        }

        public CCS_Result Install(CCS_RuntimeHost runtimeHost)
        {
            CCS_Result validation = CCS_Validation.ValidateObject(runtimeHost, "runtimeHost");
            if (!validation.IsSuccess)
            {
                return validation;
            }

            if (moduleState == CCS_ModuleState.Uninitialized)
            {
                Initialize();
                if (moduleState != CCS_ModuleState.Initialized)
                {
                    return CCS_Result.Failure($"Module {metadata.ModuleId} failed to initialize before install.");
                }
            }

            if (moduleState == CCS_ModuleState.Installed)
            {
                return CCS_Result.Success();
            }

            CCS_Result result = OnInstall(runtimeHost);
            if (!result.IsSuccess)
            {
                return result;
            }

            moduleState = CCS_ModuleState.Installed;
            CCS_Logger.Log(LogCategory, $"{metadata.ModuleId} installed.", enableDebugLogs);
            return result;
        }

        public CCS_Result Uninstall(CCS_RuntimeHost runtimeHost)
        {
            CCS_Result validation = CCS_Validation.ValidateObject(runtimeHost, "runtimeHost");
            if (!validation.IsSuccess)
            {
                CCS_Logger.LogWarning(LogCategory, validation.Message);
                return validation;
            }

            if (lifecycleState != CCS_ModuleLifecycleState.Installed)
            {
                string notInstalledMessage = $"Module {metadata.ModuleId} is not installed. Current lifecycle: {lifecycleState}";
                CCS_Logger.LogWarning(LogCategory, notInstalledMessage);
                return CCS_Result.Failure(CCS_CoreErrorCode.ModuleNotInstalled, notInstalledMessage);
            }

            lifecycleState = CCS_ModuleLifecycleState.Uninstalling;

            CCS_Result result = OnUninstall(runtimeHost);
            if (!result.IsSuccess)
            {
                lifecycleState = CCS_ModuleLifecycleState.Failed;
                CCS_Logger.LogWarning(LogCategory, $"Uninstall failed for {metadata.ModuleId}: {result.Message}");
                return result;
            }

            moduleState = CCS_ModuleState.Initialized;
            lifecycleState = CCS_ModuleLifecycleState.Uninstalled;
            CCS_Logger.Log(LogCategory, $"{metadata.ModuleId} uninstalled.", enableDebugLogs);
            return result;
        }

        public void Shutdown()
        {
            CCS_Result result = OnShutdown();
            if (!result.IsSuccess)
            {
                CCS_Logger.LogWarning(LogCategory, $"Shutdown failed for {metadata.ModuleId}: {result.Message}");
                return;
            }

            moduleState = CCS_ModuleState.Shutdown;
            CCS_Logger.Log(LogCategory, $"{metadata.ModuleId} shutdown.", enableDebugLogs);
        }

        #endregion

        #region Protected Methods

        protected virtual CCS_Result OnInitialize()
        {
            return CCS_Result.Success();
        }

        protected virtual CCS_Result OnInstall(CCS_RuntimeHost runtimeHost)
        {
            return CCS_Result.Success();
        }

        protected virtual CCS_Result OnUninstall(CCS_RuntimeHost runtimeHost)
        {
            return CCS_Result.Success();
        }

        protected virtual CCS_Result OnShutdown()
        {
            return CCS_Result.Success();
        }

        #endregion
    }
}
