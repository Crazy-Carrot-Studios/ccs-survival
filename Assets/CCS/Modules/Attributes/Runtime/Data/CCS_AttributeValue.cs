// =============================================================================
// SCRIPT: CCS_AttributeValue
// CATEGORY: Modules / Attributes / Runtime / Data
// PURPOSE: Readonly snapshot of one attribute's current and bounded values.
// PLACEMENT: Returned by CCS_AttributeContainer queries.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Value struct only. Definitions remain on CCS_AttributeDefinition assets.
// =============================================================================

namespace CCS.Modules.Attributes
{
    public readonly struct CCS_AttributeValue
    {
        #region Variables

        private readonly string attributeId;
        private readonly float current;
        private readonly float min;
        private readonly float max;

        #endregion

        #region Properties

        public string AttributeId => attributeId;

        public float Current => current;

        public float Min => min;

        public float Max => max;

        public bool IsAtMin => current <= min;

        public bool IsAtMax => current >= max;

        public float Normalized => max > min ? (current - min) / (max - min) : 0f;

        #endregion

        #region Public Methods

        public CCS_AttributeValue(string attributeId, float current, float min, float max)
        {
            this.attributeId = attributeId;
            this.current = current;
            this.min = min;
            this.max = max;
        }

        public CCS_AttributeValue WithCurrent(float newCurrent)
        {
            return new CCS_AttributeValue(attributeId, newCurrent, min, max);
        }

        #endregion
    }
}
