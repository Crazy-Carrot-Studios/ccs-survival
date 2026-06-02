using System.Collections.Generic;
using CCS.Modules.Resources;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_WildlifeHarvestDefinition
// CATEGORY: Modules / Wildlife / Runtime / Definitions
// PURPOSE: Frontier harvest drop tables aligned with the resource framework.
// PLACEMENT: Assets/CCS/Survival/Content/Wildlife/Harvest/
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: Skin and butcher tables are combined on a successful knife harvest (1.3.2).
// =============================================================================

namespace CCS.Modules.Wildlife
{
    [CreateAssetMenu(
        fileName = "CCS_WildlifeHarvestDefinition",
        menuName = "CCS/Survival/Wildlife/Wildlife Harvest Definition")]
    public sealed class CCS_WildlifeHarvestDefinition : ScriptableObject
    {
        #region Variables

        [Header("Identity")]
        [Tooltip("Stable harvest definition ID (matches wildlifeId when paired 1:1).")]
        [SerializeField] private string harvestDefinitionId = string.Empty;

        [Tooltip("Linked wildlife definition for validation and lookup.")]
        [SerializeField] private CCS_WildlifeDefinition wildlifeDefinition;

        [Header("Resource Framework")]
        [Tooltip("Resource source classification for harvest validation.")]
        [SerializeField] private CCS_ResourceSourceType resourceSourceType = CCS_ResourceSourceType.Wildlife;

        [Header("Harvest Methods")]
        [Tooltip("Drops granted from a skinning pass (knife).")]
        [SerializeField] private List<CCS_WildlifeHarvestDropDefinition> skinDrops =
            new List<CCS_WildlifeHarvestDropDefinition>();

        [Tooltip("Drops granted from a butchering pass (knife).")]
        [SerializeField] private List<CCS_WildlifeHarvestDropDefinition> butcherDrops =
            new List<CCS_WildlifeHarvestDropDefinition>();

        #endregion

        #region Properties

        public string HarvestDefinitionId => harvestDefinitionId;

        public CCS_WildlifeDefinition WildlifeDefinition => wildlifeDefinition;

        public CCS_ResourceSourceType ResourceSourceType => resourceSourceType;

        public IReadOnlyList<CCS_WildlifeHarvestDropDefinition> SkinDrops => skinDrops;

        public IReadOnlyList<CCS_WildlifeHarvestDropDefinition> ButcherDrops => butcherDrops;

        #endregion
    }
}
