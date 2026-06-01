using System.Collections.Generic;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_EquipmentVisualController
// CATEGORY: Modules / Equipment / Runtime / Visuals
// PURPOSE: Spawns and removes primitive equipped visuals driven by CCS_PlayerEquipmentService.
// PLACEMENT: Owned by CCS_PlayerEquipmentVisualBinder on the local player.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: One active visual per attachment socket. Full resync on save restore bulk changes.
// =============================================================================

namespace CCS.Modules.Equipment
{
    public sealed class CCS_EquipmentVisualController
    {
        #region Variables

        private const string LogPrefix = "[CCS_EquipmentVisual]";

        private CCS_PlayerEquipmentService equipmentService;
        private CCS_EquipmentAttachmentRig attachmentRig;
        private CCS_EquipmentVisualDefinitionLookup definitionLookup;
        private readonly Dictionary<CCS_EquipmentAttachmentSocketType, CCS_EquippedVisualInstance> activeBySocket =
            new Dictionary<CCS_EquipmentAttachmentSocketType, CCS_EquippedVisualInstance>();

        private readonly Dictionary<string, CCS_EquipmentAttachmentSocketType> itemIdToSocket =
            new Dictionary<string, CCS_EquipmentAttachmentSocketType>();

        private bool enableDebugLogs;

        #endregion

        #region Public Methods

        public void Bind(
            CCS_PlayerEquipmentService playerEquipmentService,
            CCS_EquipmentAttachmentRig rig,
            CCS_EquipmentVisualProfile visualProfile,
            bool debugLogs = false)
        {
            Unbind();

            equipmentService = playerEquipmentService;
            attachmentRig = rig;
            enableDebugLogs = debugLogs;
            definitionLookup = visualProfile != null
                ? visualProfile.BuildLookup()
                : new CCS_EquipmentVisualDefinitionLookup(null);

            if (equipmentService != null)
            {
                equipmentService.ItemEquipped += OnItemEquipped;
                equipmentService.ItemUnequipped += OnItemUnequipped;
                equipmentService.EquipmentChanged += OnEquipmentChanged;
            }

            ResyncAllEquipped();
        }

        public void Unbind()
        {
            if (equipmentService != null)
            {
                equipmentService.ItemEquipped -= OnItemEquipped;
                equipmentService.ItemUnequipped -= OnItemUnequipped;
                equipmentService.EquipmentChanged -= OnEquipmentChanged;
                equipmentService = null;
            }

            ClearAllVisuals();
            attachmentRig = null;
            definitionLookup = null;
        }

        public bool HasVisualForItem(string itemId)
        {
            if (string.IsNullOrWhiteSpace(itemId))
            {
                return false;
            }

            if (!itemIdToSocket.TryGetValue(itemId, out CCS_EquipmentAttachmentSocketType socketType))
            {
                return false;
            }

            return activeBySocket.TryGetValue(socketType, out CCS_EquippedVisualInstance instance)
                && instance != null
                && instance.IsValid;
        }

        public void ResyncAllEquipped()
        {
            ClearAllVisuals();

            if (equipmentService == null || !equipmentService.IsInitialized)
            {
                return;
            }

            if (definitionLookup == null || attachmentRig == null)
            {
                return;
            }

            bool anyEquipped = false;
            foreach (CCS_EquipmentSlotType slotType in System.Enum.GetValues(typeof(CCS_EquipmentSlotType)))
            {
                CCS_EquippedItem equippedItem = equipmentService.GetEquippedItem(slotType);
                if (equippedItem?.ItemDefinition == null)
                {
                    continue;
                }

                anyEquipped = true;
                TrySpawnVisual(slotType, equippedItem.ItemDefinition.ItemId);
            }

            if (!anyEquipped && enableDebugLogs)
            {
                Debug.Log($"{LogPrefix} Resync complete with no equipped items.");
            }
        }

        #endregion

        #region Event Handlers

        private void OnItemEquipped(CCS_EquipmentEventArgs eventArgs)
        {
            if (eventArgs?.EquippedItem?.ItemDefinition == null)
            {
                return;
            }

            TrySpawnVisual(eventArgs.Slot, eventArgs.EquippedItem.ItemDefinition.ItemId);
        }

        private void OnItemUnequipped(CCS_EquipmentEventArgs eventArgs)
        {
            if (eventArgs?.EquippedItem?.ItemDefinition == null)
            {
                return;
            }

            TryRemoveVisualForItem(eventArgs.EquippedItem.ItemDefinition.ItemId);
        }

        private void OnEquipmentChanged(CCS_EquipmentEventArgs eventArgs)
        {
            string message = eventArgs?.Message ?? string.Empty;
            if (string.IsNullOrEmpty(message))
            {
                return;
            }

            if (message.Contains("restored", System.StringComparison.OrdinalIgnoreCase) ||
                message.Contains("cleared", System.StringComparison.OrdinalIgnoreCase))
            {
                if (enableDebugLogs)
                {
                    Debug.Log($"{LogPrefix} Full visual resync: {message}");
                }

                ResyncAllEquipped();
            }
        }

        #endregion

        #region Visual Spawn / Remove

