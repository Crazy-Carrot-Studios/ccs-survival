using CCS.Modules.Equipment;
using CCS.Modules.Inventory;
using CCS.Modules.Wildlife;
using CCS.Survival;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_CombatService
// CATEGORY: Modules / Combat / Runtime / Services
// PURPOSE: Resolves equipped weapons and performs primitive melee wildlife attacks.
// PLACEMENT: Registered by CCS_SurvivalGameplayServiceRegistration from combat profile.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: SphereCast from camera forward. No physics forces or humanoid combat.
// =============================================================================

namespace CCS.Modules.Combat
{
    public sealed class CCS_CombatService : CCS_ISurvivalService
    {
        private const string LogPrefix = "[CCS_CombatService]";

        #region Variables

        private CCS_CombatProfile activeProfile;
        private CCS_PlayerEquipmentService equipmentService;
        private float lastAttackTime = -999f;
        private bool isInitialized;

        #endregion

        #region Events

        public event WildlifeDamagedHandler WildlifeDamaged;
        public event WildlifeKilledHandler WildlifeKilled;

        #endregion

        #region Properties

        public bool IsInitialized => isInitialized;

        public CCS_CombatProfile ActiveProfile => activeProfile;

        #endregion

        #region Public Methods

        public void Initialize()
        {
            if (isInitialized)
            {
                return;
            }

            isInitialized = true;
        }

        public void InitializeFromProfile(CCS_CombatProfile profile)
        {
            if (profile == null)
            {
                isInitialized = false;
                return;
            }

            CCS_SurvivalValidationResult validation = CCS_CombatValidationUtility.ValidateProfile(profile);
            if (!validation.IsSuccess)
            {
                Debug.LogWarning($"{LogPrefix} Profile validation warning: {validation.Message}");
            }

            activeProfile = profile;
            isInitialized = true;
        }

        public void BindEquipmentService(CCS_PlayerEquipmentService service)
        {
            equipmentService = service;
        }

        public CCS_CombatHitResult TryMeleeAttack(Vector3 attackOrigin, Vector3 attackDirection)
        {
            if (!isInitialized || activeProfile == null)
            {
                return CCS_CombatHitResult.Miss("Combat service is unavailable.");
            }

            if (Time.time < lastAttackTime + activeProfile.AttackCooldownSeconds)
            {
                return CCS_CombatHitResult.Miss("Attack is on cooldown.");
            }

            if (!TryResolveEquippedWeapon(out CCS_ItemDefinition weaponItem, out float damage, out float range))
            {
                return CCS_CombatHitResult.Miss("No melee weapon equipped.");
            }

            if (damage <= 0f || range <= 0f)
            {
                return CCS_CombatHitResult.Miss("Equipped weapon has no melee damage configured.", weaponItem);
            }

            Vector3 normalizedDirection = attackDirection.sqrMagnitude > 0.0001f
                ? attackDirection.normalized
                : Vector3.forward;

            if (!TryFindWildlifeTarget(
                    attackOrigin,
                    normalizedDirection,
                    range,
                    out CCS_WildlifeDamageable damageable))
            {
                lastAttackTime = Time.time;
                return CCS_CombatHitResult.Miss("No wildlife target in range.", weaponItem);
            }

            float appliedDamage = damageable.ApplyDamage(damage);
            if (appliedDamage <= 0f)
            {
                lastAttackTime = Time.time;
                return CCS_CombatHitResult.Miss("Wildlife target could not be damaged.", weaponItem);
            }

            bool targetKilled = damageable.IsDead;
            CCS_CombatDamageType combatDamageType = MapDamageType(weaponItem.DamageType);
            CCS_CombatRangeType combatRangeType = MapRangeType(weaponItem.RangeType);
            string targetName = damageable.DisplayName;
            string message = targetKilled
                ? $"{targetName} killed."
                : $"Hit {targetName} ({appliedDamage:0}).";

            CCS_CombatHitResult result = CCS_CombatHitResult.Hit(
                targetName,
                appliedDamage,
                damageable.CurrentHealth,
                targetKilled,
                combatDamageType,
                combatRangeType,
                weaponItem,
                message);

            lastAttackTime = Time.time;
            WildlifeDamaged?.Invoke(new CCS_CombatEventArgs(result));

            if (targetKilled)
            {
                HandleWildlifeKilled(damageable);
                WildlifeKilled?.Invoke(new CCS_CombatEventArgs(result));
            }

            return result;
        }

