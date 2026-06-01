using System.Collections.Generic;
using CCS.Modules.WorldResources;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_WildlifeDefinition
// CATEGORY: Modules / Wildlife / Runtime / Definitions
// PURPOSE: ScriptableObject identity and harvest rules for wildlife resource placeholders.
// PLACEMENT: Assets/CCS/Survival/Content/Wildlife/Definitions/
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: Carcass-only foundation. No live AI, combat, or spawning in 0.9.3.
// =============================================================================

namespace CCS.Modules.Wildlife
{
    [CreateAssetMenu(
        fileName = "CCS_WildlifeDefinition",
        menuName = "CCS/Survival/Wildlife/Wildlife Definition")]
    public sealed class CCS_WildlifeDefinition : ScriptableObject
    {
        #region Variables

        [Header("Identity")]
        [Tooltip("Stable reverse-DNS wildlife ID for save and runtime identity.")]
        [SerializeField] private string wildlifeId = string.Empty;

        [Tooltip("Player-facing wildlife or carcass name.")]
        [SerializeField] private string displayName = string.Empty;

        [Header("Classification")]
        [Tooltip("High-level wildlife classification.")]
        [SerializeField] private CCS_WildlifeType wildlifeType = CCS_WildlifeType.SmallGame;

        [Header("Harvest Rules")]
        [Tooltip("Tool type required to harvest this wildlife carcass.")]
        [SerializeField] private CCS_RequiredToolType harvestToolRequirement = CCS_RequiredToolType.Knife;

        [Tooltip("Number of successful harvests before the carcass is depleted.")]
        [SerializeField] private int maxHarvestCount = 1;

        [Tooltip("Placeholder respawn duration in seconds for future systems.")]
        [SerializeField] private float respawnTimeSeconds;

        [Tooltip("Placeholder for future aggressive wildlife behavior.")]
        [SerializeField] private bool isAggressive;

        [Header("Drops")]
        [Tooltip("Items granted when a wildlife harvest succeeds.")]
        [SerializeField] private List<CCS_WildlifeHarvestDropDefinition> harvestDrops =
            new List<CCS_WildlifeHarvestDropDefinition>();

        #endregion

        #region Properties

        public string WildlifeId => wildlifeId;

        public string DisplayName => displayName;

        public CCS_WildlifeType WildlifeType => wildlifeType;

        public CCS_RequiredToolType HarvestToolRequirement => harvestToolRequirement;

        public int MaxHarvestCount => maxHarvestCount;

        public float RespawnTimeSeconds => respawnTimeSeconds;

        public bool IsAggressive => isAggressive;

        public IReadOnlyList<CCS_WildlifeHarvestDropDefinition> HarvestDrops => harvestDrops;

        #endregion
    }
}
