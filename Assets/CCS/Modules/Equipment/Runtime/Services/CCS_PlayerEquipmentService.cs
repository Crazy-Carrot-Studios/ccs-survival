using System;
using System.Collections.Generic;
using CCS.Modules.SaveLoad;
using CCS.Survival;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_PlayerEquipmentService
// CATEGORY: Modules / Equipment / Runtime / Services
// PURPOSE: Runtime owner of player equipment slots, compatibility, and equipment events.
// PLACEMENT: Registered as CCS_ISurvivalService by future equipment module installer.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: References inventory item definitions only. Implements CCS_ISaveable at 0.6.2.
// =============================================================================

namespace CCS.Modules.Equipment
{
    public sealed class CCS_PlayerEquipmentService : CCS_ISurvivalService, CCS_ISaveable
    {
        private const string LogPrefix = "[CCS_PlayerEquipmentService]";

        #region Variables

        private readonly Dictionary<CCS_EquipmentSlotType, CCS_EquipmentSlot> slots =
            new Dictionary<CCS_EquipmentSlotType, CCS_EquipmentSlot>();

        private CCS_EquipmentProfile activeProfile;
        private CCS_EquipmentItemDefinitionLookup equipmentDefinitionLookup;
        private bool isInitialized;

        #endregion

        #region Events

        public event EquipmentItemEquippedHandler ItemEquipped;
        public event EquipmentItemUnequippedHandler ItemUnequipped;
        public event EquipmentChangedHandler EquipmentChanged;
        public event EquipmentDurabilityChangedHandler DurabilityChanged;
        public event EquipmentFailedHandler EquipmentFailed;

        #endregion

        #region Properties

        public bool IsInitialized => isInitialized;

        public CCS_EquipmentProfile ActiveProfile => activeProfile;

        public string SaveableId => CCS_SaveLoadSaveableIds.PlayerEquipment;

        #endregion

        #region Public Methods

        public void Initialize()
        {
            if (isInitialized)
            {
                return;
            }

            // Profile binding via InitializeFromProfile sets isInitialized when ready.
        }

        public void InitializeFromProfile(CCS_EquipmentProfile profile)
        {
            if (profile == null)
            {
                Debug.LogWarning($"{LogPrefix} InitializeFromProfile called with null profile.");
                isInitialized = false;
                return;
            }

            CCS_SurvivalValidationResult validation = CCS_EquipmentValidationUtility.ValidateProfile(profile);
            if (!validation.IsSuccess)
            {
                Debug.LogWarning($"{LogPrefix} Profile validation warning: {validation.Message}");
            }

            activeProfile = profile;
            equipmentDefinitionLookup = new CCS_EquipmentItemDefinitionLookup(profile.SaveRestoreEquipmentDefinitions);
            slots.Clear();
            CreateAllSlots();
            isInitialized = true;
        }

        public bool EquipItem(CCS_EquipmentItemDefinition equipmentDefinition)
        {
            if (equipmentDefinition == null)
            {
                RaiseEquipmentFailed(CCS_EquipmentSlotType.Head, null, "Equipment definition is null.");
                return false;
            }

            return EquipItem(equipmentDefinition.AllowedSlot, equipmentDefinition);
        }

        public bool EquipItem(CCS_EquipmentSlotType slot, CCS_EquipmentItemDefinition equipmentDefinition)
        {
            if (!EnsureInitialized())
            {
                return false;
            }

            CCS_SurvivalValidationResult definitionValidation =
                CCS_EquipmentValidationUtility.ValidateEquipmentDefinition(equipmentDefinition);

            if (!definitionValidation.IsSuccess)
            {
                RaiseEquipmentFailed(slot, equipmentDefinition, definitionValidation.Message);
                return false;
            }

            if (!ValidateSlotCompatibility(slot, equipmentDefinition))
            {
                RaiseEquipmentFailed(
                    slot,
                    equipmentDefinition,
                    "Equipment definition is not compatible with the requested slot.");
                return false;
            }

            if (!TryGetSlot(slot, out CCS_EquipmentSlot equipmentSlot))
            {
                RaiseEquipmentFailed(slot, equipmentDefinition, "Equipment slot is not available.");
                return false;
            }

            if (equipmentSlot.IsOccupied)
            {
                RaiseEquipmentFailed(slot, equipmentDefinition, "Equipment slot is already occupied.");
                return false;
            }

            CCS_DurabilityState durabilityState = CreateDurabilityState(equipmentDefinition);
            CCS_EquippedItem equippedItem = new CCS_EquippedItem(slot, equipmentDefinition, durabilityState);

            if (!equipmentSlot.TryEquip(equippedItem))
            {
                RaiseEquipmentFailed(slot, equipmentDefinition, "Failed to equip item in slot.");
                return false;
            }

            RaiseItemEquipped(slot, equipmentDefinition, equippedItem);
            RaiseEquipmentChangedForItem(slot, equipmentDefinition, equippedItem, isEquipped: true);
            return true;
        }

