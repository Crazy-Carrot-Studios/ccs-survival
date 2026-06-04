using CCS.Modules.Banking;
using CCS.Modules.Economy;
using CCS.Modules.Industry;
using CCS.Modules.Reputation;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_SettlementServiceRouteResolver
// CATEGORY: Modules / Settlements / Runtime / Services
// PURPOSE: Resolves availability and activation routing for settlement service points.
// PLACEMENT: Called by CCS_SettlementServicePoint on interact.
// AUTHOR: James Schilz
// CREATED: 2026-06-02
// NOTES: Reuses vendor and industry services; no duplicate transaction logic.
// =============================================================================

namespace CCS.Modules.Settlements
{
    public static class CCS_SettlementServiceRouteResolver
    {
        public static CCS_SettlementServiceRouteType ResolveRouteType(CCS_SettlementServicePoint servicePoint)
        {
            if (servicePoint == null)
            {
                return CCS_SettlementServiceRouteType.Unknown;
            }

            if (servicePoint.RouteOverride != CCS_SettlementServiceRouteType.Unknown
                && servicePoint.RouteOverride != CCS_SettlementServiceRouteType.Unavailable
                && servicePoint.RouteOverride != CCS_SettlementServiceRouteType.Disabled)
            {
                return servicePoint.RouteOverride;
            }

            if (servicePoint.VendorDefinition != null)
            {
                return CCS_SettlementServiceRouteType.Vendor;
            }

            if (servicePoint.ServicePointType == CCS_SettlementServicePointType.Blacksmith)
            {
                return CCS_SettlementServiceRouteType.Industry;
            }

            if (servicePoint.ServicePointType == CCS_SettlementServicePointType.Bank)
            {
                return CCS_SettlementServiceRouteType.Bank;
            }

            if (servicePoint.ServicePointType == CCS_SettlementServicePointType.LandOffice)
            {
                return CCS_SettlementServiceRouteType.LandOffice;
            }

            if (servicePoint.ServicePointType == CCS_SettlementServicePointType.ContractBoard)
            {
                return CCS_SettlementServiceRouteType.ContractBoard;
            }

            if (servicePoint.ServicePointType == CCS_SettlementServicePointType.Other)
            {
                return CCS_SettlementServiceRouteType.Unknown;
            }

            return CCS_SettlementServiceRouteType.Placeholder;
        }

        public static CCS_SettlementServiceActivationResult TryActivate(CCS_SettlementServicePoint servicePoint)
        {
            if (servicePoint == null)
            {
                return CCS_SettlementServiceActivationResult.Blocked(
                    CCS_SettlementServiceRouteType.Unknown,
                    CCS_SettlementServiceActivationStatus.Failed,
                    "Service point is missing.");
            }

            if (!servicePoint.isActiveAndEnabled)
            {
                return CCS_SettlementServiceActivationResult.Blocked(
                    CCS_SettlementServiceRouteType.Disabled,
                    CCS_SettlementServiceActivationStatus.Disabled,
                    "Service point is disabled.");
            }

            if (!servicePoint.IsAvailableFlag)
            {
                string disabledMessage = ResolveUnavailableMessage(servicePoint, "Service point is disabled.");
                CCS_SettlementDebugMessageHud.ShowMessage(servicePoint.GetInteractionDisplayName(), disabledMessage);
                return CCS_SettlementServiceActivationResult.Blocked(
                    CCS_SettlementServiceRouteType.Disabled,
                    CCS_SettlementServiceActivationStatus.Disabled,
                    disabledMessage);
            }

            CCS_SettlementServiceActivationResult availabilityResult = EvaluateAvailability(servicePoint);
            if (!availabilityResult.IsSuccess)
            {
                CCS_SettlementDebugMessageHud.ShowMessage(
                    servicePoint.GetInteractionDisplayName(),
                    availabilityResult.Message);
                return availabilityResult;
            }

            CCS_SettlementServiceRouteType routeType = ResolveRouteType(servicePoint);
            switch (routeType)
            {
                case CCS_SettlementServiceRouteType.Vendor:
                    return TryActivateVendorRoute(servicePoint);
                case CCS_SettlementServiceRouteType.Industry:
                    return TryActivateIndustryRoute(servicePoint);
                case CCS_SettlementServiceRouteType.Placeholder:
                    return TryActivatePlaceholderRoute(servicePoint);
                case CCS_SettlementServiceRouteType.Bank:
                    return TryActivateBankRoute(servicePoint);
                case CCS_SettlementServiceRouteType.LandOffice:
                    return TryActivateLandOfficeRoute(servicePoint);
                case CCS_SettlementServiceRouteType.ContractBoard:
                    return TryActivateContractBoardRoute(servicePoint);
                case CCS_SettlementServiceRouteType.Unknown:
                    CCS_SettlementDebugMessageHud.ShowMessage(
                        servicePoint.GetInteractionDisplayName(),
                        "Unknown settlement service route.");
                    return CCS_SettlementServiceActivationResult.Blocked(
                        CCS_SettlementServiceRouteType.Unknown,
                        CCS_SettlementServiceActivationStatus.UnknownRoute,
                        "Unknown settlement service route.");
                default:
                    return CCS_SettlementServiceActivationResult.Blocked(
                        routeType,
                        CCS_SettlementServiceActivationStatus.Failed,
                        "Settlement service route is not supported.");
            }
        }

