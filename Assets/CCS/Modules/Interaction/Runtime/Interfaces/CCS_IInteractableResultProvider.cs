// =============================================================================
// SCRIPT: CCS_IInteractableResultProvider
// CATEGORY: Modules / Interaction / Runtime / Interfaces
// PURPOSE: Optional interactable contract that reports interaction success or failure.
// PLACEMENT: Implemented by targets whose Interact flow can fail after validation.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Interaction service invokes TryInteract when this interface is implemented.
// =============================================================================

namespace CCS.Modules.Interaction
{
    public interface CCS_IInteractableResultProvider : CCS_IInteractable
    {
        #region Public Methods

        bool TryInteract();

        #endregion
    }
}
