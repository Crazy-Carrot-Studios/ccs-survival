using System;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_WildlifeStateMachine
// CATEGORY: Modules / Wildlife / Runtime / States
// PURPOSE: Passive wildlife state machine for idle, wander, alert, and flee.
// PLACEMENT: Owned by CCS_WildlifeAgent. No combat or death transitions.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: Alert is a brief pre-flee state when the player enters flee radius.
// =============================================================================

namespace CCS.Modules.Wildlife
{
    public sealed class CCS_WildlifeStateMachine
    {
        #region Variables

        private CCS_WildlifeAiState currentState = CCS_WildlifeAiState.Idle;
        private float stateTimer;
        private float idleDurationTarget;

        #endregion

        #region Events

        public event Action<CCS_WildlifeAiState> StateChanged;

        #endregion

        #region Properties

        public CCS_WildlifeAiState CurrentState => currentState;

        public float StateTimer => stateTimer;

        #endregion

        #region Public Methods

        public void Reset(CCS_WildlifeAiState initialState)
        {
            EnterState(initialState, true);
        }

        public void EnterState(CCS_WildlifeAiState nextState, bool force = false)
        {
            if (!force && currentState == nextState)
            {
                return;
            }

            currentState = nextState;
            stateTimer = 0f;
            StateChanged?.Invoke(currentState);
        }

        public void Tick(float deltaTime)
        {
            stateTimer += deltaTime;
        }

        public void SetIdleDurationTarget(float durationSeconds)
        {
            idleDurationTarget = Mathf.Max(0f, durationSeconds);
        }

        public bool IsIdleComplete()
        {
            return currentState == CCS_WildlifeAiState.Idle && stateTimer >= idleDurationTarget;
        }

        public bool IsAlertComplete(float alertDurationSeconds)
        {
            return currentState == CCS_WildlifeAiState.Alert && stateTimer >= alertDurationSeconds;
        }

        #endregion
    }
}