        public bool UnequipItem(CCS_EquipmentSlotType slot)
        {
            if (!EnsureInitialized())
            {
                return false;
            }

            if (!TryGetSlot(slot, out CCS_EquipmentSlot equipmentSlot))
            {
                RaiseEquipmentFailed(slot, null, "Equipment slot is not available.");
                return false;
            }

            if (equipmentSlot.IsEmpty)
            {
                RaiseEquipmentFailed(slot, null, "Equipment slot is empty.");
                return false;
            }

            CCS_EquippedItem removed = equipmentSlot.TryUnequip();
            if (removed == null)
            {
                RaiseEquipmentFailed(slot, null, "Failed to unequip item from slot.");
                return false;
            }

            RaiseItemUnequipped(slot, removed.EquipmentDefinition, removed);
            RaiseEquipmentChangedForItem(slot, removed.EquipmentDefinition, removed, isEquipped: false);
            return true;
        }

        public int GetAdditionalInventorySlots()
        {
            if (!EnsureInitialized())
            {
                return 0;
            }

            CalculateAggregateCapacityModifiers(
                out int totalAdditionalInventorySlots,
                out _);

            return totalAdditionalInventorySlots;
        }

        public float GetAdditionalCarryWeight()
        {
            if (!EnsureInitialized())
            {
                return 0f;
            }

            CalculateAggregateCapacityModifiers(
                out _,
                out float totalAdditionalCarryWeight);

            return totalAdditionalCarryWeight;
        }

        public bool IsSlotEmpty(CCS_EquipmentSlotType slot)
        {
            return TryGetSlot(slot, out CCS_EquipmentSlot equipmentSlot) && equipmentSlot.IsEmpty;
        }

        public bool IsSlotOccupied(CCS_EquipmentSlotType slot)
        {
            return TryGetSlot(slot, out CCS_EquipmentSlot equipmentSlot) && equipmentSlot.IsOccupied;
        }

        public CCS_EquippedItem GetEquippedItem(CCS_EquipmentSlotType slot)
        {
            return TryGetSlot(slot, out CCS_EquipmentSlot equipmentSlot)
                ? equipmentSlot.EquippedItem
                : null;
        }

        public bool ValidateSlotCompatibility(
            CCS_EquipmentSlotType slot,
            CCS_EquipmentItemDefinition equipmentDefinition)
        {
            return CCS_EquipmentValidationUtility.IsSlotCompatible(slot, equipmentDefinition);
        }

        public bool CanEquip(CCS_EquipmentItemDefinition equipmentDefinition)
        {
            if (!EnsureInitialized() || equipmentDefinition == null)
            {
                return false;
            }

            CCS_SurvivalValidationResult definitionValidation =
                CCS_EquipmentValidationUtility.ValidateEquipmentDefinition(equipmentDefinition);

            if (!definitionValidation.IsSuccess)
            {
                return false;
            }

            if (!TryGetSlot(equipmentDefinition.AllowedSlot, out CCS_EquipmentSlot equipmentSlot))
            {
                return false;
            }

            return equipmentSlot.CanAccept(equipmentDefinition);
        }

        public bool DamageEquippedDurability(CCS_EquipmentSlotType slot, float amount)
        {
            if (!EnsureInitialized() || amount <= 0f)
            {
                return false;
            }

            CCS_EquippedItem equippedItem = GetEquippedItem(slot);
            if (equippedItem == null || !equippedItem.HasDurability)
            {
                return false;
            }

            float newValue = equippedItem.Durability.DamageDurability(amount);
            RaiseDurabilityChanged(slot, equippedItem.EquipmentDefinition, equippedItem, newValue);
            RaiseEquipmentChanged(slot, equippedItem.EquipmentDefinition, equippedItem, "Durability damaged.");
            return true;
        }

