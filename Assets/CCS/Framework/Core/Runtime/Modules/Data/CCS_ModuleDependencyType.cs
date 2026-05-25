// =============================================================================
// SCRIPT: CCS_ModuleDependencyType
// CATEGORY: Core / Runtime / Modules / Data
// PURPOSE: Classifies declared module or service dependency metadata.
// PLACEMENT: Runtime data type. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-05-24
// NOTES: Metadata only. No automatic install or discovery.
// =============================================================================

namespace CCS.Core
{
    public enum CCS_ModuleDependencyType
    {
        RequiredModuleId = 0,
        OptionalModuleId = 1,
        RequiredServiceType = 2,
        OptionalServiceType = 3
    }
}
