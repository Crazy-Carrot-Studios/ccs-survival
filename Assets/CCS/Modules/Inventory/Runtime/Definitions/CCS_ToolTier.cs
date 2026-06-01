// =============================================================================
// SCRIPT: CCS_ToolTier
// CATEGORY: Modules / Inventory / Runtime / Definitions
// PURPOSE: Technology tier for tool effectiveness in future harvesting rules.
// PLACEMENT: Referenced by CCS_ItemDefinition tool metadata.
// AUTHOR: James Schilz
// CREATED: 2026-05-31
// NOTES: Harvest services read tier later; no durability loss in 0.9.2 foundation.
// =============================================================================

namespace CCS.Modules.Inventory
{
    public enum CCS_ToolTier
    {
        None = 0,
        Primitive = 1,
        Bone = 2,
        Stone = 3,
        Iron = 4,
        Steel = 5
    }
}
