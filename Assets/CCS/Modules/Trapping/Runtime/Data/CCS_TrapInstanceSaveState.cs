// =============================================================================
// SCRIPT: CCS_TrapInstanceSaveState
// CATEGORY: Modules / Trapping / Runtime / Data
// PURPOSE: Serializable trap instance state for unified save/load.
// PLACEMENT: Captured by CCS_TrapInstance and restored by CCS_TrapService.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// =============================================================================

namespace CCS.Modules.Trapping
{
    public sealed class CCS_TrapInstanceSaveState
    {
        public string instanceId = string.Empty;
        public string trapDefinitionId = string.Empty;
        public int trapState;
        public float positionX;
        public float positionY;
        public float positionZ;
        public float rotationY;
        public string capturedWildlifeId = string.Empty;
        public string capturedInstanceKey = string.Empty;
        public float remainingTimerSeconds;
        public bool hasCaptureData;
    }
}
