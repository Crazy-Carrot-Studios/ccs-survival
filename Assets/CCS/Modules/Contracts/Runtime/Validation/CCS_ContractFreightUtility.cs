using CCS.Modules.Inventory;
using CCS.Modules.Storage;
using CCS.Modules.Vehicles;

// =============================================================================
// SCRIPT: CCS_ContractFreightUtility
// CATEGORY: Modules / Contracts / Runtime / Validation
// PURPOSE: Resolves and removes freight cargo from wagon storage or player inventory.
// PLACEMENT: Used by CCS_ContractService freight completion flows.
// AUTHOR: James Schilz
// CREATED: 2026-06-04
// NOTES: Milestone 3.4.0 — prefers wagon cargo; safe failure when goods are missing.
// =============================================================================

namespace CCS.Modules.Contracts
{
    public static class CCS_ContractFreightUtility
    {
        public static bool TryGetOwnedQuantity(
            CCS_ContractDefinition definition,
            CCS_ContractRequirement requirement,
            CCS_ItemDefinition itemDefinition,
            CCS_PlayerInventoryService inventoryService,
            CCS_StorageService storageService,
            CCS_VehicleService vehicleService,
            out int ownedQuantity,
            out string sourceLabel)
        {
            ownedQuantity = 0;
            sourceLabel = "none";
            if (definition == null || requirement == null || itemDefinition == null)
            {
                return false;
            }

            if (definition.PreferWagonCargo
                && TryGetWagonQuantity(vehicleService, storageService, itemDefinition, out int wagonQuantity)
                && wagonQuantity >= requirement.Quantity)
            {
                ownedQuantity = wagonQuantity;
                sourceLabel = "wagon";
                return true;
            }

            if (definition.AllowPlayerInventoryFallback
                && inventoryService != null
                && inventoryService.IsInitialized)
            {
                int inventoryQuantity = inventoryService.GetQuantity(itemDefinition);
                if (inventoryQuantity >= requirement.Quantity)
                {
                    ownedQuantity = inventoryQuantity;
                    sourceLabel = "inventory";
                    return true;
                }
            }

            if (definition.PreferWagonCargo
                && TryGetWagonQuantity(vehicleService, storageService, itemDefinition, out wagonQuantity))
            {
                ownedQuantity = wagonQuantity;
                sourceLabel = "wagon";
                return wagonQuantity > 0;
            }

            if (inventoryService != null && inventoryService.IsInitialized)
            {
                ownedQuantity = inventoryService.GetQuantity(itemDefinition);
                sourceLabel = "inventory";
            }

            return ownedQuantity > 0;
        }

        public static bool TryRemoveFreightGoods(
            CCS_ContractDefinition definition,
            CCS_ContractRequirement requirement,
            CCS_ItemDefinition itemDefinition,
            CCS_PlayerInventoryService inventoryService,
            CCS_StorageService storageService,
            CCS_VehicleService vehicleService,
            out string sourceLabel)
        {
            sourceLabel = "none";
            if (definition == null || requirement == null || itemDefinition == null)
            {
                return false;
            }

            int requiredQuantity = requirement.Quantity;
            if (definition.PreferWagonCargo
                && TryRemoveFromWagon(
                    vehicleService,
                    storageService,
                    itemDefinition,
                    requiredQuantity,
                    out int removedFromWagon)
                && removedFromWagon >= requiredQuantity)
            {
                sourceLabel = "wagon";
                return true;
            }

            if (definition.AllowPlayerInventoryFallback
                && inventoryService != null
                && inventoryService.IsInitialized)
            {
                int removedFromInventory = inventoryService.RemoveItem(itemDefinition, requiredQuantity);
                if (removedFromInventory >= requiredQuantity)
                {
                    sourceLabel = "inventory";
                    return true;
                }
            }

            if (definition.PreferWagonCargo
                && TryRemoveFromWagon(
                    vehicleService,
                    storageService,
                    itemDefinition,
                    requiredQuantity,
                    out removedFromWagon)
                && removedFromWagon >= requiredQuantity)
            {
                sourceLabel = "wagon";
                return true;
            }

            return false;
        }

        private static bool TryGetWagonQuantity(
            CCS_VehicleService vehicleService,
            CCS_StorageService storageService,
            CCS_ItemDefinition itemDefinition,
            out int quantity)
        {
            quantity = 0;
            if (!TryResolveWagonContainer(vehicleService, storageService, out CCS_StorageContainer container)
                || container?.ContainerInventory == null)
            {
                return false;
            }

            quantity = container.ContainerInventory.GetQuantity(itemDefinition);
            return true;
        }

        private static bool TryRemoveFromWagon(
            CCS_VehicleService vehicleService,
            CCS_StorageService storageService,
            CCS_ItemDefinition itemDefinition,
            int requiredQuantity,
            out int removedQuantity)
        {
            removedQuantity = 0;
            if (!TryResolveWagonContainer(vehicleService, storageService, out CCS_StorageContainer container))
            {
                return false;
            }

            if (!container.TryRemoveItem(itemDefinition, requiredQuantity, out removedQuantity))
            {
                removedQuantity = 0;
            }

            return removedQuantity > 0;
        }

        private static bool TryResolveWagonContainer(
            CCS_VehicleService vehicleService,
            CCS_StorageService storageService,
            out CCS_StorageContainer container)
        {
            container = null;
            if (vehicleService == null
                || !vehicleService.IsInitialized
                || !vehicleService.OwnsWagon
                || storageService == null
                || !storageService.IsInitialized)
            {
                return false;
            }

            string cargoInstanceId = vehicleService.ActiveCargoInstanceId;
            if (string.IsNullOrWhiteSpace(cargoInstanceId))
            {
                return false;
            }

            return storageService.TryGetRegisteredContainer(cargoInstanceId, out container)
                && container != null;
        }
    }
}
