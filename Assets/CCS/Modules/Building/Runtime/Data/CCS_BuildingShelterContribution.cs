using UnityEngine;

// =============================================================================
// SCRIPT: CCS_BuildingShelterContribution
// CATEGORY: Modules / Building / Runtime / Data
// PURPOSE: Runtime shelter protection contribution from a placed building instance.
// PLACEMENT: Produced by CCS_BuildingService.Consumed by CCS_ShelterService.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-31
// NOTES: Simple radius coverage only. No enclosure detection in 0.8.5.
// =============================================================================

namespace CCS.Modules.Building
{
    public sealed class CCS_BuildingShelterContribution
    {
        #region Public Methods

        public CCS_BuildingShelterContribution(
            string buildingInstanceId,
            string pieceDefinitionId,
            Vector3 worldPosition,
            float coverageRadius,
            float wetnessProtection,
            float exposureProtection,
            float temperatureProtection)
        {
            BuildingInstanceId = buildingInstanceId ?? string.Empty;
            PieceDefinitionId = pieceDefinitionId ?? string.Empty;
            WorldPosition = worldPosition;
            CoverageRadius = coverageRadius < 0f ? 0f : coverageRadius;
            WetnessProtection = wetnessProtection < 0f ? 0f : wetnessProtection;
            ExposureProtection = exposureProtection < 0f ? 0f : exposureProtection;
            TemperatureProtection = temperatureProtection;
        }

        #endregion

        #region Properties

        public string BuildingInstanceId { get; }

        public string PieceDefinitionId { get; }

        public Vector3 WorldPosition { get; }

        public float CoverageRadius { get; }

        public float WetnessProtection { get; }

        public float ExposureProtection { get; }

        public float TemperatureProtection { get; }

        #endregion
    }
}
