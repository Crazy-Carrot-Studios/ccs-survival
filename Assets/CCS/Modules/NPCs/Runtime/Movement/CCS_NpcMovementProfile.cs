using System;
using CCS.Survival;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_NpcMovementProfile
// CATEGORY: Modules / NPCs / Runtime / Movement
// PURPOSE: Schedule hours, move speed, and arrival tolerance for placeholder NPCs.
// PLACEMENT: Assets/CCS/Survival/Profiles/NPCs/Movement/
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 4.5.0 — schedule integration via time-of-day work/home hours.
// =============================================================================

namespace CCS.Modules.NPCs
{
    [CreateAssetMenu(
        fileName = "CCS_NpcMovementProfile",
        menuName = "CCS/Survival/NPCs/NPC Movement Profile")]
    public sealed class CCS_NpcMovementProfile : CCS_SurvivalProfileBase
    {
        [SerializeField] private int workStartHour = 6;

        [SerializeField] private int workEndHour = 18;

        [SerializeField] private float moveSpeed = 1.75f;

        [SerializeField] private float arrivalTolerance = 0.5f;

        [SerializeField] private float idleRotationSpeed = 45f;

        public int WorkStartHour => workStartHour;

        public int WorkEndHour => workEndHour;

        public float MoveSpeed => moveSpeed;

        public float ArrivalTolerance => arrivalTolerance;

        public float IdleRotationSpeed => idleRotationSpeed;
    }
}
