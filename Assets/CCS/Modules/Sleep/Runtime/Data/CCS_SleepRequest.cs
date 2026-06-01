// =============================================================================
// SCRIPT: CCS_SleepRequest
// CATEGORY: Modules / Sleep / Runtime / Data
// PURPOSE: Input payload for sleep attempts through CCS_SleepService.
// PLACEMENT: Created by interactables, harnesses, and future player input.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: Zero or negative hours fall back to profile defaultSleepHours.
// =============================================================================

namespace CCS.Modules.Sleep
{
    public sealed class CCS_SleepRequest
    {
        #region Variables

        private readonly float requestedSleepHours;

        #endregion

        #region Constructors

        public CCS_SleepRequest(float requestedSleepHours = 0f)
        {
            this.requestedSleepHours = requestedSleepHours;
        }

        #endregion

        #region Properties

        public float RequestedSleepHours => requestedSleepHours;

        #endregion
    }
}
