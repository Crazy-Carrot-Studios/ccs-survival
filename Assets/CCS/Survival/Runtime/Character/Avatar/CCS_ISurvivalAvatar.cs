using UnityEngine;

// =============================================================================
// SCRIPT: CCS_ISurvivalAvatar
// CATEGORY: Survival / Runtime / Character / Avatar
// PURPOSE: Contract for the physical scene representation of a survival character (body root, spawn, possession).
// PLACEMENT: Implemented by future avatar/pawn scene objects. No movement, input, or controller dependencies.
// AUTHOR: James Schilz
// CREATED: 2026-05-24
// NOTES: Avatar is scene-facing only. See CCS_SurvivalFrameworkFutureMarkers.AvatarPossessionSystems.
// =============================================================================

namespace CCS.Survival
{
    public interface CCS_ISurvivalAvatar
    {
        #region Properties

        // Stable runtime identity for this avatar instance (not GameObject name or Unity instance ID).
        string AvatarId { get; }

        // Authority that owns this avatar representation (must match bound authority AuthorityId).
        string AuthorityId { get; }

        // Scene root for body, visuals, sockets, and future animator/controller attachment.
        Transform AvatarRoot { get; }

        // True when the avatar exists in the loaded scene hierarchy.
        bool IsSpawned { get; }

        // True when an authority is actively driving this avatar representation (future possession).
        bool IsPossessed { get; }

        #endregion
    }
}
