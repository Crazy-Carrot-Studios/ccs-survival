using System.Collections.Generic;

// =============================================================================
// SCRIPT: CCS_IModuleRegistry
// CATEGORY: Core / Runtime / Modules / Registry
// PURPOSE: Contract for manual CCS module registration and lookup by module ID.
// PLACEMENT: Runtime assembly contract. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-05-24
// NOTES: Interface only. No singleton. No auto-discovery. Compiled by CCS.Core.Runtime.
// =============================================================================

namespace CCS.Core
{
    public interface CCS_IModuleRegistry
    {
        CCS_Result RegisterModule(CCS_IModule module);

        CCS_Result UnregisterModule(string moduleId);

        bool TryGetModule(string moduleId, out CCS_IModule module);

        bool TryGetModule<TModule>(out TModule module) where TModule : class, CCS_IModule;

        bool IsRegistered(string moduleId);

        IReadOnlyCollection<CCS_IModule> GetRegisteredModules();

        void Clear();
    }
}
