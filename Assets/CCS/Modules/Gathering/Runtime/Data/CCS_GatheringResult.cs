using System.Collections.Generic;

// =============================================================================
// SCRIPT: CCS_GatheringResult
// CATEGORY: Modules / Gathering / Runtime / Data
// PURPOSE: Outcome payload for CCS_GatheringService.TryGatherNode attempts.
// PLACEMENT: Returned by gathering service; referenced by CCS_GatheringEventArgs.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-06-01
// NOTES: No exceptions for expected validation failures.
// =============================================================================

namespace CCS.Modules.Gathering
{
    public sealed class CCS_GatheringResult
    {
        #region Variables

        private readonly bool didGather;
        private readonly string message;
        private readonly CCS_GatheringNode sourceNode;
        private readonly CCS_GatheringNodeType nodeType;
        private readonly IReadOnlyList<CCS_GatheringReward> rewards;

        #endregion

        #region Properties

        public bool DidGather => didGather;

        public string Message => message;

        public CCS_GatheringNode SourceNode => sourceNode;

        public CCS_GatheringNodeType NodeType => nodeType;

        public IReadOnlyList<CCS_GatheringReward> Rewards => rewards;

        #endregion

        #region Public Methods

        public static CCS_GatheringResult Success(
            CCS_GatheringNode node,
            CCS_GatheringNodeType gatheredNodeType,
            IReadOnlyList<CCS_GatheringReward> grantedRewards,
            string successMessage)
        {
            return new CCS_GatheringResult(true, successMessage, node, gatheredNodeType, grantedRewards);
        }

        public static CCS_GatheringResult Failure(string failureMessage, CCS_GatheringNode node = null)
        {
            return new CCS_GatheringResult(
                false,
                failureMessage,
                node,
                node != null ? node.NodeType : CCS_GatheringNodeType.None,
                null);
        }

        #endregion

        #region Private Methods

        private CCS_GatheringResult(
            bool gathered,
            string resultMessage,
            CCS_GatheringNode node,
            CCS_GatheringNodeType gatheredNodeType,
            IReadOnlyList<CCS_GatheringReward> grantedRewards)
        {
            didGather = gathered;
            message = resultMessage ?? string.Empty;
            sourceNode = node;
            nodeType = gatheredNodeType;
            rewards = grantedRewards;
        }

        #endregion
    }
}
