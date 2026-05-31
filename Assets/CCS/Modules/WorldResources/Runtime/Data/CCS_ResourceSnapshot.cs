// =============================================================================
// SCRIPT: CCS_ResourceSnapshot
// CATEGORY: Modules / WorldResources / Runtime / Data
// PURPOSE: Read-only resource node snapshot for queries and future save hooks.
// PLACEMENT: Returned by harvestable components and harvest services.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Immutable after creation. No save serialization in 0.5.1.
// =============================================================================

namespace CCS.Modules.WorldResources
{
    public sealed class CCS_ResourceSnapshot
    {
        #region Public Methods

        public CCS_ResourceSnapshot(
            CCS_ResourceDefinition resourceDefinition,
            CCS_ResourceNodeState nodeState,
            string nodeKey)
        {
            ResourceDefinition = resourceDefinition;
            NodeState = nodeState;
            NodeKey = nodeKey ?? string.Empty;
        }

        #endregion

        #region Properties

        public CCS_ResourceDefinition ResourceDefinition { get; }

        public CCS_ResourceNodeState NodeState { get; }

        public string NodeKey { get; }

        public int RemainingHarvests => NodeState != null ? NodeState.RemainingHarvests : 0;

        public bool IsDepleted => NodeState != null && NodeState.IsDepleted;

        public float RespawnRemainingSeconds => NodeState != null ? NodeState.RespawnRemainingSeconds : 0f;

        #endregion
    }
}
