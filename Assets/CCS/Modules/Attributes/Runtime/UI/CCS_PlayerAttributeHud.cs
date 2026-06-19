using TMPro;
using Unity.Netcode;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_PlayerAttributeHud
// CATEGORY: Modules / Attributes / Runtime / UI
// PURPOSE: Local-owner attribute HUD for Health current/max display.
// PLACEMENT: Child canvas on networked test player prefab.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Only the local owner sees this HUD. Remote players keep nameplates only.
// =============================================================================

namespace CCS.Modules.Attributes
{
    public sealed class CCS_PlayerAttributeHud : MonoBehaviour
    {
        #region Variables

        [SerializeField] private CCS_AttributeContainer attributeContainer;

        [SerializeField] private CCS_AttributeDefinition healthDefinition;

        [SerializeField] private TMP_Text healthText;

        [SerializeField] private Canvas hudCanvas;

        #endregion

        #region Unity Callbacks

        private void Awake()
        {
            if (attributeContainer == null)
            {
                attributeContainer = GetComponentInParent<CCS_AttributeContainer>();
            }

            if (hudCanvas == null)
            {
                hudCanvas = GetComponent<Canvas>();
            }
        }

        private void OnEnable()
        {
            if (!IsLocalOwner())
            {
                if (hudCanvas != null)
                {
                    hudCanvas.enabled = false;
                }

                gameObject.SetActive(false);
                return;
            }

            if (attributeContainer != null)
            {
                attributeContainer.AttributeChanged += HandleAttributeChanged;
            }

            RefreshHealthText();
        }

        private void OnDisable()
        {
            if (attributeContainer != null)
            {
                attributeContainer.AttributeChanged -= HandleAttributeChanged;
            }
        }

        #endregion

        #region Public Methods

        public void RefreshHealthText()
        {
            if (healthText == null || attributeContainer == null)
            {
                return;
            }

            string attributeId = ResolveHealthAttributeId();
            if (!attributeContainer.TryGetValue(attributeId, out CCS_AttributeValue value))
            {
                healthText.text = $"{ResolveHealthLabel()}: -- / --";
                return;
            }

            healthText.text =
                $"{ResolveHealthLabel()}: {value.Current:0} / {value.Max:0}";
        }

        #endregion

        #region Private Methods

        private void HandleAttributeChanged(CCS_AttributeChangedEvent changedEvent)
        {
            if (!string.Equals(changedEvent.AttributeId, ResolveHealthAttributeId(), System.StringComparison.Ordinal))
            {
                return;
            }

            RefreshHealthText();
        }

        private bool IsLocalOwner()
        {
            NetworkObject networkObject = GetComponentInParent<NetworkObject>();
            if (networkObject == null || !networkObject.IsSpawned)
            {
                return true;
            }

            return networkObject.IsOwner;
        }

        private string ResolveHealthAttributeId()
        {
            if (healthDefinition != null && !string.IsNullOrWhiteSpace(healthDefinition.ProfileId))
            {
                return healthDefinition.ProfileId;
            }

            return CCS_AttributesConstants.HealthAttributeId;
        }

        private string ResolveHealthLabel()
        {
            if (healthDefinition != null && !string.IsNullOrWhiteSpace(healthDefinition.UiLabel))
            {
                return healthDefinition.UiLabel;
            }

            return CCS_AttributesConstants.HealthDisplayName;
        }

        #endregion
    }
}
