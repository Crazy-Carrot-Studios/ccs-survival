using CCS.Modules.Inventory;

// =============================================================================
// SCRIPT: CCS_CombatHitResult
// CATEGORY: Modules / Combat / Runtime / Data
// PURPOSE: Result payload for a primitive melee attack attempt.
// PLACEMENT: Returned by CCS_CombatService.TryMeleeAttack.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: Safe failures only. No damage when attack misses valid wildlife targets.
// =============================================================================

namespace CCS.Modules.Combat
{
    public sealed class CCS_CombatHitResult
    {
        #region Properties

        public bool IsSuccess { get; }

        public bool DidHitWildlife { get; }

        public bool TargetKilled { get; }

        public string TargetDisplayName { get; }

        public float DamageDealt { get; }

        public float RemainingHealth { get; }

        public CCS_CombatDamageType DamageType { get; }

        public CCS_CombatRangeType RangeType { get; }

        public CCS_ItemDefinition WeaponItem { get; }

        public string Message { get; }

        #endregion

        #region Public Methods

        public static CCS_CombatHitResult Miss(string message, CCS_ItemDefinition weaponItem = null)
        {
            return new CCS_CombatHitResult(
                false,
                false,
                false,
                string.Empty,
                0f,
                0f,
                CCS_CombatDamageType.None,
                CCS_CombatRangeType.None,
                weaponItem,
                message);
        }

        public static CCS_CombatHitResult Hit(
            string targetDisplayName,
            float damageDealt,
            float remainingHealth,
            bool targetKilled,
            CCS_CombatDamageType damageType,
            CCS_CombatRangeType rangeType,
            CCS_ItemDefinition weaponItem,
            string message)
        {
            return new CCS_CombatHitResult(
                true,
                true,
                targetKilled,
                targetDisplayName,
                damageDealt,
                remainingHealth,
                damageType,
                rangeType,
                weaponItem,
                message);
        }

        private CCS_CombatHitResult(
            bool isSuccess,
            bool didHitWildlife,
            bool targetKilled,
            string targetDisplayName,
            float damageDealt,
            float remainingHealth,
            CCS_CombatDamageType damageType,
            CCS_CombatRangeType rangeType,
            CCS_ItemDefinition weaponItem,
            string message)
        {
            IsSuccess = isSuccess;
            DidHitWildlife = didHitWildlife;
            TargetKilled = targetKilled;
            TargetDisplayName = targetDisplayName ?? string.Empty;
            DamageDealt = damageDealt;
            RemainingHealth = remainingHealth;
            DamageType = damageType;
            RangeType = rangeType;
            WeaponItem = weaponItem;
            Message = message ?? string.Empty;
        }

        #endregion
    }
}
