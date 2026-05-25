using System;

// =============================================================================
// SCRIPT: CCS_Result<T>
// CATEGORY: Core / Runtime / Data
// PURPOSE: Lightweight immutable generic result wrapper for CCS operations.
// PLACEMENT: Runtime utility type. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-05-24
// NOTES: Failure returns default(T). No exceptions or logging.
// =============================================================================

namespace CCS.Core
{
    [Serializable]
    public readonly struct CCS_Result<T>
    {
        #region Properties

        public bool IsSuccess { get; }

        public string Message { get; }

        public T Value { get; }

        #endregion

        #region Public Methods

        public CCS_Result(bool isSuccess, T value, string message)
        {
            IsSuccess = isSuccess;
            Value = value;
            Message = message ?? string.Empty;
        }

        public static CCS_Result<T> Success(T value, string message = "")
        {
            return new CCS_Result<T>(true, value, message);
        }

        public static CCS_Result<T> Failure(string message)
        {
            return new CCS_Result<T>(false, default, message);
        }

        public override string ToString()
        {
            string status = IsSuccess ? "Success" : "Failure";
            return string.IsNullOrEmpty(Message)
                ? $"[CCS_Result<{typeof(T).Name}>] {status}"
                : $"[CCS_Result<{typeof(T).Name}>] {status}: {Message}";
        }

        #endregion
    }
}
