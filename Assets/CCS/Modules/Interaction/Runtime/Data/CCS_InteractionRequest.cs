// =============================================================================
// SCRIPT: CCS_InteractionRequest
// CATEGORY: Modules / Interaction / Runtime / Data
// PURPOSE: Immutable interaction request payload from the local owner scanner.
// PLACEMENT: Built by CCS_NetworkInteractionScanner before local or server apply.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Includes origin, hit point, and range for server-side validation.
// =============================================================================

namespace CCS.Modules.Interaction
{
    public readonly struct CCS_InteractionRequest
    {
        #region Variables

        private readonly ulong requesterClientId;
        private readonly ulong targetNetworkObjectId;
        private readonly UnityEngine.Vector3 originPosition;
        private readonly UnityEngine.Vector3 hitPoint;
        private readonly float maxRange;
        private readonly string sourceLabel;

        #endregion

        #region Properties

        public ulong RequesterClientId => requesterClientId;

        public ulong TargetNetworkObjectId => targetNetworkObjectId;

        public UnityEngine.Vector3 OriginPosition => originPosition;

        public UnityEngine.Vector3 HitPoint => hitPoint;

        public float MaxRange => maxRange;

        public string SourceLabel => sourceLabel;

        #endregion

        #region Public Methods

        public CCS_InteractionRequest(
            ulong requesterClientId,
            ulong targetNetworkObjectId,
            UnityEngine.Vector3 originPosition,
            UnityEngine.Vector3 hitPoint,
            float maxRange,
            string sourceLabel = "Interact")
        {
            this.requesterClientId = requesterClientId;
            this.targetNetworkObjectId = targetNetworkObjectId;
            this.originPosition = originPosition;
            this.hitPoint = hitPoint;
            this.maxRange = maxRange;
            this.sourceLabel = sourceLabel;
        }

        #endregion
    }
}
