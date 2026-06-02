using CCS.Modules.Interaction;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_FishingSpot
// CATEGORY: Modules / Fishing / Runtime / Spots
// PURPOSE: World fishable water-source interactable registered with CCS_FishingService.
// PLACEMENT: Bootstrap test spots and future river/pond/lake/stream content.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-06-01
// NOTES: Deferred registration until fishing service exists (matches CCS_SleepSpot pattern).
// =============================================================================

namespace CCS.Modules.Fishing
{
    public sealed class CCS_FishingSpot : MonoBehaviour, CCS_IInteractableResultProvider
    {
        #region Variables

        [Header("Definition")]
        [Tooltip("Spot metadata, catch table override, and resource harvest method.")]
        [SerializeField] private CCS_FishingSpotDefinition spotDefinition;

        [Header("Dependencies")]
        [Tooltip("Optional profile override for interaction distance.")]
        [SerializeField] private CCS_FishingProfile fishingProfileOverride;

        private bool isRegisteredWithService;

        #endregion

        #region Properties

        public CCS_FishingSpotDefinition SpotDefinition => spotDefinition;

        public string SpotId => spotDefinition != null ? spotDefinition.SpotId : string.Empty;

        #endregion

        #region Unity Callbacks

        private void OnEnable()
        {
            TryRegisterWithService();
        }

        private void Start()
        {
            TryRegisterWithService();
        }

        private void Update()
        {
            if (!isRegisteredWithService)
            {
                TryRegisterWithService();
            }
        }

        private void OnDisable()
        {
            UnregisterFromService();
        }

        #endregion

        #region Public Methods

        public void ConfigureRuntime(CCS_FishingSpotDefinition definition, CCS_FishingProfile profile = null)
        {
            spotDefinition = definition;
            fishingProfileOverride = profile;
        }

        public bool CanFish()
        {
            return spotDefinition != null && spotDefinition.SupportsFishing;
        }

        public string GetInteractionDisplayName()
        {
            if (spotDefinition == null)
            {
                return "Fish";
            }

            string waterLabel = spotDefinition.WaterBodyType.ToString();
            return string.IsNullOrWhiteSpace(spotDefinition.DisplayName)
                ? $"Fish ({waterLabel})"
                : spotDefinition.DisplayName;
        }

        public bool CanInteract()
        {
            return CanFish();
        }

        public void Interact()
        {
            TryInteract();
        }

        public bool TryInteract()
        {
            // Foundation: fish through active fishing pole routing, not standalone Interact key.
            return false;
        }

        public float GetInteractionDistance()
        {
            if (spotDefinition != null && spotDefinition.InteractionDistance > 0f)
            {
                return spotDefinition.InteractionDistance;
            }

            ResolveProfile();
            return fishingProfileOverride != null
                ? fishingProfileOverride.DefaultInteractionDistance
                : 4f;
        }

        #endregion

        #region Private Methods

        private void ResolveProfile()
        {
            if (fishingProfileOverride != null)
            {
                return;
            }

            if (CCS_FishingRuntimeBridge.TryGetFishingService(out CCS_FishingService fishingService)
                && fishingService != null
                && fishingService.ActiveProfile != null)
            {
                fishingProfileOverride = fishingService.ActiveProfile;
            }
        }

        private void TryRegisterWithService()
        {
            if (isRegisteredWithService || !isActiveAndEnabled)
            {
                return;
            }

            if (!CCS_FishingRuntimeBridge.TryGetFishingService(out CCS_FishingService fishingService)
                || fishingService == null)
            {
                return;
            }

            fishingService.RegisterSpot(this);
            isRegisteredWithService = true;
        }

        private void UnregisterFromService()
        {
            if (!isRegisteredWithService)
            {
                return;
            }

            if (CCS_FishingRuntimeBridge.TryGetFishingService(out CCS_FishingService fishingService)
                && fishingService != null)
            {
                fishingService.UnregisterSpot(this);
            }

            isRegisteredWithService = false;
        }

        #endregion
    }
}
