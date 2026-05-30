// =============================================================================
// SCRIPT: CCS_IInteractable
// CATEGORY: Modules / Interaction / Runtime / Interfaces
// PURPOSE: Contract for world objects the player can detect and interact with.
// PLACEMENT: Implemented by doors, containers, stations, nodes, NPCs, quest objects, etc.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Interaction module never references inventory, crafting, equipment, save, or quests.
// =============================================================================

namespace CCS.Modules.Interaction
{
    public interface CCS_IInteractable
    {
        #region Public Methods

        string GetInteractionDisplayName();

        bool CanInteract();

        void Interact();

        float GetInteractionDistance();

        #endregion
    }
}
