// =============================================================================
// SCRIPT: CCS_TrapResult
// CATEGORY: Modules / Trapping / Runtime / Data
// PURPOSE: Outcome payload for trap service operations.
// PLACEMENT: Returned by CCS_TrapService placement, capture, and harvest methods.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// =============================================================================

namespace CCS.Modules.Trapping
{
    public sealed class CCS_TrapResult
    {
        public static CCS_TrapResult Success(string message, string instanceId = "")
        {
            return new CCS_TrapResult(CCS_TrapResultType.Success, true, message, instanceId);
        }

        public static CCS_TrapResult Failure(CCS_TrapResultType resultType, string message)
        {
            return new CCS_TrapResult(resultType, false, message, string.Empty);
        }

        public static CCS_TrapResult CaptureSuccess(string message, string instanceId)
        {
            return new CCS_TrapResult(CCS_TrapResultType.CaptureSuccess, true, message, instanceId);
        }

        private CCS_TrapResult(
            CCS_TrapResultType resultType,
            bool isSuccess,
            string message,
            string instanceId)
        {
            ResultType = resultType;
            IsSuccess = isSuccess;
            Message = message ?? string.Empty;
            InstanceId = instanceId ?? string.Empty;
        }

        public CCS_TrapResultType ResultType { get; }

        public bool IsSuccess { get; }

        public string Message { get; }

        public string InstanceId { get; }
    }
}
