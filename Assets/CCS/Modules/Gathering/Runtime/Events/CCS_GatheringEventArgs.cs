using System.Collections.Generic;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_GatheringEventArgs
// CATEGORY: Modules / Gathering / Runtime / Events
// PURPOSE: Event payload for gathering node gathered, depleted, and respawned notifications.
// PLACEMENT: Raised by CCS_GatheringService after node state changes.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-06-01
// NOTES: Includes node reference, node type, rewards, and world position.
// =============================================================================

namespace CCS.Modules.Gathering
{
    public sealed class CCS_GatheringEventArgs
    {
        #region Variables

        private readonly CCS_GatheringNode node;
        private readonly CCS_GatheringNodeType nodeType;
        private readonly IReadOnlyList<CCS_GatheringReward> rewards;
        private readonly Vector3 worldPosition;
        private readonly CCS_GatheringResult result;

        #endregion

        #region Properties

        public CCS_GatheringNode Node => node;

        public CCS_GatheringNodeType NodeType => nodeType;

        public IReadOnlyList<CCS_GatheringReward> Rewards => rewards;

        public Vector3 WorldPosition => worldPosition;

        public CCS_GatheringResult Result => result;

        #endregion

        #region Public Methods

        public CCS_GatheringEventArgs(CCS_GatheringResult gatheringResult)
        {
            result = gatheringResult;
            node = gatheringResult?.SourceNode;
            nodeType = gatheringResult != null ? gatheringResult.NodeType : CCS_GatheringNodeType.None;
            rewards = gatheringResult?.Rewards;
            worldPosition = node != null ? node.transform.position : Vector3.zero;
        }

        #endregion
    }
}
