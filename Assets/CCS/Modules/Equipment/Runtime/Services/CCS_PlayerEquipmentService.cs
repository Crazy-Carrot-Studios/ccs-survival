using System.Collections.Generic;
using CCS.Survival;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_PlayerEquipmentService
// CATEGORY: Modules / Equipment / Runtime / Services
// PURPOSE: Runtime owner of player equipment slots, compatibility, and equipment events.
// PLACEMENT: Registered as CCS_ISurvivalService by future equipment module installer.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: References inventory item definitions only. No UI, combat, or visual coupling.
// =============================================================================

namespace CCS.Modules.Equipment
{
    public sealed class CCS_PlayerEquipmentService : CCS_ISurvivalService
    {
        private const string LogPrefix = "[CCS_PlayerEquipmentService]";

        #region Variables

        private readonly Dictionary<CCS_EquipmentSlotType, CCS_EquipmentSlot> slots =
            new Dictionary<CCS_EquipmentSlotType, CCS_EquipmentSlot>();

        private CCS_EquipmentProfile activeProfile;
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
            RaiseEquipmentChanged(slot, equipmentDefinition, equippedItem, "Item equipped.");
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
            RaiseEquipmentChanged(slot, removed.EquipmentDefinition, null, "Item unequipped.");
            return true;
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

            RaiseEquipmentChanged(CCS_EquipmentSlotType.Head, null, null, "All equipment cleared.");
        }

        public CCS_EquipmentSnapshot CreateSnapshot()
        {
            if (!EnsureInitialized())
            {
                return new CCS_EquipmentSnapshot(System.Array.Empty<CCS_EquippedItem>(), 0, 0);
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

            return new CCS_EquipmentSnapshot(equippedItems, occupiedCount, slots.Count);
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

        #endregion
    }
}