        public bool RepairEquippedDurability(CCS_EquipmentSlotType slot, float amount)
        {
            if (!EnsureInitialized() || amount <= 0f)
            {
                return false;
            }

            CCS_EquippedItem equippedItem = GetEquippedItem(slot);
            if (equippedItem == null || !equippedItem.HasDurability)
            {
                return false;
            }

            float newValue = equippedItem.Durability.RepairDurability(amount);
            RaiseDurabilityChanged(slot, equippedItem.EquipmentDefinition, equippedItem, newValue);
            RaiseEquipmentChanged(slot, equippedItem.EquipmentDefinition, equippedItem, "Durability repaired.");
            return true;
        }

        public void ClearAllEquipment()
        {
            if (!EnsureInitialized())
            {
                return;
            }

            foreach (KeyValuePair<CCS_EquipmentSlotType, CCS_EquipmentSlot> entry in slots)
            {
                entry.Value.Clear();
            }

            RaiseEquipmentChanged(
                CCS_EquipmentSlotType.Head,
                null,
                null,
                "All equipment cleared.");
        }

        public CCS_EquipmentSnapshot CreateSnapshot()
        {
            if (!EnsureInitialized())
            {
                return new CCS_EquipmentSnapshot(
                    System.Array.Empty<CCS_EquippedItem>(),
                    0,
                    0,
                    0,
                    0f);
            }

            List<CCS_EquippedItem> equippedItems = new List<CCS_EquippedItem>();
            int occupiedCount = 0;

            foreach (KeyValuePair<CCS_EquipmentSlotType, CCS_EquipmentSlot> entry in slots)
            {
                if (entry.Value.IsOccupied)
                {
                    equippedItems.Add(entry.Value.EquippedItem);
                    occupiedCount++;
                }
            }

            CalculateAggregateCapacityModifiers(
                out int totalAdditionalInventorySlots,
                out float totalAdditionalCarryWeight);

            return new CCS_EquipmentSnapshot(
                equippedItems,
                occupiedCount,
                slots.Count,
                totalAdditionalInventorySlots,
                totalAdditionalCarryWeight);
        }

        public string CaptureState()
        {
            if (!EnsureInitialized())
            {
                return JsonUtility.ToJson(new CCS_EquipmentSaveData());
            }

            CalculateAggregateCapacityModifiers(
                out int totalAdditionalInventorySlots,
                out float totalAdditionalCarryWeight);

            CCS_EquipmentSaveData saveData = new CCS_EquipmentSaveData
            {
                saveDataVersion = CCS_EquipmentSaveData.CurrentSaveDataVersion,
                additionalInventorySlots = totalAdditionalInventorySlots,
                additionalCarryWeight = totalAdditionalCarryWeight,
                equippedSlots = BuildEquippedSaveEntries()
            };

            return JsonUtility.ToJson(saveData);
        }

        public void RestoreState(string stateJson)
        {
            if (!EnsureInitialized())
            {
                Debug.LogWarning($"{LogPrefix} RestoreState skipped because service is not initialized.");
                return;
            }

            if (string.IsNullOrWhiteSpace(stateJson))
            {
                ClearAllEquipment();
                return;
            }

            CCS_EquipmentSaveData saveData = JsonUtility.FromJson<CCS_EquipmentSaveData>(stateJson);
            if (saveData == null)
            {
                Debug.LogWarning($"{LogPrefix} RestoreState skipped because payload could not be parsed.");
                return;
            }

            if (saveData.saveDataVersion <= 0)
            {
                Debug.LogWarning($"{LogPrefix} RestoreState skipped because saveDataVersion is missing.");
                return;
            }

            ClearAllEquipmentSilently();
            RestoreEquippedSaveEntries(saveData.equippedSlots);
            RaiseEquipmentChanged(
                CCS_EquipmentSlotType.Head,
                null,
                null,
                "Equipment restored from save.");
        }

        #endregion

        #region Private Methods

        private void CreateAllSlots()
        {
            foreach (CCS_EquipmentSlotType slotType in System.Enum.GetValues(typeof(CCS_EquipmentSlotType)))
            {
                slots[slotType] = new CCS_EquipmentSlot(slotType);
            }
        }

