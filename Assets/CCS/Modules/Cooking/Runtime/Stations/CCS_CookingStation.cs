using UnityEngine;

// =============================================================================
// SCRIPT: CCS_CookingStation
// CATEGORY: Modules / Cooking / Runtime / Stations
// PURPOSE: World cooking station state for campfire cooking without player input logic.
// PLACEMENT: Attach to CCS_TestCampfire and building-placed campfire instances.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-06-01
// NOTES: Inventory and recipe execution are handled by CCS_CookingService.
// =============================================================================

namespace CCS.Modules.Cooking
{
    public sealed class CCS_CookingStation : MonoBehaviour
    {
        #region Variables

        [Header("Station Identity")]
        [Tooltip("Cooking station archetype used for profile validation.")]
        [SerializeField] private CCS_CookingStationType stationType = CCS_CookingStationType.Campfire;

        [Tooltip("When false, the station cannot cook until lit or activated.")]
        [SerializeField] private bool isStationActive;

        [Header("Runtime State")]
        [Tooltip("Recipe currently being cooked on this station.")]
        [SerializeField] private string currentRecipeId = string.Empty;

        [Tooltip("When true, fuel was consumed for the active or last cook cycle.")]
        [SerializeField] private bool hasFuelLoaded;

        private CCS_CookingProfile configuredProfile;
        private CCS_CookingService cookingService;
        private bool isCooking;

        #endregion

        #region Properties

        public CCS_CookingStationType StationType => stationType;

        public bool IsStationActive => isStationActive;

        public string CurrentRecipeId => currentRecipeId;

        public bool HasFuelLoaded => hasFuelLoaded;

        public bool IsCooking => isCooking;

        public Vector3 WorldPosition => transform.position;

        #endregion

        #region Unity Callbacks

        private void OnEnable()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            RegisterWithService();
        }

        private void OnDisable()
        {
            if (cookingService != null)
            {
                cookingService.UnregisterStation(this);
            }
        }

        #endregion

        #region Public Methods

        public void ConfigureFromProfile(CCS_CookingProfile profile, bool startActive)
        {
            configuredProfile = profile;
            isStationActive = startActive;
            currentRecipeId = string.Empty;
            hasFuelLoaded = false;
            isCooking = false;

            if (Application.isPlaying)
            {
                RegisterWithService();
            }
        }

        public void SetStationActive(bool active)
        {
            isStationActive = active;
        }

        public bool CanCook()
        {
            ResolveService();
            return isStationActive
                && !isCooking
                && cookingService != null
                && cookingService.IsInitialized;
        }

        public bool StartCooking(string recipeId)
        {
            ResolveService();
            if (cookingService == null || string.IsNullOrWhiteSpace(recipeId))
            {
                return false;
            }

            CCS_CookingResult result = cookingService.TryStartCooking(this, recipeId);
            return result != null && result.IsSuccess;
        }

        public void ApplyCookingStarted(string recipeId)
        {
            currentRecipeId = recipeId ?? string.Empty;
            isCooking = true;
            hasFuelLoaded = true;
        }

        public void CompleteCooking()
        {
            currentRecipeId = string.Empty;
            isCooking = false;
            hasFuelLoaded = false;
        }

        public void CancelCooking()
        {
            currentRecipeId = string.Empty;
            isCooking = false;
            hasFuelLoaded = false;
        }

        #endregion

        #region Private Methods

        private void RegisterWithService()
        {
            ResolveService();
            cookingService?.RegisterStation(this);
        }

        private void ResolveService()
        {
            if (cookingService != null && cookingService.IsInitialized)
            {
                return;
            }

            CCS_CookingRuntimeBridge.TryGetCookingService(out cookingService);
        }

        #endregion
    }
}
