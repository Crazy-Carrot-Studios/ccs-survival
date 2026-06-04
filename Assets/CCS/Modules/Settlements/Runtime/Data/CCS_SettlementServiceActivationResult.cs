// =============================================================================
// SCRIPT: CCS_SettlementServiceActivationResult
// CATEGORY: Modules / Settlements / Runtime / Data
// PURPOSE: Structured activation outcome for settlement service routing.
// PLACEMENT: Returned by CCS_SettlementServiceRouteResolver and service points.
// AUTHOR: James Schilz
// CREATED: 2026-06-02
// NOTES: Milestone 1.8.1 settlement service routing polish.
// =============================================================================

namespace CCS.Modules.Settlements
{
    public sealed class CCS_SettlementServiceActivationResult
    {
        public CCS_SettlementServiceRouteType RouteType { get; set; } = CCS_SettlementServiceRouteType.Unknown;

        public CCS_SettlementServiceActivationStatus Status { get; set; } = CCS_SettlementServiceActivationStatus.Failed;

        public string Message { get; set; } = string.Empty;

        public string ServiceAccessResultType { get; set; } = string.Empty;

        public string MissingRequirementMessage { get; set; } = string.Empty;

        public bool IsSuccess => Status == CCS_SettlementServiceActivationStatus.Succeeded;

        public static CCS_SettlementServiceActivationResult Success(
            CCS_SettlementServiceRouteType routeType,
            string message)
        {
            return new CCS_SettlementServiceActivationResult
            {
                RouteType = routeType,
                Status = CCS_SettlementServiceActivationStatus.Succeeded,
                Message = message ?? string.Empty
            };
        }

        public static CCS_SettlementServiceActivationResult Blocked(
            CCS_SettlementServiceRouteType routeType,
            CCS_SettlementServiceActivationStatus status,
            string message)
        {
            return new CCS_SettlementServiceActivationResult
            {
                RouteType = routeType,
                Status = status,
                Message = message ?? string.Empty
            };
        }
    }
}