        public static CCS_SettlementServiceActivationResult EvaluateAvailability(CCS_SettlementServicePoint servicePoint)
        {
            if (servicePoint == null)
            {
                return CCS_SettlementServiceActivationResult.Blocked(
                    CCS_SettlementServiceRouteType.Unavailable,
                    CCS_SettlementServiceActivationStatus.Unavailable,
                    "Service point is missing.");
            }

            if (servicePoint.RequiredSettlementDiscovered
                && !IsSettlementDiscovered(servicePoint))
            {
                return CCS_SettlementServiceActivationResult.Blocked(
                    CCS_SettlementServiceRouteType.Unavailable,
                    CCS_SettlementServiceActivationStatus.Unavailable,
                    ResolveUnavailableMessage(servicePoint, "Discover this settlement before using services."));
            }

            if (servicePoint.RequiredCampTier >= 0)
            {
                return CCS_SettlementServiceActivationResult.Blocked(
                    CCS_SettlementServiceRouteType.Unavailable,
                    CCS_SettlementServiceActivationStatus.Unavailable,
                    ResolveUnavailableMessage(
                        servicePoint,
                        $"Requires camp tier {servicePoint.RequiredCampTier} (future requirement)."));
            }

            if (servicePoint.ServicePointType == CCS_SettlementServicePointType.Blacksmith
                && !TryResolveIndustryService(out _))
            {
                return CCS_SettlementServiceActivationResult.Blocked(
                    CCS_SettlementServiceRouteType.Unavailable,
                    CCS_SettlementServiceActivationStatus.Unavailable,
                    ResolveUnavailableMessage(servicePoint, "Industry services are not available."));
            }

            CCS_ServiceAccessResult accessResult = EvaluateServiceAccess(servicePoint);
            if (!accessResult.IsAllowed)
            {
                return BlockedFromAccessResult(servicePoint, accessResult);
            }

            return CCS_SettlementServiceActivationResult.Success(
                ResolveRouteType(servicePoint),
                "Service point is available.");
        }

        public static CCS_ServiceAccessResult EvaluateServiceAccess(CCS_SettlementServicePoint servicePoint)
        {
            if (servicePoint == null)
            {
                return CCS_ServiceAccessResult.Denied(
                    CCS_ServiceAccessResultType.DeniedUnavailable,
                    "Service point is missing.");
            }

            CCS_ReputationService reputationService = null;
            CCS_ReputationRuntimeBridge.TryGetReputationService(out reputationService);
            return servicePoint.EvaluateServiceAccess(reputationService);
        }

