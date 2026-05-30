// =============================================================================
// SCRIPT: CCS_DurabilityState
// CATEGORY: Modules / Equipment / Runtime / Data
// PURPOSE: Runtime durability tracking for equipped items.
// PLACEMENT: Owned by CCS_EquippedItem instances.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Foundation only. No durability loss gameplay in 0.4.1.
// =============================================================================

namespace CCS.Modules.Equipment
{
    public sealed class CCS_DurabilityState
    {
        #region Public Methods

        public CCS_DurabilityState(float maxDurability, float currentDurability = -1f)
        {
            MaxDurability = maxDurability > 0f ? maxDurability : 0f;
            CurrentDurability = currentDurability < 0f
                ? MaxDurability
                : Clamp(currentDurability, 0f, MaxDurability);
        }

        public float DamageDurability(float amount)
        {
            if (amount <= 0f || MaxDurability <= 0f)
            {
                return CurrentDurability;
            }

            CurrentDurability = Clamp(CurrentDurability - amount, 0f, MaxDurability);
            return CurrentDurability;
        }

        public float RepairDurability(float amount)
        {
            if (amount <= 0f || MaxDurability <= 0f)
            {
                return CurrentDurability;
            }

            CurrentDurability = Clamp(CurrentDurability + amount, 0f, MaxDurability);
            return CurrentDurability;
        }

        public bool IsBroken => MaxDurability > 0f && CurrentDurability <= 0f;

        #endregion

        #region Properties

        public float CurrentDurability { get; private set; }

        public float MaxDurability { get; }

        #endregion

        #region Private Methods

        private static float Clamp(float value, float min, float max)
        {
            if (value < min)
            {
                return min;
            }

            return value > max ? max : value;
        }

        #endregion
    }
}
