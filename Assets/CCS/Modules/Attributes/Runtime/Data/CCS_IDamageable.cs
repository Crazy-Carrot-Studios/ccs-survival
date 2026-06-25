// =============================================================================
// SCRIPT: CCS_IDamageable
// CATEGORY: Modules / Attributes / Runtime / Data
// PURPOSE: Shared damage target interface used by weapons, AI, and gameplay systems.
// PLACEMENT: Implemented by runtime health-bearing components.
// AUTHOR: James Schilz
// CREATED: 2026-06-25
// NOTES: Avoids direct module dependency on concrete health components.
// =============================================================================

namespace CCS.Modules.Attributes
{
    public interface CCS_IDamageable
    {
        float MaxHealth { get; }

        float CurrentHealth { get; }

        bool IsDead { get; }

        bool ApplyDamage(CCS_DamageInfo damageInfo);
    }
}
