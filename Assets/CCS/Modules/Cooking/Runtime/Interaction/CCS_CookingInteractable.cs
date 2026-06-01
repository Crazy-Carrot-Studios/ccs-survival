using CCS.Modules.Interaction;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_CookingInteractable
// CATEGORY: Modules / Cooking / Runtime / Interaction
// PURPOSE: Interaction entry for campfire cooking using CCS_CookingService recipes.
// PLACEMENT: Same GameObject as CCS_CookingStation on bootstrap and placed campfires.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-06-01
// NOTES: Auto-starts first valid recipe when raw food and fuel are available.
// =============================================================================

namespace CCS.Modules.Cooking
{
    [RequireComponent(typeof(CCS_CookingStation))]
    public sealed class CCS_CookingInteractable : MonoBehaviour, CCS_IInteractableResultProvider
    {
        #region Variables

        [Header("Dependencies")]
        [Tooltip("Cooking station executed when the player interacts.")]
        [SerializeField] private CCS_CookingStation cookingStation;

        [Tooltip("Optional profile override when registry services are unavailable.")]
        [SerializeField] private CCS_CookingProfile cookingProfileOverride;

        private CCS_CookingService cookingService;
        private CCS_CookingProfile resolvedProfile;
        private string lastInteractionMessage = string.Empty;

        #endregion

        #region Unity Callbacks

        private void Awake()
        {
            if (cookingStation == null)
            {
                cookingStation = GetComponent<CCS_CookingStation>();
            }
        }

        #endregion

        #region Public Methods

        public void ConfigureRuntime(CCS_CookingProfile profile, CCS_CookingStation station, bool startActive)
        {
            cookingProfileOverride = profile;
            resolvedProfile = profile;
            cookingStation = station != null ? station : cookingStation;
            cookingStation?.ConfigureFromProfile(profile, startActive);
        }

        public string GetInteractionDisplayName()
        {
            if (cookingStation != null && cookingStation.IsCooking)
            {
                return "Cooking...";
            }

            if (cookingStation != null && !cookingStation.IsStationActive)
            {
                return "Light Campfire";
            }

            return "Cook Food";
        }

        public bool CanInteract()
        {
            ResolveServices();
            return cookingStation != null
                && cookingService != null
                && cookingService.IsInitialized
                && !cookingStation.IsCooking;
        }

        public void Interact()
        {
            TryInteract();
        }

        public bool TryInteract()
        {
            ResolveServices();
            if (cookingStation == null || cookingService == null)
            {
                lastInteractionMessage = "Cooking is unavailable.";
                return false;
            }

            if (cookingStation.IsCooking)
            {
                lastInteractionMessage = "Campfire is already cooking.";
                return false;
            }

            if (!cookingStation.IsStationActive)
            {
                cookingStation.SetStationActive(true);
                lastInteractionMessage = "Campfire lit.";
                return true;
            }

            if (!cookingService.TryFindFirstCookableRecipe(cookingStation, out CCS_CookingRecipe recipe, out string failureMessage))
            {
                lastInteractionMessage = failureMessage;
                return false;
            }

            CCS_CookingResult result = cookingService.TryStartCooking(cookingStation, recipe.RecipeId);
            lastInteractionMessage = result?.Message ?? "Cooking failed.";
            return result != null && result.IsSuccess;
        }

        public float GetInteractionDistance()
        {
            ResolveProfile();
            return resolvedProfile != null ? resolvedProfile.DefaultInteractDistance : 3f;
        }

        public string GetLastInteractionMessage()
        {
            return lastInteractionMessage;
        }

        #endregion

        #region Private Methods

        private void ResolveServices()
        {
            if (cookingStation == null)
            {
                cookingStation = GetComponent<CCS_CookingStation>();
            }

            if (cookingService == null)
            {
                CCS_CookingRuntimeBridge.TryGetCookingService(out cookingService);
            }

            ResolveProfile();
        }

        private void ResolveProfile()
        {
            if (cookingProfileOverride != null)
            {
                resolvedProfile = cookingProfileOverride;
                return;
            }

            if (resolvedProfile != null)
            {
                return;
            }

            if (cookingService != null && cookingService.ActiveProfile != null)
            {
                resolvedProfile = cookingService.ActiveProfile;
            }
        }

        #endregion
    }
}
