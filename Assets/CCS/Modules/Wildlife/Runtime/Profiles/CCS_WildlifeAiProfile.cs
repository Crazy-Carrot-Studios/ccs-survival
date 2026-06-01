using System;
using CCS.Survival;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_WildlifeAiProfile
// CATEGORY: Modules / Wildlife / Runtime / Profiles
// PURPOSE: Tuning profile for passive rabbit and deer AI movement and flee behavior.
// PLACEMENT: Assets/CCS/Survival/Profiles/Wildlife/ (project shell configuration).
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: Transform movement only. No NavMesh, combat, or death tuning.
// =============================================================================

namespace CCS.Modules.Wildlife
{
    [CreateAssetMenu(
        fileName = "CCS_WildlifeAiProfile",
        menuName = "CCS/Survival/Wildlife/Wildlife AI Profile")]
    public sealed class CCS_WildlifeAiProfile : CCS_SurvivalProfileBase
    {
        #region Variables

        [Header("Rabbit")]
        [SerializeField] private CCS_WildlifeAiSpeciesSettings rabbitSettings = new CCS_WildlifeAiSpeciesSettings
        {
            wanderRadius = 10f,
            fleeRadius = 8f,
            moveSpeed = 4f
        };

        [Header("Deer")]
        [SerializeField] private CCS_WildlifeAiSpeciesSettings deerSettings = new CCS_WildlifeAiSpeciesSettings
        {
            wanderRadius = 20f,
            fleeRadius = 15f,
            moveSpeed = 6f
        };

        [Header("Shared Timing")]
        [Tooltip("Minimum idle pause before picking a new wander destination.")]
        [SerializeField] private float minimumIdleSeconds = 1f;

        [Tooltip("Maximum idle pause before picking a new wander destination.")]
        [SerializeField] private float maximumIdleSeconds = 3f;

        [Tooltip("Seconds spent in alert before fleeing begins.")]
        [SerializeField] private float alertDurationSeconds = 0.25f;

        [Tooltip("Distance used when choosing a flee destination away from the player.")]
        [SerializeField] private float fleeDestinationDistance = 6f;

        #endregion

        #region Properties

        public float MinimumIdleSeconds => minimumIdleSeconds;

        public float MaximumIdleSeconds => maximumIdleSeconds;

        public float AlertDurationSeconds => alertDurationSeconds;

        public float FleeDestinationDistance => fleeDestinationDistance;

        #endregion

        #region Public Methods

        public CCS_WildlifeAiSpeciesSettings GetSpeciesSettings(CCS_WildlifeAiSpecies species)
        {
            return species == CCS_WildlifeAiSpecies.Deer ? deerSettings : rabbitSettings;
        }

        #endregion
    }

    [Serializable]
    public struct CCS_WildlifeAiSpeciesSettings
    {
        [Tooltip("Maximum distance from home position when wandering.")]
        public float wanderRadius;

        [Tooltip("Player distance that triggers alert and flee behavior.")]
        public float fleeRadius;

        [Tooltip("Transform movement speed in units per second.")]
        public float moveSpeed;
    }
}