        private static CCS_SettlementServiceActivationResult BlockedFromAccessResult(
            CCS_SettlementServicePoint servicePoint,
            CCS_ServiceAccessResult accessResult)
        {
            CCS_SettlementServiceActivationStatus status = MapAccessResultToStatus(accessResult.ResultType);
            string displayName = servicePoint.GetInteractionDisplayName();
            CCS_SettlementDebugMessageHud.ShowServiceAccessResult(
                displayName,
                accessResult.ResultType.ToString(),
                accessResult.Message,
                accessResult.MissingRequirementPlaceholder);

            CCS_SettlementServiceActivationResult blocked = CCS_SettlementServiceActivationResult.Blocked(
                CCS_SettlementServiceRouteType.Unavailable,
                status,
                accessResult.Message);
            blocked.ServiceAccessResultType = accessResult.ResultType.ToString();
            blocked.MissingRequirementMessage = accessResult.MissingRequirementPlaceholder;
            return blocked;
        }

        private static CCS_SettlementServiceActivationStatus MapAccessResultToStatus(
            CCS_ServiceAccessResultType resultType)
        {
            switch (resultType)
            {
                case CCS_ServiceAccessResultType.DeniedReputation:
                    return CCS_SettlementServiceActivationStatus.DeniedReputation;
                case CCS_ServiceAccessResultType.DeniedDisabled:
                    return CCS_SettlementServiceActivationStatus.Disabled;
                case CCS_ServiceAccessResultType.MissingRequirement:
                    return CCS_SettlementServiceActivationStatus.MissingRequirement;
                default:
                    return CCS_SettlementServiceActivationStatus.Unavailable;
            }
        }

        private static CCS_SettlementServiceActivationResult TryActivateVendorRoute(CCS_SettlementServicePoint servicePoint)
        {
            CCS_VendorDefinition vendorDefinition = servicePoint.VendorDefinition;
            if (vendorDefinition == null)
            {
                return CCS_SettlementServiceActivationResult.Blocked(
                    CCS_SettlementServiceRouteType.Vendor,
                    CCS_SettlementServiceActivationStatus.ServiceMissing,
                    "Vendor definition is not assigned.");
            }

            if (!CCS_EconomyRuntimeBridge.TryGetVendorService(out CCS_VendorService vendorService)
                || !vendorService.IsInitialized)
            {
                return CCS_SettlementServiceActivationResult.Blocked(
                    CCS_SettlementServiceRouteType.Vendor,
                    CCS_SettlementServiceActivationStatus.ServiceMissing,
                    "Vendor service is not ready.");
            }

            vendorService.SetActiveVendor(vendorDefinition, servicePoint.ResolveSettlementId());
            CCS_VendorDebugHud.NotifyVendorActivated(vendorDefinition, servicePoint.ResolveSettlementId());
            return CCS_SettlementServiceActivationResult.Success(
                CCS_SettlementServiceRouteType.Vendor,
                $"Vendor route active: {vendorDefinition.DisplayName}.");
        }

        private static CCS_SettlementServiceActivationResult TryActivateIndustryRoute(CCS_SettlementServicePoint servicePoint)
        {
            if (!TryResolveIndustryService(out CCS_IndustryService industryService))
            {
                return CCS_SettlementServiceActivationResult.Blocked(
                    CCS_SettlementServiceRouteType.Industry,
                    CCS_SettlementServiceActivationStatus.ServiceMissing,
                    "Industry service is not ready.");
            }

            CCS_IndustryProfile profile = industryService.ActiveProfile;
            if (profile == null)
            {
                return CCS_SettlementServiceActivationResult.Blocked(
                    CCS_SettlementServiceRouteType.Industry,
                    CCS_SettlementServiceActivationStatus.ServiceMissing,
                    "Industry profile is not assigned.");
            }

            CCS_SettlementIndustryServiceHud.ShowIndustrySummary(
                servicePoint.GetInteractionDisplayName(),
                profile);
            return CCS_SettlementServiceActivationResult.Success(
                CCS_SettlementServiceRouteType.Industry,
                "Industry service summary opened.");
        }

