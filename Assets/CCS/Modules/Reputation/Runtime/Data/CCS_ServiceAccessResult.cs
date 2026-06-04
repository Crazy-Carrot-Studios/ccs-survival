// =============================================================================
// SCRIPT: CCS_ServiceAccessResult
// CATEGORY: Modules / Reputation / Runtime / Data
// PURPOSE: Structured outcome for service access rule evaluation.
// PLACEMENT: Returned by CCS_ServiceAccessEvaluationUtility and settlement routing.
// AUTHOR: James Schilz
// CREATED: 2026-06-04
// NOTES: Milestone 2.8.0 service access and price modifier foundation.
// =============================================================================

namespace CCS.Modules.Reputation
{
    public sealed class CCS_ServiceAccessResult
    {
        public CCS_ServiceAccessResult(
            CCS_ServiceAccessResultType resultType,
            string message,
            string missingRequirementPlaceholder = "")
        {
            ResultType = resultType;
            Message = message ?? string.Empty;
            MissingRequirementPlaceholder = missingRequirementPlaceholder ?? string.Empty;
        }

        public CCS_ServiceAccessResultType ResultType { get; }

        public string Message { get; }

        public string MissingRequirementPlaceholder { get; }

        public bool IsAllowed => ResultType == CCS_ServiceAccessResultType.Allowed;

        public static CCS_ServiceAccessResult Allowed(string message = "Service access allowed.")
        {
            return new CCS_ServiceAccessResult(CCS_ServiceAccessResultType.Allowed, message);
        }

        public static CCS_ServiceAccessResult Denied(
            CCS_ServiceAccessResultType resultType,
            string message,
            string missingRequirementPlaceholder = "")
        {
            return new CCS_ServiceAccessResult(resultType, message, missingRequirementPlaceholder);
        }
    }
}
