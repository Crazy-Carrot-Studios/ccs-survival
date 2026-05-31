using UnityEngine;

// =============================================================================
// SCRIPT: CCS_BuildingPlacementState
// CATEGORY: Modules / Building / Runtime / Data
// PURPOSE: Mutable runtime placement mode state owned by CCS_BuildingPlacementService.
// PLACEMENT: Internal to placement service. Exposed through CCS_BuildingPlacementSnapshot.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-31
// NOTES: Tracks active snap match and preview validity for 0.8.3 snapping.
// =============================================================================

namespace CCS.Modules.Building
{
    public sealed class CCS_BuildingPlacementState
    {
        #region Variables

        public bool IsPlacementModeActive;

        public string ActivePieceId = string.Empty;

        public CCS_BuildingPieceType ActivePieceType = CCS_BuildingPieceType.Custom;

        public Vector3 PreviewPosition;

        public Quaternion PreviewRotation = Quaternion.identity;

        public bool IsPlacementValid;

        public bool HasSnapTarget;

        public CCS_BuildingSnapPointType SnapTargetType = CCS_BuildingSnapPointType.Free;

        public bool IsSnappedPreview;

        public CCS_BuildingSnapMatch ActiveSnapMatch = CCS_BuildingSnapMatch.Empty;

        #endregion

        #region Public Methods

        public void Clear()
        {
            IsPlacementModeActive = false;
            ActivePieceId = string.Empty;
            ActivePieceType = CCS_BuildingPieceType.Custom;
            PreviewPosition = Vector3.zero;
            PreviewRotation = Quaternion.identity;
            IsPlacementValid = false;
            HasSnapTarget = false;
            SnapTargetType = CCS_BuildingSnapPointType.Free;
            IsSnappedPreview = false;
            ActiveSnapMatch = CCS_BuildingSnapMatch.Empty;
        }

        public void ActivatePlacement(string pieceId, CCS_BuildingPieceType pieceType)
        {
            IsPlacementModeActive = true;
            ActivePieceId = pieceId ?? string.Empty;
            ActivePieceType = pieceType;
            PreviewPosition = Vector3.zero;
            PreviewRotation = Quaternion.identity;
            IsPlacementValid = false;
            HasSnapTarget = false;
            SnapTargetType = CCS_BuildingSnapPointType.Free;
            IsSnappedPreview = false;
            ActiveSnapMatch = CCS_BuildingSnapMatch.Empty;
        }

        public void UpdatePreview(Vector3 position, Quaternion rotation, bool isValid)
        {
            PreviewPosition = position;
            PreviewRotation = rotation;
            IsPlacementValid = isValid;
            HasSnapTarget = false;
            SnapTargetType = CCS_BuildingSnapPointType.Free;
            IsSnappedPreview = false;
            ActiveSnapMatch = CCS_BuildingSnapMatch.Empty;
        }

        public void UpdatePreviewWithSnap(
            Vector3 position,
            Quaternion rotation,
            bool isValid,
            CCS_BuildingSnapMatch snapMatch,
            bool isSnappedPreview)
        {
            PreviewPosition = position;
            PreviewRotation = rotation;
            IsPlacementValid = isValid;
            ActiveSnapMatch = snapMatch;
            IsSnappedPreview = isSnappedPreview;

            if (snapMatch.HasMatch)
            {
                HasSnapTarget = true;
                SnapTargetType = snapMatch.TargetSnapPointType;
                return;
            }

            HasSnapTarget = false;
            SnapTargetType = CCS_BuildingSnapPointType.Free;
        }

        public CCS_BuildingPlacementSnapshot CreateSnapshot()
        {
            return new CCS_BuildingPlacementSnapshot(
                IsPlacementModeActive,
                ActivePieceId,
                ActivePieceType,
                PreviewPosition,
                PreviewRotation,
                IsPlacementValid,
                HasSnapTarget,
                SnapTargetType,
                IsSnappedPreview);
        }

        #endregion
    }
}
