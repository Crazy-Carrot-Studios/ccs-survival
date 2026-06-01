// =============================================================================
// SCRIPT: CCS_SaveEventArgs
// CATEGORY: Modules / SaveSystem / Runtime / Events
// PURPOSE: Event payload for save and load lifecycle notifications.
// PLACEMENT: Raised by CCS_SaveService on save/load operations.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-06-01
// NOTES: UI/HUD remain decoupled; subscribe through composition wiring only.
// =============================================================================

namespace CCS.Modules.SaveSystem
{
    public sealed class CCS_SaveEventArgs
    {
        #region Variables

        private readonly string savePath;
        private readonly string timestamp;
        private readonly bool isSuccess;
        private readonly string message;

        #endregion

        #region Public Methods

        public CCS_SaveEventArgs(string savePath, string timestamp, bool isSuccess, string message = "")
        {
            this.savePath = savePath ?? string.Empty;
            this.timestamp = timestamp ?? string.Empty;
            this.isSuccess = isSuccess;
            this.message = message ?? string.Empty;
        }

        #endregion

        #region Properties

        public string SavePath => savePath;

        public string Timestamp => timestamp;

        public bool IsSuccess => isSuccess;

        public string Message => message;

        #endregion
    }
}
