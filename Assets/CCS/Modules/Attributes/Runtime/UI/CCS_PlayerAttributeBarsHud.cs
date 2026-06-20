using TMPro;
using Unity.Netcode;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_PlayerAttributeBarsHud
// CATEGORY: Modules / Attributes / Runtime / UI
// PURPOSE: Local-owner gameplay attribute bar HUD for Master Test.
// PLACEMENT: Child canvas on networked test player prefab.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Reads Health and Stamina from CCS_AttributeContainer snapshots only.
// =============================================================================

namespace CCS.Modules.Attributes
{
    public sealed class CCS_PlayerAttributeBarsHud : MonoBehaviour
    {
        #region Variables

        [SerializeField] private CCS_AttributeContainer attributeContainer;

        [SerializeField] private CCS_AttributeDefinition healthDefinition;

        [SerializeField] private CCS_AttributeDefinition staminaDefinition;

        [SerializeField] private Canvas hudCanvas;

        [SerializeField] private CCS_AttributeBarView healthBar;

        [SerializeField] private CCS_AttributeBarView staminaBar;

        [SerializeField] private CCS_AttributeBarView hungerBar;

        [SerializeField] private CCS_AttributeBarView thirstBar;

        [SerializeField] private CCS_StaminaController staminaController;

        private bool isLocalOwnerActive;

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

            if (staminaController == null)
            {
                staminaController = GetComponentInParent<CCS_StaminaController>();
            }
        }

        private void OnEnable()
        {
            isLocalOwnerActive = IsLocalOwner();
            if (!isLocalOwnerActive)
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

            RefreshAllBars();
        }

        private void OnDisable()
        {
            if (attributeContainer != null)
            {
                attributeContainer.AttributeChanged -= HandleAttributeChanged;
            }

            isLocalOwnerActive = false;
        }

        #endregion

        #region Public Methods

        public void RefreshAllBars()
        {
            RefreshHealthBar();
            RefreshStaminaBar();
            RefreshPlaceholderBars();
        }

        #endregion

        #region Private Methods

        private void HandleAttributeChanged(CCS_AttributeChangedEvent changedEvent)
        {
            if (string.Equals(changedEvent.AttributeId, ResolveHealthAttributeId(), System.StringComparison.Ordinal))
            {
                RefreshHealthBar();
                return;
            }

            if (string.Equals(changedEvent.AttributeId, ResolveStaminaAttributeId(), System.StringComparison.Ordinal))
            {
                RefreshStaminaBar();
            }
        }

        private void RefreshHealthBar()
        {
            if (healthBar == null)
            {
                return;
            }

            string attributeId = ResolveHealthAttributeId();
            if (attributeContainer == null
                || !attributeContainer.TryGetValue(attributeId, out CCS_AttributeValue value))
            {
                healthBar.SetValues(ResolveHealthLabel(), 0f, 0f);
                return;
            }

            healthBar.SetValues(ResolveHealthLabel(), value.Current, value.Max);
        }

        private void RefreshStaminaBar()
        {
            if (staminaBar == null)
            {
                return;
            }

            string attributeId = ResolveStaminaAttributeId();
            if (attributeContainer == null
                || !attributeContainer.TryGetValue(attributeId, out CCS_AttributeValue value))
            {
                staminaBar.SetValues(
                    CCS_AttributeBarsHudStyle.StaminaBarLabel,
                    CCS_AttributesConstants.StaminaDefaultMax,
                    CCS_AttributesConstants.StaminaDefaultMax,
                    ResolveStaminaStatusSuffix());
                return;
            }

            staminaBar.SetValues(
                CCS_AttributeBarsHudStyle.StaminaBarLabel,
                value.Current,
                value.Max,
                ResolveStaminaStatusSuffix());
        }

        private string ResolveStaminaStatusSuffix()
        {
            if (staminaController == null)
            {
                return null;
            }

            if (staminaController.IsExhausted)
            {
                return CCS_AttributesConstants.StaminaExhaustedStatusText;
            }

            if (staminaController.IsSprintLocked)
            {
                return CCS_AttributesConstants.StaminaRecoveringStatusText;
            }

            return null;
        }

        private void RefreshPlaceholderBars()
        {
            if (hungerBar != null)
            {
                hungerBar.SetValues(
                    CCS_AttributeBarsHudStyle.HungerBarLabel,
                    CCS_AttributeBarsHudStyle.PlaceholderMax,
                    CCS_AttributeBarsHudStyle.PlaceholderMax,
                    CCS_AttributeBarsHudStyle.PlaceholderStatusSuffix);
            }

            if (thirstBar != null)
            {
                thirstBar.SetValues(
                    CCS_AttributeBarsHudStyle.ThirstBarLabel,
                    CCS_AttributeBarsHudStyle.PlaceholderMax,
                    CCS_AttributeBarsHudStyle.PlaceholderMax,
                    CCS_AttributeBarsHudStyle.PlaceholderStatusSuffix);
            }
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

        private string ResolveStaminaAttributeId()
        {
            if (staminaDefinition != null && !string.IsNullOrWhiteSpace(staminaDefinition.ProfileId))
            {
                return staminaDefinition.ProfileId;
            }

            return CCS_AttributesConstants.StaminaAttributeId;
        }

        private string ResolveHealthLabel()
        {
            if (healthDefinition != null && !string.IsNullOrWhiteSpace(healthDefinition.UiLabel))
            {
                return healthDefinition.UiLabel.ToUpperInvariant();
            }

            return CCS_AttributesConstants.HealthDisplayName.ToUpperInvariant();
        }

        #endregion
    }
}
