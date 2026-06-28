using CCS.Modules.Attributes;
using Unity.Netcode;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_AIBanditController
// CATEGORY: Modules / AI / Runtime / Components
// PURPOSE: Server-authoritative orchestrator for bandit AI profile, brain, and death.
// PLACEMENT: AI bandit root with NetworkObject.
// AUTHOR: James Schilz
// CREATED: 2026-06-25
// NOTES: In offline sessions behaves as local authority.
// =============================================================================

namespace CCS.Modules.AI
{
    [DefaultExecutionOrder(60)]
    public sealed class CCS_AIBanditController : NetworkBehaviour
    {
        [SerializeField] private CCS_AIBanditProfile profile;
        [SerializeField] private CCS_AIBanditBrain brain;
        [SerializeField] private CCS_NetworkHealth networkHealth;
        [SerializeField] private bool enableControllerDebugLogs;

        private bool initialized;

        public CCS_AIBanditProfile Profile => profile;

        private void Awake()
        {
            ResolveReferences();
            InitializeProfileBindings();
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            ResolveReferences();
            InitializeProfileBindings();
            initialized = true;
        }

        private void Update()
        {
            if (!initialized)
            {
                ResolveReferences();
                InitializeProfileBindings();
                initialized = true;
            }

            if (!IsAuthorityActive())
            {
                return;
            }

            if (networkHealth != null && networkHealth.IsDead)
            {
                brain?.ForceDeadState();
                return;
            }

            if (networkHealth != null && !networkHealth.IsDamageReady)
            {
                return;
            }

            brain?.TickBrain(canThink: true);
        }

        private bool IsAuthorityActive()
        {
            if (!IsSpawned || NetworkManager == null || !NetworkManager.IsListening)
            {
                return true;
            }

            return IsServer;
        }

        private void ResolveReferences()
        {
            if (brain == null)
            {
                brain = GetComponent<CCS_AIBanditBrain>();
            }

            if (networkHealth == null)
            {
                networkHealth = GetComponent<CCS_NetworkHealth>();
            }
        }

        private void InitializeProfileBindings()
        {
            if (brain != null)
            {
                brain.SetProfile(profile);
            }

            if (enableControllerDebugLogs && profile == null)
            {
                Debug.LogWarning("[AI] AIBanditController missing profile.", this);
            }
        }
    }
}
