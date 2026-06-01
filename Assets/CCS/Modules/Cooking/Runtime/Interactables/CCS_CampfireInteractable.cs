using CCS.Modules.Interaction;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_CampfireInteractable
// CATEGORY: Modules / Cooking / Runtime / Interactables
// PURPOSE: Interactable MonoBehaviour wrapper for campfire light and cook actions.
// PLACEMENT: Attach to campfire placeholders in bootstrap verification scenes.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: Unlit, Lit, and Cooking states only. No fuel system in 0.9.4 foundation.
// =============================================================================

namespace CCS.Modules.Cooking
{
    public sealed class CCS_CampfireInteractable : MonoBehaviour, CCS_IInteractableResultProvider
    {
        #region Variables

        [Header("Campfire Configuration")]
        [Tooltip("Campfire definition that drives cook timing and identity.")]
        [SerializeField] private CCS_CampfireDefinition campfireDefinition;

        [Tooltip("Optional cooking profile used when registry services are unavailable.")]
        [SerializeField] private CCS_CookingProfile cookingProfile;

        [Header("Interaction")]
        [Tooltip("Maximum distance at which this campfire accepts interaction requests.")]
        [SerializeField] private float interactionDistance = 3f;

        [Tooltip("When enabled, the campfire registers as lit on startup.")]
        [SerializeField] private bool assumeLitOnStart;

        private string instanceKey;
        private CCS_CampfireService campfireService;
        private bool servicesResolved;

        #endregion

        #region Properties

        public CCS_CampfireDefinition CampfireDefinition => campfireDefinition;

        public string InstanceKey => instanceKey;

        #endregion

        #region Unity Callbacks

        private void Awake()
        {
            if (string.IsNullOrWhiteSpace(instanceKey))
            {
                instanceKey = gameObject.name + "_" + GetEntityId();
            }
        }

        private void Start()
        {
            ResolveServices();
            RegisterCampfireIfNeeded();
        }

        #endregion

        #region Public Methods

        public void ConfigureRuntime(
            CCS_CampfireDefinition definition,
            CCS_CookingProfile profile,
            string configuredInstanceKey,
            bool assumeLitOnStartOverride = false)
        {
            campfireDefinition = definition;
            cookingProfile = profile;
            instanceKey = configuredInstanceKey ?? string.Empty;
            assumeLitOnStart = assumeLitOnStartOverride;
        }

        public string GetInteractionDisplayName()
        {
            if (!TryGetCurrentState(out CCS_CampfireState campfireState))
            {
                return "Campfire";
            }

            switch (campfireState)
            {
                case CCS_CampfireState.Unlit:
                    return "Light Campfire";
                case CCS_CampfireState.Lit:
                    return "Cook Meat";
                case CCS_CampfireState.Cooking:
                    return "Cooking...";
                default:
                    return campfireDefinition != null && !string.IsNullOrWhiteSpace(campfireDefinition.DisplayName)
                        ? campfireDefinition.DisplayName
                        : "Campfire";
            }
        }

        public bool CanInteract()
        {
            if (campfireDefinition == null || campfireService == null)
            {
                return false;
            }

            if (!TryGetCurrentState(out CCS_CampfireState campfireState))
            {
                return false;
            }

            return campfireState != CCS_CampfireState.Cooking
                && campfireState != CCS_CampfireState.BurnedOut;
        }

        public void Interact()
        {
            TryInteract();
        }

        public bool TryInteract()
        {
            if (campfireService == null || campfireDefinition == null)
            {
                return false;
            }

            if (!TryGetCurrentState(out CCS_CampfireState campfireState))
            {
                return false;
            }

            if (campfireState == CCS_CampfireState.Unlit)
            {
                return campfireService.TryLightCampfire(instanceKey);
            }

            if (campfireState == CCS_CampfireState.Lit)
            {
                CCS_CookingResult result = campfireService.TryCookMeatAtCampfire(instanceKey);
                return result.IsSuccess;
            }

            return false;
        }

        public float GetInteractionDistance()
        {
            return interactionDistance;
        }

        public CCS_CampfireSnapshot GetSnapshot()
        {
            if (campfireService == null)
            {
                return new CCS_CampfireSnapshot(campfireDefinition, CCS_CampfireState.Unlit, instanceKey);
            }

            return campfireService.GetSnapshot(instanceKey);
        }

        #endregion

        #region Private Methods

        private void ResolveServices()
        {
            if (servicesResolved)
            {
                return;
            }

            if (CCS_CookingRuntimeBridge.TryGetCampfireService(out CCS_CampfireService registryCampfireService))
            {
                campfireService = registryCampfireService;
            }

            servicesResolved = campfireService != null;
        }

        private void RegisterCampfireIfNeeded()
        {
            if (campfireService == null || campfireDefinition == null)
            {
                return;
            }

            if (campfireService.TryGetCampfireState(instanceKey, out _))
            {
                return;
            }

            campfireService.RegisterCampfire(instanceKey, campfireDefinition, assumeLitOnStart);
        }

        private bool TryGetCurrentState(out CCS_CampfireState campfireState)
        {
            campfireState = CCS_CampfireState.Unlit;

            if (campfireService == null)
            {
                return false;
            }

            return campfireService.TryGetCampfireState(instanceKey, out campfireState);
        }

        #endregion
    }
}
