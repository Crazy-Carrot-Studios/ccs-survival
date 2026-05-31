// =============================================================================
// SCRIPT: CCS_ISaveable
// CATEGORY: Modules / SaveLoad / Runtime / Interfaces
// PURPOSE: Contract for modules and systems that participate in save/load.
// PLACEMENT: Implemented by future gameplay modules and development test saveables.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: No gameplay assumptions. Capture/restore payloads are module-owned JSON strings.
// =============================================================================

namespace CCS.Modules.SaveLoad
{
    public interface CCS_ISaveable
    {
        #region Properties

        string SaveableId { get; }

        #endregion

        #region Public Methods

        string CaptureState();

        void RestoreState(string stateJson);

        #endregion
    }
}
