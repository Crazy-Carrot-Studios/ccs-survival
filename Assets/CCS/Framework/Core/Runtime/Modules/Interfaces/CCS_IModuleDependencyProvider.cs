using System.Collections.Generic;

// =============================================================================
// SCRIPT: CCS_IModuleDependencyProvider
// CATEGORY: Core / Runtime / Modules / Interfaces
// PURPOSE: Allows modules to declare dependency module IDs without resolution logic.
// PLACEMENT: Runtime assembly contract. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-05-24
// NOTES: Interface only. No automatic discovery or resolution in this milestone.
// =============================================================================

namespace CCS.Core
{
    public interface CCS_IModuleDependencyProvider
    {
        IReadOnlyList<string> RequiredModuleIds { get; }
    }
}
