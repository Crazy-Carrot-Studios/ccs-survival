using CCS.Modules.Interaction;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_VehicleWorldActor
// CATEGORY: Modules / Vehicles / Runtime / Components
// PURPOSE: World representation of an owned vehicle with hitch follow and cargo hooks.
// PLACEMENT: PF_CCS_FrontierWagon root.
// AUTHOR: James Schilz
// CREATED: 2026-06-02
// =============================================================================

namespace CCS.Modules.Vehicles
{
    public sealed class CCS_VehicleWorldActor : MonoBehaviour
    {
        [SerializeField] private string vehicleInstanceId = string.Empty;
        [SerializeField] private CCS_VehicleDefinition vehicleDefinition;
        [SerializeField] private CCS_WagonCargoContainer cargoContainer;
        [SerializeField] private CCS_VehicleInteractable vehicleInteractable;
        [SerializeField] private float interactionDistance = 3.5f;

        public string VehicleInstanceId => vehicleInstanceId ?? string.Empty;

        public CCS_VehicleDefinition VehicleDefinition => vehicleDefinition;

        public CCS_WagonCargoContainer Cargo => cargoContainer;

        public float InteractionDistance => interactionDistance < 0.5f ? 3.5f : interactionDistance;

        private void Awake()
        {
            if (cargoContainer == null)
            {
                cargoContainer = GetComponentInChildren<CCS_WagonCargoContainer>();
            }

            if (vehicleInteractable == null)
            {
                vehicleInteractable = GetComponent<CCS_VehicleInteractable>();
            }
        }

        public void Configure(
            string instanceId,
            CCS_VehicleDefinition definition,
            CCS_WagonCargoContainer cargo,
            string cargoInstanceId,
            int cargoSlots)
        {
            vehicleInstanceId = instanceId ?? string.Empty;
            vehicleDefinition = definition;
            cargoContainer = cargo;

            if (cargoContainer != null)
            {
                cargoContainer.ConfigureForVehicle(
                    CCS_VehicleContentIds.WagonCargoContainerId,
                    cargoInstanceId,
                    "Wagon Cargo",
                    cargoSlots);
            }

            if (vehicleInteractable != null)
            {
                vehicleInteractable.BindVehicleInstanceId(vehicleInstanceId);
            }
        }

        public void TickFollowHitch(Transform hitchPoint, Vector3 localOffset, float smoothing, float deltaTime)
        {
            if (hitchPoint == null || deltaTime <= 0f)
            {
                return;
            }

            Vector3 targetPosition = hitchPoint.TransformPoint(localOffset);
            Quaternion targetRotation = Quaternion.LookRotation(
                hitchPoint.forward,
                Vector3.up);

            float blend = Mathf.Clamp01(smoothing * deltaTime);
            transform.position = Vector3.Lerp(transform.position, targetPosition, blend);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, blend);
        }
    }
}