        private static CCS_SettlementServiceActivationResult TryActivateBankRoute(CCS_SettlementServicePoint servicePoint)
        {
            if (!CCS_BankingRuntimeBridge.TryGetBankingService(out CCS_BankingService bankingService)
                || !bankingService.IsInitialized)
            {
                return CCS_SettlementServiceActivationResult.Blocked(
                    CCS_SettlementServiceRouteType.Bank,
                    CCS_SettlementServiceActivationStatus.ServiceMissing,
                    "Banking service is not ready.");
            }

            CCS_BankingDebugHud.NotifyBankActivated(servicePoint.GetInteractionDisplayName());
            return CCS_SettlementServiceActivationResult.Success(
                CCS_SettlementServiceRouteType.Bank,
                "Bank debug panel opened.");
        }

        private static CCS_SettlementServiceActivationResult TryActivateLandOfficeRoute(CCS_SettlementServicePoint servicePoint)
        {
            if (!CCS_BankingRuntimeBridge.TryGetBankingService(out CCS_BankingService bankingService)
                || !bankingService.IsInitialized)
            {
                return CCS_SettlementServiceActivationResult.Blocked(
                    CCS_SettlementServiceRouteType.LandOffice,
                    CCS_SettlementServiceActivationStatus.ServiceMissing,
                    "Land office service is not ready.");
            }

            CCS_BankingDebugHud.NotifyLandOfficeActivated(servicePoint.GetInteractionDisplayName());
            return CCS_SettlementServiceActivationResult.Success(
                CCS_SettlementServiceRouteType.LandOffice,
                "Land office debug panel opened.");
        }

        private static CCS_SettlementServiceActivationResult TryActivateContractBoardRoute(CCS_SettlementServicePoint servicePoint)
        {
            if (CCS_SettlementContractBoardActivationBridge.TryActivate(servicePoint, out CCS_SettlementServiceActivationResult result))
            {
                return result;
            }

            return CCS_SettlementServiceActivationResult.Blocked(
                CCS_SettlementServiceRouteType.ContractBoard,
                CCS_SettlementServiceActivationStatus.ServiceMissing,
                "Contract service is not ready.");
        }

        private static CCS_SettlementServiceActivationResult TryActivatePlaceholderRoute(CCS_SettlementServicePoint servicePoint)
        {
            string message = string.IsNullOrWhiteSpace(servicePoint.PlaceholderMessage)
                ? "Service coming soon."
                : servicePoint.PlaceholderMessage;
            CCS_SettlementDebugMessageHud.ShowMessage(servicePoint.GetInteractionDisplayName(), message);
            return CCS_SettlementServiceActivationResult.Success(
                CCS_SettlementServiceRouteType.Placeholder,
                message);
        }

        private static bool TryResolveIndustryService(out CCS_IndustryService industryService)
        {
            industryService = CCS_IndustryRuntimeBridge.ResolveService(null);
            return industryService != null && industryService.IsInitialized;
        }

        private static bool IsSettlementDiscovered(CCS_SettlementServicePoint servicePoint)
        {
            CCS_SettlementLocation location = servicePoint.SettlementLocation;
            if (location?.SettlementDefinition == null
                || !CCS_SettlementRuntimeBridge.TryGetSettlementService(out CCS_SettlementService settlementService)
                || !settlementService.IsInitialized)
            {
                return false;
            }

            return settlementService.IsDiscovered(location.SettlementDefinition.SettlementId);
        }

        private static string ResolveUnavailableMessage(CCS_SettlementServicePoint servicePoint, string fallback)
        {
            return string.IsNullOrWhiteSpace(servicePoint.UnavailableReason) ? fallback : servicePoint.UnavailableReason;
        }
    }
}
