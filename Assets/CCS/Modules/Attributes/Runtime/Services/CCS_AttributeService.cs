using UnityEngine;

// =============================================================================
// SCRIPT: CCS_AttributeService
// CATEGORY: Modules / Attributes / Runtime / Services
// PURPOSE: Read-only access surface for attribute snapshots on a bound container.
// PLACEMENT: Optional companion on player roots that already have CCS_AttributeContainer.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Thin service wrapper. Mutation stays on container/replicator authority paths.
// =============================================================================

namespace CCS.Modules.Attributes
{
    public sealed class CCS_AttributeService : MonoBehaviour
    {
        #region Variables

        [SerializeField] private CCS_AttributeContainer attributeContainer;

        #endregion

        #region Properties

        public CCS_AttributeContainer Container => attributeContainer;

        #endregion

        #region Unity Callbacks

        private void Awake()
        {
            if (attributeContainer == null)
            {
                attributeContainer = GetComponent<CCS_AttributeContainer>();
            }
        }

        #endregion

        #region Public Methods

        public bool TryGetAttribute(string attributeId, out CCS_AttributeValue value)
        {
            if (attributeContainer == null)
            {
                value = default;
                return false;
            }

            return attributeContainer.TryGetValue(attributeId, out value);
        }

        public bool TryGetHealth(out CCS_AttributeValue value)
        {
            return TryGetAttribute(CCS_AttributesConstants.HealthAttributeId, out value);
        }

        public bool TryGetStamina(out CCS_AttributeValue value)
        {
            return TryGetAttribute(CCS_AttributesConstants.StaminaAttributeId, out value);
        }

        #endregion
    }
}
