// =============================================================================
// SCRIPT: CCS_TrapEventArgs
// CATEGORY: Modules / Trapping / Runtime / Events
// PURPOSE: Event payload for trap lifecycle notifications.
// PLACEMENT: Raised by CCS_TrapService on place, trigger, harvest, and break.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// =============================================================================

namespace CCS.Modules.Trapping
{
    public sealed class CCS_TrapEventArgs
    {
        public CCS_TrapEventArgs(
            CCS_TrapInstance trapInstance,
            CCS_TrapResult result,
            bool isSuccess,
            string message)
        {
            TrapInstance = trapInstance;
            Result = result;
            IsSuccess = isSuccess;
            Message = message ?? string.Empty;
        }

        public CCS_TrapInstance TrapInstance { get; }

        public CCS_TrapResult Result { get; }

        public bool IsSuccess { get; }

        public string Message { get; }
    }

    public delegate void TrapPlacedHandler(CCS_TrapEventArgs eventArgs);
    public delegate void TrapTriggeredHandler(CCS_TrapEventArgs eventArgs);
    public delegate void TrapHarvestedHandler(CCS_TrapEventArgs eventArgs);
    public delegate void TrapBrokenHandler(CCS_TrapEventArgs eventArgs);
}
