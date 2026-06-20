using Unity.Netcode;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_NetworkAttributeReplicator
// CATEGORY: Modules / Attributes / Runtime / Components
// PURPOSE: Server-authoritative attribute replication for multiplayer test players.
// PLACEMENT: Networked player root alongside CCS_AttributeContainer.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Solo/offline sessions bypass Netcode and mutate the container locally.
//        Health is the first replicated attribute in v0.3.0.
// =============================================================================

namespace CCS.Modules.Attributes
{
    public sealed class CCS_NetworkAttributeReplicator : NetworkBehaviour
    {
        #region Variables

        [SerializeField] private CCS_AttributeContainer attributeContainer;

        [SerializeField] private CCS_AttributeDefinition healthDefinition;

        private readonly NetworkVariable<float> replicatedHealth =
            new NetworkVariable<float>(
                0f,
                NetworkVariableReadPermission.Everyone,
                NetworkVariableWritePermission.Server);

        #endregion

        #region Unity Callbacks

        private void Awake()
        {
            if (attributeContainer == null)
            {
                attributeContainer = GetComponent<CCS_AttributeContainer>();
            }
        }

        #endregion

        #region Public Methods

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            replicatedHealth.OnValueChanged += HandleReplicatedHealthChanged;

            if (IsServer)
            {
                float initialHealth = ResolveInitialHealth();
                replicatedHealth.Value = initialHealth;
                ApplyHealthToContainer(initialHealth, raiseEvents: false);
            }
            else
            {
                ApplyHealthToContainer(replicatedHealth.Value, raiseEvents: true);
            }
        }

        public override void OnNetworkDespawn()
        {
            replicatedHealth.OnValueChanged -= HandleReplicatedHealthChanged;
            base.OnNetworkDespawn();
        }

        public void RequestSelfDamage(float amount, string sourceLabel = "TestDamage")
        {
            if (amount <= 0f)
            {
                return;
            }

            if (!IsSpawned || NetworkManager == null || !NetworkManager.IsListening)
            {
                ApplyDamageLocally(amount, sourceLabel);
                return;
            }

            if (IsServer && IsOwner)
            {
                ApplyDamageOnServer(amount, sourceLabel);
                return;
            }

            if (IsOwner)
            {
                RequestSelfDamageServerRpc(amount, sourceLabel);
            }
        }

        public float GetReplicatedHealth()
        {
            return replicatedHealth.Value;
        }

        public void ApplyAuthorityHealthValue(float newHealth)
        {
            if (attributeContainer == null)
            {
                return;
            }

            string attributeId = ResolveHealthAttributeId();
            if (!IsSpawned || NetworkManager == null || !NetworkManager.IsListening)
            {
                attributeContainer.SetValue(attributeId, newHealth);
                return;
            }

            if (!IsServer)
            {
                return;
            }

            attributeContainer.SetValue(attributeId, newHealth);
            if (attributeContainer.TryGetValue(attributeId, out CCS_AttributeValue value))
            {
                replicatedHealth.Value = value.Current;
            }
        }

        #endregion

        #region Private Methods

        [ServerRpc]
        private void RequestSelfDamageServerRpc(float amount, string sourceLabel, ServerRpcParams rpcParams = default)
        {
            if (rpcParams.Receive.SenderClientId != OwnerClientId)
            {
                return;
            }

            ApplyDamageOnServer(amount, sourceLabel);
        }

        private void ApplyDamageOnServer(float amount, string sourceLabel)
        {
            if (!IsServer || attributeContainer == null)
            {
                return;
            }

            string attributeId = ResolveHealthAttributeId();
            CCS_DamageRequest request = new CCS_DamageRequest(attributeId, amount, sourceLabel);
            if (!attributeContainer.ApplyDamage(request, out CCS_DamageAppliedEvent damageEvent))
            {
                return;
            }

            replicatedHealth.Value = damageEvent.ResultingValue.Current;
        }

        private void ApplyDamageLocally(float amount, string sourceLabel)
        {
            if (attributeContainer == null)
            {
                return;
            }

            string attributeId = ResolveHealthAttributeId();
            CCS_DamageRequest request = new CCS_DamageRequest(attributeId, amount, sourceLabel);
            attributeContainer.ApplyDamage(request, out _);
        }

        private void HandleReplicatedHealthChanged(float previousValue, float newValue)
        {
            if (IsServer)
            {
                return;
            }

            ApplyHealthToContainer(newValue, raiseEvents: !Mathf.Approximately(previousValue, newValue));
        }

        private void ApplyHealthToContainer(float healthValue, bool raiseEvents)
        {
            if (attributeContainer == null)
            {
                return;
            }

            attributeContainer.SetValue(ResolveHealthAttributeId(), healthValue, raiseEvents);
        }

        private float ResolveInitialHealth()
        {
            if (attributeContainer != null
                && attributeContainer.TryGetValue(ResolveHealthAttributeId(), out CCS_AttributeValue value))
            {
                return value.Current;
            }

            if (healthDefinition != null)
            {
                return healthDefinition.DefaultValue;
            }

            return 100f;
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
