using CCS.Survival;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_CraftingProfile
// CATEGORY: Modules / Crafting / Runtime / Profiles
// PURPOSE: Tuning profile for hand crafting, queueing, and craft timing rules.
// PLACEMENT: Assets/CCS/Survival/Profiles/Crafting/ (project shell configuration).
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: No UI, save, or world station references in 0.5.0 foundation.
// =============================================================================

namespace CCS.Modules.Crafting
{
    [CreateAssetMenu(
        fileName = "CCS_CraftingProfile",
        menuName = "CCS/Survival/Crafting/Crafting Profile")]
    public sealed class CCS_CraftingProfile : CCS_SurvivalProfileBase
    {
        #region Variables

        [Header("Hand Crafting")]
        [Tooltip("When enabled, Hand station recipes may be crafted without a world station.")]
        [SerializeField] private bool allowHandCrafting = true;

        [Header("Queueing (Deferred)")]
        [Tooltip("When enabled, future systems may queue timed crafts.")]
        [SerializeField] private bool allowQueueing;

        [Tooltip("Maximum queued crafts when queueing is enabled.")]
        [SerializeField] private int maxQueueSize = 1;

        [Header("Timing")]
        [Tooltip("Multiplier applied to recipe craft time seconds.")]
        [SerializeField] private float craftTimeMultiplier = 1f;

        #endregion

        #region Properties

        public bool AllowHandCrafting => allowHandCrafting;

        public bool AllowQueueing => allowQueueing;

        public int MaxQueueSize => maxQueueSize;

        public float CraftTimeMultiplier => craftTimeMultiplier;

        #endregion
    }
}
