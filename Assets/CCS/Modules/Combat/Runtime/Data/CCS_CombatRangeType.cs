// =============================================================================
// SCRIPT: CCS_CombatRangeType
// CATEGORY: Modules / Combat / Runtime / Data
// PURPOSE: Engagement range categories for primitive melee combat.
// PLACEMENT: Referenced by CCS_CombatHitResult and weapon item metadata mapping.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: Melee-only foundation for 0.9.8 hunting milestone.
// =============================================================================

namespace CCS.Modules.Combat
{
    public enum CCS_CombatRangeType
    {
        None = 0,
        Melee = 1,
        ShortRanged = 2,
        LongRanged = 3
    }
}
