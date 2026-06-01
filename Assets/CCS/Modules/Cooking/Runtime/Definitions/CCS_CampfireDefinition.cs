using UnityEngine;

// =============================================================================
// SCRIPT: CCS_CampfireDefinition
// CATEGORY: Modules / Cooking / Runtime / Definitions
// PURPOSE: ScriptableObject rules for campfire interactables and cooking timing.
// PLACEMENT: Assets/CCS/Survival/Content/Cooking/Definitions/
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: No fuel system in 0.9.4 foundation.
// =============================================================================

namespace CCS.Modules.Cooking
{
    [CreateAssetMenu(
        fileName = "CCS_CampfireDefinition",
        menuName = "CCS/Survival/Cooking/Campfire Definition")]
    public sealed class CCS_CampfireDefinition : ScriptableObject
    {
        #region Variables

        [Header("Identity")]
        [Tooltip("Stable reverse-DNS campfire ID for save and runtime identity.")]
        [SerializeField] private string campfireId = string.Empty;

        [Tooltip("Player-facing campfire name.")]
        [SerializeField] private string displayName = string.Empty;

        [Header("Cooking")]
        [Tooltip("Seconds required to cook one item on this campfire.")]
        [SerializeField] private float cookTimeSeconds = 5f;

        [Tooltip("When true, newly placed campfires start lit.")]
        [SerializeField] private bool isLitOnPlacement;

        [Tooltip("Placeholder for future multi-item cook queues.")]
        [SerializeField] private int maxQueueCount = 1;

        #endregion

        #region Properties

        public string CampfireId => campfireId;

        public string DisplayName => displayName;

        public float CookTimeSeconds => cookTimeSeconds;

        public bool IsLitOnPlacement => isLitOnPlacement;

        public int MaxQueueCount => maxQueueCount;

        #endregion
    }
}
