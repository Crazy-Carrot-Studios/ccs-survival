using CCS.Modules.Interaction;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_SleepSpotInteractable
// CATEGORY: Modules / Sleep / Runtime / Interactables
// PURPOSE: Interaction handoff that starts sleep at a placed bedroll through the service.
// PLACEMENT: PF_CCS_PrimitiveBedroll alongside CCS_SleepSpot.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-06-01
// NOTES: Milestone 1.1.3 foundation. No polished sleep UI.
// =============================================================================

namespace CCS.Modules.Sleep
{
    public sealed class CCS_SleepSpotInteractable : MonoBehaviour, CCS_IInteractableResultProvider
    {
        #region Variables

        [Header("Interaction")]
        [Tooltip("Maximum distance at which this bedroll accepts interaction requests.")]
        [SerializeField] private float interactionDistance = 3f;

        [Tooltip("Optional override label. Empty uses sleep spot display name.")]
        [SerializeField] private string interactionDisplayNameOverride = string.Empty;

        private CCS_SleepSpot sleepSpot;

        #endregion

        #region Unity Callbacks

        private void Awake()
        {
            sleepSpot = GetComponent<CCS_SleepSpot>();
            if (sleepSpot == null)
            {
                sleepSpot = GetComponentInParent<CCS_SleepSpot>();
            }
        }

        #endregion

        #region Public Methods

        public string GetInteractionDisplayName()
        {
            if (!string.IsNullOrWhiteSpace(interactionDisplayNameOverride))
            {
                return interactionDisplayNameOverride;
            }

            return sleepSpot != null && !string.IsNullOrWhiteSpace(sleepSpot.DisplayName)
                ? $"Sleep at {sleepSpot.DisplayName}"
                : "Sleep at Bedroll";
        }

        public float GetInteractionDistance()
        {
            return interactionDistance < 0.1f ? 3f : interactionDistance;
        }

        public bool CanInteract()
        {
            return isActiveAndEnabled && sleepSpot != null && sleepSpot.CanSleep();
        }

        public void Interact()
        {
            TryInteract();
        }

        public bool TryInteract()
        {
            if (sleepSpot == null || !sleepSpot.CanSleep())
            {
                return false;
            }

            return sleepSpot.Sleep();
        }

        #endregion
    }
}
