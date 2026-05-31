using System.Collections.Generic;

// =============================================================================
// SCRIPT: CCS_ResourceEventArgs
// CATEGORY: Modules / WorldResources / Runtime / Events
// PURPOSE: Event payload for harvest, depletion, and respawn notifications.
// PLACEMENT: Passed to world resource service event subscribers.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: No UI, save, or interaction visual data in 0.5.1.
// =============================================================================

namespace CCS.Modules.WorldResources
{
    public sealed class CCS_ResourceEventArgs
    {
        #region Public Methods

        public CCS_ResourceEventArgs(
            CCS_ResourceDefinition resourceDefinition,
            CCS_ResourceNodeState nodeState,
            string nodeKey = "",
            IReadOnlyList<CCS_HarvestedItemDrop> drops = null,
            string message = "")
        {
            ResourceDefinition = resourceDefinition;
            NodeState = nodeState;
            NodeKey = nodeKey ?? string.Empty;
            Drops = drops ?? System.Array.Empty<CCS_HarvestedItemDrop>();
            Message = message ?? string.Empty;
        }

        #endregion

        #region Properties

        public CCS_ResourceDefinition ResourceDefinition { get; }

        public CCS_ResourceNodeState NodeState { get; }

        public string NodeKey { get; }

        public IReadOnlyList<CCS_HarvestedItemDrop> Drops { get; }

        public string Message { get; }

        #endregion
    }
}
