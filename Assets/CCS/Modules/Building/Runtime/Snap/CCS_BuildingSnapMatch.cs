using UnityEngine;

// =============================================================================
// SCRIPT: CCS_BuildingSnapMatch
// CATEGORY: Modules / Building / Runtime / Snap
// PURPOSE: Result payload for snap matching during placement preview.
// PLACEMENT: Produced by CCS_BuildingPlacementService.FindBestSnapMatch().
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-31
// NOTES: Includes snapped transform and target/source snap point identity.
// =============================================================================

namespace CCS.Modules.Building
{
    public readonly struct CCS_BuildingSnapMatch
    {
        #region Public Methods

        public CCS_BuildingSnapMatch(
            bool hasMatch,
            string targetInstanceId,
            string targetSnapPointId,
            CCS_BuildingSnapPointType targetSnapPointType,
            string sourceSnapPointId,
            CCS_BuildingSnapPointType sourceSnapPointType,
            Vector3 snappedPosition,
            Quaternion snappedRotation)
        {
            HasMatch = hasMatch;
            TargetInstanceId = targetInstanceId ?? string.Empty;
            TargetSnapPointId = targetSnapPointId ?? string.Empty;
            TargetSnapPointType = targetSnapPointType;
            SourceSnapPointId = sourceSnapPointId ?? string.Empty;
            SourceSnapPointType = sourceSnapPointType;
            SnappedPosition = snappedPosition;
            SnappedRotation = snappedRotation;
        }

        public static CCS_BuildingSnapMatch Empty =>
            new CCS_BuildingSnapMatch(
                false,
                string.Empty,
                string.Empty,
                CCS_BuildingSnapPointType.Free,
                string.Empty,
                CCS_BuildingSnapPointType.Free,
                Vector3.zero,
                Quaternion.identity);

        #endregion

        #region Properties

        public bool HasMatch { get; }

        public string TargetInstanceId { get; }

        public string TargetSnapPointId { get; }

        public CCS_BuildingSnapPointType TargetSnapPointType { get; }

        public string SourceSnapPointId { get; }

        public CCS_BuildingSnapPointType SourceSnapPointType { get; }

        public Vector3 SnappedPosition { get; }

        public Quaternion SnappedRotation { get; }

        #endregion
    }
}
