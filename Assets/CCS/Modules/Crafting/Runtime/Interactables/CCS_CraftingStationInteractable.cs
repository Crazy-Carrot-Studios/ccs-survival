using CCS.Modules.Interaction;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_CraftingStationInteractable
// CATEGORY: Modules / Crafting / Runtime / Interactables
// PURPOSE: Sets active crafting station context when the player interacts.
// PLACEMENT: Attach to primitive workbench and campfire objects in bootstrap scenes.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-06-01
// NOTES: Milestone 1.1.1 workstation foundation. No crafting UI.
// =============================================================================

namespace CCS.Modules.Crafting
{
    public sealed class CCS_CraftingStationInteractable : MonoBehaviour, CCS_IInteractableResultProvider
    {
        #region Variables

        [Header("Station")]
        [Tooltip("Crafting station type granted while this object is the active context source.")]
        [SerializeField] private CCS_CraftingStationType stationType = CCS_CraftingStationType.Workbench;

        [Tooltip("Stable station id used by future world station systems.")]
        [SerializeField] private string stationId = string.Empty;

        [Tooltip("Player-facing station label shown in interaction prompts.")]
        [SerializeField] private string stationDisplayName = "Workbench";

        [Header("Interaction")]
        [Tooltip("Maximum distance at which this station accepts interaction requests.")]
        [SerializeField] private float interactionDistance = 3f;

        #endregion

        #region Public Methods

        public void ConfigureRuntime(
            CCS_CraftingStationType configuredStationType,
            string configuredStationId,
            string configuredDisplayName)
        {
            stationType = configuredStationType;
            stationId = configuredStationId ?? string.Empty;
            stationDisplayName = configuredDisplayName ?? string.Empty;
        }

        public string GetInteractionDisplayName()
        {
            return string.IsNullOrWhiteSpace(stationDisplayName)
                ? stationType.ToString()
                : stationDisplayName;
        }

        public float GetInteractionDistance()
        {
            return interactionDistance < 0.1f ? 3f : interactionDistance;
        }

        public bool CanInteract()
        {
            return isActiveAndEnabled;
        }

        public void Interact()
        {
            TryInteract();
        }

        public bool TryInteract()
        {
            CCS_CraftingStationContext stationContext = new CCS_CraftingStationContext(
                stationType,
                stationDisplayName,
                string.IsNullOrWhiteSpace(stationId) ? name : stationId);

            if (CCS_CraftingRuntimeBridge.TryGetCraftingService(out CCS_CraftingService craftingService)
                && craftingService.IsInitialized)
            {
                craftingService.SetActiveStationContext(stationContext);
            }

            if (CCS_CraftingRuntimeBridge.TryGetCraftingRecipeService(out CCS_CraftingRecipeService recipeService)
                && recipeService.IsInitialized)
            {
                recipeService.ApplyActiveStationContext(stationContext);
            }

            return true;
        }

        #endregion
    }
}
