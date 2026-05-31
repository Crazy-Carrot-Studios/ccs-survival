using UnityEngine;

// =============================================================================
// SCRIPT: CCS_BuildingRuntimeSnapPoint
// CATEGORY: Modules / Building / Runtime / Snap
// PURPOSE: World-space snap point state for a placed building instance.
// PLACEMENT: Owned by CCS_BuildingInstance runtime snap point list.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-31
// NOTES: Occupancy updated when another piece snaps to this point.
// =============================================================================

namespace CCS.Modules.Building
{
    public sealed class CCS_BuildingRuntimeSnapPoint
    {
        #region Public Methods

        public CCS_BuildingRuntimeSnapPoint(
            string instanceId,
            string snapPointId,
            CCS_BuildingSnapPointType snapPointType,
            Vector3 localPosition,
            Quaternion localRotation)
        {
            InstanceId = instanceId ?? string.Empty;
            SnapPointId = snapPointId ?? string.Empty;
            SnapPointType = snapPointType;
            localOffsetPosition = localPosition;
            localOffsetRotation = localRotation;
            worldPosition = Vector3.zero;
            worldRotation = Quaternion.identity;
        }

        public void UpdateWorldTransform(Vector3 instancePosition, Quaternion instanceRotation)
        {
            worldRotation = instanceRotation * localOffsetRotation;
            worldPosition = instancePosition + (instanceRotation * localOffsetPosition);
        }

        public void SetOccupied(bool occupied)
        {
            isOccupied = occupied;
        }

        #endregion

        #region Properties

        public string InstanceId { get; }

        public string SnapPointId { get; }

        public CCS_BuildingSnapPointType SnapPointType { get; }

        public Vector3 WorldPosition => worldPosition;

        public Quaternion WorldRotation => worldRotation;

        public bool IsOccupied => isOccupied;

        #endregion

        #region Variables

        private readonly Vector3 localOffsetPosition;
        private readonly Quaternion localOffsetRotation;
        private Vector3 worldPosition;
        private Quaternion worldRotation;
        private bool isOccupied;

        #endregion
    }
}
