using System;
using System.Collections.Generic;

// =============================================================================
// SCRIPT: CCS_ModuleHost
// CATEGORY: Core / Runtime / Modules / Host
// PURPOSE: Owns module registry and exposes safe module lookup for the runtime host.
// PLACEMENT: Instantiated by CCS_RuntimeHost. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-05-24
// NOTES: No singleton. No static registry. No auto-discovery or scene scanning.
// =============================================================================

namespace CCS.Core
{
    public sealed class CCS_ModuleHost
    {
        private const string LogCategory = "Module Host";

        #region Variables

        private readonly CCS_ModuleRegistry moduleRegistry;
        private readonly bool enableDebugLogs;

        #endregion

        #region Public Methods

        public CCS_ModuleHost(bool enableDebugLogs)
        {
            moduleRegistry = new CCS_ModuleRegistry(enableDebugLogs);
            this.enableDebugLogs = enableDebugLogs;
        }

        public CCS_Result RegisterInstalledModule(CCS_IModule module)
        {
            CCS_Result registerResult = moduleRegistry.RegisterModule(module);
            if (!registerResult.IsSuccess)
            {
                CCS_Logger.LogWarning(LogCategory, registerResult.Message);
                return registerResult;
            }

            CCS_Logger.Log(LogCategory, $"Installed module registered: {module.Metadata.ModuleId}", enableDebugLogs);
            return registerResult;
        }

        public CCS_Result UnregisterInstalledModule(string moduleId)
        {
            CCS_Result unregisterResult = moduleRegistry.UnregisterModule(moduleId);
            if (!unregisterResult.IsSuccess)
            {
                CCS_Logger.LogWarning(LogCategory, unregisterResult.Message);
                return unregisterResult;
            }

            CCS_Logger.Log(LogCategory, $"Installed module unregistered: {moduleId}", enableDebugLogs);
            return unregisterResult;
        }

        public CCS_Result UninstallModule(CCS_RuntimeHost runtimeHost, string moduleId)
        {
            CCS_Result hostValidation = CCS_CoreValidation.ValidateRuntimeHost(runtimeHost);
            if (!hostValidation.IsSuccess)
            {
                CCS_Logger.LogWarning(LogCategory, hostValidation.Message);
                return hostValidation;
            }

            CCS_Result moduleIdValidation = CCS_CoreValidation.ValidateModuleId(moduleId);
            if (!moduleIdValidation.IsSuccess)
            {
                CCS_Logger.LogWarning(LogCategory, moduleIdValidation.Message);
                return moduleIdValidation;
            }

            if (!TryGetModule(moduleId, out CCS_IModule module))
            {
                string missingMessage = $"Module ID not registered: {moduleId}";
                CCS_Logger.LogWarning(LogCategory, missingMessage);
                return CCS_Result.Failure(CCS_CoreErrorCode.ModuleNotRegistered, missingMessage);
            }

            CCS_Result uninstallResult = module.Uninstall(runtimeHost);
            if (!uninstallResult.IsSuccess)
            {
                return uninstallResult;
            }

            return UnregisterInstalledModule(moduleId);
        }

        public bool TryGetModule(string moduleId, out CCS_IModule module)
        {
            return moduleRegistry.TryGetModule(moduleId, out module);
        }

        public bool TryGetModule<TModule>(out TModule module) where TModule : class, CCS_IModule
        {
            return moduleRegistry.TryGetModule(out module);
        }

        public bool IsModuleRegistered(string moduleId)
        {
            return moduleRegistry.IsRegistered(moduleId);
        }

        public CCS_Result TryPreflightInstall(string moduleId)
        {
            CCS_Result moduleIdValidation = CCS_CoreValidation.ValidateModuleId(moduleId);
            if (!moduleIdValidation.IsSuccess)
            {
                return moduleIdValidation;
            }

            if (IsModuleRegistered(moduleId) || IsModuleInstalled(moduleId))
            {
                return CCS_Result.Failure(
                    CCS_CoreErrorCode.ModuleAlreadyInstalled,
                    $"Duplicate module install blocked for ID: {moduleId}");
            }

            return CCS_Result.Success();
        }

        public bool IsModuleInstalled(string moduleId)
        {
            if (!TryGetModule(moduleId, out CCS_IModule module))
            {
                return false;
            }

            return module.LifecycleState == CCS_ModuleLifecycleState.Installed;
        }

        public bool TryGetModuleLifecycleState(string moduleId, out CCS_ModuleLifecycleState lifecycleState)
        {
            lifecycleState = CCS_ModuleLifecycleState.Uninstalled;

            if (!TryGetModule(moduleId, out CCS_IModule module))
            {
                return false;
            }

            lifecycleState = module.LifecycleState;
            return true;
        }

        public IReadOnlyCollection<CCS_IModule> GetRegisteredModules()
        {
            return moduleRegistry.GetRegisteredModules();
        }

        public void Clear()
        {
            moduleRegistry.Clear();
            CCS_Logger.Log(LogCategory, "Module host cleared.", enableDebugLogs);
        }

        public CCS_ModuleDiagnosticsInfo[] BuildModuleDiagnosticsSnapshot()
        {
            IReadOnlyCollection<CCS_IModule> registeredModules = moduleRegistry.GetRegisteredModules();
            if (registeredModules.Count == 0)
            {
                return Array.Empty<CCS_ModuleDiagnosticsInfo>();
            }

            List<CCS_ModuleDiagnosticsInfo> snapshot = new List<CCS_ModuleDiagnosticsInfo>(registeredModules.Count);

            foreach (CCS_IModule module in registeredModules)
            {
                if (module == null)
                {
                    continue;
                }

                snapshot.Add(new CCS_ModuleDiagnosticsInfo(
                    module.Metadata.ModuleId,
                    module.LifecycleState));
            }

            return snapshot.ToArray();
        }

        #endregion
    }
}