        private void TrySpawnVisual(CCS_EquipmentSlotType slotType, string itemId)
        {
            if (string.IsNullOrWhiteSpace(itemId) || definitionLookup == null || attachmentRig == null)
            {
                return;
            }

            PruneStaleVisualEntries();

            if (itemIdToSocket.TryGetValue(itemId, out CCS_EquipmentAttachmentSocketType trackedSocket)
                && activeBySocket.TryGetValue(trackedSocket, out CCS_EquippedVisualInstance trackedInstance)
                && trackedInstance != null
                && trackedInstance.ItemId == itemId
                && trackedInstance.IsValid)
            {
                return;
            }

            if (!definitionLookup.TryGetDefinition(itemId, out CCS_EquipmentVisualDefinition definition))
            {
                return;
            }

            if (definition.VisualPrefab == null)
            {
                if (enableDebugLogs)
                {
                    Debug.LogWarning($"{LogPrefix} Visual prefab missing for item {itemId}.");
                }

                return;
            }

            if (!attachmentRig.TryGetSocket(definition.AttachmentSocket, out CCS_EquipmentAttachmentSocket socket)
                || socket == null
                || socket.SocketTransform == null)
            {
                if (enableDebugLogs)
                {
                    Debug.LogWarning($"{LogPrefix} Missing socket {definition.AttachmentSocket} for item {itemId}.");
                }

                return;
            }

            if (activeBySocket.TryGetValue(definition.AttachmentSocket, out CCS_EquippedVisualInstance existing))
            {
                RemoveVisualAtSocket(definition.AttachmentSocket, existing);
            }

            Transform parent = socket.SocketTransform;
            GameObject instance = Object.Instantiate(definition.VisualPrefab, parent);
            if (instance == null)
            {
                return;
            }

            instance.name = $"{definition.VisualPrefab.name}_Equipped";
            Transform instanceTransform = instance.transform;
            instanceTransform.localPosition = definition.LocalPositionOffset;
            instanceTransform.localRotation = Quaternion.Euler(definition.LocalEulerOffset);
            instanceTransform.localScale = definition.LocalScale;

            CCS_EquippedVisualInstance visualInstance = new CCS_EquippedVisualInstance(
                itemId,
                slotType,
                definition.AttachmentSocket,
                instance);

            activeBySocket[definition.AttachmentSocket] = visualInstance;
            itemIdToSocket[itemId] = definition.AttachmentSocket;
            CCS_EquipmentVisualRuntimeBridge.NotifyVisualSpawned(itemId);

            if (enableDebugLogs)
            {
                Debug.Log($"{LogPrefix} Spawned visual for {itemId} on {definition.AttachmentSocket}.");
            }
        }

        private void TryRemoveVisualForItem(string itemId)
        {
            if (string.IsNullOrWhiteSpace(itemId))
            {
                return;
            }

            PruneStaleVisualEntries();

            if (!itemIdToSocket.TryGetValue(itemId, out CCS_EquipmentAttachmentSocketType socketType))
            {
                return;
            }

            activeBySocket.TryGetValue(socketType, out CCS_EquippedVisualInstance instance);
            if (!RemoveVisualAtSocket(socketType, instance))
            {
                return;
            }

            CCS_EquipmentVisualRuntimeBridge.NotifyVisualRemoved(itemId);

            if (enableDebugLogs)
            {
                Debug.Log($"{LogPrefix} Removed visual for {itemId}.");
            }
        }

        private bool RemoveVisualAtSocket(
            CCS_EquipmentAttachmentSocketType socketType,
            CCS_EquippedVisualInstance instance)
        {
            bool removed = false;
            if (instance != null)
            {
                if (!string.IsNullOrWhiteSpace(instance.ItemId))
                {
                    itemIdToSocket.Remove(instance.ItemId);
                }

                if (instance.VisualRoot != null)
                {
                    Object.Destroy(instance.VisualRoot);
                    removed = true;
                }
            }

            if (activeBySocket.Remove(socketType))
            {
                removed = true;
            }

            return removed;
        }

        private void PruneStaleVisualEntries()
        {
            List<CCS_EquipmentAttachmentSocketType> staleSockets = null;
            foreach (KeyValuePair<CCS_EquipmentAttachmentSocketType, CCS_EquippedVisualInstance> pair in activeBySocket)
            {
                if (pair.Value == null || !pair.Value.IsValid)
                {
                    staleSockets ??= new List<CCS_EquipmentAttachmentSocketType>();
                    staleSockets.Add(pair.Key);
                }
            }

            if (staleSockets == null)
            {
                return;
            }

            for (int index = 0; index < staleSockets.Count; index++)
            {
                CCS_EquipmentAttachmentSocketType socketType = staleSockets[index];
                activeBySocket.TryGetValue(socketType, out CCS_EquippedVisualInstance instance);
                RemoveVisualAtSocket(socketType, instance);
            }
        }

        private void ClearAllVisuals()
        {
            foreach (KeyValuePair<CCS_EquipmentAttachmentSocketType, CCS_EquippedVisualInstance> pair in activeBySocket)
            {
                if (pair.Value?.VisualRoot != null)
                {
                    Object.Destroy(pair.Value.VisualRoot);
                }
            }

            activeBySocket.Clear();
            itemIdToSocket.Clear();
            CCS_EquipmentVisualRuntimeBridge.ClearAll();
        }

        #endregion
    }
}
