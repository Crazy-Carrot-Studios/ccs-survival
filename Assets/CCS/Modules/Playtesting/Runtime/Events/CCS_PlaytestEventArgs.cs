// =============================================================================
// SCRIPT: CCS_PlaytestEventArgs
// CATEGORY: Modules / Playtesting / Runtime / Events
// PURPOSE: Event payload for playtest checklist lifecycle notifications.
// PLACEMENT: Raised by CCS_PlaytestService on step changes.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-06-01
// NOTES: Decoupled from UI; HUD may subscribe for refresh only.
// =============================================================================

namespace CCS.Modules.Playtesting
{
    public sealed class CCS_PlaytestEventArgs
    {
        #region Variables

        private readonly string stepId;
        private readonly CCS_PlaytestStepType stepType;
        private readonly CCS_PlaytestStepStatus status;
        private readonly string message;

        #endregion

        #region Public Methods

        public CCS_PlaytestEventArgs(
            string stepId,
            CCS_PlaytestStepType stepType,
            CCS_PlaytestStepStatus status,
            string message = "")
        {
            this.stepId = stepId ?? string.Empty;
            this.stepType = stepType;
            this.status = status;
            this.message = message ?? string.Empty;
        }

        #endregion

        #region Properties

        public string StepId => stepId;

        public CCS_PlaytestStepType StepType => stepType;

        public CCS_PlaytestStepStatus Status => status;

        public string Message => message;

        #endregion
    }
}
