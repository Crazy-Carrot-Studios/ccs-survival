using UnityEngine;

// =============================================================================
// SCRIPT: CCS_GatheringNode
// CATEGORY: Modules / Gathering / Runtime / Nodes
// PURPOSE: Primitive gathering node state for availability, depletion, and respawn.
// PLACEMENT: Bootstrap test objects such as CCS_TestGatheringSmallTree.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-06-01
// NOTES: No inventory logic. Gathering is executed through CCS_GatheringService.
// =============================================================================

namespace CCS.Modules.Gathering
{
    public sealed class CCS_GatheringNode : MonoBehaviour
    {
        #region Variables

        [Header("Node Identity")]
        [Tooltip("Stable save id used by CCS_SaveService for node depletion persistence.")]
        [SerializeField] private string saveNodeId = string.Empty;

        [Tooltip("Gathering node archetype used to resolve profile rewards.")]
        [SerializeField] private CCS_GatheringNodeType nodeType = CCS_GatheringNodeType.SmallTree;

        [Tooltip("When false, the node cannot be gathered until respawn restores availability.")]
        [SerializeField] private bool isAvailable = true;

        [Header("Gather Point")]
        [Tooltip("Optional world gather anchor. Defaults to this transform when unset.")]
        [SerializeField] private Transform gatherPoint;

        private CCS_GatheringProfile configuredProfile;
        private CCS_GatheringService gatheringService;
        private float respawnTimer;
        private bool isRegistered;

        #endregion

        #region Properties

        public CCS_GatheringNodeType NodeType => nodeType;

        public string SaveNodeId => saveNodeId;

        public bool IsAvailable => isAvailable;

        public float RespawnTimer => respawnTimer;

        public Transform GatherPoint => gatherPoint != null ? gatherPoint : transform;

        #endregion

        #region Unity Callbacks

        private void OnEnable()
        {
            TryRegisterWithService();
        }

        private void OnDisable()
        {
            if (gatheringService != null && isRegistered)
            {
                gatheringService.UnregisterNode(this);
                isRegistered = false;
            }
        }

        private void Update()
        {
            TickRespawn();
        }

        #endregion

        #region Public Methods

        public void ConfigureFromProfile(CCS_GatheringProfile profile, CCS_GatheringNodeType configuredNodeType)
        {
            configuredProfile = profile;
            nodeType = configuredNodeType;
            isAvailable = true;
            respawnTimer = 0f;
        }

        public void ConfigureSaveNodeId(string configuredSaveNodeId)
        {
            if (!string.IsNullOrWhiteSpace(configuredSaveNodeId))
            {
                saveNodeId = configuredSaveNodeId;
            }
        }

        public bool MatchesSaveNodeId(string nodeId)
        {
            return !string.IsNullOrWhiteSpace(saveNodeId)
                && saveNodeId == nodeId;
        }

        public CCS_GatheringNodeSaveState CaptureSaveState()
        {
            return new CCS_GatheringNodeSaveState
            {
                nodeId = saveNodeId,
                isAvailable = isAvailable,
                respawnTimer = respawnTimer
            };
        }

        public void ApplySaveState(bool available, float savedRespawnTimer)
        {
            isAvailable = available;
            respawnTimer = savedRespawnTimer;
        }

        public bool CanGather()
        {
            ResolveService();
            return isAvailable
                && gatheringService != null
                && gatheringService.IsInitialized;
        }

        public bool Gather()
        {
            ResolveService();
            if (gatheringService == null)
            {
                return false;
            }

            CCS_GatheringResult result = gatheringService.TryGatherNode(this);
            return result != null && result.DidGather;
        }

        public void Deplete(CCS_GatheringProfile profile)
        {
            isAvailable = false;
            respawnTimer = 0f;

            if (profile == null || !profile.RespawnEnabled)
            {
                return;
            }

            respawnTimer = profile.RespawnDelaySeconds;
        }

        #endregion

        #region Private Methods

        private void TryRegisterWithService()
        {
            ResolveService();
            if (gatheringService == null || isRegistered)
            {
                return;
            }

            gatheringService.RegisterNode(this);
            isRegistered = true;
        }

        private void ResolveService()
        {
            if (gatheringService != null && gatheringService.IsInitialized)
            {
                return;
            }

            CCS_GatheringRuntimeBridge.TryGetGatheringService(out gatheringService);
        }

        private void TickRespawn()
        {
            if (isAvailable || configuredProfile == null || !configuredProfile.RespawnEnabled)
            {
                return;
            }

            if (respawnTimer > 0f)
            {
                respawnTimer -= Time.deltaTime;
                if (respawnTimer > 0f)
                {
                    return;
                }
            }

            isAvailable = true;
            respawnTimer = 0f;
            ResolveService();
            gatheringService?.NotifyNodeRespawned(this);
        }

        #endregion
    }
}
