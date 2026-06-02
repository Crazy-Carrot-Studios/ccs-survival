using System;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_LivestockInstance
// CATEGORY: Modules / Ranching / Runtime / Data
// PURPOSE: Runtime owned livestock with production timer and structure assignment.
// PLACEMENT: Managed by CCS_RanchService.
// AUTHOR: James Schilz
// CREATED: 2026-06-02
// NOTES: Milestone 2.1.0 ranching foundation.
// =============================================================================

namespace CCS.Modules.Ranching
{
    public sealed class CCS_LivestockInstance
    {
        public CCS_LivestockInstance(
            string instanceId,
            CCS_LivestockDefinition definition,
            Vector3 worldPosition,
            string campOwnerId)
        {
            InstanceId = instanceId ?? string.Empty;
            Definition = definition;
            WorldPosition = worldPosition;
            CampOwnerId = campOwnerId ?? string.Empty;
            State = CCS_LivestockState.Idle;
        }

        public string InstanceId { get; }

        public CCS_LivestockDefinition Definition { get; }

        public CCS_LivestockState State { get; set; }

        public string AssignedStructureInstanceId { get; set; } = string.Empty;

        public Vector3 WorldPosition { get; set; }

        public float ProductionElapsedSeconds { get; set; }

        public string LastProducedItemId { get; set; } = string.Empty;

        public int LastProducedQuantity { get; set; }

        public string CampOwnerId { get; set; }

        public GameObject WorldObject { get; set; }

        public CCS_LivestockType LivestockType => Definition != null ? Definition.LivestockType : CCS_LivestockType.Chicken;

        public string LivestockDefinitionId => Definition != null ? Definition.LivestockId : string.Empty;

        public CCS_LivestockSnapshot ToSnapshot()
        {
            return new CCS_LivestockSnapshot
            {
                instanceId = InstanceId,
                livestockDefinitionId = LivestockDefinitionId,
                livestockType = (int)LivestockType,
                livestockState = (int)State,
                assignedStructureInstanceId = AssignedStructureInstanceId ?? string.Empty,
                positionX = WorldPosition.x,
                positionY = WorldPosition.y,
                positionZ = WorldPosition.z,
                productionElapsedSeconds = ProductionElapsedSeconds,
                lastProducedItemId = LastProducedItemId ?? string.Empty,
                lastProducedQuantity = LastProducedQuantity,
                campOwnerId = CampOwnerId ?? string.Empty
            };
        }

        public static CCS_LivestockInstance FromSnapshot(
            CCS_LivestockSnapshot snapshot,
            CCS_LivestockDefinition definition)
        {
            if (snapshot == null || definition == null)
            {
                return null;
            }

            CCS_LivestockInstance instance = new CCS_LivestockInstance(
                snapshot.instanceId,
                definition,
                new Vector3(snapshot.positionX, snapshot.positionY, snapshot.positionZ),
                snapshot.campOwnerId)
            {
                State = Enum.IsDefined(typeof(CCS_LivestockState), snapshot.livestockState)
                    ? (CCS_LivestockState)snapshot.livestockState
                    : CCS_LivestockState.Idle,
                AssignedStructureInstanceId = snapshot.assignedStructureInstanceId ?? string.Empty,
                ProductionElapsedSeconds = snapshot.productionElapsedSeconds,
                LastProducedItemId = snapshot.lastProducedItemId ?? string.Empty,
                LastProducedQuantity = snapshot.lastProducedQuantity
            };
            return instance;
        }
    }
}
