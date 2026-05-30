using UnityEngine;

// =============================================================================
// SCRIPT: CCS_InteractableBase
// CATEGORY: Modules / Interaction / Runtime / Interaction
// PURPOSE: Reusable MonoBehaviour base for future interactable world objects.
// PLACEMENT: Attach to doors, chests, workbenches, resource nodes, NPCs, quest boards, etc.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Target decides what Interact() does. No UI or gameplay module references here.
// =============================================================================

namespace CCS.Modules.Interaction
{
    public abstract class CCS_InteractableBase : MonoBehaviour, CCS_IInteractable
    {
        #region Variables

        [Header("Interaction")]
        [Tooltip("Prompt label shown when this object is focused (future UI reads via IInteractable).")]
        [SerializeField] private string interactionDisplayName = "Interact";

        [Tooltip("Maximum distance at which this object accepts interaction requests.")]
        [SerializeField] private float interactionDistance = 3f;

        [Tooltip("When false, CanInteract returns false regardless of distance.")]
        [SerializeField] private bool interactionEnabled = true;

        #endregion

        #region Public Methods

        public virtual string GetInteractionDisplayName()
        {
            return interactionDisplayName;
        }

        public virtual bool CanInteract()
        {
            return interactionEnabled;
        }

        public virtual void Interact()
        {
            OnInteract();
        }

        public virtual float GetInteractionDistance()
        {
            return interactionDistance;
        }

        #endregion

        #region Protected Methods

        protected virtual void OnInteract()
        {
        }

        #endregion
    }
}
