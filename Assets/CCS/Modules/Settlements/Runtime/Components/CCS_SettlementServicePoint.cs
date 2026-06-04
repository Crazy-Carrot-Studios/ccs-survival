using CCS.Modules.Economy;
using CCS.Modules.Interaction;
using CCS.Modules.Reputation;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_SettlementServicePoint
// CATEGORY: Modules / Settlements / Runtime / Components
// PURPOSE: Interactable settlement service that routes to vendors, industry, or placeholders.
// PLACEMENT: Child objects under CCS_SettlementLocation.
// AUTHOR: James Schilz
// CREATED: 2026-06-02
// NOTES: Reuses CCS_VendorService and CCS_IndustryService; no duplicate vendor logic.
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

        [Header("Availability")]
        [SerializeField] private bool isAvailable = true;

        [SerializeField] private string unavailableReason = string.Empty;

        [SerializeField] private bool requiredSettlementDiscovered;

        [SerializeField] private int requiredCampTier = -1;

        [Header("Routing")]
        [SerializeField] private CCS_SettlementServiceRouteType routeOverride = CCS_SettlementServiceRouteType.Unknown;

        [Tooltip("Vendor-backed service points open the existing vendor debug flow.")]
        [SerializeField] private CCS_VendorDefinition vendorDefinition;

        [Tooltip("Placeholder message for non-vendor, non-industry services.")]
        [SerializeField] private string placeholderMessage = "Service coming soon.";

        [Header("Interaction")]
        [SerializeField] private float interactionDistance = 3f;

        [SerializeField] private string interactionDisplayNameOverride = string.Empty;

        #endregion

        #region Properties

        public string ServicePointId => servicePointId ?? string.Empty;

        public CCS_SettlementServicePointType ServicePointType => servicePointType;

        public CCS_VendorDefinition VendorDefinition => vendorDefinition;

        public CCS_SettlementLocation SettlementLocation => settlementLocation;

        public bool IsAvailableFlag => isAvailable;

        public string UnavailableReason => unavailableReason ?? string.Empty;

        public bool RequiredSettlementDiscovered => requiredSettlementDiscovered;

        public int RequiredCampTier => requiredCampTier;

        public CCS_SettlementServiceRouteType RouteOverride => routeOverride;

        public string PlaceholderMessage => placeholderMessage ?? string.Empty;

        public bool OffersLoanServices =>
            servicePointType == CCS_SettlementServicePointType.Bank;

        public bool OffersBankingServices =>
            servicePointType == CCS_SettlementServicePointType.Bank
            || servicePointType == CCS_SettlementServicePointType.LandOffice;

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
                CCS_SettlementServicePointType.ContractBoard => "Contract Board",
                _ => "Settlement Service"
            };
        }

        public float GetInteractionDistance()
        {
            return interactionDistance < 0.1f ? 3f : interactionDistance;
        }

        public bool CanInteract()
        {
            return isActiveAndEnabled && isAvailable;
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

            CCS_SettlementServiceActivationResult result = CCS_SettlementServiceRouteResolver.TryActivate(this);
            settlementLocation?.NotifyServicePointUsed(this, result);
            return result.IsSuccess;
        }

        public CCS_ServiceAccessResult EvaluateServiceAccess(CCS_ReputationService reputationService)
        {
            string settlementId = ResolveSettlementId();
            bool isDiscovered = IsSettlementDiscovered();
            return CCS_ServiceAccessEvaluationUtility.EvaluateForServicePoint(
                reputationService,
                settlementId,
                ServicePointId,
                (int)ServicePointType,
                isDiscovered);
        }

        public string ResolveSettlementId()
        {
            if (settlementLocation == null)
            {
                settlementLocation = GetComponentInParent<CCS_SettlementLocation>();
            }

            return settlementLocation?.SettlementDefinition != null
                ? settlementLocation.SettlementDefinition.SettlementId
                : string.Empty;
        }

        private bool IsSettlementDiscovered()
        {
            if (settlementLocation?.SettlementDefinition == null
                || !CCS_SettlementRuntimeBridge.TryGetSettlementService(out CCS_SettlementService settlementService)
                || !settlementService.IsInitialized)
            {
                return false;
            }

            return settlementService.IsDiscovered(settlementLocation.SettlementDefinition.SettlementId);
        }

        #endregion
    }
}
