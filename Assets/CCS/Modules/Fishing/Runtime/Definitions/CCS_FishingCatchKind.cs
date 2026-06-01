// =============================================================================
// SCRIPT: CCS_FishingCatchKind
// CATEGORY: Modules / Fishing / Runtime / Definitions
// PURPOSE: Categories for weighted catch table rolls.
// PLACEMENT: Used by CCS_FishingCatchDefinition entries on spots and profiles.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: Nothing entries grant no inventory item but still consume a roll slot.
// =============================================================================

namespace CCS.Modules.Fishing
{
    public enum CCS_FishingCatchKind
    {
        Fish = 0,
        SmallFish = 1,
        Junk = 2,
        Nothing = 3
    }
}
