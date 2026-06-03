using UnityEngine;

// =============================================================================
// SCRIPT: CCS_CropInstance
// CATEGORY: Modules / Farming / Runtime / Data
// PURPOSE: Runtime crop state attached to a farm plot instance.
// PLACEMENT: Owned by CCS_FarmPlotInstance.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-06-02
// NOTES: Timer-based growth stages. Milestone 2.2.0.
// =============================================================================

namespace CCS.Modules.Farming
{
    public sealed class CCS_CropInstance
    {
        public CCS_CropInstance(CCS_CropDefinition definition)
        {
            Definition = definition;
            GrowthStage = CCS_CropGrowthStage.Planted;
            GrowthElapsedSeconds = 0f;
        }

        public CCS_CropDefinition Definition { get; }

        public CCS_CropGrowthStage GrowthStage { get; set; }

        public float GrowthElapsedSeconds { get; set; }

        public GameObject VisualObject { get; set; }

        public void ApplySnapshot(CCS_CropSnapshot snapshot, CCS_CropDefinition definition)
        {
            if (snapshot == null || definition == null || !snapshot.hasCrop)
            {
                return;
            }

            GrowthElapsedSeconds = snapshot.growthElapsedSeconds;
            GrowthStage = (CCS_CropGrowthStage)snapshot.growthStage;
        }

        public CCS_CropSnapshot CaptureSnapshot()
        {
            return new CCS_CropSnapshot
            {
                cropDefinitionId = Definition != null ? Definition.CropId : string.Empty,
                growthStage = (int)GrowthStage,
                growthElapsedSeconds = GrowthElapsedSeconds,
                hasCrop = Definition != null && GrowthStage != CCS_CropGrowthStage.Empty
            };
        }

        public void UpdateGrowthStageFromElapsed()
        {
            if (Definition == null)
            {
                GrowthStage = CCS_CropGrowthStage.Empty;
                return;
            }

            if (GrowthStage == CCS_CropGrowthStage.Harvested || GrowthStage == CCS_CropGrowthStage.Empty)
            {
                return;
            }

            float duration = Definition.GrowthDurationSeconds;
            if (duration <= 0f)
            {
                GrowthStage = CCS_CropGrowthStage.Mature;
                return;
            }

            float progress = GrowthElapsedSeconds / duration;
            if (progress >= 1f)
            {
                GrowthStage = CCS_CropGrowthStage.Mature;
                return;
            }

            if (progress < 0.08f)
            {
                GrowthStage = CCS_CropGrowthStage.Planted;
            }
            else if (progress < 0.35f)
            {
                GrowthStage = CCS_CropGrowthStage.Sprouting;
            }
            else
            {
                GrowthStage = CCS_CropGrowthStage.Growing;
            }
        }
    }
}