        private CCS_EquipmentSaveSlotEntry[] BuildEquippedSaveEntries()
        {
            List<CCS_EquipmentSaveSlotEntry> saveEntries = new List<CCS_EquipmentSaveSlotEntry>();

            foreach (KeyValuePair<CCS_EquipmentSlotType, CCS_EquipmentSlot> entry in slots)
            {
                if (!entry.Value.IsOccupied)
                {
                    continue;
                }

                CCS_EquippedItem equippedItem = entry.Value.EquippedItem;
                if (equippedItem?.EquipmentDefinition?.ItemDefinition == null)
                {
                    continue;
                }

                CCS_EquipmentSaveSlotEntry saveEntry = new CCS_EquipmentSaveSlotEntry
                {
                    slotType = entry.Key.ToString(),
                    itemId = equippedItem.EquipmentDefinition.ItemDefinition.ItemId ?? string.Empty,
                    hasDurability = equippedItem.HasDurability,
                    currentDurability = equippedItem.HasDurability
                        ? equippedItem.Durability.CurrentDurability
                        : 0f,
                    maxDurability = equippedItem.HasDurability
                        ? equippedItem.Durability.MaxDurability
                        : 0f
                };

                saveEntries.Add(saveEntry);
            }

            return saveEntries.ToArray();
        }

        private void RestoreEquippedSaveEntries(CCS_EquipmentSaveSlotEntry[] equippedSaveEntries)
        {
            if (equippedSaveEntries == null || equippedSaveEntries.Length == 0)
            {
                return;
            }

            int restoredCount = 0;
            int skippedCount = 0;

            for (int index = 0; index < equippedSaveEntries.Length; index++)
            {
                if (TryRestoreEquippedSaveEntry(equippedSaveEntries[index]))
                {
                    restoredCount++;
                    continue;
                }

                skippedCount++;
            }

            if (skippedCount > 0)
            {
                Debug.LogWarning(
                    $"{LogPrefix} RestoreState skipped {skippedCount} equipped slot(s) due to missing definitions or invalid mappings.");
            }

            Debug.Log($"{LogPrefix} RestoreState restored {restoredCount} equipped slot(s).");
        }

        private bool TryRestoreEquippedSaveEntry(CCS_EquipmentSaveSlotEntry saveEntry)
        {
            if (saveEntry == null
                || string.IsNullOrWhiteSpace(saveEntry.itemId)
                || string.IsNullOrWhiteSpace(saveEntry.slotType))
            {
                return false;
            }

            if (!System.Enum.TryParse(saveEntry.slotType, out CCS_EquipmentSlotType slotType))
            {
                Debug.LogWarning($"{LogPrefix} RestoreState skipped unknown slot type '{saveEntry.slotType}'.");
                return false;
            }

            if (equipmentDefinitionLookup == null
                || !equipmentDefinitionLookup.TryGetDefinitionByItemId(
                    saveEntry.itemId,
                    out CCS_EquipmentItemDefinition equipmentDefinition))
            {
                Debug.LogWarning(
                    $"{LogPrefix} RestoreState skipped missing equipment definition for item '{saveEntry.itemId}'.");
                return false;
            }

            if (!ValidateSlotCompatibility(slotType, equipmentDefinition))
            {
                Debug.LogWarning(
                    $"{LogPrefix} RestoreState skipped invalid slot mapping for item '{saveEntry.itemId}' in slot '{slotType}'.");
                return false;
            }

            if (!TryGetSlot(slotType, out CCS_EquipmentSlot equipmentSlot))
            {
                return false;
            }

            CCS_DurabilityState durabilityState = CreateRestoreDurabilityState(saveEntry, equipmentDefinition);
            CCS_EquippedItem equippedItem = new CCS_EquippedItem(slotType, equipmentDefinition, durabilityState);
            if (!equipmentSlot.TryEquip(equippedItem))
            {
                return false;
            }

            return true;
        }

        private static CCS_DurabilityState CreateRestoreDurabilityState(
            CCS_EquipmentSaveSlotEntry saveEntry,
            CCS_EquipmentItemDefinition equipmentDefinition)
        {
            if (saveEntry == null || equipmentDefinition == null || !equipmentDefinition.DurabilityEnabled)
            {
                return null;
            }

            if (saveEntry.hasDurability)
            {
                return new CCS_DurabilityState(saveEntry.maxDurability, saveEntry.currentDurability);
            }

            return new CCS_DurabilityState(equipmentDefinition.MaxDurability);
        }

