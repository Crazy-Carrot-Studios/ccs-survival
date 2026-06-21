// =============================================================================
// SCRIPT: CCS_InteractionKind
// CATEGORY: Modules / Interaction / Runtime / Data
// PURPOSE: Supported right-hand interaction categories for the Interaction module.
// PLACEMENT: Runtime enum. Referenced by CCS_InteractableDefinition.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Pickup and WalkThroughDoor only. No left-hand or blend variants yet.
// =============================================================================

namespace CCS.Modules.Interaction
{
    public enum CCS_InteractionKind
    {
        Pickup = 0,
        WalkThroughDoor = 1
    }
}
