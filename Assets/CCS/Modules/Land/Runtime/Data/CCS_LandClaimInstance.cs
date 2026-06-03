using System;
using System.Collections.Generic;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_LandClaimInstance
// CATEGORY: Modules / Land / Runtime / Data
// PURPOSE: Runtime land claim instance with associated structure tracking.
// PLACEMENT: Owned by CCS_LandClaimService.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-06-02
// NOTES: Milestone 2.3.0 — single-player ownership foundation.
// =============================================================================

namespace CCS.Modules.Land
{
    public sealed class CCS_LandClaimInstance
    {
        private readonly HashSet<string> associatedStructureIds =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        public CCS_LandClaimInstance(
            string instanceId,
            CCS_LandClaimDefinition definition,
            Vector3 worldPosition,
            float claimRadius,
            string ownerId,
            string regionId,
            CCS_LandClaimState claimState)
        {
            InstanceId = instanceId ?? string.Empty;
            Definition = definition;
            WorldPosition = worldPosition;
            ClaimRadius = claimRadius > 0f ? claimRadius : definition?.ClaimRadius ?? 12f;
            OwnerId = ownerId ?? string.Empty;
            RegionId = regionId ?? string.Empty;
            ClaimState = claimState;
        }

        public string InstanceId { get; }

        public CCS_LandClaimDefinition Definition { get; }

        public Vector3 WorldPosition { get; }

        public float ClaimRadius { get; }

        public string OwnerId { get; }

        public string RegionId { get; }

        public CCS_LandClaimState ClaimState { get; set; }

        public GameObject WorldObject { get; set; }

        public IReadOnlyCollection<string> AssociatedStructureIds => associatedStructureIds;

        public bool ContainsPosition(Vector3 position)
        {
            float horizontalDistance = Vector2.Distance(
                new Vector2(WorldPosition.x, WorldPosition.z),
                new Vector2(position.x, position.z));
            return horizontalDistance <= ClaimRadius;
        }

        public bool TryAddAssociatedStructure(string structureInstanceId)
        {
            if (string.IsNullOrWhiteSpace(structureInstanceId))
            {
                return false;
            }

            return associatedStructureIds.Add(structureInstanceId);
        }

        public bool RemoveAssociatedStructure(string structureInstanceId)
        {
            if (string.IsNullOrWhiteSpace(structureInstanceId))
            {
                return false;
            }

            return associatedStructureIds.Remove(structureInstanceId);
        }

        public void ClearAssociatedStructures()
        {
            associatedStructureIds.Clear();
        }

        public void ApplySnapshotAssociations(string[] structureIds)
        {
            associatedStructureIds.Clear();
            if (structureIds == null || structureIds.Length == 0)
            {
                return;
            }

            for (int index = 0; index < structureIds.Length; index++)
            {
                string structureId = structureIds[index];
                if (!string.IsNullOrWhiteSpace(structureId))
                {
                    associatedStructureIds.Add(structureId);
                }
            }
        }

        public bool HasAssociatedStructure(string structureInstanceId)
        {
            return !string.IsNullOrWhiteSpace(structureInstanceId)
                && associatedStructureIds.Contains(structureInstanceId);
        }

        public CCS_LandClaimSnapshot CaptureSnapshot()
        {
            string[] structureIds = new string[associatedStructureIds.Count];
            associatedStructureIds.CopyTo(structureIds);
            return new CCS_LandClaimSnapshot
            {
                instanceId = InstanceId,
                claimDefinitionId = Definition != null ? Definition.ClaimDefinitionId : string.Empty,
                ownerId = OwnerId,
                regionId = RegionId,
                positionX = WorldPosition.x,
                positionY = WorldPosition.y,
                positionZ = WorldPosition.z,
                claimRadius = ClaimRadius,
                claimState = (int)ClaimState,
                associatedStructureIds = structureIds
            };
        }
    }
}
