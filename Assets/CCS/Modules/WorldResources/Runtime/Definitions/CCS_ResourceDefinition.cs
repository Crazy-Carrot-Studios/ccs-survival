using System.Collections.Generic;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_ResourceDefinition
// CATEGORY: Modules / WorldResources / Runtime / Definitions
// PURPOSE: ScriptableObject identity and harvest rules for world resource nodes.
// PLACEMENT: Create assets under project content folders.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: No final art, terrain, save, or UI references in 0.5.1 foundation.
// =============================================================================

namespace CCS.Modules.WorldResources
{
    [CreateAssetMenu(
        fileName = "CCS_ResourceDefinition",
        menuName = "CCS/Survival/World Resources/Resource Definition")]
    public sealed class CCS_ResourceDefinition : ScriptableObject
    {
        #region Variables

        [Header("Identity")]
        [Tooltip("Stable reverse-DNS resource ID for save and runtime identity.")]
        [SerializeField] private string resourceId = string.Empty;

        [Tooltip("Player-facing resource name.")]
        [SerializeField] private string displayName = string.Empty;

        [Header("Classification")]
        [Tooltip("High-level node classification.")]
        [SerializeField] private CCS_ResourceNodeType nodeType = CCS_ResourceNodeType.Gatherable;

        [Header("Harvest Rules")]
        [Tooltip("Number of successful harvests before the node is depleted.")]
        [SerializeField] private int maxHarvestCount = 1;

        [Tooltip("Base respawn duration in seconds before profile multipliers.")]
        [SerializeField] private float respawnTimeSeconds = 60f;

        [Tooltip("Tool type required to harvest this resource.")]
        [SerializeField] private CCS_RequiredToolType requiredToolType = CCS_RequiredToolType.None;

        [Header("Drops")]
        [Tooltip("Items granted when a harvest succeeds.")]
        [SerializeField] private List<CCS_ResourceDropDefinition> dropDefinitions =
            new List<CCS_ResourceDropDefinition>();

        #endregion

        #region Properties

        public string ResourceId => resourceId;

        public string DisplayName => displayName;

        public CCS_ResourceNodeType NodeType => nodeType;

        public int MaxHarvestCount => maxHarvestCount;

        public float RespawnTimeSeconds => respawnTimeSeconds;

        public CCS_RequiredToolType RequiredToolType => requiredToolType;

        public IReadOnlyList<CCS_ResourceDropDefinition> DropDefinitions => dropDefinitions;

        #endregion
    }
}
