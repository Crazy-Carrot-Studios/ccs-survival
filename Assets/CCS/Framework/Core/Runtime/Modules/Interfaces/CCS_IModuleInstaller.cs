// =============================================================================
// SCRIPT: CCS_IModuleInstaller
// CATEGORY: Core / Runtime / Modules / Interfaces
// PURPOSE: Allows modules to install through the existing bootstrap pipeline.
// PLACEMENT: Runtime assembly contract. Implemented by future module installers.
// AUTHOR: James Schilz
// CREATED: 2026-05-24
// NOTES: Interface only. Extends CCS_IBootstrapInstaller. No auto-discovery.
// =============================================================================

namespace CCS.Core
{
    public interface CCS_IModuleInstaller : CCS_IBootstrapInstaller
    {
        CCS_IModule Module { get; }
    }
}
