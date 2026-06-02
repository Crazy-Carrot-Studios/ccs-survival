using System;
using System.Collections.Generic;
using CCS.Core;
using CCS.Modules.Combat;
using CCS.Modules.Equipment;
using CCS.Modules.Inventory;
using CCS.Survival;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_FirearmService
// CATEGORY: Modules / Firearms / Runtime / Services
// PURPOSE: Firearm loaded-round state, reload, fire routing, and save snapshots.
// PLACEMENT: Registered by CCS_SurvivalGameplayServiceRegistration.
// AUTHOR: James Schilz
// CREATED: 2026-06-02
// =============================================================================

namespace CCS.Modules.Firearms
{
    public sealed class CCS_FirearmService : CCS_ISurvivalService
    {
        private const string LogPrefix = "[CCS_FirearmService]";

        private CCS_FirearmProfile activeProfile;
        private CCS_PlayerInventoryService inventoryService;
        private CCS_PlayerEquipmentService equipmentService;
        private CCS_CombatService combatService;
        private readonly Dictionary<string, int> loadedRoundsByFirearmItemId =
            new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        private bool isInitialized;

        public bool IsInitialized => isInitialized;

        public CCS_FirearmSnapshot CurrentSnapshot => CaptureSnapshot();

        public void Initialize()
        {
            isInitialized = true;
        }

        public void InitializeFromProfile(CCS_FirearmProfile profile)
        {
            activeProfile = profile;
            if (profile == null)
            {
                isInitialized = false;
                return;
            }

            CCS_SurvivalValidationResult validation = CCS_FirearmValidationUtility.ValidateProfile(profile);
            if (!validation.IsSuccess)
            {
                Debug.LogWarning($"{LogPrefix} Profile validation warning: {validation.Message}");
            }

            isInitialized = validation.IsSuccess;
            EnsureAllFirearmStates();
        }

        public void BindInventoryService(CCS_PlayerInventoryService inventory)
        {
            inventoryService = inventory;
        }

        public void BindEquipmentService(CCS_PlayerEquipmentService equipment)
        {
            equipmentService = equipment;
        }

        public void BindCombatService(CCS_CombatService combat)
        {
            combatService = combat;
        }

        public int GetLoadedRounds(string firearmItemId)
        {
            if (string.IsNullOrWhiteSpace(firearmItemId))
            {
                return 0;
            }

            return loadedRoundsByFirearmItemId.TryGetValue(firearmItemId, out int loaded) ? loaded : 0;
        }

        public CCS_FirearmUseResult TryFire(CCS_FirearmUseRequest request)
        {
            if (!isInitialized || activeProfile == null)
            {
                return CCS_FirearmUseResult.ServiceUnavailable();
            }

            if (!TryGetEquippedFirearmItem(out CCS_ItemDefinition firearmItem, out CCS_FirearmDefinition firearmDefinition))
            {
                return new CCS_FirearmUseResult(
                    CCS_FirearmUseResultType.NotEquipped,
                    "Equip a firearm in the main hand to fire.",
                    false);
            }

            int loaded = GetLoadedRounds(firearmItem.ItemId);
            if (loaded <= 0)
            {
                return new CCS_FirearmUseResult(
                    CCS_FirearmUseResultType.Empty,
                    "Firearm is empty. Press R to reload.",
                    false,
                    firearmItem.ItemId);
            }

            if (combatService == null || !combatService.IsInitialized)
            {
                return new CCS_FirearmUseResult(
                    CCS_FirearmUseResultType.ServiceUnavailable,
                    "Combat service is unavailable.",
                    false,
                    firearmItem.ItemId);
            }

            loadedRoundsByFirearmItemId[firearmItem.ItemId] = loaded - 1;
            CCS_CombatHitResult combatResult = combatService.TryFirearmAttack(
                request.UseOrigin,
                request.UseDirection,
                firearmDefinition.Damage,
                firearmDefinition.Range);

            if (combatResult == null)
            {
                return new CCS_FirearmUseResult(
                    CCS_FirearmUseResultType.ServiceUnavailable,
                    "Combat result was unavailable.",
                    false,
                    firearmItem.ItemId);
            }

            if (combatResult.DidHitWildlife)
            {
                return new CCS_FirearmUseResult(
                    CCS_FirearmUseResultType.CombatHit,
                    combatResult.Message,
                    true,
                    firearmItem.ItemId);
            }

            return new CCS_FirearmUseResult(
                CCS_FirearmUseResultType.CombatMiss,
                string.IsNullOrWhiteSpace(combatResult.Message)
                    ? "No wildlife target in range."
                    : combatResult.Message,
                true,
                firearmItem.ItemId);
        }

