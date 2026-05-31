using System.Collections.Generic;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_BuildingInstance
// CATEGORY: Modules / Building / Runtime / Data
// PURPOSE: Represents a placed building structure instance in the world.
// PLACEMENT: Owned by CCS_BuildingService placed instance catalog.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-31
// NOTES: Runtime snap points and occupancy persistence in 0.8.3–0.8.4.
// =============================================================================

namespace CCS.Modules.Building
{
    public sealed class CCS_BuildingInstance
    {
        #region Variables

        private readonly List<CCS_BuildingRuntimeSnapPoint> runtimeSnapPoints =
            new List<CCS_BuildingRuntimeSnapPoint>();

        #endregion

        #region Public Methods

        public CCS_BuildingInstance(
            string instanceId,
            string pieceId,
            Vector3 position,
            Quaternion rotation,
            float creationTime)
        {
            InstanceId = instanceId ?? string.Empty;
            PieceId = pieceId ?? string.Empty;
            Position = position;
            Rotation = rotation;
            CreationTime = creationTime;
        }

        public void InitializeRuntimeSnapPoints(CCS_BuildingPieceDefinition definition)
        {
            runtimeSnapPoints.Clear();

            if (definition == null || definition.SnapPoints == null)
            {
                return;
            }

            for (int index = 0; index < definition.SnapPoints.Count; index++)
            {
                CCS_BuildingSnapPoint snapPoint = definition.SnapPoints[index];
                if (snapPoint == null || string.IsNullOrWhiteSpace(snapPoint.SnapPointId))
                {
                    continue;
                }

                CCS_BuildingRuntimeSnapPoint runtimeSnapPoint = new CCS_BuildingRuntimeSnapPoint(
                    InstanceId,
                    snapPoint.SnapPointId,
                    snapPoint.SnapPointType,
                    snapPoint.LocalPosition,
                    snapPoint.LocalRotation);
                runtimeSnapPoint.UpdateWorldTransform(Position, Rotation);
                runtimeSnapPoints.Add(runtimeSnapPoint);
            }
        }

        public void RefreshRuntimeSnapPointTransforms()
        {
            for (int index = 0; index < runtimeSnapPoints.Count; index++)
            {
                runtimeSnapPoints[index].UpdateWorldTransform(Position, Rotation);
            }
        }

        public bool TrySetSnapPointOccupied(string snapPointId, bool occupied)
        {
            if (string.IsNullOrWhiteSpace(snapPointId))
            {
                return false;
            }

            for (int index = 0; index < runtimeSnapPoints.Count; index++)
            {
                CCS_BuildingRuntimeSnapPoint runtimeSnapPoint = runtimeSnapPoints[index];
                if (runtimeSnapPoint.SnapPointId != snapPointId)
                {
                    continue;
                }

                runtimeSnapPoint.SetOccupied(occupied);
                return true;
            }

            return false;
        }

        public bool TryGetRuntimeSnapPoint(string snapPointId, out CCS_BuildingRuntimeSnapPoint runtimeSnapPoint)
        {
            runtimeSnapPoint = null;

            if (string.IsNullOrWhiteSpace(snapPointId))
            {
                return false;
            }

            for (int index = 0; index < runtimeSnapPoints.Count; index++)
            {
                if (runtimeSnapPoints[index].SnapPointId != snapPointId)
                {
                    continue;
                }

                runtimeSnapPoint = runtimeSnapPoints[index];
                return true;
            }

            return false;
        }

        public void SetTargetSnapConnection(string targetInstanceId, string targetSnapPointId)
        {
            TargetSnapInstanceId = targetInstanceId ?? string.Empty;
            TargetSnapPointId = targetSnapPointId ?? string.Empty;
        }

        public void ApplyOccupiedSnapPoints(IReadOnlyList<string> occupiedSnapPointIds)
        {
            if (occupiedSnapPointIds == null || occupiedSnapPointIds.Count == 0)
            {
                return;
            }

            for (int index = 0; index < occupiedSnapPointIds.Count; index++)
            {
                TrySetSnapPointOccupied(occupiedSnapPointIds[index], true);
            }
        }

        public List<string> CollectOccupiedSnapPointIds()
        {
            List<string> occupiedSnapPointIds = new List<string>();

            for (int index = 0; index < runtimeSnapPoints.Count; index++)
            {
                CCS_BuildingRuntimeSnapPoint runtimeSnapPoint = runtimeSnapPoints[index];
                if (runtimeSnapPoint.IsOccupied)
                {
                    occupiedSnapPointIds.Add(runtimeSnapPoint.SnapPointId);
                }
            }

            return occupiedSnapPointIds;
        }

        public bool HasOccupiedSnapPoint(string snapPointId)
        {
            if (!TryGetRuntimeSnapPoint(snapPointId, out CCS_BuildingRuntimeSnapPoint runtimeSnapPoint))
            {
                return false;
            }

            return runtimeSnapPoint.IsOccupied;
        }

        #endregion

        #region Properties

        public string InstanceId { get; }

        public string PieceId { get; }

        public Vector3 Position { get; }

        public Quaternion Rotation { get; }

        public float CreationTime { get; }

        public string TargetSnapInstanceId { get; private set; } = string.Empty;

        public string TargetSnapPointId { get; private set; } = string.Empty;

        public IReadOnlyList<CCS_BuildingRuntimeSnapPoint> RuntimeSnapPoints => runtimeSnapPoints;

        #endregion
    }
}
