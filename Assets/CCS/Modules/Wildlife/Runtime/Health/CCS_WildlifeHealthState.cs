// =============================================================================
// SCRIPT: CCS_WildlifeHealthState
// CATEGORY: Modules / Wildlife / Runtime / Health
// PURPOSE: Runtime health state for living wildlife damageable targets.
// PLACEMENT: Owned by CCS_WildlifeDamageable on living wildlife placeholders.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: No healing or regeneration in 0.9.8 foundation.
// =============================================================================

namespace CCS.Modules.Wildlife
{
    public sealed class CCS_WildlifeHealthState
    {
        #region Variables

        private float currentHealth;
        private float maxHealth;

        #endregion

        #region Properties

        public float CurrentHealth => currentHealth;

        public float MaxHealth => maxHealth;

        public bool IsDead => currentHealth <= 0f;

        #endregion

        #region Public Methods

        public void Initialize(float maximumHealth)
        {
            maxHealth = maximumHealth > 0f ? maximumHealth : 1f;
            currentHealth = maxHealth;
        }

        public float ApplyDamage(float damageAmount)
        {
            if (IsDead || damageAmount <= 0f)
            {
                return 0f;
            }

            float appliedDamage = damageAmount > currentHealth ? currentHealth : damageAmount;
            currentHealth -= appliedDamage;
            if (currentHealth < 0f)
            {
                currentHealth = 0f;
            }

            return appliedDamage;
        }

        #endregion
    }
}
