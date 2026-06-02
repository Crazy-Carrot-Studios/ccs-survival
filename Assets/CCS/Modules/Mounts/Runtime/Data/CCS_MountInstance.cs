using UnityEngine;

// =============================================================================
// SCRIPT: CCS_MountInstance
// CATEGORY: Modules / Mounts / Runtime / Data
// PURPOSE: Runtime owned mount record bound to an optional world actor.
// PLACEMENT: Owned by CCS_MountService.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// =============================================================================

namespace CCS.Modules.Mounts
{
    public sealed class CCS_MountInstance
    {
        public CCS_MountInstance(
            string instanceId,
            CCS_MountDefinition definition,
            string displayName,
            CCS_MountWorldActor worldActor)
        {
            InstanceId = instanceId ?? string.Empty;
            Definition = definition;
            DisplayName = displayName ?? definition?.DisplayName ?? "Mount";
            WorldActor = worldActor;
            State = CCS_MountState.Idle;
        }

        public string InstanceId { get; }

        public CCS_MountDefinition Definition { get; }

        public string DisplayName { get; set; }

        public CCS_MountState State { get; set; }

        public CCS_MountWorldActor WorldActor { get; set; }

        public string SaddlebagInstanceId { get; set; } = string.Empty;

        public Vector3 WorldPosition =>
            WorldActor != null ? WorldActor.transform.position : Vector3.zero;

        public float WorldRotationY =>
            WorldActor != null ? WorldActor.transform.eulerAngles.y : 0f;
    }
}
