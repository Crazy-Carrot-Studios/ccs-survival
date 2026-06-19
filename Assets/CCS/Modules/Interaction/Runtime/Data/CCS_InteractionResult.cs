// =============================================================================
// SCRIPT: CCS_InteractionResult
// CATEGORY: Modules / Interaction / Runtime / Data
// PURPOSE: Immutable interaction outcome returned by interactable targets.
// PLACEMENT: Returned from CCS_IInteractable.Interact.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Used for local feedback and replicated completion events.
// =============================================================================

namespace CCS.Modules.Interaction
{
    public readonly struct CCS_InteractionResult
    {
        #region Variables

        private readonly bool succeeded;
        private readonly string message;
        private readonly ulong targetNetworkObjectId;

        #endregion

        #region Properties

        public bool Succeeded => succeeded;

        public string Message => message;

        public ulong TargetNetworkObjectId => targetNetworkObjectId;

        #endregion

        #region Public Methods

        public CCS_InteractionResult(bool succeeded, string message, ulong targetNetworkObjectId)
        {
            this.succeeded = succeeded;
            this.message = message;
            this.targetNetworkObjectId = targetNetworkObjectId;
        }

        public static CCS_InteractionResult Success(ulong targetNetworkObjectId, string message = "Interaction succeeded.")
        {
            return new CCS_InteractionResult(true, message, targetNetworkObjectId);
        }

        public static CCS_InteractionResult Failure(ulong targetNetworkObjectId, string message)
        {
            return new CCS_InteractionResult(false, message, targetNetworkObjectId);
        }

        #endregion
    }
}