        public CCS_CombatHitResult TryRangedAttack(Vector3 attackOrigin, Vector3 attackDirection)
        {
            if (!isInitialized || activeProfile == null)
            {
                return CCS_CombatHitResult.Miss("Combat service is unavailable.");
            }

            if (Time.time < lastAttackTime + activeProfile.AttackCooldownSeconds)
            {
                return CCS_CombatHitResult.Miss("Attack is on cooldown.");
            }

            if (!TryResolveEquippedRangedWeapon(out CCS_ItemDefinition weaponItem, out float damage, out float range))
            {
                return CCS_CombatHitResult.Miss("No ranged weapon equipped.");
            }

            if (damage <= 0f || range <= 0f)
            {
                return CCS_CombatHitResult.Miss("Equipped bow has no ranged damage configured.", weaponItem);
            }

            Vector3 normalizedDirection = attackDirection.sqrMagnitude > 0.0001f
                ? attackDirection.normalized
                : Vector3.forward;

            if (!TryFindWildlifeTarget(
                    attackOrigin,
                    normalizedDirection,
                    range,
                    out CCS_WildlifeDamageable damageable))
            {
                lastAttackTime = Time.time;
                return CCS_CombatHitResult.Miss("No wildlife target in range.", weaponItem);
            }

            float appliedDamage = damageable.ApplyDamage(damage);
            if (appliedDamage <= 0f)
            {
                lastAttackTime = Time.time;
                return CCS_CombatHitResult.Miss("Wildlife target could not be damaged.", weaponItem);
            }

            bool targetKilled = damageable.IsDead;
            CCS_CombatDamageType combatDamageType = MapDamageType(weaponItem.DamageType);
            CCS_CombatRangeType combatRangeType = MapRangeType(weaponItem.RangeType);
            string targetName = damageable.DisplayName;
            string message = targetKilled
                ? $"{targetName} killed with bow."
                : $"Bow hit {targetName} ({appliedDamage:0}).";

            CCS_CombatHitResult result = CCS_CombatHitResult.Hit(
                targetName,
                appliedDamage,
                damageable.CurrentHealth,
                targetKilled,
                combatDamageType,
                combatRangeType,
                weaponItem,
                message);

            lastAttackTime = Time.time;
            WildlifeDamaged?.Invoke(new CCS_CombatEventArgs(result));

            if (targetKilled)
            {
                HandleWildlifeKilled(damageable);
                WildlifeKilled?.Invoke(new CCS_CombatEventArgs(result));
            }

            return result;
        }

        #endregion

        #region Private Methods

        private bool TryResolveEquippedWeapon(out CCS_ItemDefinition weaponItem, out float damage, out float range)
        {
            weaponItem = null;
            damage = 0f;
            range = 0f;

            if (equipmentService == null || !equipmentService.IsInitialized)
            {
                return false;
            }

            CCS_EquippedItem mainHand = equipmentService.GetEquippedItem(CCS_EquipmentSlotType.MainHand);
            if (mainHand?.ItemDefinition == null || !mainHand.ItemDefinition.HasWeaponIdentity)
            {
                return false;
            }

            weaponItem = mainHand.ItemDefinition;
            if (CCS_ItemGameplayUtility.IsBowWeaponItem(weaponItem))
            {
                return false;
            }

            damage = weaponItem.MeleeDamage;
            range = weaponItem.MeleeRange;
            return damage > 0f && range > 0f;
        }

