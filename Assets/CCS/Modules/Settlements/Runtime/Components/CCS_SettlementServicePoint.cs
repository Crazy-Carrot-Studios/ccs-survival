using CCS.Modules.Economy;
using CCS.Modules.Interaction;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_SettlementServicePoint
// CATEGORY: Modules / Settlements / Runtime / Components
// PURPOSE: Interactable settlement service that routes to vendors or placeholder messages.
// PLACEMENT: Child objects under CCS_SettlementLocation.
// AUTHOR: James Schilz
// CREATED: 2026-06-02
// NOTES: Reuses CCS_VendorService; no duplicate vendor transaction logic.
// =============================================================================

namespace CCS.Modules.Settlements
{
    public sealed class CCS_SettlementServicePoint : MonoBehaviour, CCS_IInteractableResultProvider
    {
        #region Variables

        [Header("Identity")]
        [SerializeField] private string servicePointId = string.Empty;

        [SerializeField] private CCS_SettlementServicePointType servicePointType = CCS_SettlementServicePointType.GeneralStore;

        [Header("Settlement")]
        [SerializeField] private CCS_SettlementLocation settlementLocation;

        [Header("Routing")]
        [Tooltip("Vendor-backed service points open the existing vendor debug flow.")]
        [SerializeField] private CCS_VendorDefinition vendorDefinition;

        [Tooltip("Placeholder message for non-vendor services such as blacksmith.")]
        [SerializeField] private string placeholderMessage = "Service coming soon.";

        [Header("Interaction")]
        [SerializeField] private float interactionDistance = 3f;

        [SerializeField] private string interactionDisplayNameOverride = string.Empty;

        #endregion

        #region Properties

        public string ServicePointId => servicePointId ?? string.Empty;

        public CCS_SettlementServicePointType ServicePointType => servicePointType;

        public CCS_VendorDefinition VendorDefinition => vendorDefinition;

        #endregion

        #region Public Methods

        public string GetInteractionDisplayName()
        {
            if (!string.IsNullOrWhiteSpace(interactionDisplayNameOverride))
            {
                return interactionDisplayNameOverride;
            }

            return servicePointType switch
            {
                CCS_SettlementServicePointType.GeneralStore => "General Store",
                CCS_SettlementServicePointType.Stable => "Stable",
                CCS_SettlementServicePointType.Gunsmith => "Gunsmith",
                CCS_SettlementServicePointType.Blacksmith => "Blacksmith",
                _ => "Settlement Service"
            };
        }

        public float GetInteractionDistance()
        {
            return interactionDistance < 0.1f ? 3f : interactionDistance;
        }

        public bool CanInteract()
        {
            return isActiveAndEnabled;
        }

        public void Interact()
        {
            TryInteract();
        }

        public bool TryInteract()
        {
            if (settlementLocation == null)
            {
                settlementLocation = GetComponentInParent<CCS_SettlementLocation>();
            }

            settlementLocation?.NotifyServicePointUsed(this);

            if (vendorDefinition != null)
            {
                return TryActivateVendor(vendorDefinition);
            }

            CCS_SettlementDebugMessageHud.ShowMessage(
                GetInteractionDisplayName(),
                string.IsNullOrWhiteSpace(placeholderMessage)
                    ? "Service coming soon."
                    : placeholderMessage);
            return true;
        }

        #endregion

        #region Private Methods

        private static bool TryActivateVendor(CCS_VendorDefinition definition)
        {
            if (definition == null
                || !CCS_EconomyRuntimeBridge.TryGetVendorService(out CCS_VendorService vendorService)
                || !vendorService.IsInitialized)
            {
                return false;
            }

            vendorService.SetActiveVendor(definition);
            CCS_VendorDebugHud.NotifyVendorActivated(definition);
            return true;
        }

        #endregion
    }
}
