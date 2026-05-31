using UnityEngine;

// =============================================================================
// SCRIPT: CCS_BuildingInstance
// CATEGORY: Modules / Building / Runtime / Data
// PURPOSE: Represents a placed building structure instance in the world.
// PLACEMENT: Owned by CCS_BuildingService placed instance catalog.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-31
// NOTES: No durability fields in 0.8.1. No structural integrity yet.
// =============================================================================

namespace CCS.Modules.Building
{
    public sealed class CCS_BuildingInstance
    {
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

        #endregion

        #region Properties

        public string InstanceId { get; }

        public string PieceId { get; }

        public Vector3 Position { get; }

        public Quaternion Rotation { get; }

        public float CreationTime { get; }

        #endregion
    }
}
