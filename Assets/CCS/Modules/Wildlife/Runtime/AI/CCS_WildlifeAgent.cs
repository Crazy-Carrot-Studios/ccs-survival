using UnityEngine;

// =============================================================================
// SCRIPT: CCS_WildlifeAgent
// CATEGORY: Modules / Wildlife / Runtime / AI
// PURPOSE: Passive living wildlife agent with wander, idle, alert, and flee behavior.
// PLACEMENT: Bootstrap test objects CCS_TestRabbit and CCS_TestDeer primitive placeholders.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: Transform movement only. No combat, health, damage, death, or NavMesh.
// =============================================================================

namespace CCS.Modules.Wildlife
{
    public sealed class CCS_WildlifeAgent : MonoBehaviour
    {
        #region Variables

        [Header("Agent Identity")]
        [Tooltip("Display name used by HUD debug and validation.")]
        [SerializeField] private string agentDisplayName = "Rabbit";

        [Tooltip("Species used to resolve wander, flee, and move speed tuning.")]
        [SerializeField] private CCS_WildlifeAiSpecies species = CCS_WildlifeAiSpecies.Rabbit;

        [Header("Dependencies")]
        [Tooltip("Optional profile override. Falls back to CCS_WildlifeAiService active profile.")]
        [SerializeField] private CCS_WildlifeAiProfile aiProfileOverride;

        [Tooltip("Player transform used for flee detection. Resolved at runtime when unset.")]
        [SerializeField] private Transform playerTransform;

        private CCS_WildlifeStateMachine stateMachine;
        private CCS_WildlifeMovementController movementController;
        private CCS_WildlifeAiService aiService;
        private CCS_WildlifeAiProfile resolvedProfile;
        private CCS_WildlifeAiSpeciesSettings speciesSettings;
        private Vector3 homePosition;
        private bool isConfigured;

        #endregion

        #region Properties

        public string AgentDisplayName => agentDisplayName;

        public CCS_WildlifeAiSpecies Species => species;

        #endregion

        #region Public Methods

        public CCS_WildlifeAiSnapshot CreateSnapshot()
        {
            if (stateMachine == null)
            {
                return new CCS_WildlifeAiSnapshot(agentDisplayName, CCS_WildlifeAiState.Idle);
            }

            return new CCS_WildlifeAiSnapshot(agentDisplayName, stateMachine.CurrentState);
        }

        public void ConfigureForBootstrap(
            string displayName,
            CCS_WildlifeAiSpecies agentSpecies,
            CCS_WildlifeAiProfile profile,
            Transform player)
        {
            EnsureRuntimeObjects();
            agentDisplayName = displayName;
            species = agentSpecies;
            aiProfileOverride = profile;
            playerTransform = player;
            TryConfigureAgent();
        }

        #endregion

        #region Unity Callbacks

        private void Awake()
        {
            homePosition = transform.position;
            EnsureRuntimeObjects();
        }

        private void OnEnable()
        {
            EnsureRuntimeObjects();
            TryConfigureAgent();
        }

        private void Start()
        {
            if (!isConfigured)
            {
                TryConfigureAgent();
            }

            if (CCS_WildlifeRuntimeBridge.TryGetAiService(out aiService))
            {
                aiService.RegisterAgent(this);
            }
        }

        private void OnDisable()
        {
            if (aiService != null)
            {
                aiService.UnregisterAgent(this);
                aiService = null;
            }
        }

        private void Update()
        {
            if (!isConfigured || stateMachine == null || movementController == null)
            {
                return;
            }

            if (playerTransform == null
                && !CCS_WildlifeRuntimeBridge.TryResolvePlayerTransform(out playerTransform))
            {
                return;
            }

            TickBehavior(Time.deltaTime);

            if (movementController.UpdateMovement(Time.deltaTime)
                && stateMachine.CurrentState == CCS_WildlifeAiState.Wander)
            {
                BeginIdleState();
            }
        }

        private void OnDestroy()
        {
            if (stateMachine != null)
            {
                stateMachine.StateChanged -= HandleStateChanged;
            }
        }

