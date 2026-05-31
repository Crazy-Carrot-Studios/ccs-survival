using UnityEngine;

// =============================================================================
// SCRIPT: CCS_BuildingPlacementSnapshot
// CATEGORY: Modules / Building / Runtime / Data
// PURPOSE: Read-only placement mode snapshot for HUD and debug tooling.
// PLACEMENT: Produced by CCS_BuildingPlacementService and CCS_BuildingService.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-31
// NOTES: Preview position/rotation placeholders until input systems arrive.
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
            bool isPlacementValid)
        {
            IsPlacementModeActive = isPlacementModeActive;
            ActivePieceId = activePieceId ?? string.Empty;
            ActivePieceType = activePieceType;
            PreviewPosition = previewPosition;
            PreviewRotation = previewRotation;
            IsPlacementValid = isPlacementValid;
        }

        public static CCS_BuildingPlacementSnapshot Empty =>
            new CCS_BuildingPlacementSnapshot(
                false,
                string.Empty,
                CCS_BuildingPieceType.Custom,
                Vector3.zero,
                Quaternion.identity,
                false);

        #endregion

        #region Properties

        public bool IsPlacementModeActive { get; }

        public string ActivePieceId { get; }

        public CCS_BuildingPieceType ActivePieceType { get; }

        public Vector3 PreviewPosition { get; }

        public Quaternion PreviewRotation { get; }

        public bool IsPlacementValid { get; }

        #endregion
    }
}
