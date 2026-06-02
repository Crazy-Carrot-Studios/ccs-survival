using System;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_VehicleSnapshot
// CATEGORY: Modules / Vehicles / Runtime / Data
// PURPOSE: Serializable ownership and world state for a single owned vehicle.
// PLACEMENT: Captured by CCS_VehicleService and persisted through CCS_SaveService.
// AUTHOR: James Schilz
// CREATED: 2026-06-02
// =============================================================================

namespace CCS.Modules.Vehicles
{
    [Serializable]
    public sealed class CCS_VehicleSnapshot
    {
        public bool ownsVehicle;
        public string vehicleDefinitionId = string.Empty;
        public string instanceId = string.Empty;
        public string displayName = "Frontier Wagon";
        public int vehicleState;
        public float positionX;
        public float positionY;
        public float positionZ;
        public float rotationY;
        public string cargoInstanceId = string.Empty;
        public string hitchedMountInstanceId = string.Empty;

        public Vector3 Position => new Vector3(positionX, positionY, positionZ);

        public CCS_VehicleState VehicleState =>
            Enum.IsDefined(typeof(CCS_VehicleState), vehicleState)
                ? (CCS_VehicleState)vehicleState
                : CCS_VehicleState.Idle;

        public static CCS_VehicleSnapshot Empty => new CCS_VehicleSnapshot();
    }
}
