// =============================================================================
// SCRIPT: CCS_AIBanditState
// CATEGORY: Modules / AI / Runtime / Data
// PURPOSE: Canonical state machine states for CCS bandit AI behavior.
// PLACEMENT: Shared enum used by brain/controller/validation.
// AUTHOR: James Schilz
// CREATED: 2026-06-25
// NOTES: v0.7.0 network AI bandit combat foundation.
// =============================================================================

namespace CCS.Modules.AI
{
    public enum CCS_AIBanditState
    {
        Idle = 0,
        AcquireTarget = 1,
        MoveToRange = 2,
        DrawWeapon = 3,
        Aim = 4,
        Fire = 5,
        Cooldown = 6,
        Dead = 7,
    }
}
