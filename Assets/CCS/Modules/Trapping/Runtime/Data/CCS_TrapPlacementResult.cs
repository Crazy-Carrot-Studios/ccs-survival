// =============================================================================
// SCRIPT: CCS_TrapPlacementResult
// CATEGORY: Modules / Trapping / Runtime / Data
// PURPOSE: Outcome for trap placement preview and confirmation attempts.
// PLACEMENT: Returned by CCS_TrapService placement methods.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// =============================================================================

using UnityEngine;

namespace CCS.Modules.Trapping
{
    public sealed class CCS_TrapPlacementResult
    {
        public static CCS_TrapPlacementResult Preview(Vector3 previewPosition, bool isValid, string message)
        {
            return new CCS_TrapPlacementResult(
                isSuccess: true,
                isPreview: true,
                isValid,
                previewPosition,
                string.Empty,
                message);
        }

        public static CCS_TrapPlacementResult Placed(string instanceId, string message)
        {
            return new CCS_TrapPlacementResult(
                isSuccess: true,
                isPreview: false,
                isValid: true,
                Vector3.zero,
                instanceId,
                message);
        }

        public static CCS_TrapPlacementResult Failure(string message)
        {
            return new CCS_TrapPlacementResult(
                isSuccess: false,
                isPreview: false,
                isValid: false,
                Vector3.zero,
                string.Empty,
                message);
        }

        private CCS_TrapPlacementResult(
            bool isSuccess,
            bool isPreview,
            bool isValid,
            Vector3 previewPosition,
            string instanceId,
            string message)
        {
            IsSuccess = isSuccess;
            IsPreview = isPreview;
            IsValid = isValid;
            PreviewPosition = previewPosition;
            InstanceId = instanceId ?? string.Empty;
            Message = message ?? string.Empty;
        }

        public bool IsSuccess { get; }

        public bool IsPreview { get; }

        public bool IsValid { get; }

        public Vector3 PreviewPosition { get; }

        public string InstanceId { get; }

        public string Message { get; }
    }
}
