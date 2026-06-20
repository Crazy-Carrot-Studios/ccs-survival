using Unity.Netcode;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_HealthRegenController
// CATEGORY: Modules / Attributes / Runtime / Components
// PURPOSE: Server-authoritative delayed health regeneration after damage.
// PLACEMENT: Player root alongside CCS_AttributeContainer.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Solo/offline mutates container locally. Multiplayer regen runs on server only.
// =============================================================================

namespace CCS.Modules.Attributes
{
    public sealed class CCS_HealthRegenController : NetworkBehaviour
    {
        #region Variables

        [SerializeField] private CCS_AttributeContainer attributeContainer;

        [SerializeField] private CCS_AttributeDefinition healthDefinition;

        [SerializeField] private CCS_NetworkAttributeReplicator networkAttributeReplicator;

        [Header("Health Regen Tuning")]
        [SerializeField] private float regenDelaySeconds = 5f;

        [SerializeField] private float regenPerSecond = 3f;

        private readonly NetworkVariable<bool> replicatedIsDead =
            new NetworkVariable<bool>(
                false,
                NetworkVariableReadPermission.Everyone,
                NetworkVariableWritePermission.Server);

        private readonly NetworkVariable<bool> replicatedIsRegenerating =
            new NetworkVariable<bool>(
                false,
                NetworkVariableReadPermission.Everyone,
                NetworkVariableWritePermission.Server);

        private float lastDamageTime = float.NegativeInfinity;

        private bool localIsDead;

        private bool localIsRegenerating;

        #endregion

        #region Properties

        public bool IsDead => UsesReplicatedState() ? replicatedIsDead.Value : localIsDead;

        public bool IsRegenerating => UsesReplicatedState() ? replicatedIsRegenerating.Value : localIsRegenerating;

        #endregion

        #region Unity Callbacks

        private void Awake()
        {
            if (attributeContainer == null)
            {
                attributeContainer = GetComponent<CCS_AttributeContainer>();
            }

            if (networkAttributeReplicator == null)
            {
                networkAttributeReplicator = GetComponent<CCS_NetworkAttributeReplicator>();
            }
        }

        private void OnEnable()
        {
            if (attributeContainer != null)
            {
                attributeContainer.DamageApplied += HandleDamageApplied;
            }

            RefreshStateFlags();
            PublishStateFlags();
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            RefreshStateFlags();
            PublishStateFlags();
        }

        private void OnDisable()
        {
            if (attributeContainer != null)
            {
                attributeContainer.DamageApplied -= HandleDamageApplied;
            }

            localIsRegenerating = false;
            PublishStateFlags();
        }

        private void Update()
        {
            if (!HasHealthAuthority() || attributeContainer == null)
            {
                return;
            }

            float deltaTime = Time.deltaTime;
            if (deltaTime <= 0f)
            {
                return;
            }

            string attributeId = ResolveHealthAttributeId();
            if (!attributeContainer.TryGetValue(attributeId, out CCS_AttributeValue value))
            {
                localIsDead = false;
                localIsRegenerating = false;
                PublishStateFlags();
                return;
            }

            float current = value.Current;
            float max = value.Max;
            localIsDead = current <= 0f;

            if (localIsDead || current >= max)
            {
                localIsRegenerating = false;
                PublishStateFlags();
                return;
            }

            if (Time.time - lastDamageTime < regenDelaySeconds)
            {
                localIsRegenerating = false;
                PublishStateFlags();
                return;
            }

            localIsRegenerating = true;
            float newHealth = Mathf.Min(max, current + regenPerSecond * deltaTime);
            if (Mathf.Approximately(newHealth, current))
            {
                localIsRegenerating = false;
                PublishStateFlags();
                return;
            }

            ApplyHealthOnAuthority(newHealth);
            PublishStateFlags();
        }

        #endregion

        #region Private Methods

        private void HandleDamageApplied(CCS_DamageAppliedEvent damageEvent)
        {
            if (!string.Equals(damageEvent.AttributeId, ResolveHealthAttributeId(), System.StringComparison.Ordinal))
            {
                return;
            }

            lastDamageTime = Time.time;
            RefreshStateFlags();
            PublishStateFlags();
        }

        private void RefreshStateFlags()
        {
            if (attributeContainer == null
                || !attributeContainer.TryGetValue(ResolveHealthAttributeId(), out CCS_AttributeValue value))
            {
                localIsDead = false;
                localIsRegenerating = false;
                return;
            }

            localIsDead = value.Current <= 0f;
            if (localIsDead || value.Current >= value.Max)
            {
                localIsRegenerating = false;
            }
        }

        private void PublishStateFlags()
        {
            if (!UsesReplicatedState() || !IsServer)
            {
                return;
            }

            replicatedIsDead.Value = localIsDead;
            replicatedIsRegenerating.Value = localIsRegenerating;
        }

        private void ApplyHealthOnAuthority(float newHealth)
        {
            if (networkAttributeReplicator != null)
            {
                networkAttributeReplicator.ApplyAuthorityHealthValue(newHealth);
                return;
            }

            attributeContainer.SetValue(ResolveHealthAttributeId(), newHealth);
        }

        private bool HasHealthAuthority()
        {
            if (!UsesReplicatedState())
            {
                return true;
            }

            return IsServer;
        }

        private bool UsesReplicatedState()
        {
            return IsSpawned && NetworkManager != null && NetworkManager.IsListening;
        }

        private string ResolveHealthAttributeId()
        {
            if (healthDefinition != null && !string.IsNullOrWhiteSpace(healthDefinition.ProfileId))
            {
                return healthDefinition.ProfileId;
            }

            return CCS_AttributesConstants.HealthAttributeId;
        }

        #endregion
    }
}