        private bool TryResolveEquippedRangedWeapon(out CCS_ItemDefinition weaponItem, out float damage, out float range)
        {
            weaponItem = null;
            damage = 0f;
            range = 0f;

            if (equipmentService == null || !equipmentService.IsInitialized)
            {
                return false;
            }

            CCS_EquippedItem mainHand = equipmentService.GetEquippedItem(CCS_EquipmentSlotType.MainHand);
            if (mainHand?.ItemDefinition == null || !mainHand.ItemDefinition.HasWeaponIdentity)
            {
                return false;
            }

            weaponItem = mainHand.ItemDefinition;
            if (!CCS_ItemGameplayUtility.IsBowWeaponItem(weaponItem))
            {
                return false;
            }

            damage = weaponItem.MeleeDamage;
            range = weaponItem.MeleeRange;
            return damage > 0f && range > 0f;
        }

        private bool TryFindWildlifeTarget(
            Vector3 attackOrigin,
            Vector3 attackDirection,
            float attackRange,
            out CCS_WildlifeDamageable damageable)
        {
            damageable = null;
            RaycastHit[] hits = Physics.SphereCastAll(
                attackOrigin,
                activeProfile.HitSphereRadius,
                attackDirection,
                attackRange,
                activeProfile.WildlifeHitLayers,
                QueryTriggerInteraction.Ignore);

            if (hits == null || hits.Length == 0)
            {
                return false;
            }

            float closestDistance = float.MaxValue;
            for (int index = 0; index < hits.Length; index++)
            {
                CCS_WildlifeDamageable candidate =
                    hits[index].collider.GetComponentInParent<CCS_WildlifeDamageable>();
                if (candidate == null || candidate.IsDead)
                {
                    continue;
                }

                if (hits[index].distance < closestDistance)
                {
                    closestDistance = hits[index].distance;
                    damageable = candidate;
                }
            }

            return damageable != null;
        }

        private void HandleWildlifeKilled(CCS_WildlifeDamageable damageable)
        {
            if (damageable == null || activeProfile == null)
            {
                return;
            }

            CCS_CombatWildlifeSpeciesSettings speciesSettings =
                activeProfile.GetSpeciesSettings(damageable.Species);
            CCS_WildlifeDefinition carcassDefinition = activeProfile.GetCarcassDefinition(damageable.Species);
            Transform targetTransform = damageable.transform;

            CCS_WildlifeCarcassSpawnUtility.SpawnCarcass(
                targetTransform.position,
                targetTransform.rotation,
                carcassDefinition,
                activeProfile.WildlifeProfile,
                speciesSettings.carcassPrimitive,
                speciesSettings.carcassLocalScale,
                speciesSettings.carcassObjectName);

            Object.Destroy(damageable.gameObject);
        }

        private static CCS_CombatDamageType MapDamageType(CCS_DamageType inventoryDamageType)
        {
            switch (inventoryDamageType)
            {
                case CCS_DamageType.Slash:
                    return CCS_CombatDamageType.Slash;
                case CCS_DamageType.Pierce:
                    return CCS_CombatDamageType.Pierce;
                case CCS_DamageType.Blunt:
                    return CCS_CombatDamageType.Blunt;
                default:
                    return CCS_CombatDamageType.None;
            }
        }

        private static CCS_CombatRangeType MapRangeType(CCS_RangeType inventoryRangeType)
        {
            switch (inventoryRangeType)
            {
                case CCS_RangeType.Melee:
                    return CCS_CombatRangeType.Melee;
                case CCS_RangeType.ShortRanged:
                    return CCS_CombatRangeType.ShortRanged;
                case CCS_RangeType.LongRanged:
                    return CCS_CombatRangeType.LongRanged;
                default:
                    return CCS_CombatRangeType.None;
            }
        }

        #endregion
    }
}