        public CCS_FirearmUseResult TryReloadEquippedFirearm()
        {
            if (!isInitialized || activeProfile == null)
            {
                return CCS_FirearmUseResult.ServiceUnavailable();
            }

            if (!TryGetEquippedFirearmItem(out CCS_ItemDefinition firearmItem, out CCS_FirearmDefinition firearmDefinition))
            {
                return new CCS_FirearmUseResult(
                    CCS_FirearmUseResultType.NotEquipped,
                    "Equip a firearm in the main hand to reload.",
                    false);
            }

            int capacity = firearmDefinition.MagazineCapacity;
            int loaded = GetLoadedRounds(firearmItem.ItemId);
            int needed = capacity - loaded;
            if (needed <= 0)
            {
                return new CCS_FirearmUseResult(
                    CCS_FirearmUseResultType.Reloaded,
                    "Firearm is already full.",
                    true,
                    firearmItem.ItemId);
            }

            if (inventoryService == null || !inventoryService.IsInitialized)
            {
                return new CCS_FirearmUseResult(
                    CCS_FirearmUseResultType.ServiceUnavailable,
                    "Inventory service is unavailable.",
                    false,
                    firearmItem.ItemId);
            }

            CCS_AmmoDefinition ammoDefinition = firearmDefinition.AmmoDefinition;
            CCS_ItemDefinition ammoItem = ammoDefinition?.InventoryItem;
            if (ammoItem == null)
            {
                return new CCS_FirearmUseResult(
                    CCS_FirearmUseResultType.NoAmmo,
                    "Ammo item definition is missing.",
                    false,
                    firearmItem.ItemId);
            }

            int available = inventoryService.GetQuantity(ammoItem);
            if (available <= 0)
            {
                return new CCS_FirearmUseResult(
                    CCS_FirearmUseResultType.NoAmmo,
                    "No compatible ammunition in inventory.",
                    false,
                    firearmItem.ItemId);
            }

            int toConsume = Mathf.Min(needed, available);
            inventoryService.RemoveItem(ammoItem, toConsume);
            loadedRoundsByFirearmItemId[firearmItem.ItemId] = loaded + toConsume;

            return new CCS_FirearmUseResult(
                CCS_FirearmUseResultType.Reloaded,
                $"Reloaded {toConsume} round(s). Loaded {loaded + toConsume}/{capacity}.",
                true,
                firearmItem.ItemId);
        }

        public CCS_FirearmSnapshot CaptureSnapshot()
        {
            EnsureAllFirearmStates();
            List<CCS_FirearmStateEntry> entries = new List<CCS_FirearmStateEntry>();
            string equippedItemId = GetEquippedFirearmItemId();

            IReadOnlyList<CCS_FirearmDefinition> definitions = activeProfile?.FirearmDefinitions;
            if (definitions != null)
            {
                for (int index = 0; index < definitions.Count; index++)
                {
                    CCS_FirearmDefinition definition = definitions[index];
                    if (definition == null || string.IsNullOrWhiteSpace(definition.InventoryItemId))
                    {
                        continue;
                    }

                    entries.Add(new CCS_FirearmStateEntry
                    {
                        firearmItemId = definition.InventoryItemId,
                        loadedRounds = GetLoadedRounds(definition.InventoryItemId),
                        activeEquippedItemId = string.Equals(
                            equippedItemId,
                            definition.InventoryItemId,
                            StringComparison.OrdinalIgnoreCase)
                            ? definition.InventoryItemId
                            : string.Empty
                    });
                }
            }

            return new CCS_FirearmSnapshot { firearmStates = entries.ToArray() };
        }

        public void RestoreSnapshot(CCS_FirearmSnapshot snapshot)
        {
            loadedRoundsByFirearmItemId.Clear();
            EnsureAllFirearmStates();

            if (snapshot?.firearmStates == null)
            {
                return;
            }

            for (int index = 0; index < snapshot.firearmStates.Length; index++)
            {
                CCS_FirearmStateEntry entry = snapshot.firearmStates[index];
                if (entry == null || string.IsNullOrWhiteSpace(entry.firearmItemId))
                {
                    continue;
                }

                loadedRoundsByFirearmItemId[entry.firearmItemId] = Mathf.Max(0, entry.loadedRounds);
            }
        }

        private void EnsureAllFirearmStates()
        {
            if (activeProfile?.FirearmDefinitions == null)
            {
                return;
            }

            IReadOnlyList<CCS_FirearmDefinition> definitions = activeProfile.FirearmDefinitions;
            for (int index = 0; index < definitions.Count; index++)
            {
                CCS_FirearmDefinition definition = definitions[index];
                if (definition == null || string.IsNullOrWhiteSpace(definition.InventoryItemId))
                {
                    continue;
                }

                if (!loadedRoundsByFirearmItemId.ContainsKey(definition.InventoryItemId))
                {
                    loadedRoundsByFirearmItemId[definition.InventoryItemId] = 0;
                }
            }
        }

        private bool TryGetEquippedFirearmItem(
            out CCS_ItemDefinition firearmItem,
            out CCS_FirearmDefinition firearmDefinition)
        {
            firearmItem = null;
            firearmDefinition = null;

            if (equipmentService == null || !equipmentService.IsInitialized)
            {
                return false;
            }

            CCS_EquippedItem mainHand = equipmentService.GetEquippedItem(CCS_EquipmentSlotType.MainHand);
            firearmItem = mainHand?.ItemDefinition;
            if (firearmItem == null || !CCS_ItemGameplayUtility.IsFirearmWeaponItem(firearmItem))
            {
                return false;
            }

            return activeProfile != null
                && activeProfile.TryGetFirearmByItemId(firearmItem.ItemId, out firearmDefinition);
        }

        private string GetEquippedFirearmItemId()
        {
            return TryGetEquippedFirearmItem(out CCS_ItemDefinition item, out _)
                ? item.ItemId
                : string.Empty;
        }
    }
}
