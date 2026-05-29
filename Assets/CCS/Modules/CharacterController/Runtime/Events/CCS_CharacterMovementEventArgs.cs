// =============================================================================
// SCRIPT: CCS_CharacterMovementEventArgs
// CATEGORY: Modules / CharacterController / Runtime / Events
// PURPOSE: Event payload for movement, grounding, jump, sprint, crouch, and stamina hooks.
// PLACEMENT: Passed to CCS_CharacterMovementService subscribers.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Stamina drain is a request only — Survival Core is not called in 0.3.8.
// =============================================================================

namespace CCS.Modules.CharacterController
{
    public sealed class CCS_CharacterMovementEventArgs
    {
        #region Public Methods

        public CCS_CharacterMovementEventArgs(
            CCS_CharacterMovementSnapshot previousSnapshot,
            CCS_CharacterMovementSnapshot currentSnapshot,
            float staminaDrainPerSecond = 0f)
        {
            PreviousSnapshot = previousSnapshot;
            CurrentSnapshot = currentSnapshot;
            StaminaDrainPerSecond = staminaDrainPerSecond;
        }

        #endregion

        #region Properties

        public CCS_CharacterMovementSnapshot PreviousSnapshot { get; }

        public CCS_CharacterMovementSnapshot CurrentSnapshot { get; }

        public float StaminaDrainPerSecond { get; }

        #endregion
    }
}
