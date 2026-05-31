using UnityEngine;

// =============================================================================
// SCRIPT: CCS_BuildingPlacementSnapshot
// CATEGORY: Modules / Building / Runtime / Data
// PURPOSE: Read-only placement mode snapshot for HUD and debug tooling.
// PLACEMENT: Produced by CCS_BuildingPlacementService and CCS_BuildingService.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-31
// NOTES: Includes snap target and validity for 0.8.3 HUD debug display.
// =============================================================================

namespace CCS.Modules.Building
{
    public readonly struct CCS_BuildingPlacementSnapshot
    {
        #region Public Methods

        public CCS_BuildingPlacementSnapshot(
            bool isPlacementModeActive,
            string activePieceId,
            CCS_BuildingPieceType activePieceType,
            Vector3 previewPosition,
            Quaternion previewRotation,
            bool isPlacementValid,
            bool hasSnapTarget,
            CCS_BuildingSnapPointType snapTargetType,
            bool isSnappedPreview)
        {
            IsPlacementModeActive = isPlacementModeActive;
            ActivePieceId = activePieceId ?? string.Empty;
            ActivePieceType = activePieceType;
            PreviewPosition = previewPosition;
            PreviewRotation = previewRotation;
            IsPlacementValid = isPlacementValid;
            HasSnapTarget = hasSnapTarget;
            SnapTargetType = snapTargetType;
            IsSnappedPreview = isSnappedPreview;
        }

        public static CCS_BuildingPlacementSnapshot Empty =>
            new CCS_BuildingPlacementSnapshot(
                false,
                string.Empty,
                CCS_BuildingPieceType.Custom,
                Vector3.zero,
                Quaternion.identity,
                false,
                false,
                CCS_BuildingSnapPointType.Free,
                false);

        #endregion

        #region Properties

        public bool IsPlacementModeActive { get; }

        public string ActivePieceId { get; }

        public CCS_BuildingPieceType ActivePieceType { get; }

        public Vector3 PreviewPosition { get; }

        public Quaternion PreviewRotation { get; }

        public bool IsPlacementValid { get; }

        public bool HasSnapTarget { get; }

        public CCS_BuildingSnapPointType SnapTargetType { get; }

        public bool IsSnappedPreview { get; }

        #endregion
    }
}
