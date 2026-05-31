// =============================================================================
// SCRIPT: CCS_RequiredToolType
// CATEGORY: Modules / WorldResources / Runtime / Definitions
// PURPOSE: Tool categories required to harvest specific resource definitions.
// PLACEMENT: Referenced by CCS_ResourceDefinition and harvest requests.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Foundation enum only. Tool gameplay deferred.
// =============================================================================

namespace CCS.Modules.WorldResources
{
    public enum CCS_RequiredToolType
    {
        None = 0,
        Axe = 1,
        Pickaxe = 2,
        Knife = 3,
        Shovel = 4
    }
}
