using UnityEngine;

// =============================================================================
// SCRIPT: CCS_BuildingPieceSnapshot
// CATEGORY: Modules / Building / Runtime / Data
// PURPOSE: Read-only building piece snapshot with placement placeholders.
// PLACEMENT: Produced by CCS_BuildingState and CCS_BuildingService lookups.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-31
// NOTES: World position and rotation are placeholders. No transform spawning in 0.8.0.
// =============================================================================

namespace CCS.Modules.Building
{
    public readonly struct CCS_BuildingPieceSnapshot
    {
        #region Public Methods

        public CCS_BuildingPieceSnapshot(
            string pieceId,
            CCS_BuildingPieceType pieceType,
            Vector3 worldPosition,
            Quaternion worldRotation)
        {
            PieceId = pieceId ?? string.Empty;
            PieceType = pieceType;
            WorldPosition = worldPosition;
            WorldRotation = worldRotation;
        }

        public static CCS_BuildingPieceSnapshot Empty =>
            new CCS_BuildingPieceSnapshot(string.Empty, CCS_BuildingPieceType.Custom, Vector3.zero, Quaternion.identity);

        #endregion

        #region Properties

        public string PieceId { get; }

        public CCS_BuildingPieceType PieceType { get; }

        public Vector3 WorldPosition { get; }

        public Quaternion WorldRotation { get; }

        #endregion
    }
}
