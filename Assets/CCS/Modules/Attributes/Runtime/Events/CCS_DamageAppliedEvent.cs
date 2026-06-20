// =============================================================================
// SCRIPT: CCS_DamageAppliedEvent
// CATEGORY: Modules / Attributes / Runtime / Events
// PURPOSE: Payload raised after damage is applied to an attribute.
// PLACEMENT: Dispatched by CCS_AttributeContainer.ApplyDamage.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Includes requested and applied amounts for audit and UI feedback.
// =============================================================================

namespace CCS.Modules.Attributes
{
    public readonly struct CCS_DamageAppliedEvent
    {
        #region Variables

        private readonly string attributeId;
        private readonly float requestedAmount;
        private readonly float appliedAmount;
        private readonly CCS_AttributeValue resultingValue;

        #endregion

        #region Properties

        public string AttributeId => attributeId;

        public float RequestedAmount => requestedAmount;

        public float AppliedAmount => appliedAmount;

        public CCS_AttributeValue ResultingValue => resultingValue;

        #endregion

        #region Public Methods

        public CCS_DamageAppliedEvent(
            string attributeId,
            float requestedAmount,
            float appliedAmount,
            CCS_AttributeValue resultingValue)
        {
            this.attributeId = attributeId;
            this.requestedAmount = requestedAmount;
            this.appliedAmount = appliedAmount;
            this.resultingValue = resultingValue;
        }

        #endregion
    }
}
