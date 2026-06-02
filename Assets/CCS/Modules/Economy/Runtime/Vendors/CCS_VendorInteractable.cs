using CCS.Modules.Interaction;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_VendorInteractable
// CATEGORY: Modules / Economy / Runtime / Vendors
// PURPOSE: Interaction handoff that activates a vendor for debug/test transactions.
// PLACEMENT: CCS_TestGeneralStore bootstrap object.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: No final vendor UI. Opens debug transaction flow via CCS_VendorDebugHud.
// =============================================================================

namespace CCS.Modules.Economy
{
    public sealed class CCS_VendorInteractable : MonoBehaviour, CCS_IInteractableResultProvider
    {
        #region Variables

        [Header("Vendor")]
        [Tooltip("Vendor definition used for buy/sell.")]
        [SerializeField] private CCS_VendorDefinition vendorDefinition;

        [Header("Interaction")]
        [SerializeField] private float interactionDistance = 3f;

        [SerializeField] private string interactionDisplayNameOverride = string.Empty;

        #endregion

        #region Public Methods

        public string GetInteractionDisplayName()
        {
            if (!string.IsNullOrWhiteSpace(interactionDisplayNameOverride))
            {
                return interactionDisplayNameOverride;
            }

            return vendorDefinition != null && !string.IsNullOrWhiteSpace(vendorDefinition.DisplayName)
                ? vendorDefinition.DisplayName
                : "Vendor";
        }

        public float GetInteractionDistance()
        {
            return interactionDistance < 0.1f ? 3f : interactionDistance;
        }

        public bool CanInteract()
        {
            return isActiveAndEnabled && vendorDefinition != null;
        }

        public void Interact()
        {
            TryInteract();
        }

        public bool TryInteract()
        {
            if (vendorDefinition == null
                || !CCS_EconomyRuntimeBridge.TryGetVendorService(out CCS_VendorService vendorService)
                || !vendorService.IsInitialized)
            {
                return false;
            }

            vendorService.SetActiveVendor(vendorDefinition);
            CCS_VendorDebugHud.NotifyVendorActivated(vendorDefinition);
            return true;
        }

        public CCS_VendorDefinition VendorDefinition => vendorDefinition;

        #endregion
    }
}
