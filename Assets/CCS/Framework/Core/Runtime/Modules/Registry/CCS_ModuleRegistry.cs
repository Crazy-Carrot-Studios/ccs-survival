using System;
using System.Collections.Generic;

// =============================================================================
// SCRIPT: CCS_ModuleRegistry
// CATEGORY: Core / Runtime / Modules / Registry
// PURPOSE: Lightweight runtime registry for manual CCS module registration and lookup.
// PLACEMENT: Instantiated by bootstrap code. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-05-24
// NOTES: No static singleton. No auto-discovery. No UnityEditor references.
// =============================================================================

namespace CCS.Core
{
    public sealed class CCS_ModuleRegistry : CCS_IModuleRegistry
    {
        private const string LogCategory = "Module Registry";

        #region Variables

        private readonly Dictionary<string, CCS_IModule> registeredModules;
        private readonly bool enableDebugLogs;

        #endregion

        #region Public Methods

        public CCS_ModuleRegistry()
            : this(false)
        {
        }

        public CCS_ModuleRegistry(bool enableDebugLogs)
        {
            registeredModules = new Dictionary<string, CCS_IModule>(StringComparer.Ordinal);
            this.enableDebugLogs = enableDebugLogs;
        }

        public CCS_Result RegisterModule(CCS_IModule module)
        {
            CCS_Result moduleValidation = CCS_CoreValidation.ValidateModule(module);
            if (!moduleValidation.IsSuccess)
            {
                CCS_Logger.LogWarning(LogCategory, moduleValidation.Message);
                return moduleValidation;
            }

            string moduleId = module.Metadata.ModuleId;

            if (registeredModules.ContainsKey(moduleId))
            {
                string duplicateMessage = $"Module ID already registered: {moduleId}";
                CCS_Logger.LogWarning(LogCategory, duplicateMessage);
                return CCS_Result.Failure(CCS_CoreErrorCode.DuplicateModuleId, duplicateMessage);
            }

            registeredModules.Add(moduleId, module);
            CCS_Logger.Log(LogCategory, $"Registered module: {moduleId}", enableDebugLogs);
            return CCS_Result.Success();
        }

        public CCS_Result UnregisterModule(string moduleId)
        {
            CCS_Result moduleIdValidation = CCS_CoreValidation.ValidateModuleId(moduleId);
            if (!moduleIdValidation.IsSuccess)
            {
                CCS_Logger.LogWarning(LogCategory, moduleIdValidation.Message);
                return moduleIdValidation;
            }

            if (!registeredModules.Remove(moduleId))
            {
                string missingMessage = $"Module ID not registered: {moduleId}";
                CCS_Logger.LogWarning(LogCategory, missingMessage);
                return CCS_Result.Failure(CCS_CoreErrorCode.ModuleNotRegistered, missingMessage);
            }

            CCS_Logger.Log(LogCategory, $"Unregistered module: {moduleId}", enableDebugLogs);
            return CCS_Result.Success();
        }

        public bool TryGetModule(string moduleId, out CCS_IModule module)
        {
            module = null;

            if (!CCS_CoreValidation.IsValidId(moduleId))
            {
                return false;
            }

            return registeredModules.TryGetValue(moduleId, out module);
        }

        public bool TryGetModule<TModule>(out TModule module) where TModule : class, CCS_IModule
        {
            module = null;

            foreach (CCS_IModule registeredModule in registeredModules.Values)
            {
                if (registeredModule is TModule typedModule)
                {
                    module = typedModule;
                    return true;
                }
            }

            return false;
        }

        public bool IsRegistered(string moduleId)
        {
            if (!CCS_CoreValidation.IsValidId(moduleId))
            {
                return false;
            }

            return registeredModules.ContainsKey(moduleId);
        }

        public IReadOnlyCollection<CCS_IModule> GetRegisteredModules()
        {
            return registeredModules.Values;
        }

        public void Clear()
        {
            registeredModules.Clear();
            CCS_Logger.Log(LogCategory, "Cleared all registered modules.", enableDebugLogs);
        }

        #endregion
    }
}
