// =============================================================================
// SCRIPT: CCS_InteractionCompletedEvent
// CATEGORY: Modules / Interaction / Runtime / Events
// PURPOSE: Payload raised after an interaction attempt completes on the scanner.
// PLACEMENT: Dispatched by CCS_NetworkInteractionScanner after local/server apply.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Local event struct for test feedback and future UI hooks.
// =============================================================================

namespace CCS.Modules.Interaction
{
    public readonly struct CCS_InteractionCompletedEvent
    {
        #region Variables

        private readonly CCS_InteractionRequest request;
        private readonly CCS_InteractionResult result;

        #endregion

        #region Properties

        public CCS_InteractionRequest Request => request;

        public CCS_InteractionResult Result => result;

        #endregion

        #region Public Methods

        public CCS_InteractionCompletedEvent(CCS_InteractionRequest request, CCS_InteractionResult result)
        {
            this.request = request;
            this.result = result;
        }

        #endregion
    }
}
