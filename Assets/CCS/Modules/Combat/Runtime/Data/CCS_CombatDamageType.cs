// =============================================================================
// SCRIPT: CCS_CombatDamageType
// CATEGORY: Modules / Combat / Runtime / Data
// PURPOSE: Damage categories used by primitive melee combat resolution.
// PLACEMENT: Referenced by CCS_CombatHitResult and weapon item metadata mapping.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: Aligns with inventory CCS_DamageType placeholders for 0.9.8 foundation.
// =============================================================================

namespace CCS.Modules.Combat
{
    public enum CCS_CombatDamageType
    {
        None = 0,
        Slash = 1,
        Pierce = 2,
        Blunt = 3
    }
}
