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
// NOTES: Health reads from CCS_AttributeContainer. Stamina is local-only placeholder.
// =============================================================================

namespace CCS.Modules.Attributes
{
    public sealed class CCS_PlayerAttributeBarsHud : MonoBehaviour
    {
        #region Variables

        [SerializeField] private CCS_AttributeContainer attributeContainer;

        [SerializeField] private CCS_AttributeDefinition healthDefinition;

        [SerializeField] private Canvas hudCanvas;

        [SerializeField] private CCS_AttributeBarView healthBar;

        [SerializeField] private CCS_AttributeBarView staminaBar;

        [SerializeField] private CCS_AttributeBarView hungerBar;

        [SerializeField] private CCS_AttributeBarView thirstBar;

        private float staminaCurrent = CCS_AttributeBarsHudStyle.StaminaMax;

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

            staminaCurrent = CCS_AttributeBarsHudStyle.StaminaMax;
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

        private void Update()
        {
            if (!isLocalOwnerActive)
            {
                return;
            }

            UpdateLocalStamina(Time.deltaTime);
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
            if (!string.Equals(changedEvent.AttributeId, ResolveHealthAttributeId(), System.StringComparison.Ordinal))
            {
                return;
            }

            RefreshHealthBar();
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

            staminaBar.SetValues(
                CCS_AttributeBarsHudStyle.StaminaBarLabel,
                staminaCurrent,
                CCS_AttributeBarsHudStyle.StaminaMax);
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

        private void UpdateLocalStamina(float deltaTime)
        {
            if (deltaTime <= 0f)
            {
                return;
            }

            if (IsSprintActive())
            {
                staminaCurrent = Mathf.Max(
                    0f,
                    staminaCurrent - CCS_AttributeBarsHudStyle.StaminaDrainPerSecond * deltaTime);
            }
            else
            {
                staminaCurrent = Mathf.Min(
                    CCS_AttributeBarsHudStyle.StaminaMax,
                    staminaCurrent + CCS_AttributeBarsHudStyle.StaminaRegenPerSecond * deltaTime);
            }

            RefreshStaminaBar();
        }

        private static bool IsSprintActive()
        {
            if (!Input.GetKey(KeyCode.LeftShift))
            {
                return false;
            }

            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");
            return horizontal * horizontal + vertical * vertical > 0.01f;
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
                return healthDefinition.UiLabel.ToUpperInvariant();
            }

            return CCS_AttributesConstants.HealthDisplayName.ToUpperInvariant();
        }

        #endregion
    }
}
