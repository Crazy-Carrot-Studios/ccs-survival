// =============================================================================
// SCRIPT: CCS_AttributeChangedEvent
// CATEGORY: Modules / Attributes / Runtime / Events
// PURPOSE: Payload raised when an attribute value changes on a container.
// PLACEMENT: Dispatched by CCS_AttributeContainer after clamped writes.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Local event struct. Future cross-module dispatch may use CCS_EventDispatcher.
// =============================================================================

namespace CCS.Modules.Attributes
{
    public readonly struct CCS_AttributeChangedEvent
    {
        #region Variables

        private readonly string attributeId;
        private readonly CCS_AttributeValue previousValue;
        private readonly CCS_AttributeValue currentValue;

        #endregion

        #region Properties

        public string AttributeId => attributeId;

        public CCS_AttributeValue PreviousValue => previousValue;

        public CCS_AttributeValue CurrentValue => currentValue;

        #endregion

        #region Public Methods

        public CCS_AttributeChangedEvent(
            string attributeId,
            CCS_AttributeValue previousValue,
            CCS_AttributeValue currentValue)
        {
            this.attributeId = attributeId;
            this.previousValue = previousValue;
            this.currentValue = currentValue;
        }

        #endregion
    }
}
