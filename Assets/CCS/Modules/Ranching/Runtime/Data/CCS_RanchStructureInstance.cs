using System;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_RanchStructureInstance
// CATEGORY: Modules / Ranching / Runtime / Data
// PURPOSE: Runtime placed ranch structure instance.
// PLACEMENT: Managed by CCS_RanchService.
// AUTHOR: James Schilz
// CREATED: 2026-06-02
// NOTES: Milestone 2.1.0 ranching foundation.
// =============================================================================

namespace CCS.Modules.Ranching
{
    public sealed class CCS_RanchStructureInstance
    {
        public CCS_RanchStructureInstance(
            string instanceId,
            CCS_RanchStructureDefinition definition,
            Vector3 worldPosition,
            float rotationY,
            string campOwnerId)
        {
            InstanceId = instanceId ?? string.Empty;
            Definition = definition;
            WorldPosition = worldPosition;
            RotationY = rotationY;
            CampOwnerId = campOwnerId ?? string.Empty;
        }

        public string InstanceId { get; }

        public CCS_RanchStructureDefinition Definition { get; }

        public Vector3 WorldPosition { get; }

        public float RotationY { get; }

        public string CampOwnerId { get; }

        public GameObject WorldObject { get; set; }

        public CCS_RanchStructureKind StructureKind =>
            Definition != null ? Definition.StructureKind : CCS_RanchStructureKind.ChickenCoop;

        public string StructureDefinitionId =>
            Definition != null ? Definition.StructureDefinitionId : string.Empty;

        public CCS_RanchStructureSnapshot ToSnapshot()
        {
            return new CCS_RanchStructureSnapshot
            {
                instanceId = InstanceId,
                structureDefinitionId = StructureDefinitionId,
                structureKind = (int)StructureKind,
                positionX = WorldPosition.x,
                positionY = WorldPosition.y,
                positionZ = WorldPosition.z,
                rotationY = RotationY,
                campOwnerId = CampOwnerId ?? string.Empty
            };
        }

        public static CCS_RanchStructureInstance FromSnapshot(
            CCS_RanchStructureSnapshot snapshot,
            CCS_RanchStructureDefinition definition)
        {
            if (snapshot == null || definition == null)
            {
                return null;
            }

            return new CCS_RanchStructureInstance(
                snapshot.instanceId,
                definition,
                new Vector3(snapshot.positionX, snapshot.positionY, snapshot.positionZ),
                snapshot.rotationY,
                snapshot.campOwnerId);
        }
    }
}
