// =============================================================================
// SCRIPT: CCS_CampStructureKind
// CATEGORY: Modules / Shelter / Runtime / Data
// PURPOSE: Structure kinds evaluated for frontier camp tier requirements.
// PLACEMENT: Referenced by CCS_CampRequirement and tier evaluation.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// =============================================================================

namespace CCS.Modules.Shelter
{
    public enum CCS_CampStructureKind
    {
        None = 0,
        Shelter = 1,
        Bedroll = 2,
        Campfire = 3,
        Storage = 4,
        WorkArea = 5,
        Barn = 6,
        Stable = 7,
        Garden = 8,
        Livestock = 9,
        SawTable = 10,
        CharcoalKiln = 11,
        PrimitiveForge = 12
    }
}
