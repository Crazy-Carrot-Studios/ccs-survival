// =============================================================================
// SCRIPT: CCS_HealRequest
// CATEGORY: Modules / Attributes / Runtime / Data
// PURPOSE: Immutable heal request payload for server-authoritative application.
// PLACEMENT: Passed into CCS_AttributeContainer.ApplyHeal.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Mirrors CCS_DamageRequest for future healing systems.
// =============================================================================

namespace CCS.Modules.Attributes
{
    public readonly struct CCS_HealRequest
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

        public CCS_HealRequest(string attributeId, float amount, string sourceLabel = "Heal")
        {
            this.attributeId = attributeId;
            this.amount = amount;
            this.sourceLabel = sourceLabel;
        }
    }
}
