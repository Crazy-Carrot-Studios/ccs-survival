using System;
using System.Collections.Generic;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_VehicleDefinition
// CATEGORY: Modules / Vehicles / Runtime / Definitions
// PURPOSE: Generic vehicle definition (wagon, cart, stagecoach, mine cart).
// PLACEMENT: Assets/CCS/Survival/Content/Vehicles/
// AUTHOR: James Schilz
// CREATED: 2026-06-02
// =============================================================================

namespace CCS.Modules.Vehicles
{
    [CreateAssetMenu(
        fileName = "CCS_VehicleDefinition",
        menuName = "CCS/Survival/Vehicles/Vehicle Definition")]
    public sealed class CCS_VehicleDefinition : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string vehicleId = "ccs.survival.vehicle.example";
        [SerializeField] private string displayName = "Vehicle";
        [SerializeField] private string description = string.Empty;

        [Header("Movement Placeholder")]
        [SerializeField] private float movementDragPlaceholder = 1.25f;

        [Header("Cargo / Hitch")]
        [SerializeField] private int cargoSlotCount = 24;
        [SerializeField] private string cargoContainerDefinitionId = CCS_VehicleContentIds.WagonCargoContainerId;
        [SerializeField] private string[] hitchCompatibleMountIds = { CCS_VehicleContentIds.HitchCompatibleHorseMountId };

        [Header("Economy")]
        [SerializeField] private int purchaseValue = 1800;

        [Header("World")]
        [SerializeField] private GameObject worldPrefab;
        [SerializeField] private Vector3 followOffsetLocal = new Vector3(0f, 0f, -3.25f);
        [SerializeField] private float followSmoothing = 10f;

        public string VehicleId => vehicleId ?? string.Empty;

        public string DisplayName => displayName ?? string.Empty;

        public string Description => description ?? string.Empty;

        public float MovementDragPlaceholder => movementDragPlaceholder < 0f ? 0f : movementDragPlaceholder;

        public int CargoSlotCount => cargoSlotCount < 1 ? 1 : cargoSlotCount;

        public string CargoContainerDefinitionId => cargoContainerDefinitionId ?? string.Empty;

        public int PurchaseValue => purchaseValue < 0 ? 0 : purchaseValue;

        public GameObject WorldPrefab => worldPrefab;

        public Vector3 FollowOffsetLocal => followOffsetLocal;

        public float FollowSmoothing => followSmoothing < 0.1f ? 10f : followSmoothing;

        public bool IsHitchCompatible(string mountId)
        {
            if (string.IsNullOrWhiteSpace(mountId) || hitchCompatibleMountIds == null)
            {
                return false;
            }

            for (int index = 0; index < hitchCompatibleMountIds.Length; index++)
            {
                if (string.Equals(hitchCompatibleMountIds[index], mountId, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
