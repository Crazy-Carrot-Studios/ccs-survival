using UnityEngine;

// =============================================================================
// SCRIPT: CCS_VehicleInstance
// CATEGORY: Modules / Vehicles / Runtime / Services
// PURPOSE: Runtime owned-vehicle record linking definition, state, and world actor.
// PLACEMENT: Managed by CCS_VehicleService.
// AUTHOR: James Schilz
// CREATED: 2026-06-02
// =============================================================================

namespace CCS.Modules.Vehicles
{
    public sealed class CCS_VehicleInstance
    {
        public CCS_VehicleInstance(
            string instanceId,
            CCS_VehicleDefinition definition,
            string displayName,
            CCS_VehicleWorldActor worldActor)
        {
            InstanceId = instanceId ?? string.Empty;
            Definition = definition;
            DisplayName = displayName ?? string.Empty;
            WorldActor = worldActor;
            State = CCS_VehicleState.Idle;
        }

        public string InstanceId { get; }

        public CCS_VehicleDefinition Definition { get; }

        public string DisplayName { get; set; }

        public CCS_VehicleState State { get; set; }

        public string CargoInstanceId { get; set; }

        public string HitchedMountInstanceId { get; set; }

        public CCS_VehicleWorldActor WorldActor { get; private set; }

        public Vector3 WorldPosition =>
            WorldActor != null ? WorldActor.transform.position : Vector3.zero;

        public float WorldRotationY =>
            WorldActor != null ? WorldActor.transform.eulerAngles.y : 0f;

        public void BindWorldActor(CCS_VehicleWorldActor actor)
        {
            WorldActor = actor;
        }
    }
}
