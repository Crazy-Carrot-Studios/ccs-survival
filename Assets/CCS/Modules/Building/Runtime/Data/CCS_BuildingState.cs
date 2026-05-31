using System.Collections.Generic;

// =============================================================================
// SCRIPT: CCS_BuildingState
// CATEGORY: Modules / Building / Runtime / Data
// PURPOSE: Mutable runtime building catalog state owned by CCS_BuildingService.
// PLACEMENT: Internal to building service. Exposed through snapshots and save data.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-31
// NOTES: Tracks registered definitions only. No placed structures in 0.8.0.
// =============================================================================

namespace CCS.Modules.Building
{
    public sealed class CCS_BuildingState
    {
        #region Variables

        private readonly List<string> registeredPieceIds = new List<string>();

        #endregion

        #region Properties

        public int RegisteredDefinitionCount => registeredPieceIds.Count;

        public IReadOnlyList<string> RegisteredPieceIds => registeredPieceIds;

        #endregion

        #region Public Methods

        public bool ContainsPieceId(string pieceId)
        {
            return !string.IsNullOrWhiteSpace(pieceId) && registeredPieceIds.Contains(pieceId);
        }

        public bool RegisterPieceId(string pieceId)
        {
            if (string.IsNullOrWhiteSpace(pieceId) || registeredPieceIds.Contains(pieceId))
            {
                return false;
            }

            registeredPieceIds.Add(pieceId);
            return true;
        }

        public bool UnregisterPieceId(string pieceId)
        {
            if (string.IsNullOrWhiteSpace(pieceId))
            {
                return false;
            }

            return registeredPieceIds.Remove(pieceId);
        }

        public void Clear()
        {
            registeredPieceIds.Clear();
        }

        public void ReplaceRegisteredPieceIds(IEnumerable<string> pieceIds)
        {
            registeredPieceIds.Clear();

            if (pieceIds == null)
            {
                return;
            }

            foreach (string pieceId in pieceIds)
            {
                RegisterPieceId(pieceId);
            }
        }

        #endregion
    }
}
