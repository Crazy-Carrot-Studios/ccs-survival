using CCS.Modules.Interaction;
using CCS.Modules.Shelter;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_BedrollSleepInteractable
// CATEGORY: Modules / Sleep / Runtime / Interaction
// PURPOSE: Simple rest point that requests sleep through CCS_SleepService on interact.
// PLACEMENT: Attach to bootstrap test rest placeholders such as CCS_TestBedrollRestPoint.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: Primitive placeholder only. No final bed art or sleep UI in 0.9.6.
// =============================================================================

namespace CCS.Modules.Sleep
{
    public sealed class CCS_BedrollSleepInteractable : MonoBehaviour, CCS_IInteractableResultProvider
    {
        #region Variables

        [Header("Sleep Configuration")]
        [Tooltip("Optional sleep profile reference for bootstrap validation only.")]
        [SerializeField] private CCS_SleepProfile sleepProfile;

        [Tooltip("Hours requested when interacting. Zero uses profile defaultSleepHours.")]
        [SerializeField] private float sleepHours;

        [Header("Interaction")]
        [Tooltip("Maximum distance at which this rest point accepts interaction requests.")]
        [SerializeField] private float interactionDistance = 3f;

        [Tooltip("When enabled, updates shelter subject position from this transform each frame.")]
        [SerializeField] private bool updateShelterSubjectPosition = true;

        private CCS_SleepService sleepService;

        #endregion

        #region Unity Callbacks

        private void Start()
        {
            ResolveServices();
        }

        private void Update()
        {
            if (!updateShelterSubjectPosition)
            {
                return;
            }

            if (CCS_SleepRuntimeBridge.TryGetShelterService(out CCS_ShelterService shelterService)
                && shelterService.IsInitialized)
            {
                shelterService.SetSubjectPosition(transform.position);
            }
        }

        #endregion

        #region Public Methods

        public void ConfigureRuntime(CCS_SleepProfile profile, float configuredSleepHours)
        {
            sleepProfile = profile;
            sleepHours = configuredSleepHours;
        }

        public string GetInteractionDisplayName()
        {
            return "Sleep";
        }

        public bool CanInteract()
        {
            ResolveServices();
            return sleepService != null && sleepService.IsInitialized;
        }

        public void Interact()
        {
            TryInteract();
        }

        public bool TryInteract()
        {
            ResolveServices();
            if (sleepService == null || !sleepService.IsInitialized)
            {
                return false;
            }

            CCS_SleepResult result = sleepService.TrySleep(new CCS_SleepRequest(sleepHours));
            return result.IsSuccess;
        }

        public float GetInteractionDistance()
        {
            return interactionDistance;
        }

        #endregion

        #region Private Methods

        private void ResolveServices()
        {
            if (CCS_SleepRuntimeBridge.TryGetSleepService(out CCS_SleepService registrySleepService))
            {
                sleepService = registrySleepService;
            }
        }

        #endregion
    }
}
