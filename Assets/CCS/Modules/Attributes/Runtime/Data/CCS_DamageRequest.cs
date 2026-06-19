// =============================================================================
// SCRIPT: CCS_DamageRequest
// CATEGORY: Modules / Attributes / Runtime / Data
// PURPOSE: Immutable damage request payload for server-authoritative application.
// PLACEMENT: Passed into CCS_AttributeContainer.ApplyDamage.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Test and gameplay callers build requests; server validates before apply.
// =============================================================================

namespace CCS.Modules.Attributes
{
    public readonly struct CCS_DamageRequest
    {
        #region Variables

        private readonly string attributeId;
        private readonly float amount;
        private readonly string sourceLabel;

        #endregion

        #region Properties

        public string AttributeId => attributeId;

        public float Amount => amount;

        public string SourceLabel => sourceLabel;

        #endregion

        #region Public Methods

        public CCS_DamageRequest(string attributeId, float amount, string sourceLabel = "Damage")
        {
            this.attributeId = attributeId;
            this.amount = amount;
            this.sourceLabel = sourceLabel;
        }
    }
}
