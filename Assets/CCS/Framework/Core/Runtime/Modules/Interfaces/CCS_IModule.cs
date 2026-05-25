// =============================================================================
// SCRIPT: CCS_IModule
// CATEGORY: Core / Runtime / Modules / Interfaces
// PURPOSE: Base contract for future CCS framework modules.
// PLACEMENT: Runtime assembly contract. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-05-24
// NOTES: Interface only. Install/Uninstall require CCS_RuntimeHost. Extends CCS_ISystem.
// =============================================================================

using System.Collections.Generic;

namespace CCS.Core
{
    public interface CCS_IModule : CCS_ISystem
    {
        CCS_ModuleMetadata Metadata { get; }

        IReadOnlyCollection<CCS_ModuleDependency> Dependencies { get; }

        CCS_ModuleState ModuleState { get; }

        CCS_ModuleLifecycleState LifecycleState { get; }

        CCS_Result Install(CCS_RuntimeHost runtimeHost);

        CCS_Result Uninstall(CCS_RuntimeHost runtimeHost);
    }
}
