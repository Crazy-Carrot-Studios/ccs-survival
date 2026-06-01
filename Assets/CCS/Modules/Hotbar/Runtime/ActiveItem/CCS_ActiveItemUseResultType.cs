// =============================================================================
// SCRIPT: CCS_ActiveItemUseResultType
// CATEGORY: Modules / Hotbar / Runtime / ActiveItem
// PURPOSE: Outcome codes for active item use attempts.
// PLACEMENT: Returned by CCS_ActiveItemService.TryUseActiveItem.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: Non-error outcomes use NoBehaviorRegistered or NoTarget instead of exceptions.
// =============================================================================

namespace CCS.Modules.Hotbar
{
    public enum CCS_ActiveItemUseResultType
    {
        None = 0,
        Success = 1,
        NoActiveItem = 2,
        NoBehaviorRegistered = 3,
        ServiceUnavailable = 4,
        OnCooldown = 5,
        NoTarget = 6,
        WeaponNotEquipped = 7,
        CombatHit = 8,
        CombatMiss = 9
    }
}