        private void ClearAllEquipmentSilently()
        {
            foreach (KeyValuePair<CCS_EquipmentSlotType, CCS_EquipmentSlot> entry in slots)
            {
                entry.Value.Clear();
            }
        }

        private bool TryGetSlot(CCS_EquipmentSlotType slot, out CCS_EquipmentSlot equipmentSlot)
        {
            return slots.TryGetValue(slot, out equipmentSlot);
        }

        private static CCS_DurabilityState CreateDurabilityState(CCS_EquipmentItemDefinition equipmentDefinition)
        {
            if (equipmentDefinition == null || !equipmentDefinition.DurabilityEnabled)
            {
                return null;
            }

            return new CCS_DurabilityState(equipmentDefinition.MaxDurability);
        }

        private bool EnsureInitialized()
        {
            if (isInitialized && slots.Count > 0)
            {
                return true;
            }

            Debug.LogWarning($"{LogPrefix} Service is not initialized.");
            return false;
        }

        private void RaiseItemEquipped(
            CCS_EquipmentSlotType slot,
            CCS_EquipmentItemDefinition equipmentDefinition,
            CCS_EquippedItem equippedItem)
        {
            ItemEquipped?.Invoke(new CCS_EquipmentEventArgs(slot, equipmentDefinition, equippedItem));
        }

        private void RaiseItemUnequipped(
            CCS_EquipmentSlotType slot,
            CCS_EquipmentItemDefinition equipmentDefinition,
            CCS_EquippedItem equippedItem)
        {
            ItemUnequipped?.Invoke(new CCS_EquipmentEventArgs(slot, equipmentDefinition, equippedItem));
        }

        private void RaiseEquipmentChanged(
            CCS_EquipmentSlotType slot,
            CCS_EquipmentItemDefinition equipmentDefinition,
            CCS_EquippedItem equippedItem,
            string message)
        {
            EquipmentChanged?.Invoke(new CCS_EquipmentEventArgs(
                slot,
                equipmentDefinition,
                equippedItem,
                message: message));
        }

        private void RaiseDurabilityChanged(
            CCS_EquipmentSlotType slot,
            CCS_EquipmentItemDefinition equipmentDefinition,
            CCS_EquippedItem equippedItem,
            float durabilityValue)
        {
            DurabilityChanged?.Invoke(new CCS_EquipmentEventArgs(
                slot,
                equipmentDefinition,
                equippedItem,
                durabilityValue: durabilityValue,
                message: "Durability changed."));
        }

        private void RaiseEquipmentFailed(
            CCS_EquipmentSlotType slot,
            CCS_EquipmentItemDefinition equipmentDefinition,
            string message)
        {
            EquipmentFailed?.Invoke(new CCS_EquipmentEventArgs(
                slot,
                equipmentDefinition,
                message: message));
        }

        private void CalculateAggregateCapacityModifiers(
            out int totalAdditionalInventorySlots,
            out float totalAdditionalCarryWeight)
        {
            List<CCS_EquippedItem> equippedItems = new List<CCS_EquippedItem>();

            foreach (KeyValuePair<CCS_EquipmentSlotType, CCS_EquipmentSlot> entry in slots)
            {
                if (entry.Value.IsOccupied)
                {
                    equippedItems.Add(entry.Value.EquippedItem);
                }
            }

            CCS_EquipmentCapacityModifierUtility.CalculateAggregateModifiers(
                equippedItems,
                out totalAdditionalInventorySlots,
                out totalAdditionalCarryWeight);
        }

        private void RaiseEquipmentChangedForItem(
            CCS_EquipmentSlotType slot,
            CCS_EquipmentItemDefinition equipmentDefinition,
            CCS_EquippedItem equippedItem,
            bool isEquipped)
        {
            if (CCS_EquipmentCapacityModifierUtility.AffectsCapacity(equipmentDefinition))
            {
                string action = isEquipped ? "equipped" : "unequipped";
                RaiseEquipmentChanged(
                    slot,
                    equipmentDefinition,
                    isEquipped ? equippedItem : null,
                    $"Capacity-affecting equipment {action}.");
                return;
            }

            string defaultMessage = isEquipped ? "Item equipped." : "Item unequipped.";
            RaiseEquipmentChanged(
                slot,
                equipmentDefinition,
                isEquipped ? equippedItem : null,
                defaultMessage);
        }

        #endregion
    }
}
