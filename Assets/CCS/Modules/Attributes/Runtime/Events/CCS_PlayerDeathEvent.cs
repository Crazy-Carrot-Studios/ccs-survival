// =============================================================================
// SCRIPT: CCS_PlayerDeathEvent
// CATEGORY: Modules / Attributes / Runtime / Events
// PURPOSE: Placeholder death signal when a vital attribute reaches its minimum.
// PLACEMENT: Dispatched by CCS_AttributeContainer when Health hits min.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: No respawn flow in v0.3.0. Event exists for future gameplay hooks.
// =============================================================================

namespace CCS.Modules.Attributes
{
    public readonly struct CCS_PlayerDeathEvent
    {
        #region Variables

        private readonly string attributeId;
        private readonly CCS_AttributeValue finalValue;

        #endregion

        #region Properties

        public string AttributeId => attributeId;

        public CCS_AttributeValue FinalValue => finalValue;

        #endregion

        #region Public Methods

        public CCS_PlayerDeathEvent(string attributeId, CCS_AttributeValue finalValue)
        {
            this.attributeId = attributeId;
            this.finalValue = finalValue;
        }
    }
}
