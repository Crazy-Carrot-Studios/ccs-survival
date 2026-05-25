using System;

// =============================================================================
// SCRIPT: CCS_ModuleDependency
// CATEGORY: Core / Runtime / Modules / Data
// PURPOSE: Declares a module or service dependency without resolution logic.
// PLACEMENT: Runtime data type. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-05-24
// NOTES: Metadata only. Manual registration policy remains authoritative.
// =============================================================================

namespace CCS.Core
{
    public readonly struct CCS_ModuleDependency
    {
        #region Properties

        public CCS_ModuleDependencyType DependencyType { get; }

        public string ModuleId { get; }

        public Type ServiceType { get; }

        #endregion

        #region Public Methods

        public static CCS_ModuleDependency RequiredModule(string moduleId)
        {
            return new CCS_ModuleDependency(CCS_ModuleDependencyType.RequiredModuleId, moduleId, null);
        }

        public static CCS_ModuleDependency OptionalModule(string moduleId)
        {
            return new CCS_ModuleDependency(CCS_ModuleDependencyType.OptionalModuleId, moduleId, null);
        }

        public static CCS_ModuleDependency RequiredService<TService>() where TService : class, CCS_IService
        {
            return new CCS_ModuleDependency(CCS_ModuleDependencyType.RequiredServiceType, null, typeof(TService));
        }

        public static CCS_ModuleDependency OptionalService<TService>() where TService : class, CCS_IService
        {
            return new CCS_ModuleDependency(CCS_ModuleDependencyType.OptionalServiceType, null, typeof(TService));
        }

        private CCS_ModuleDependency(CCS_ModuleDependencyType dependencyType, string moduleId, Type serviceType)
        {
            DependencyType = dependencyType;
            ModuleId = moduleId ?? string.Empty;
            ServiceType = serviceType;
        }

        #endregion
    }
}
