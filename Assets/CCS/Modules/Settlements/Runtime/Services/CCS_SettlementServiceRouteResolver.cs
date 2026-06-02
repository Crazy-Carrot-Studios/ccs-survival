using CCS.Modules.Economy;
using CCS.Modules.Industry;
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

            return CCS_SettlementServiceActivationResult.Success(
                ResolveRouteType(servicePoint),
                "Service point is available.");
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

            vendorService.SetActiveVendor(vendorDefinition);
            CCS_VendorDebugHud.NotifyVendorActivated(vendorDefinition);
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
