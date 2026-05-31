// =============================================================================
// SCRIPT: CCS_HarvestRequest
// CATEGORY: Modules / WorldResources / Runtime / Data
// PURPOSE: Represents a harvest attempt against a resource node.
// PLACEMENT: Passed to CCS_ResourceHarvestService harvest methods.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: No UI or interaction visual references.
// =============================================================================

namespace CCS.Modules.WorldResources
{
    public sealed class CCS_HarvestRequest
    {
        #region Public Methods

        public CCS_HarvestRequest(
            CCS_ResourceDefinition resourceDefinition,
            CCS_ResourceNodeState nodeState,
            CCS_RequiredToolType equippedToolType,
            string nodeKey = "")
        {
            ResourceDefinition = resourceDefinition;
            NodeState = nodeState;
            EquippedToolType = equippedToolType;
            NodeKey = nodeKey ?? string.Empty;
        }

        #endregion

        #region Properties

        public CCS_ResourceDefinition ResourceDefinition { get; }

        public CCS_ResourceNodeState NodeState { get; }

        public CCS_RequiredToolType EquippedToolType { get; }

        public string NodeKey { get; }

        #endregion
    }
}
