using CCS.Modules.Interaction;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_VehicleInteractable
// CATEGORY: Modules / Vehicles / Runtime / Interactables
// PURPOSE: Hitch, unhitch, park, summon, and cargo interaction for owned wagons.
// PLACEMENT: PF_CCS_FrontierWagon root.
// AUTHOR: James Schilz
// CREATED: 2026-06-02
// =============================================================================

namespace CCS.Modules.Vehicles
{
    public sealed class CCS_VehicleInteractable : MonoBehaviour, CCS_IInteractableResultProvider
    {
        [SerializeField] private float interactionDistance = 3.5f;
        [SerializeField] private string vehicleInstanceId = string.Empty;
        [SerializeField] private CCS_VehicleWorldActor worldActor;

        public void BindVehicleInstanceId(string instanceId)
        {
            vehicleInstanceId = instanceId ?? string.Empty;
        }

        private void Awake()
        {
            if (worldActor == null)
            {
                worldActor = GetComponent<CCS_VehicleWorldActor>();
            }
        }

        public string GetInteractionDisplayName()
        {
            if (!CCS_VehicleRuntimeBridge.TryGetVehicleService(out CCS_VehicleService vehicleService)
                || !vehicleService.IsInitialized
                || vehicleService.ActiveVehicleInstanceId != vehicleInstanceId)
            {
                return "Frontier Wagon";
            }

            if (vehicleService.IsHitched)
            {
                return "Unhitch Wagon";
            }

            return vehicleService.OwnsWagon ? "Hitch Wagon" : "Frontier Wagon";
        }

        public float GetInteractionDistance()
        {
            return interactionDistance < 0.5f ? 3.5f : interactionDistance;
        }

        public bool CanInteract()
        {
            return isActiveAndEnabled
                && !string.IsNullOrWhiteSpace(vehicleInstanceId)
                && CCS_VehicleRuntimeBridge.TryGetVehicleService(out CCS_VehicleService vehicleService)
                && vehicleService.IsInitialized
                && vehicleService.OwnsWagon;
        }

        public void Interact()
        {
            TryInteract();
        }

        public bool TryInteract()
        {
            if (!CanInteract()
                || !CCS_VehicleRuntimeBridge.TryGetVehicleService(out CCS_VehicleService vehicleService))
            {
                return false;
            }

            if (vehicleService.IsHitched)
            {
                return vehicleService.TryUnhitchFromHorse();
            }

            return vehicleService.TryHitchToOwnedHorse();
        }
    }
}
