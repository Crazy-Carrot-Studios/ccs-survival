using System;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_SurvivalState
// CATEGORY: Survival / Runtime / Survival / Data
// PURPOSE: Serializable runtime container for Phase 1 survival vitals and life state.
// PLACEMENT: Runtime data type. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-05-27
// NOTES: Value-type snapshot for events and debug readouts. Mutations occur through CCS_SurvivalModule.
// =============================================================================

namespace CCS.Survival
{
    [Serializable]
    public struct CCS_SurvivalState
    {
        #region Variables

        [SerializeField] private float health;
        [SerializeField] private float hunger;
        [SerializeField] private float thirst;
        [SerializeField] private float stamina;
        [SerializeField] private float bodyTemperature;
        [SerializeField] private float exposure;
        [SerializeField] private float injurySeverity;
        [SerializeField] private bool isAlive;

        #endregion

        #region Public Methods

        public static CCS_SurvivalState CreateDefault()
        {
            return new CCS_SurvivalState
            {
                health = 100f,
                hunger = 100f,
                thirst = 100f,
                stamina = 100f,
                bodyTemperature = 37f,
                exposure = 0f,
                injurySeverity = 0f,
                isAlive = true
            };
        }

        #endregion

        #region Properties

        public float Health
        {
            get => health;
            set => health = value;
        }

        public float Hunger
        {
            get => hunger;
            set => hunger = value;
        }

        public float Thirst
        {
            get => thirst;
            set => thirst = value;
        }

        public float Stamina
        {
            get => stamina;
            set => stamina = value;
        }

        public float BodyTemperature
        {
            get => bodyTemperature;
            set => bodyTemperature = value;
        }

        public float Exposure
        {
            get => exposure;
            set => exposure = value;
        }

        public float InjurySeverity
        {
            get => injurySeverity;
            set => injurySeverity = value;
        }

        public bool IsAlive
        {
            get => isAlive;
            set => isAlive = value;
        }

        #endregion
    }
}
