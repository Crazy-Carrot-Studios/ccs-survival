using System.Collections.Generic;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_BuildingInstance
// CATEGORY: Modules / Building / Runtime / Data
// PURPOSE: Represents a placed building structure instance in the world.
// PLACEMENT: Owned by CCS_BuildingService placed instance catalog.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-31
// NOTES: Runtime snap points derived from definition in 0.8.3.
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

        #endregion

        #region Properties

        public string InstanceId { get; }

        public string PieceId { get; }

        public Vector3 Position { get; }

        public Quaternion Rotation { get; }

        public float CreationTime { get; }

        public IReadOnlyList<CCS_BuildingRuntimeSnapPoint> RuntimeSnapPoints => runtimeSnapPoints;

        #endregion
    }
}
