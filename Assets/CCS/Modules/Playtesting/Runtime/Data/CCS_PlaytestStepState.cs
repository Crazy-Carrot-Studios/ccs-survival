// =============================================================================
// SCRIPT: CCS_PlaytestStepState
// CATEGORY: Modules / Playtesting / Runtime / Data
// PURPOSE: Runtime checklist state for a single playtest step definition.
// PLACEMENT: Owned by CCS_PlaytestService.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-06-01
// NOTES: Not serialized; rebuilt from profile on reset.
// =============================================================================

namespace CCS.Modules.Playtesting
{
    public sealed class CCS_PlaytestStepState
    {
        #region Variables

        private CCS_PlaytestStepStatus status = CCS_PlaytestStepStatus.NotStarted;
        private float activeElapsedSeconds;
        private int progressCount;

        #endregion

        #region Public Methods

        public CCS_PlaytestStepState(CCS_PlaytestStepDefinition definition)
        {
            Definition = definition;
        }

        public void Reset()
        {
            status = CCS_PlaytestStepStatus.NotStarted;
            activeElapsedSeconds = 0f;
            progressCount = 0;
        }

        public void SetStatus(CCS_PlaytestStepStatus newStatus)
        {
            status = newStatus;
            if (newStatus == CCS_PlaytestStepStatus.Active)
            {
                activeElapsedSeconds = 0f;
            }
        }

        public void TickActive(float deltaTime)
        {
            if (status == CCS_PlaytestStepStatus.Active && deltaTime > 0f)
            {
                activeElapsedSeconds += deltaTime;
            }
        }

        public void AddProgress(int amount = 1)
        {
            progressCount += amount < 1 ? 1 : amount;
        }

        #endregion

        #region Properties

        public CCS_PlaytestStepDefinition Definition { get; }

        public CCS_PlaytestStepStatus Status => status;

        public float ActiveElapsedSeconds => activeElapsedSeconds;

        public int ProgressCount => progressCount;

        public bool HasMetRequiredCount =>
            Definition == null || progressCount >= Definition.RequiredCount;

        public bool HasTimedOut =>
            Definition != null
            && Definition.TimeoutSeconds > 0f
            && status == CCS_PlaytestStepStatus.Active
            && activeElapsedSeconds >= Definition.TimeoutSeconds;

        #endregion
    }
}
