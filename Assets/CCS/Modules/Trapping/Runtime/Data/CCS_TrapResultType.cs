// =============================================================================
// SCRIPT: CCS_TrapResultType
// CATEGORY: Modules / Trapping / Runtime / Data
// PURPOSE: Typed outcomes for trap placement, capture, and harvest operations.
// PLACEMENT: Returned by CCS_TrapService and CCS_TrapResult.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// =============================================================================

namespace CCS.Modules.Trapping
{
    public enum CCS_TrapResultType
    {
        Success = 0,
        Failed = 1,
        CaptureSuccess = 2,
        CaptureFailed = 3,
        NoWildlife = 4,
        WrongLocation = 5,
        BrokenTrap = 6,
        WrongTool = 7,
        InventoryFull = 8,
        ServiceUnavailable = 9,
        TargetUnavailable = 10
    }
}
