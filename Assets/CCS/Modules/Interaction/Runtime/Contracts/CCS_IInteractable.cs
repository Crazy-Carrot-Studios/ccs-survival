// =============================================================================
// SCRIPT: CCS_IInteractable
// CATEGORY: Modules / Interaction / Runtime / Contracts
// PURPOSE: Contract for objects the local owner scanner can request to interact with.
// PLACEMENT: Implemented by test and future gameplay interactable components.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Server-authoritative interactables validate requests before applying state.
// =============================================================================

namespace CCS.Modules.Interaction
{
    public interface CCS_IInteractable
    {
        #region Public Methods

        bool CanInteract(CCS_InteractionRequest request);

        bool Interact(CCS_InteractionRequest request, out CCS_InteractionResult result);

        #endregion
    }
}
