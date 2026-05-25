using System;

// =============================================================================
// SCRIPT: CCS_Result
// CATEGORY: Core / Runtime / Data
// PURPOSE: Lightweight immutable non-generic result for CCS operations.
// PLACEMENT: Runtime utility type. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-05-24
// NOTES: No exceptions, logging, or generics. Compiled by CCS.Core.Runtime.
// =============================================================================

namespace CCS.Core
{
    [Serializable]
    public readonly struct CCS_Result
    {
        #region Properties

        public bool IsSuccess { get; }

        public string Message { get; }

        public CCS_CoreErrorCode ErrorCode { get; }

        #endregion

        #region Public Methods

        public CCS_Result(bool isSuccess, string message)
            : this(isSuccess, message, isSuccess ? CCS_CoreErrorCode.None : CCS_CoreErrorCode.Unknown)
        {
        }

        public CCS_Result(bool isSuccess, string message, CCS_CoreErrorCode errorCode)
        {
            IsSuccess = isSuccess;
            Message = message ?? string.Empty;
            ErrorCode = isSuccess ? CCS_CoreErrorCode.None : errorCode;
        }

        public static CCS_Result Success()
        {
            return new CCS_Result(true, string.Empty, CCS_CoreErrorCode.None);
        }

        public static CCS_Result Success(string message)
        {
            return new CCS_Result(true, message, CCS_CoreErrorCode.None);
        }

        public static CCS_Result Failure(string message)
        {
            return new CCS_Result(false, message, CCS_CoreErrorCode.Unknown);
        }

        public static CCS_Result Failure(CCS_CoreErrorCode errorCode, string message)
        {
            return new CCS_Result(false, message, errorCode);
        }

        public override string ToString()
        {
            string status = IsSuccess ? "Success" : "Failure";
            if (string.IsNullOrEmpty(Message))
            {
                return ErrorCode == CCS_CoreErrorCode.None
                    ? $"[CCS_Result] {status}"
                    : $"[CCS_Result] {status} ({ErrorCode})";
            }

            return ErrorCode == CCS_CoreErrorCode.None
                ? $"[CCS_Result] {status}: {Message}"
                : $"[CCS_Result] {status} ({ErrorCode}): {Message}";
        }

        #endregion
    }
}
