using UnityEngine;

// =============================================================================
// SCRIPT: CCS_FarmPlotInstance
// CATEGORY: Modules / Farming / Runtime / Data
// PURPOSE: Runtime farm plot with optional single crop instance.
// PLACEMENT: Owned by CCS_FarmService.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-06-02
// NOTES: Milestone 2.2.0 farming foundation.
// =============================================================================

namespace CCS.Modules.Farming
{
    public sealed class CCS_FarmPlotInstance
    {
        public CCS_FarmPlotInstance(
            string instanceId,
            CCS_FarmPlotDefinition definition,
            Vector3 worldPosition,
            float rotationY,
            string campOwnerId)
        {
            InstanceId = instanceId;
            Definition = definition;
            WorldPosition = worldPosition;
            RotationY = rotationY;
            CampOwnerId = campOwnerId;
        }

        public string InstanceId { get; }

        public CCS_FarmPlotDefinition Definition { get; }

        public Vector3 WorldPosition { get; set; }

        public float RotationY { get; set; }

        public string CampOwnerId { get; }

        public CCS_CropInstance Crop { get; private set; }

        public GameObject WorldObject { get; set; }

        public bool HasCrop => Crop != null && Crop.GrowthStage != CCS_CropGrowthStage.Empty;

        public bool CanPlant => Crop == null || Crop.GrowthStage == CCS_CropGrowthStage.Empty;

        public bool CanHarvest => Crop != null && Crop.GrowthStage == CCS_CropGrowthStage.Mature;

        public void PlantCrop(CCS_CropDefinition cropDefinition)
        {
            Crop = cropDefinition != null ? new CCS_CropInstance(cropDefinition) : null;
        }

        public void ClearCrop()
        {
            Crop = null;
        }

        public void ApplySnapshot(CCS_FarmPlotSnapshot snapshot, CCS_CropProfile profile)
        {
            if (snapshot?.crop == null || !snapshot.crop.hasCrop || profile == null)
            {
                Crop = null;
                return;
            }

            if (!profile.TryGetCropById(snapshot.crop.cropDefinitionId, out CCS_CropDefinition cropDefinition))
            {
                Crop = null;
                return;
            }

            Crop = new CCS_CropInstance(cropDefinition);
            Crop.ApplySnapshot(snapshot.crop, cropDefinition);
        }

        public CCS_FarmPlotSnapshot CaptureSnapshot()
        {
            CCS_FarmPlotSnapshot snapshot = new CCS_FarmPlotSnapshot
            {
                instanceId = InstanceId,
                plotDefinitionId = Definition != null ? Definition.PlotDefinitionId : string.Empty,
                positionX = WorldPosition.x,
                positionY = WorldPosition.y,
                positionZ = WorldPosition.z,
                rotationY = RotationY,
                campOwnerId = CampOwnerId ?? string.Empty,
                crop = Crop != null ? Crop.CaptureSnapshot() : new CCS_CropSnapshot()
            };

            if (snapshot.crop != null && !HasCrop)
            {
                snapshot.crop.hasCrop = false;
            }

            return snapshot;
        }
    }
}