        #endregion

        #region Private Methods

        private void EnsureRuntimeObjects()
        {
            if (movementController == null)
            {
                movementController = new CCS_WildlifeMovementController(transform);
            }

            if (stateMachine == null)
            {
                stateMachine = new CCS_WildlifeStateMachine();
                stateMachine.StateChanged += HandleStateChanged;
            }
        }

        private void TryConfigureAgent()
        {
            resolvedProfile = ResolveProfile();
            if (resolvedProfile == null)
            {
                isConfigured = false;
                return;
            }

            speciesSettings = resolvedProfile.GetSpeciesSettings(species);
            movementController.SetMoveSpeed(speciesSettings.moveSpeed);
            BeginIdleState();
            isConfigured = true;
        }

        private CCS_WildlifeAiProfile ResolveProfile()
        {
            if (aiProfileOverride != null)
            {
                return aiProfileOverride;
            }

            if (CCS_WildlifeRuntimeBridge.TryGetAiService(out CCS_WildlifeAiService service)
                && service.ActiveProfile != null)
            {
                return service.ActiveProfile;
            }

            return null;
        }

        private void TickBehavior(float deltaTime)
        {
            stateMachine.Tick(deltaTime);

            float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
            if (distanceToPlayer <= speciesSettings.fleeRadius)
            {
                if (stateMachine.CurrentState != CCS_WildlifeAiState.Flee
                    && stateMachine.CurrentState != CCS_WildlifeAiState.Alert)
                {
                    movementController.Stop();
                    stateMachine.EnterState(CCS_WildlifeAiState.Alert);
                }

                if (stateMachine.CurrentState == CCS_WildlifeAiState.Alert
                    && stateMachine.IsAlertComplete(resolvedProfile.AlertDurationSeconds))
                {
                    BeginFleeState();
                }

                return;
            }

            if (stateMachine.CurrentState == CCS_WildlifeAiState.Flee)
            {
                if (distanceToPlayer > speciesSettings.fleeRadius * 2f)
                {
                    BeginWanderState();
                }

                return;
            }

            if (stateMachine.CurrentState == CCS_WildlifeAiState.Alert)
            {
                BeginIdleState();
                return;
            }

            switch (stateMachine.CurrentState)
            {
                case CCS_WildlifeAiState.Idle:
                    if (stateMachine.IsIdleComplete())
                    {
                        BeginWanderState();
                    }

                    break;
            }
        }

        private void BeginIdleState()
        {
            movementController.Stop();
            stateMachine.EnterState(CCS_WildlifeAiState.Idle);

            float minimumIdle = resolvedProfile.MinimumIdleSeconds;
            float maximumIdle = resolvedProfile.MaximumIdleSeconds;
            float idleDuration = maximumIdle > minimumIdle
                ? Random.Range(minimumIdle, maximumIdle)
                : minimumIdle;
            stateMachine.SetIdleDurationTarget(idleDuration);
        }

        private void BeginWanderState()
        {
            Vector3 wanderPoint = CCS_WildlifeMovementController.PickRandomWanderPoint(
                homePosition,
                speciesSettings.wanderRadius);
            movementController.SetDestination(wanderPoint);
            stateMachine.EnterState(CCS_WildlifeAiState.Wander);
        }

        private void BeginFleeState()
        {
            Vector3 fleePoint = CCS_WildlifeMovementController.PickFleePoint(
                transform.position,
                playerTransform.position,
                resolvedProfile.FleeDestinationDistance);
            movementController.SetDestination(fleePoint);
            stateMachine.EnterState(CCS_WildlifeAiState.Flee);
        }

        private void HandleStateChanged(CCS_WildlifeAiState newState)
        {
            if (aiService == null
                && CCS_WildlifeRuntimeBridge.TryGetAiService(out aiService))
            {
                aiService.RegisterAgent(this);
            }

            aiService?.NotifyAgentStateChanged(this);
        }

        #endregion
    }
}
