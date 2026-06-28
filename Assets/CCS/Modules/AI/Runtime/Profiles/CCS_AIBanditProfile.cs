using CCS.Project;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_AIBanditProfile
// CATEGORY: Modules / AI / Runtime / Profiles
// PURPOSE: Profile-driven tuning for network-ready bandit sensing, movement, and combat.
// PLACEMENT: ScriptableObject asset under AI/Content/Profiles.
// AUTHOR: James Schilz
// CREATED: 2026-06-25
// NOTES: v0.7.0 defaults target simple deterministic master-test behavior.
// =============================================================================

namespace CCS.Modules.AI
{
    [CreateAssetMenu(
        fileName = "CCS_AIBanditProfile",
        menuName = "CCS/AI/Bandit Profile",
        order = 0)]
    public sealed class CCS_AIBanditProfile : CCS_SurvivalProfileBase
    {
        [Header("Identity")]
        [SerializeField] private string displayName = CCS_AIConstants.AIBanditLabel;

        [Header("Health")]
        [SerializeField] private float maxHealth = 100f;

        [Header("Sensing")]
        [SerializeField] private float detectionRange = 45f;
        [SerializeField] private float lineOfSightHeight = CCS_AIConstants.DefaultAimChestHeight;
        [SerializeField] private LayerMask visibilityMask = ~0;

        [Header("Movement")]
        [SerializeField] private float moveSpeed = 2.8f;
        [SerializeField] private float rotationSpeed = 540f;
        [SerializeField] private float minimumPreferredRange = 6f;
        [SerializeField] private float movementStopDistance = 1.5f;

        [Header("Pathfinding")]
        [SerializeField] private float repathIntervalSeconds = 0.15f;
        [SerializeField] private float destinationUpdateThreshold = 0.35f;
        [SerializeField] private float targetSampleRadius = 4f;
        [SerializeField] private bool pathRefreshWhenStale = true;
        [SerializeField] private float loseSightRepathGraceSeconds = 0.15f;

        [Header("Combat")]
        [SerializeField] private float attackRange = 14f;
        [SerializeField] private float aimSettleSeconds = 0.55f;
        [SerializeField] private float fireCooldownSeconds = 1.5f;
        [SerializeField] private float shotDamage = 20f;
        [SerializeField] private float shotMaxRange = 45f;

        [Header("Spawn")]
        [SerializeField] private float spawnDistanceFromPlayer = 24f;
        [SerializeField] private float spawnSideOffset = 8f;

        [Header("Debug")]
        [SerializeField] private bool enableAIDebugLogs;

        public string DisplayName => displayName;

        public float MaxHealth => maxHealth;

        public float DetectionRange => detectionRange;

        public float LineOfSightHeight => lineOfSightHeight;

        public LayerMask VisibilityMask => visibilityMask;

        public float MoveSpeed => moveSpeed;

        public float RotationSpeed => rotationSpeed;

        public float MinimumPreferredRange => minimumPreferredRange;

        public float MovementStopDistance => movementStopDistance;

        public float RepathIntervalSeconds => repathIntervalSeconds;

        public float DestinationUpdateThreshold => destinationUpdateThreshold;

        public float TargetSampleRadius => targetSampleRadius;

        public bool PathRefreshWhenStale => pathRefreshWhenStale;

        public float LoseSightRepathGraceSeconds => loseSightRepathGraceSeconds;

        public float AttackRange => attackRange;

        public float AimSettleSeconds => aimSettleSeconds;

        public float FireCooldownSeconds => fireCooldownSeconds;

        public float ShotDamage => shotDamage;

        public float ShotMaxRange => shotMaxRange;

        public float SpawnDistanceFromPlayer => spawnDistanceFromPlayer;

        public float SpawnSideOffset => spawnSideOffset;

        public bool EnableAIDebugLogs => enableAIDebugLogs;
    }
}
