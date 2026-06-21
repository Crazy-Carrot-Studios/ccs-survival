// =============================================================================
// SCRIPT: CCS_InteractionResult
// CATEGORY: Modules / Interaction / Runtime / Data
// PURPOSE: Immutable interaction outcome returned by interactable targets.
// PLACEMENT: Returned from CCS_IInteractable.Interact.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Includes animationKey for local-owner visual routing.
// =============================================================================

namespace CCS.Modules.Interaction
{
    public readonly struct CCS_InteractionResult
    {
        #region Variables

        private readonly bool succeeded;
        private readonly string message;
        private readonly ulong targetNetworkObjectId;
        private readonly CCS_InteractionAnimationKey animationKey;

        #endregion

        #region Properties

        public bool Succeeded => succeeded;

        public string Message => message;

        public ulong TargetNetworkObjectId => targetNetworkObjectId;

        public CCS_InteractionAnimationKey AnimationKey => animationKey;

        #endregion

        #region Public Methods

        public CCS_InteractionResult(
            bool succeeded,
            string message,
            ulong targetNetworkObjectId,
            CCS_InteractionAnimationKey animationKey)
        {
            this.succeeded = succeeded;
            this.message = message;
            this.targetNetworkObjectId = targetNetworkObjectId;
            this.animationKey = animationKey;
        }

        public static CCS_InteractionResult Success(
            ulong targetNetworkObjectId,
            CCS_InteractionAnimationKey animationKey,
            string message = "Interaction succeeded.")
        {
            return new CCS_InteractionResult(true, message, targetNetworkObjectId, animationKey);
        }

        public static CCS_InteractionResult Failure(ulong targetNetworkObjectId, string message)
        {
            return new CCS_InteractionResult(
                false,
                message,
                targetNetworkObjectId,
                CCS_InteractionAnimationKey.PickUp_RH);
        }

        #endregion
    }
}
