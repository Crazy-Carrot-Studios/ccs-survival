using Unity.Netcode;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_NetworkHealth
// CATEGORY: Modules / Attributes / Runtime / Components
// PURPOSE: Server-authoritative networked health component implementing CCS_IDamageable.
// PLACEMENT: Networked character root with CCS_AttributeContainer.
// AUTHOR: James Schilz
// CREATED: 2026-06-25
// NOTES: Mirrors CCS_NetworkAttributeReplicator pattern while exposing combat interface.
// =============================================================================

namespace CCS.Modules.Attributes
{
    public sealed class CCS_NetworkHealth : NetworkBehaviour, CCS_IDamageable
    {
        [SerializeField] private CCS_AttributeContainer attributeContainer;
        [SerializeField] private CCS_AttributeDefinition healthDefinition;
        [SerializeField] private bool enableHealthDebugLogs;

        private readonly NetworkVariable<float> replicatedHealth =
            new NetworkVariable<float>(
                0f,
                NetworkVariableReadPermission.Everyone,
                NetworkVariableWritePermission.Server);

        private readonly NetworkVariable<bool> replicatedDead =
            new NetworkVariable<bool>(
                false,
                NetworkVariableReadPermission.Everyone,
                NetworkVariableWritePermission.Server);

        public float MaxHealth
        {
            get
            {
                if (attributeContainer != null
                    && attributeContainer.TryGetValue(ResolveHealthAttributeId(), out CCS_AttributeValue value))
                {
                    return value.Max;
                }

                return healthDefinition != null ? healthDefinition.MaxValue : 100f;
            }
        }

        public float CurrentHealth => replicatedHealth.Value;

        public bool IsDead => replicatedDead.Value;

        private void Awake()
        {
            if (attributeContainer == null)
            {
                attributeContainer = GetComponent<CCS_AttributeContainer>();
            }
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            replicatedHealth.OnValueChanged += HandleReplicatedHealthChanged;
            replicatedDead.OnValueChanged += HandleReplicatedDeadChanged;

            if (IsServer)
            {
                float initialHealth = ResolveInitialHealth();
                replicatedHealth.Value = initialHealth;
                replicatedDead.Value = initialHealth <= 0f;
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
            replicatedDead.OnValueChanged -= HandleReplicatedDeadChanged;
            base.OnNetworkDespawn();
        }

        public bool ApplyDamage(CCS_DamageInfo damageInfo)
        {
            if (damageInfo.Amount <= 0f || IsDead)
            {
                return false;
            }

            if (!IsSpawned || NetworkManager == null || !NetworkManager.IsListening)
            {
                return ApplyDamageLocally(damageInfo);
            }

            if (IsServer)
            {
                return ApplyDamageOnServer(damageInfo);
            }

            RequestDamageServerRpc(
                damageInfo.Amount,
                (int)damageInfo.SourceType,
                damageInfo.HitPoint,
                damageInfo.HitDirection,
                damageInfo.SourceNetworkObjectId,
                damageInfo.AttributeId);
            return true;
        }

        [ServerRpc(RequireOwnership = false)]
        private void RequestDamageServerRpc(
            float amount,
            int sourceType,
            Vector3 hitPoint,
            Vector3 hitDirection,
            ulong sourceNetworkObjectId,
            string attributeId,
            ServerRpcParams rpcParams = default)
        {
            CCS_DamageInfo damageInfo = new CCS_DamageInfo(
                amount,
                hitPoint,
                hitDirection,
                (CCS_DamageSourceType)sourceType,
                sourceObject: null,
                sourceNetworkObjectId,
                attributeId);
            ApplyDamageOnServer(damageInfo);
        }

        private bool ApplyDamageOnServer(CCS_DamageInfo damageInfo)
        {
            if (!IsServer || attributeContainer == null || IsDead)
            {
                return false;
            }

            string attributeId = string.IsNullOrWhiteSpace(damageInfo.AttributeId)
                ? ResolveHealthAttributeId()
                : damageInfo.AttributeId;
            CCS_DamageRequest request = new CCS_DamageRequest(
                attributeId,
                damageInfo.Amount,
                damageInfo.SourceType.ToString());
            if (!attributeContainer.ApplyDamage(request, out CCS_DamageAppliedEvent damageEvent))
            {
                return false;
            }

            replicatedHealth.Value = damageEvent.ResultingValue.Current;
            replicatedDead.Value = damageEvent.ResultingValue.IsAtMin;

            if (enableHealthDebugLogs)
            {
                Debug.Log(
                    $"[Attributes] NetworkHealth damage={damageEvent.AppliedAmount:0.##} health={replicatedHealth.Value:0.##}",
                    this);
            }

            return damageEvent.AppliedAmount > 0f;
        }

        private bool ApplyDamageLocally(CCS_DamageInfo damageInfo)
        {
            if (attributeContainer == null || IsDead)
            {
                return false;
            }

            CCS_DamageRequest request = new CCS_DamageRequest(
                ResolveHealthAttributeId(),
                damageInfo.Amount,
                damageInfo.SourceType.ToString());
            if (!attributeContainer.ApplyDamage(request, out CCS_DamageAppliedEvent damageEvent))
            {
                return false;
            }

            replicatedHealth.Value = damageEvent.ResultingValue.Current;
            replicatedDead.Value = damageEvent.ResultingValue.IsAtMin;
            return damageEvent.AppliedAmount > 0f;
        }

        private void HandleReplicatedHealthChanged(float previousValue, float newValue)
        {
            if (IsServer)
            {
                return;
            }

            ApplyHealthToContainer(newValue, raiseEvents: !Mathf.Approximately(previousValue, newValue));
        }

        private void HandleReplicatedDeadChanged(bool previousValue, bool newValue)
        {
            if (previousValue == newValue || !enableHealthDebugLogs)
            {
                return;
            }

            Debug.Log(newValue ? "[Attributes] NetworkHealth entered dead state." : "[Attributes] NetworkHealth revived.", this);
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

            return healthDefinition != null ? healthDefinition.DefaultValue : 100f;
        }

        private string ResolveHealthAttributeId()
        {
            if (healthDefinition != null && !string.IsNullOrWhiteSpace(healthDefinition.ProfileId))
            {
                return healthDefinition.ProfileId;
            }

            return CCS_AttributesConstants.HealthAttributeId;
        }
    }
}
