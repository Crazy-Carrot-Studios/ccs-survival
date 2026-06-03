using CCS.Modules.Interaction;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_FarmPlotInteractable
// CATEGORY: Modules / Farming / Runtime / Components
// PURPOSE: Interactable farm plot for harvesting mature crops.
// PLACEMENT: Spawned on farm plot world objects by CCS_FarmService.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-06-02
// NOTES: Milestone 2.2.0 — harvest via Interact key when crop is mature.
// =============================================================================

namespace CCS.Modules.Farming
{
    public sealed class CCS_FarmPlotInteractable : MonoBehaviour, CCS_IInteractableResultProvider
    {
        private string plotInstanceId = string.Empty;

        public void ConfigureRuntime(string instanceId)
        {
            plotInstanceId = instanceId ?? string.Empty;
        }

        public bool CanInteract()
        {
            if (!CCS_FarmRuntimeBridge.TryGetFarmService(out CCS_FarmService farmService)
                || !farmService.TryGetPlot(plotInstanceId, out CCS_FarmPlotInstance plot))
            {
                return false;
            }

            return plot.CanHarvest || plot.CanPlant;
        }

        public void Interact()
        {
            TryInteract();
        }

        public bool TryInteract()
        {
            if (!CCS_FarmRuntimeBridge.TryGetFarmService(out CCS_FarmService farmService))
            {
                return false;
            }

            if (farmService.TryHarvestPlot(plotInstanceId))
            {
                return true;
            }

            return false;
        }

        public string GetInteractionDisplayName()
        {
            if (!CCS_FarmRuntimeBridge.TryGetFarmService(out CCS_FarmService farmService)
                || !farmService.TryGetPlot(plotInstanceId, out CCS_FarmPlotInstance plot))
            {
                return "Farm Plot";
            }

            if (plot.CanHarvest && plot.Crop?.Definition != null)
            {
                return $"Harvest {plot.Crop.Definition.DisplayName}";
            }

            return plot.Definition != null ? plot.Definition.DisplayName : "Farm Plot";
        }

        public float GetInteractionDistance()
        {
            if (CCS_FarmRuntimeBridge.TryGetFarmService(out CCS_FarmService farmService)
                && farmService.TryGetPlot(plotInstanceId, out CCS_FarmPlotInstance plot)
                && plot.Definition != null)
            {
                return plot.Definition.InteractionDistance;
            }

            return 3f;
        }
    }
}
