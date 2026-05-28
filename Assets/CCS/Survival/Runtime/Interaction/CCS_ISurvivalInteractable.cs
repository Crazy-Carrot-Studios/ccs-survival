using UnityEngine;

// =============================================================================
// SCRIPT: CCS_ISurvivalInteractable
// CATEGORY: Survival / Runtime / Interaction
// PURPOSE: Contract for world objects the player can interact with.
// PLACEMENT: Implement on pickup stations, doors, containers, and other interactables.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Scanner resolves targets by collider; keep prompts short for debug overlay use.
// =============================================================================

namespace CCS.Survival.Interaction
{
    public interface CCS_ISurvivalInteractable
    {
        string InteractionPrompt { get; }

        bool CanInteract(GameObject interactor);

        void Interact(GameObject interactor);
    }
}
